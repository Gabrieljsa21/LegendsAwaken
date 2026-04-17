using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class HeroiConfigRepository(LegendsAwakenDbContext db) : IHeroiConfigRepository
{
    public async Task<HeroiConfig?> ObterPorIdAsync(Guid id) =>
        await db.HeroiConfigs.FindAsync(id);

    public async Task<HeroiConfig?> ObterPorNomeAsync(string nome) =>
        await db.HeroiConfigs.FirstOrDefaultAsync(h => h.Nome == nome);

    public async Task<List<HeroiConfig>> ListarTodosAsync() =>
        await db.HeroiConfigs.OrderBy(h => h.Nome).ToListAsync();

    public async Task<HeroiUnlockConfig?> ObterUnlockConfigAsync(Guid heroiId) =>
        await db.HeroiUnlockConfigs
            .Include(u => u.Heroi)
            .FirstOrDefaultAsync(u => u.HeroiId == heroiId);
}
