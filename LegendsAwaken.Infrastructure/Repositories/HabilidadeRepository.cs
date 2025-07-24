using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class HabilidadeRepository : IHabilidadeRepository
    {
        private readonly LegendsAwakenDbContext _context;

        public HabilidadeRepository(LegendsAwakenDbContext context)
        {
            _context = context;
        }

        public async Task<List<Habilidade>> ObterTodasAsync()
        {
            return await _context.Habilidades
                .Include(h => h.HabilidadeBonusAtributos)
                .ToListAsync();
        }

        public async Task<Habilidade?> ObterPorIdAsync(string id)
        {
            return await _context.Habilidades
                .Include(h => h.HabilidadeBonusAtributos)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task AdicionarAsync(Habilidade habilidade)
        {
            await _context.Habilidades.AddAsync(habilidade);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Habilidade habilidade)
        {
            _context.Habilidades.Update(habilidade);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(string id)
        {
            var habilidade = await ObterPorIdAsync(id);
            if (habilidade is not null)
            {
                _context.Habilidades.Remove(habilidade);
                await _context.SaveChangesAsync();
            }
        }
    }
}
