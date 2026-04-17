using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly LegendsAwakenDbContext _context;
        public ItemRepository(LegendsAwakenDbContext context) => _context = context;

        public async Task<Item?> ObterPorIdAsync(Guid id)
            => await _context.Itens.Include(i => i.Bonus).FirstOrDefaultAsync(i => i.Id == id);

        public async Task<List<Item>> ObterPorProprietarioAsync(ulong usuarioId)
            => await _context.Itens.Include(i => i.Bonus)
                .Where(i => i.ProprietarioId == usuarioId)
                .ToListAsync();

        public async Task<List<Item>> ObterEquipadosPorHeroiAsync(Guid heroiId)
            => await _context.Itens.Include(i => i.Bonus)
                .Where(i => i.HeroiEquipadoId == heroiId)
                .ToListAsync();

        public async Task AdicionarAsync(Item item)
        {
            await _context.Itens.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Item item)
        {
            _context.Itens.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}
