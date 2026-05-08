using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public record FlagsColetaResult(
    IReadOnlyList<string> FlagsGeradas,
    IReadOnlyList<string> FlagsExpiradas,
    IReadOnlyList<string> FlagsCompostas);

public class TorreExploracaoService
{
    private readonly ITorreExploracaoRepository _exploracaoRepo;
    private readonly ITorreBoosterRepository _boosterRepo;
    private readonly ITorreRepository _torreRepo;
    private readonly IHeroiRepository _heroiRepo;
    private readonly ICidadeRepository _cidadeRepo;
    private readonly FragmentService _fragmentService;
    private readonly HeroiLevelUpService _levelUpService;
    private readonly BiomeService _biomeService;
    private readonly RecruitmentService _recruitmentService;
    private readonly RewardDistributionService _rewardService;
    private readonly TorreFlagService _flagService;
    private readonly IHeroiPericiaRepository _periciaRepo;

    public TorreExploracaoService(
        ITorreExploracaoRepository exploracaoRepo,
        ITorreBoosterRepository boosterRepo,
        ITorreRepository torreRepo,
        IHeroiRepository heroiRepo,
        ICidadeRepository cidadeRepo,
        FragmentService fragmentService,
        HeroiLevelUpService levelUpService,
        BiomeService biomeService,
        RecruitmentService recruitmentService,
        RewardDistributionService rewardService,
        TorreFlagService flagService,
        IHeroiPericiaRepository periciaRepo)
    {
        _exploracaoRepo   = exploracaoRepo;
        _boosterRepo      = boosterRepo;
        _torreRepo        = torreRepo;
        _heroiRepo        = heroiRepo;
        _cidadeRepo       = cidadeRepo;
        _fragmentService  = fragmentService;
        _levelUpService   = levelUpService;
        _biomeService     = biomeService;
        _recruitmentService = recruitmentService;
        _rewardService    = rewardService;
        _flagService      = flagService;
        _periciaRepo      = periciaRepo;
    }

