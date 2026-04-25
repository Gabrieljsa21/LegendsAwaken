using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class PartyService
    {
        private readonly IPartyRepository _partyRepository;
        private readonly IHeroiRepository _heroiRepository;

        public PartyService(IPartyRepository repo, IHeroiRepository hrRepo)
        {
            _partyRepository = repo;
            _heroiRepository = hrRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public Task<List<Party>> ObterPartiesUsuarioAsync(ulong userId)
            => _partyRepository.ObterPartiesPorUsuarioAsync(userId);

        public Task<Party?> ObterPorIdAsync(Guid partyId)
            => _partyRepository.ObterPorIdAsync(partyId);

        // ── Create ────────────────────────────────────────────────────────────

        public async Task<Party> CriarPartyAsync(ulong userId, string nome)
        {
            var existentes = await _partyRepository.ObterPartiesPorUsuarioAsync(userId);
            if (existentes.Any(p => p.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Você já tem um grupo com esse nome.");

            var nova = new Party
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                Nome = nome,
                NomeModoManual = true,
            };
            return await _partyRepository.CriarAsync(nova);
        }

        /// <summary>Creates a group with pre-loaded heroes and an auto-generated name.</summary>
        public async Task<Party> CriarComHeroisAsync(ulong userId, List<Heroi> herois)
        {
            if (herois.Count > 5)
                throw new InvalidOperationException("Máximo de 5 heróis por grupo.");

            var nova = new Party
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                Nome = GerarNomeAuto(herois),
                NomeModoManual = false,
            };
            await _partyRepository.CriarAsync(nova);

            foreach (var h in herois)
                await _partyRepository.AdicionarHeroiAsync(nova.Id, h.Id);

            return await _partyRepository.ObterPorIdAsync(nova.Id) ?? nova;
        }

        /// <summary>Creates the recommended group: top-5 by PS, guaranteeing 1 Tank and 1 Healer when available.</summary>
        public async Task<Party> CriarRecomendadaAsync(ulong userId, List<Heroi> todosHerois)
        {
            if (!todosHerois.Any())
                throw new InvalidOperationException("Sem heróis disponíveis para criar grupo.");

            var sorted    = todosHerois.OrderByDescending(HeroPowerScoreService.Calcular).ToList();
            var selected  = new List<Heroi>();

            var melhorTank   = sorted.FirstOrDefault(h => h.Funcao == FuncaoTatica.Frente);
            var melhorHealer = sorted.FirstOrDefault(h => h.Funcao == FuncaoTatica.Curandeiro);

            if (melhorTank != null)
                selected.Add(melhorTank);
            if (melhorHealer != null && melhorHealer != melhorTank)
                selected.Add(melhorHealer);

            foreach (var h in sorted)
            {
                if (selected.Count >= 5) break;
                if (!selected.Contains(h)) selected.Add(h);
            }

            return await CriarComHeroisAsync(userId, selected);
        }

        // ── Membership ────────────────────────────────────────────────────────

        public async Task AdicionarHeroiAsync(Guid partyId, Guid heroiId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new InvalidOperationException("Grupo não encontrado.");
            if (party.Membros.Count >= 5)
                throw new InvalidOperationException("Grupo já está cheio (5 heróis).");
            if (party.Membros.Any(m => m.HeroiId == heroiId))
                throw new InvalidOperationException("Herói já está no grupo.");

            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId)
                ?? throw new InvalidOperationException("Herói não encontrado.");

            await _partyRepository.AdicionarHeroiAsync(partyId, heroiId);
        }

        /// <summary>Adds a hero and refreshes the auto-name (if not in manual mode).</summary>
        public async Task AdicionarHeroiComNomeAutoAsync(Guid partyId, Guid heroiId)
        {
            await AdicionarHeroiAsync(partyId, heroiId);
            await AtualizarNomeAutoSeNecessarioAsync(partyId);
        }

        public async Task RemoverHeroiAsync(Guid partyId, Guid heroiId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new InvalidOperationException("Grupo não encontrado.");
            await _partyRepository.RemoverHeroiAsync(party.Id, heroiId);
        }

        /// <summary>Removes a hero and refreshes the auto-name (if not in manual mode).</summary>
        public async Task RemoverHeroiComNomeAutoAsync(Guid partyId, Guid heroiId)
        {
            await RemoverHeroiAsync(partyId, heroiId);
            await AtualizarNomeAutoSeNecessarioAsync(partyId);
        }

        // ── Name management ───────────────────────────────────────────────────

        public async Task AtualizarNomeManualAsync(Guid partyId, string novoNome)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new InvalidOperationException("Grupo não encontrado.");
            party.Nome = novoNome.Trim();
            party.NomeModoManual = true;
            await _partyRepository.AtualizarAsync(party);
        }

        public async Task ToggleModoNomeAsync(Guid partyId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new InvalidOperationException("Grupo não encontrado.");
            party.NomeModoManual = !party.NomeModoManual;
            if (!party.NomeModoManual)
                party.Nome = GerarNomeAuto(party.Membros.Select(m => m.Heroi).ToList());
            await _partyRepository.AtualizarAsync(party);
        }

        // ── Delete ────────────────────────────────────────────────────────────

        public async Task DeletarAsync(Guid partyId, ulong userId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new InvalidOperationException("Grupo não encontrado.");
            if (party.UsuarioId != userId)
                throw new InvalidOperationException("Permissão negada.");
            await _partyRepository.DeletarAsync(partyId);
        }

        // ── Static helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Auto-name: show all names if ≤ 3 heroes; otherwise first 2 + "+N".
        /// </summary>
        public static string GerarNomeAuto(IList<Heroi> herois)
        {
            if (!herois.Any()) return "Grupo Vazio";
            if (herois.Count <= 3)
                return string.Join(" / ", herois.Select(h => h.Nome));
            var dois = herois.Take(2).Select(h => h.Nome);
            return string.Join(" / ", dois) + $" +{herois.Count - 2}";
        }

        // ── Private ───────────────────────────────────────────────────────────

        private async Task AtualizarNomeAutoSeNecessarioAsync(Guid partyId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId);
            if (party is null || party.NomeModoManual) return;
            party.Nome = GerarNomeAuto(party.Membros.Select(m => m.Heroi).ToList());
            await _partyRepository.AtualizarAsync(party);
        }
    }
}
