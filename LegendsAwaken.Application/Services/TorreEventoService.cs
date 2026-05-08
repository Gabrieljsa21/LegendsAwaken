using LegendsAwaken.Application.Config;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class TorreEventoService(
    ITorreEventoRepository eventoRepo,
    ITorreExploracaoRepository exploracaoRepo)
{
    private static readonly TimeSpan DefaultExpiracao = TimeSpan.FromDays(7);

    public async Task<TorreEvento> GerarEventoAsync(TorreExploracao exp, int threshold)
    {
        var flagDoThreshold = ThresholdParaFlag(threshold);
        if ((exp.CheckpointsProcessados & flagDoThreshold) != 0)
            throw new InvalidOperationException(
                $"Checkpoint {threshold}% já foi processado para exploração {exp.Id}.");

        var seed = HashCode.Combine(exp.Seed, threshold, exp.AndarNumero);
        var rng = new EventoRng(seed);

        var tags = DeserializarTags(exp.ConsequenceTags);
        var candidatos = CheckpointEventoCatalog.FiltrarParaAndar(exp.AndarNumero, tags).ToList();

        if (!candidatos.Any())
            candidatos = CheckpointEventoCatalog.Todos
                .Where(e => e.MinAndar <= exp.AndarNumero)
                .ToList();

        var config = rng.EscolherPonderado(candidatos, c => c.Peso);

        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            ExploracaoId = exp.Id,
            Status = EventoStatus.Ativo,
            Tipo = config.Tipo,
            Tier = config.Tier,
            Raridade = config.Raridade,
            EventoKey = config.Key,
            ProgressoNoCheckpoint = threshold,
            AndarOrigem = exp.AndarNumero,
            EventoSeed = seed,
            SnapshotCombatStateJson = SerializarSnapshot(exp),
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = config.Tier == TierEvento.Maior ? DateTime.UtcNow.Add(DefaultExpiracao) : null,
            Exploracao = exp
        };

        await eventoRepo.AdicionarAsync(evento);
        return evento;
    }

    public async Task ResolverAsync(Guid eventoId, string opcaoKey, TorreExploracao exp)
    {
        // Early validation: reject any key that doesn't appear in any catalog entry.
        // Must happen before the repo call so invalid keys throw ArgumentException immediately.
        var config = CheckpointEventoCatalog.Todos
            .FirstOrDefault(c => c.Opcoes != null && c.Opcoes.Any(o => o.Key == opcaoKey));
        if (config is null)
            throw new ArgumentException(
                $"Opção '{opcaoKey}' não existe em nenhum evento do catálogo.", nameof(opcaoKey));

        var (grau, progressoBonus, descricaoEfeito) = AplicarEfeito(config, opcaoKey, exp);

        if (progressoBonus > 0)
        {
            int proximoThreshold = ProximoThresholdNaoProcessado(exp.CheckpointsProcessados, exp.Progresso);
            if (proximoThreshold > 0)
                progressoBonus = Math.Min(progressoBonus, proximoThreshold - (int)exp.Progresso - 1);
            exp.Progresso = Math.Min(100, exp.Progresso + progressoBonus);
        }

        if (config.ConsequenceTags?.Length > 0)
        {
            var tags = DeserializarTags(exp.ConsequenceTags).ToList();
            tags.AddRange(config.ConsequenceTags);
            exp.ConsequenceTags = JsonSerializer.Serialize(tags);
        }

        exp.Status = StatusExploracao.Ativa;
        exp.Version++;

        // Fetch and update the persisted evento if one is active for this exploration.
        var evento = await eventoRepo.ObterAtivoAsync(exp.Id);
        if (evento is not null)
        {
            evento.OpcaoKey = opcaoKey;
            evento.ResolvidoEm = DateTime.UtcNow;
            evento.Status = EventoStatus.Resolvido;
            evento.ResultadoJson = JsonSerializer.Serialize(new
            {
                titulo = config.Titulo,
                descricao = descricaoEfeito,
                grauSucesso = grau.ToString(),
                progressoBonus,
                publico = true,
                schemaVersion = 1
            });
            await eventoRepo.AtualizarAsync(evento);
        }

        await exploracaoRepo.AtualizarAsync(exp);
    }

    public async Task ResolverMenorInlineAsync(CheckpointEventoConfig config, TorreExploracao exp)
    {
        var (_, progressoBonus, descricao) = AplicarEfeito(config, opcaoKey: null, exp);

        if (progressoBonus > 0)
        {
            int proximo = ProximoThresholdNaoProcessado(exp.CheckpointsProcessados, exp.Progresso);
            if (proximo > 0)
                progressoBonus = Math.Min(progressoBonus, proximo - (int)exp.Progresso - 1);
            exp.Progresso = Math.Min(100, exp.Progresso + progressoBonus);
        }

        await eventoRepo.AdicionarLogAsync(new TorreEventoLog
        {
            ExploracaoId = exp.Id,
            Texto = $"[{config.Titulo}] {descricao}",
        });
    }

    public async Task RecuperarExpiradosAsync()
    {
        var expirados = await eventoRepo.ObterExpiradosAsync(DateTime.UtcNow);
        foreach (var evento in expirados)
        {
            var exp = evento.Exploracao;
            evento.Status = EventoStatus.Expirado;
            evento.OpcaoKey = "expirado";
            evento.ResolvidoEm = DateTime.UtcNow;
            evento.ResultadoJson = JsonSerializer.Serialize(new
            {
                titulo = "Evento expirado",
                descricao = "A party continuou sem tomar uma decisão. Sem bônus ou penalidade.",
                grauSucesso = GrauSucesso.Falha.ToString(),
                progressoBonus = 0,
                publico = true,
                schemaVersion = 1
            });
            exp.Status = StatusExploracao.Ativa;
            exp.Version++;
            await eventoRepo.AtualizarAsync(evento);
            await exploracaoRepo.AtualizarAsync(exp);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    public static CheckpointFlags ThresholdParaFlag(int threshold) => threshold switch
    {
        25  => CheckpointFlags.P25,
        50  => CheckpointFlags.P50,
        75  => CheckpointFlags.P75,
        100 => CheckpointFlags.P100,
        _   => throw new ArgumentOutOfRangeException(nameof(threshold))
    };

    private static int ProximoThresholdNaoProcessado(CheckpointFlags flags, double progressoAtual)
    {
        int[] thresholds = { 25, 50, 75, 100 };
        foreach (var t in thresholds)
        {
            if ((flags & ThresholdParaFlag(t)) == 0 && t > progressoAtual)
                return t;
        }
        return 0;
    }

    private static IEnumerable<string> DeserializarTags(string? json)
    {
        if (string.IsNullOrEmpty(json)) return Enumerable.Empty<string>();
        return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
    }

    private static string? SerializarSnapshot(TorreExploracao exp)
    {
        if (string.IsNullOrEmpty(exp.HeroisIds)) return null;
        return JsonSerializer.Serialize(new { heroisIds = exp.HeroisIds });
    }

    private static (GrauSucesso Grau, int ProgressoBonus, string Descricao) AplicarEfeito(
        CheckpointEventoConfig config, string? opcaoKey, TorreExploracao exp)
    {
        return config.Key switch
        {
            "encruzilhada_mercador" => opcaoKey switch
            {
                "pagar"   => (GrauSucesso.SucessoTotal,  10, "Você pagou o preço. O mercador cede passagem. +10% progresso."),
                "forccar" => (GrauSucesso.SucessoParcial, 5,  "Forçou passagem com dificuldade. +5% progresso."),
                "recuar"  => (GrauSucesso.Falha,          0,  "A party recua prudentemente. Sem bônus."),
                _         => (GrauSucesso.Falha, 0, "")
            },
            "trilha_oculta" => opcaoKey switch
            {
                "explorar" => (GrauSucesso.SucessoTotal, 15, "A trilha encurta o caminho. +15% progresso."),
                "ignorar"  => (GrauSucesso.Falha,        0,  "A party segue pela rota principal. Sem bônus."),
                _          => (GrauSucesso.Falha, 0, "")
            },
            "chuva_de_fragmentos" => (GrauSucesso.SucessoTotal, 0, "Fragmentos coletados da câmara abandonada."),
            "armadilha_detectada" => (GrauSucesso.SucessoTotal, 0, "Armadilha contornada com sucesso. Nenhum dano."),
            "teste_forca_porta" => opcaoKey switch
            {
                "arrombar" => (GrauSucesso.SucessoTotal, 10, "Porta arrombada! +10% progresso."),
                _          => (GrauSucesso.Falha, 0, "")
            },
            "sombra_perseguindo" => opcaoKey switch
            {
                "fugir"     => (GrauSucesso.SucessoTotal, 5,  "A party escapa rapidamente. +5% progresso."),
                "enfrentar" => (GrauSucesso.SucessoTotal, 8,  "A ameaça é neutralizada. +8% progresso."),
                _           => (GrauSucesso.Falha, 0, "")
            },
            _ => (GrauSucesso.Falha, 0, "Evento desconhecido.")
        };
    }
}