    // ── Tick ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Advances an active exploration by one tick. Should be called on every
    /// relevant user command. Idempotent if called too frequently (debounce 0.1 min).
    /// </summary>
    public async Task ProcessarAsync(Guid usuarioId)
    {
        var exploracao = await _exploracaoRepo.ObterAtivaAsync(usuarioId);
        if (exploracao == null) return;

        var agora   = DateTime.UtcNow;
        var elapsed = (agora - exploracao.UltimoTickEm).TotalMinutes;
        if (elapsed < 0.1) return;

        // Load heroes
        var heroisIds = ParseHeroisIds(exploracao.HeroisIds);
        if (heroisIds.Count == 0)
        {
            exploracao.Status      = StatusExploracao.Falha;
            exploracao.ConcluidoEm = agora;
            await _exploracaoRepo.AtualizarAsync(exploracao);
            return;
        }

        var herois = new List<Heroi>();
        foreach (var id in heroisIds)
        {
            var h = await _heroiRepo.ObterPorIdAsync(id);
            if (h != null) herois.Add(h);
        }

        if (herois.Count == 0)
        {
            exploracao.Status      = StatusExploracao.Falha;
            exploracao.ConcluidoEm = agora;
            await _exploracaoRepo.AtualizarAsync(exploracao);
            return;
        }

        double teamPS  = HeroPowerScoreService.CalcularParty(herois);
        double cdi     = HeroPowerScoreService.CalcularCDI(exploracao.AndarNumero);
        double ratio   = HeroPowerScoreService.CalcularRatio(teamPS, cdi);

        // Booster: Eficiencia
        double boosterMult = exploracao.BoosterAtivo == TipoBooster.Eficiencia ? 1.20 : 1.0;

        double progressoPorMinuto = Math.Min(1.5 * ratio * boosterMult, 3.0);

        // Failure check
        double failChancePorMinuto = ratio >= 1.0
            ? 0.001
            : 0.05 * (1.0 - ratio);
        double totalFailChance = Math.Min(failChancePorMinuto * elapsed, 0.80);

        if (Random.Shared.NextDouble() < totalFailChance)
        {
            exploracao.Status          = StatusExploracao.Falha;
            exploracao.HeroisFeridosIds = exploracao.HeroisIds;
            exploracao.ConcluidoEm     = agora;
            await _exploracaoRepo.AtualizarAsync(exploracao);
            return;
        }

        // Progress advance
        double progressoGanho = Math.Min(progressoPorMinuto * elapsed, 100.0 - exploracao.Progresso);
        double newProgress     = exploracao.Progresso + progressoGanho;

        // ── Skill event (20% chance per tick) ────────────────────────────────────
        if (Random.Shared.NextDouble() < PericiaEventoConfig.ChanceEventoPorAndar)
        {
            var eventoIdx = Random.Shared.Next(PericiaEventoConfig.Eventos.Count);
            var evento    = PericiaEventoConfig.Eventos[eventoIdx];

            // Load pericias for all heroes in this party
            var pericias = await _periciaRepo.ObterPorHeroisAsync(herois.Select(h => h.Id));

            bool sucesso;
            if (evento.EhGrupo)
            {
                (sucesso, _) = SkillCheckService.RolarGrupo(
                    herois, evento.PericiaExigida, evento.DC,
                    pericias, evento.RollContext ?? new SkillRollContext());
            }
            else
            {
                // Pick the hero with the highest bonus for this skill
                var periciaAtributo = SkillCheckService.AtributoDePericia(evento.PericiaExigida);
                var heroi = herois
                    .OrderByDescending(h => h.ObterAtributosTotais(SkillCheckService.EmptyBonus)
                        .Get(periciaAtributo))
                    .First();
                (sucesso, _) = SkillCheckService.Rolar(
                    heroi, evento.PericiaExigida, evento.DC,
                    pericias, evento.RollContext ?? new SkillRollContext());
            }

            double eventBonus = sucesso ? 0.05 : -0.10;
            progressoGanho = Math.Clamp(
                progressoGanho + eventBonus * 100.0,
                0.0,
                100.0 - exploracao.Progresso);
            newProgress = exploracao.Progresso + progressoGanho;
        }

        // Checkpoints
        int interval        = exploracao.CheckpointInterval;
        int nextCheckpoint  = exploracao.UltimoCheckpoint + interval;
        double ouroBoosterMult = exploracao.BoosterAtivo == TipoBooster.Ouro ? 1.30 : 1.0;
        double fragBoosterMult = exploracao.BoosterAtivo == TipoBooster.Fragmento ? 1.50 : 1.0;

        while (nextCheckpoint <= newProgress && nextCheckpoint <= 100)
        {
            int ouro = (int)((exploracao.AndarNumero * 2 + 10) * (1.0 + ratio * 0.3) * ouroBoosterMult);
            exploracao.LootOuro += ouro;

            if (Random.Shared.NextDouble() < 0.20 * fragBoosterMult)
            {
                var drops = await _fragmentService.ProcessarDropAsync(usuarioId, exploracao.AndarNumero);
                if (drops.Count > 0)
                    exploracao.LootFragmentosQtd += drops.Sum(d => d.Quantidade);
            }

            exploracao.UltimoCheckpoint = nextCheckpoint;
            nextCheckpoint += interval;
        }

        // Floor clear
        if (newProgress >= 100.0)
        {
            exploracao.Status      = StatusExploracao.Concluida;
            exploracao.ConcluidoEm = agora;

            // Mark floor objective
            var andar = await _torreRepo.ObterAndarPorUsuarioAsync(usuarioId);
            if (andar != null && andar.Numero == exploracao.AndarNumero)
            {
                andar.ObjetivoCumprido = true;
                andar.DataAlteracao    = agora;
                await _torreRepo.AtualizarAsync(andar);
            }

            // XP grant
            int xpBase     = 10 + exploracao.AndarNumero * 5;
            double bossMult = andar?.Tipo switch
            {
                LegendsAwaken.Domain.Entities.TipoAndar.BossFacil   => 1.5,
                LegendsAwaken.Domain.Entities.TipoAndar.BossMedio   => 2.0,
                LegendsAwaken.Domain.Entities.TipoAndar.BossDificil => 3.0,
                _                                                    => 1.0,
            };
            int xpFinal = (int)(xpBase * bossMult);

            foreach (var heroi in herois)
            {
                _levelUpService.AplicarXp(heroi, xpFinal);
                await _heroiRepo.AtualizarAsync(heroi);
            }

            // Bioma / Marco checks
            _ = await _biomeService.EBiomaNovoAsync(exploracao.AndarNumero);
            _ = _biomeService.EAndarDeMarco(exploracao.AndarNumero);

            // Create next floor
            int proximoNumero                              = exploracao.AndarNumero + 1;
            LegendsAwaken.Domain.Entities.TipoAndar proximoTipo = DefinirTipoAndar(proximoNumero);
            var proximoAndar  = new TorreAndar
            {
                Id               = Guid.NewGuid(),
                Numero           = proximoNumero,
                Tipo             = proximoTipo,
                NivelDificuldade = CalcularDificuldade(proximoNumero, proximoTipo),
                UsuarioId        = usuarioId,
                CriadoEm         = agora,
                ObjetivoCumprido = false,
            };
            await _torreRepo.AdicionarAsync(proximoAndar);
        }

        exploracao.Progresso     = Math.Min(newProgress, 100.0);
        exploracao.UltimoTickEm  = agora;
        await _exploracaoRepo.AtualizarAsync(exploracao);
    }

