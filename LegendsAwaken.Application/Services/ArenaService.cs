using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public record TreinoResult(int XpGanho, int NiveisGanhos, string? Erro);

    public record DesafioResult(int OndasSobrevividas, int XpTotal, int OuroTotal);

    public class ArenaService
    {
        private readonly IHeroiRepository _heroiRepository;
        private readonly ICidadeRepository _cidadeRepository;
        private readonly HeroiLevelUpService _levelUpService;

        private const int CustoOuro = 100;
        private const int CustoComida = 10;
        private static readonly TimeSpan CooldownTreino = TimeSpan.FromHours(4);
        private static readonly TimeSpan CooldownDesafio = TimeSpan.FromHours(24);

        // Tracks last challenge time per user (in-memory; resets on bot restart — acceptable for 3A.2)
        private static readonly Dictionary<ulong, DateTime> _ultimoDesafio = new();

        public ArenaService(
            IHeroiRepository heroiRepository,
            ICidadeRepository cidadeRepository,
            HeroiLevelUpService levelUpService)
        {
            _heroiRepository = heroiRepository;
            _cidadeRepository = cidadeRepository;
            _levelUpService = levelUpService;
        }

        public async Task<TreinoResult> TreinarAsync(ulong usuarioId, Guid heroiId)
        {
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
            if (cidade == null) return new TreinoResult(0, 0, "Você não tem uma cidade.");

            // Check resources
            if (cidade.Recursos.Ouro < CustoOuro)
                return new TreinoResult(0, 0, $"Ouro insuficiente. Custo: {CustoOuro}.");
            if (cidade.Recursos.Comida < CustoComida)
                return new TreinoResult(0, 0, $"Comida insuficiente. Custo: {CustoComida}.");

            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId);
            if (heroi == null || heroi.UsuarioId != usuarioId)
                return new TreinoResult(0, 0, "Herói não encontrado.");

            // Cooldown check
            if (heroi.Treinamento?.UltimoTreino != null)
            {
                var desde = DateTime.UtcNow - heroi.Treinamento.UltimoTreino.Value;
                if (desde < CooldownTreino)
                {
                    var restante = CooldownTreino - desde;
                    return new TreinoResult(0, 0, $"Cooldown: ainda faltam {restante.Hours}h {restante.Minutes}m.");
                }
            }

            // Cost
            cidade.Recursos.Adicionar(-CustoOuro,   "ouro");
            cidade.Recursos.Adicionar(-CustoComida, "comida");
            await _cidadeRepository.AtualizarAsync(cidade);

            // XP = 3× the XP that next tower floor would give at hero's level
            int raridade = (int)heroi.Raridade;
            int xpBase = _levelUpService.XpParaProximoNivel(heroi.Nivel, raridade);
            int xpGanho = xpBase * 3;

            int niveisGanhos = _levelUpService.AplicarXp(heroi, xpGanho);

            if (heroi.Treinamento == null)
                heroi.Treinamento = new Treinamento
                {
                    Tipo = "Arena",
                    Inicio = DateTime.UtcNow,
                    Fim = DateTime.UtcNow,
                    ResultadoEsperado = "XP"
                };
            heroi.Treinamento.UltimoTreino = DateTime.UtcNow;

            await _heroiRepository.AtualizarAsync(heroi);

            return new TreinoResult(xpGanho, niveisGanhos, null);
        }

        public async Task<(DesafioResult? result, string? erro)> DesafioOndasAsync(ulong usuarioId, List<Heroi> party)
        {
            // Daily cooldown
            if (_ultimoDesafio.TryGetValue(usuarioId, out var ultimo))
            {
                var desde = DateTime.UtcNow - ultimo;
                if (desde < CooldownDesafio)
                {
                    var restante = CooldownDesafio - desde;
                    return (null, $"Cooldown: disponível em {restante.Hours}h {restante.Minutes}m.");
                }
            }

            if (!party.Any()) return (null, "Party vazia.");

            int ondas = 0;
            int xpTotal = 0;
            int ouroTotal = 0;
            double partyHpTotal = party.Sum(h => (double)h.Status.VidaMaxima);
            double hpAtual = partyHpTotal;

            while (hpAtual > 0)
            {
                ondas++;
                double escala = Math.Pow(1.15, ondas - 1);
                // Enemy damage this wave = base 30% of party HP × scaling
                double danoRecebido = partyHpTotal * 0.30 * escala;
                // Wave succeeds if random chance based on HP ratio
                double hpRatio = hpAtual / partyHpTotal;
                double chanceVencer = hpRatio * 0.85 + 0.10; // 10-95% range
                if (Random.Shared.NextDouble() > chanceVencer) break;

                hpAtual = Math.Max(0, hpAtual - danoRecebido * 0.3); // heroes take some damage too
                xpTotal += 50 * ondas;
                ouroTotal += 30 * ondas;
            }

            _ultimoDesafio[usuarioId] = DateTime.UtcNow;

            // Grant XP to all party heroes
            foreach (var heroi in party)
            {
                int xpPorHeroi = xpTotal / party.Count;
                _levelUpService.AplicarXp(heroi, xpPorHeroi);
                await _heroiRepository.AtualizarAsync(heroi);
            }

            // Grant Ouro to city
            var cidade = await _cidadeRepository.ObterPorProprietarioIdAsync(usuarioId);
            if (cidade != null)
            {
                cidade.Recursos.Adicionar(ouroTotal, "ouro");
                await _cidadeRepository.AtualizarAsync(cidade);
            }

            return (new DesafioResult(ondas, xpTotal, ouroTotal), null);
        }
    }
}
