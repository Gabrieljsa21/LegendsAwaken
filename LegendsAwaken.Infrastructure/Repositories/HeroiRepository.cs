using LegendsAwaken.Domain;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class HeroiRepository : IHeroiRepository
    {
        private readonly LegendsAwakenDbContext _dbContext;

        public HeroiRepository(LegendsAwakenDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Heroi?> ObterPorIdAsync(Guid heroiId)
        {
            var entity = await _dbContext.Herois.FindAsync(heroiId);
            return entity;
        }

        public async Task<List<Heroi>> ObterPorUsuarioIdAsync(ulong usuarioId)
        {
            return await _dbContext.Herois
                .AsNoTracking()
                .Where(h => h.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<Heroi>> ObterTodosAsync()
        {
            return await _dbContext.Herois.AsNoTracking().ToListAsync();
        }

        public async Task AdicionarAsync(Heroi heroi)
        {
            await _dbContext.Herois.AddAsync(heroi);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Heroi heroi)
        {
            _dbContext.Herois.Update(heroi);
            await _dbContext.SaveChangesAsync();
        }

    }
}