    // ── Start ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts a new exploration for the user on their current floor.
    /// Validates no active/pending exploration exists and heroes are eligible.
    /// </summary>
    public async Task<TorreExploracao> IniciarAsync(
        Guid usuarioId,
        List<Guid> heroisIds,
        TipoBooster? booster)
    {
        var ativa = await _exploracaoRepo.ObterAtivaAsync(usuarioId);
        if (ativa != null)
            throw new InvalidOperationException("Ja existe uma exploracao ativa.");

        var pendente = await _exploracaoRepo.ObterPendenteAsync(usuarioId);
        if (pendente != null)
            throw new InvalidOperationException("Ha uma exploracao pendente de coleta.");

        // Validate heroes
        var herois = new List<Heroi>();
        foreach (var id in heroisIds)
        {
            var h = await _heroiRepo.ObterPorIdAsync(id);
            if (h == null)
                throw new InvalidOperationException($"Heroi {id} nao encontrado.");
            if (h.EstadoSustento == EstadoSustento.Inativo)
                throw new InvalidOperationException($"Heroi {h.Nome} esta inativo.");
            // Degradado: allow entry but ObterAtributosTotais applies -25% to all attributes
            herois.Add(h);
        }

        // Validate heroes are not wounded from a previous failed exploration
        if (pendente != null && !string.IsNullOrEmpty(pendente.HeroisFeridosIds))
        {
            var feridosIds = ParseHeroisIds(pendente.HeroisFeridosIds);
            foreach (var id in heroisIds)
            {
                if (feridosIds.Contains(id))
                    throw new InvalidOperationException($"Heroi {id} esta ferido e nao pode explorar.");
            }
        }

        // Consume booster if requested
        if (booster.HasValue)
        {
            bool consumido = await _boosterRepo.ConsumirAsync(usuarioId, booster.Value);
            if (!consumido)
                throw new InvalidOperationException($"Booster {booster.Value} nao disponivel.");
        }

        int checkpointInterval = booster == TipoBooster.Checkpoint
            ? Math.Max(10, 25 - 5)  // = 20
            : 25;

        double progressoInicial = booster == TipoBooster.Progresso ? 10.0 : 0.0;

        var andar = await _torreRepo.ObterAndarPorUsuarioAsync(usuarioId)
            ?? throw new InvalidOperationException("Nenhum andar ativo encontrado.");

        // Apply arc flag boss HP modifiers if this is a boss floor
        // Guard above (line 214-216) ensures IniciarAsync can only succeed once per active exploration,
        // so HP reduction cannot be applied twice to the same floor instance.
        if (TorreArcoConfig.EBossFloor(andar.Numero) && andar.Inimigos.Count > 0)
        {
            var (hpReduction, _) = await _flagService.ObterModificadoresBossAsync(usuarioId, andar.Numero);
            if (hpReduction > 0)
            {
                var fator = 1.0 - hpReduction;
                foreach (var inimigo in andar.Inimigos)
                    inimigo.Atributos.Constituicao = (int)(inimigo.Atributos.Constituicao * fator);
                await _torreRepo.AtualizarAsync(andar);
            }
        }

        var exploracao = new TorreExploracao
        {
            Id                  = Guid.NewGuid(),
            UsuarioId           = usuarioId,
            AndarNumero         = andar.Numero,
            Progresso           = progressoInicial,
            UltimoCheckpoint    = 0,
            CheckpointInterval  = checkpointInterval,
            Status              = StatusExploracao.Ativa,
            IniciadoEm          = DateTime.UtcNow,
            UltimoTickEm        = DateTime.UtcNow,
            HeroisIds           = string.Join(",", heroisIds.Select(id => id.ToString())),
            BoosterAtivo        = booster,
            LootOuro            = 0,
            LootFragmentosQtd   = 0,
            LootFragmentosHeroiId = "",
            HeroisFeridosIds    = "",
        };

        await _exploracaoRepo.SalvarAsync(exploracao);
        return exploracao;
    }

