using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class SlotOcupacaoRepository : ISlotOcupacaoRepository
    {
        private readonly LegendsAwakenDbContext _context;
        public SlotOcupacaoRepository(LegendsAwakenDbContext context) => _context = context;

        public async Task<List<SlotOcupacao>> ObterPorConstrucaoAsync(Guid construcaoId)
            => await _context.SlotOcupacoes.Where(s => s.ConstrucaoId == construcaoId).ToListAsync();

        public async Task<SlotOcupacao?> ObterPorHeroiAsync(Guid heroiId)
            => await _context.SlotOcupacoes.FirstOrDefaultAsync(s => s.HeroiId == heroiId);

        public async Task AdicionarAsync(SlotOcupacao slot)
        {
            _context.SlotOcupacoes.Add(slot);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(SlotOcupacao slot)
        {
            _context.SlotOcupacoes.Remove(slot);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SlotOcupacao>> ObterPorCidadeAsync(Guid cidadeId)
        {
            // Get all ConstrucaoIds for this city via the shadow FK "CidadeId", then get all slots
            var construcaoIds = await _context.Set<Construcao>()
                .Where(c => EF.Property<Guid>(c, "CidadeId") == cidadeId)
                .Select(c => c.Id)
                .ToListAsync();
            return await _context.SlotOcupacoes
                .Where(s => construcaoIds.Contains(s.ConstrucaoId))
                .ToListAsync();
        }
    }
}
