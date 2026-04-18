using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TipoEventoAlto = LegendsAwaken.Domain.Enum.TipoEventoAlto;

namespace LegendsAwaken.Application.Services
{
    /// <summary>
    /// Resultado de SubirAndarAsync. Contém o XP concedido, level-ups por herói,
    /// fragmentos dropados, novo bioma detectado, herói desbloqueado e payloads de UI.
    /// </summary>
    public record SubirAndarResult(
        bool Sucesso,
        int XpConcedido,
        int OuroGanho,
        IReadOnlyDictionary<string, int> NiveisGanhosPorHeroi,
        IReadOnlyList<FragmentDropResult> Fragmentos,
        Bioma? NovoBioma,
        HeroiConfig? HeroiDesbloqueado,
        IReadOnlyList<RewardPayload> RewardPayloads
    );

    public class TorreService
    {
        private readonly ITorreRepository _torreRepository;
        private readonly IHeroiRepository _heroiRepository;
        private readonly HeroiLevelUpService _levelUpService;
        private readonly FragmentService _fragmentService;
        private readonly BiomeService _biomeService;
        private readonly RecruitmentService _recruitmentService;
        private readonly RewardDistributionService _rewardService;

        public TorreService(
            ITorreRepository torreRepository,
            IHeroiRepository heroiRepository,
            HeroiLevelUpService levelUpService,
            FragmentService fragmentService,
            BiomeService biomeService,
            RecruitmentService recruitmentService,
            RewardDistributionService rewardService)
        {
            _torreRepository = torreRepository;
            _heroiRepository = heroiRepository;
            _levelUpService = levelUpService;
            _fragmentService = fragmentService;
            _biomeService = biomeService;
            _recruitmentService = recruitmentService;
            _rewardService = rewardService;
        }

        public async Task<TorreAndar?> ObterAndarAtualAsync(Guid usuarioId)
        {
            return await _torreRepository.ObterAndarPorUsuarioAsync(usuarioId);
        }

        /// <summary>
        /// Avança o usuário para o próximo andar da Torre, concedendo XP a cada herói participante.
        /// Fase 3A: fórmula linear (10 + Numero*5), com multiplicadores por tipo de boss.
        /// </summary>
        public async Task<SubirAndarResult> SubirAndarAsync(Guid usuarioId, List<Heroi> heroisParticipantes)
        {
            var andarAtual = await ObterAndarAtualAsync(usuarioId);
            if (andarAtual == null)
                throw new InvalidOperationException("Andar atual não encontrado.");

            if (!andarAtual.ObjetivoCumprido)
                return new SubirAndarResult(
                    Sucesso: false,
                    XpConcedido: 0,
                    OuroGanho: 0,
                    NiveisGanhosPorHeroi: new Dictionary<string, int>(),
                    Fragmentos: [],
                    NovoBioma: null,
                    HeroiDesbloqueado: null,
                    RewardPayloads: []);

            int xpConcedido = CalcularXpDoAndar(andarAtual);
            int ouroConcedido = CalcularOuroDoAndar(andarAtual);

            var niveisGanhos = new Dictionary<string, int>();
            foreach (var heroi in heroisParticipantes)
            {
                int ganhos = _levelUpService.AplicarXp(heroi, xpConcedido);
                if (ganhos > 0)
                    niveisGanhos[heroi.Nome] = ganhos;
                await _heroiRepository.AtualizarAsync(heroi);
            }

            var proximoAndar = new TorreAndar
            {
                Id = Guid.NewGuid(),
                Numero = andarAtual.Numero + 1,
                Tipo = DefinirTipoAndar(andarAtual.Numero + 1),
                UsuarioId = usuarioId,
                CriadoEm = DateTime.UtcNow,
                ObjetivoCumprido = false,
            };

            await _torreRepository.AdicionarAsync(proximoAndar);

            // Extensão 1: drop de fragmentos (acontece no andar que foi vencido)
            var drops = await _fragmentService.ProcessarDropAsync(usuarioId, andarAtual.Numero);
            var rewardPayloads = new List<RewardPayload>();

            foreach (var drop in drops)
                rewardPayloads.Add(_rewardService.GerarMicroPico(drop));

            // Extensão 2: detecção de bioma novo (para o próximo andar)
            Bioma? novoBioma = null;
            if (await _biomeService.EBiomaNovoAsync(proximoAndar.Numero))
            {
                novoBioma = await _biomeService.ObterBiomaPorAndarAsync(proximoAndar.Numero);
                if (novoBioma is not null)
                    rewardPayloads.Add(_rewardService.GerarPicoAlto(TipoEventoAlto.DescobertaBioma, novoBioma));
            }

            // Marco da Torre: verificar unlock de herói icônico
            HeroiConfig? heroiDesbloqueado = null;
            if (_biomeService.EAndarDeMarco(proximoAndar.Numero))
            {
                var recrutamento = await _recruitmentService.ProcessarMarcoTorreAsync(usuarioId, proximoAndar.Numero);
                if (recrutamento?.Sucesso == true && recrutamento.Heroi is not null)
                {
                    heroiDesbloqueado = recrutamento.Heroi;
                    rewardPayloads.Add(_rewardService.GerarPicoAlto(TipoEventoAlto.HeroiIconicoDesbloqueado, heroi: recrutamento.Heroi));
                }
            }

            return new SubirAndarResult(
                Sucesso: true,
                XpConcedido: xpConcedido,
                OuroGanho: ouroConcedido,
                NiveisGanhosPorHeroi: niveisGanhos,
                Fragmentos: drops,
                NovoBioma: novoBioma,
                HeroiDesbloqueado: heroiDesbloqueado,
                RewardPayloads: rewardPayloads);
        }

        /// <summary>
        /// Registra o objetivo do andar como cumprido, habilitando SubirAndarAsync.
        /// </summary>
        public async Task MarcarObjetivoCumpridoAsync(Guid andarId)
        {
            var andar = await _torreRepository.ObterPorIdAsync(andarId);
            if (andar == null)
                throw new InvalidOperationException("Andar não encontrado.");

            andar.ObjetivoCumprido = true;
            andar.DataAlteracao = DateTime.UtcNow;

            await _torreRepository.AtualizarAsync(andar);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// XP base por andar: 10 + Numero*5. Boss floors têm multiplicadores.
        /// Andar 1 = 15 XP | Andar 10 = 60 XP | Andar 25 = 135 XP (×3 = 405 XP)
        /// </summary>
        private static int CalcularXpDoAndar(TorreAndar andar)
        {
            int xpBase = 10 + andar.Numero * 5;
            double mult = andar.Tipo switch
            {
                TipoAndar.BossFacil   => 1.5,
                TipoAndar.BossMedio   => 2.0,
                TipoAndar.BossDificil => 3.0,
                _                     => 1.0
            };
            return (int)(xpBase * mult);
        }

        private static int CalcularOuroDoAndar(TorreAndar andar)
        {
            int ouroBase = 5 + andar.Numero * 3;
            double mult = andar.Tipo switch
            {
                TipoAndar.BossFacil   => 1.5,
                TipoAndar.BossMedio   => 2.0,
                TipoAndar.BossDificil => 3.0,
                _                     => 1.0
            };
            return (int)(ouroBase * mult);
        }

        private static TipoAndar DefinirTipoAndar(int numeroAndar)
        {
            if (numeroAndar % 25 == 0) return TipoAndar.BossDificil;
            if (numeroAndar % 10 == 0) return TipoAndar.BossMedio;
            if (numeroAndar %  5 == 0) return TipoAndar.BossFacil;
            return TipoAndar.Normal;
        }
    }
}