    // ── Collect ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Collects loot from a concluded or failed exploration.
    /// Requires the Discord user ID to look up the cidade via the EF repository.
    /// Returns the exploration entity and a flags result (only populated on success).
    /// </summary>
    public async Task<(TorreExploracao? Exploracao, FlagsColetaResult Flags)> ColetarAsync(Guid usuarioId, ulong discordId)
    {
        var exploracao = await _exploracaoRepo.ObterPendenteAsync(usuarioId);
        if (exploracao == null) return (null, new FlagsColetaResult([], [], []));

        // Credit ouro to cidade
        if (exploracao.LootOuro > 0)
        {
            var cidade = await _cidadeRepo.ObterPorProprietarioIdAsync(discordId);
            if (cidade != null)
            {
                cidade.Recursos.Ouro += exploracao.LootOuro;
                await _cidadeRepo.AtualizarAsync(cidade);
            }
        }

        // Capture the status before marking as collected, to drive flag processing.
        bool foiSucesso = exploracao.Status == StatusExploracao.Concluida;

        exploracao.Status = StatusExploracao.Coletada;
        await _exploracaoRepo.AtualizarAsync(exploracao);

        // --- Processamento de flags de arco (apenas no caminho de sucesso) ---
        FlagsColetaResult flagsResult = new([], [], []);
        if (foiSucesso)
            flagsResult = await ProcessarFlagsAsync(usuarioId, exploracao.AndarNumero);

        return (exploracao, flagsResult);
    }

