using LegendsAwaken.Domain.Entities;
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
            return await _dbContext.Herois
                .Include(h => h.BonusAtributos)
                .FirstOrDefaultAsync(h => h.Id == heroiId);
        }

        public async Task<List<Heroi>> ObterPorUsuarioIdAsync(ulong usuarioId)
        {
            return await _dbContext.Herois
                .AsNoTracking()
                .AsSplitQuery()
                .Include(h => h.Habilidades)
                    .ThenInclude(hh => hh.Habilidade)
                        .ThenInclude(h => h.HabilidadeBonusAtributos)
                .Include(h => h.BonusAtributos)
                .Where(h => h.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<Heroi>> ObterTodosAsync()
        {
            return await _dbContext.Herois
                .AsNoTracking()
                .AsSplitQuery()
                .Include(h => h.Habilidades)
                    .ThenInclude(hh => hh.Habilidade)
                        .ThenInclude(h => h.HabilidadeBonusAtributos)
                .Include(h => h.BonusAtributos)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Heroi heroi)
        {
            try
            {
                await _dbContext.Herois.AddAsync(heroi);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao adicionar heroi no banco de dados:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task AtualizarAsync(Heroi heroi)
        {
            try
            {
                // Clear stale tracked entities before attaching to avoid identity conflicts
                // when the same Habilidade (shared by multiple heroes) is already tracked
                _dbContext.ChangeTracker.Clear();
                _dbContext.Herois.Attach(heroi);
                _dbContext.Entry(heroi).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao atualizar heroi no banco de dados:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
