using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    /// <summary>
    /// Resultado de SubirAndarAsync. Contém o XP concedido e os level-ups por herói.
    /// </summary>
    public record SubirAndarResult(
        bool Sucesso,
        int XpConcedido,
        int OuroGanho,
        IReadOnlyDictionary<string, int> NiveisGanhosPorHeroi
    );

    public class TorreService
    {
        private readonly ITorreRepository _torreRepository;
        private readonly IHeroiRepository _heroiRepository;
        private readonly HeroiLevelUpService _levelUpService;

        public TorreService(
            ITorreRepository torreRepository,
            IHeroiRepository heroiRepository,
            HeroiLevelUpService levelUpService)
        {
            _torreRepository = torreRepository;
            _heroiRepository = heroiRepository;
            _levelUpService = levelUpService;
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
                    NiveisGanhosPorHeroi: new Dictionary<string, int>());

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

            return new SubirAndarResult(
                Sucesso: true,
                XpConcedido: xpConcedido,
                OuroGanho: ouroConcedido,
                NiveisGanhosPorHeroi: niveisGanhos);
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