    private async Task<FlagsColetaResult> ProcessarFlagsAsync(Guid userId, int andar)
    {
        var andarDef = TorreArcoConfig.ObterAndar(andar);
        var flagsGeradas = new List<string>();
        var flagsExpiradas = new List<string>();

        if (andarDef is not null)
        {
            // Objetivo secundário: 65% de chance de sucesso
            if (andarDef.ObjetivoSecundario is { } sec)
            {
                if (Random.Shared.NextDouble() < 0.65)
                {
                    await _flagService.GerarFlagAsync(userId, andar, sec.FlagNome);
                    flagsGeradas.Add(sec.FlagNome);
                }
                else
                {
                    await _flagService.MarcarSecundarioExpiradoAsync(userId, andar);
                    flagsExpiradas.Add(sec.FlagNome);
                }
            }

            // Flags adicionais do andar (primárias — sempre em sucesso, exceto a do secundário)
            foreach (var flag in andarDef.FlagsGeradasPossiveis
                .Where(f => andarDef.ObjetivoSecundario is null || f != andarDef.ObjetivoSecundario.FlagNome))
            {
                await _flagService.GerarFlagAsync(userId, andar, flag);
                flagsGeradas.Add(flag);
            }
        }

        // Avaliar compostas
        var compostas = await _flagService.ObterFlagsCompostasAtivasAsync(userId);
        var novasCompostas = compostas
            .Where(c => !flagsGeradas.Contains(c))
            .ToList();
        foreach (var comp in novasCompostas)
            await _flagService.GerarFlagAsync(userId, andar, comp);

        return new FlagsColetaResult(flagsGeradas, flagsExpiradas, novasCompostas);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Abandons the active exploration without granting any loot.
    /// </summary>
    public async Task CancelarAsync(Guid usuarioId)
    {
        var exploracao = await _exploracaoRepo.ObterAtivaAsync(usuarioId);
        if (exploracao == null) return;

        exploracao.Status      = StatusExploracao.Coletada; // abandon without loot
        exploracao.ConcluidoEm = DateTime.UtcNow;
        await _exploracaoRepo.AtualizarAsync(exploracao);
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public Task<TorreExploracao?> ObterAtivaAsync(Guid usuarioId)
        => _exploracaoRepo.ObterAtivaAsync(usuarioId);

    public Task<TorreExploracao?> ObterPendenteAsync(Guid usuarioId)
        => _exploracaoRepo.ObterPendenteAsync(usuarioId);

    public Task<List<(TipoBooster Tipo, int Quantidade)>> ObterBoostersAsync(Guid usuarioId)
        => _boosterRepo.ListarAsync(usuarioId);

    // ── Booster grant ─────────────────────────────────────────────────────────

    /// <summary>
    /// Grants boosters to a user (e.g. as milestone reward).
    /// </summary>
    public Task AplicarBoosterGratuitoAsync(Guid usuarioId, TipoBooster tipo, int qtd)
        => _boosterRepo.AdicionarAsync(usuarioId, tipo, qtd);

    // ── Static summary ────────────────────────────────────────────────────────

    public record ExploracaoResumo(
        double Progresso,
        int UltimoCheckpoint,
        int ProximoCheckpoint,
        double WinChance,
        double TeamPS,
        double CDI,
        double Ratio,
        int LootOuro,
        int LootFragmentos,
        StatusExploracao Status,
        bool HeroisFeridos
    );

    public static ExploracaoResumo ObterResumo(
        TorreExploracao exp,
        double teamPS,
        double cdi)
    {
        double ratio       = HeroPowerScoreService.CalcularRatio(teamPS, cdi);
        double winChance   = HeroPowerScoreService.CalcularWinChance(ratio);
        int proximoCheckpoint = exp.UltimoCheckpoint + exp.CheckpointInterval;

        return new ExploracaoResumo(
            Progresso:         exp.Progresso,
            UltimoCheckpoint:  exp.UltimoCheckpoint,
            ProximoCheckpoint: proximoCheckpoint,
            WinChance:         winChance,
            TeamPS:            teamPS,
            CDI:               cdi,
            Ratio:             ratio,
            LootOuro:          exp.LootOuro,
            LootFragmentos:    exp.LootFragmentosQtd,
            Status:            exp.Status,
            HeroisFeridos:     !string.IsNullOrEmpty(exp.HeroisFeridosIds)
        );
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static List<Guid> ParseHeroisIds(string s)
        => s.Split(',', StringSplitOptions.RemoveEmptyEntries)
           .Select(id => Guid.Parse(id.Trim()))
           .ToList();

    private static LegendsAwaken.Domain.Entities.TipoAndar DefinirTipoAndar(int numeroAndar)
    {
        if (numeroAndar % 25 == 0) return LegendsAwaken.Domain.Entities.TipoAndar.BossDificil;
        if (numeroAndar % 10 == 0) return LegendsAwaken.Domain.Entities.TipoAndar.BossMedio;
        if (numeroAndar %  5 == 0) return LegendsAwaken.Domain.Entities.TipoAndar.BossFacil;
        return LegendsAwaken.Domain.Entities.TipoAndar.Normal;
    }

    private static int CalcularDificuldade(int numero, LegendsAwaken.Domain.Entities.TipoAndar tipo)
    {
        int base_ = 5 + numero * 3;
        double mult = tipo switch
        {
            LegendsAwaken.Domain.Entities.TipoAndar.BossFacil   => 1.5,
            LegendsAwaken.Domain.Entities.TipoAndar.BossMedio   => 2.0,
            LegendsAwaken.Domain.Entities.TipoAndar.BossDificil => 3.0,
            _                                                    => 1.0,
        };
        return (int)(base_ * mult);
    }
}
