using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class PartyRepository : IPartyRepository
    {
        private readonly LegendsAwakenDbContext _db;
        public PartyRepository(LegendsAwakenDbContext db) => _db = db;

        public async Task<Party> CriarAsync(Party party)
        {
            await _db.Parties.AddAsync(party);
            await _db.SaveChangesAsync();
            return party;
        }

        public async Task<List<Party>> ObterPartiesPorUsuarioAsync(ulong userId)
        {
            return await _db.Parties
                .Include(p => p.Membros)
                    .ThenInclude(ph => ph.Heroi)
                .Where(p => p.UsuarioId == userId)
                .ToListAsync();
        }

        public async Task<Party?> ObterPorIdAsync(Guid partyId)
        {
            return await _db.Parties
                .Include(p => p.Membros)
                    .ThenInclude(ph => ph.Heroi)
                .FirstOrDefaultAsync(p => p.Id == partyId);
        }

        public async Task AdicionarHeroiAsync(Guid partyId, Guid heroiId)
        {
            _db.PartyHeroes.Add(new PartyHero { PartyId = partyId, HeroiId = heroiId });
            await _db.SaveChangesAsync();
        }

        public async Task RemoverHeroiAsync(Guid partyId, Guid heroiId)
        {
            var ph = await _db.PartyHeroes.FindAsync(partyId, heroiId);
            if (ph != null)
            {
                _db.PartyHeroes.Remove(ph);
                await _db.SaveChangesAsync();
            }
        }

        public Task SalvarAsync() => _db.SaveChangesAsync();
    }
}
