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
            var entity = await _dbContext.Herois.FindAsync(heroiId);
            return entity;
        }

        public async Task<List<Heroi>> ObterPorUsuarioIdAsync(ulong usuarioId)
        {
            return await _dbContext.Herois
                .AsNoTracking()
                .Include(h => h.Habilidades)
                    .ThenInclude(hh => hh.Habilidade)
                        .ThenInclude(h => h.HabilidadeBonusAtributos) // incluir bônus de atributos
                .Where(h => h.UsuarioId == usuarioId)
                .ToListAsync();

        }

        public async Task<List<Heroi>> ObterTodosAsync()
        {
            return await _dbContext.Herois
                .AsNoTracking()
                .Include(h => h.Habilidades)
                    .ThenInclude(hh => hh.Habilidade)
                        .ThenInclude(h => h.HabilidadeBonusAtributos) // incluir bônus de atributos
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
                Console.WriteLine("Erro ao adicionar herói no banco de dados:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task AtualizarAsync(Heroi heroi)
        {
            try
            {
                _dbContext.Herois.Update(heroi);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao atualizar herói no banco de dados:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


    }
}
