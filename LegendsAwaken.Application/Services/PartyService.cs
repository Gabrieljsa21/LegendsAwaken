using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<Party> CriarPartyAsync(ulong userId, string nome)
        {
            var existentes = await _partyRepository.ObterPartiesPorUsuarioAsync(userId);

            if (existentes.Any(p => p.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Você já tem uma party com esse nome.");

            var nova = new Party
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                Nome = nome
            };

            return await _partyRepository.CriarAsync(nova);
        }

        public Task<List<Party>> ObterPartiesUsuarioAsync(ulong userId)
            => _partyRepository.ObterPartiesPorUsuarioAsync(userId);

        public async Task AdicionarHeroiAsync(Guid partyId, Guid heroiId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new Exception("Party não encontrada.");

            if (party.Membros.Count >= 5)
                throw new Exception("Party já está cheia (5 heróis).");

            var heroi = await _heroiRepository.ObterPorIdAsync(heroiId)
                ?? throw new Exception("Herói não encontrado.");

            if (party.Membros.Any(m => m.HeroiId == heroiId))
                throw new Exception("Herói já está na party.");

            await _partyRepository.AdicionarHeroiAsync(party.Id, heroiId);
        }

        public async Task RemoverHeroiAsync(Guid partyId, Guid heroiId)
        {
            var party = await _partyRepository.ObterPorIdAsync(partyId)
                ?? throw new Exception("Party não encontrada.");

            await _partyRepository.RemoverHeroiAsync(party.Id, heroiId);
        }

    }

}
