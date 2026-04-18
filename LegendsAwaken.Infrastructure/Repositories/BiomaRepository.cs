using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class BiomaRepository(LegendsAwakenDbContext db) : IBiomaRepository
{
    public async Task<Bioma?> ObterPorAndarAsync(int andar) =>
        await db.Biomas
            .FirstOrDefaultAsync(b => b.AndarInicio <= andar && b.AndarFim >= andar);

    public async Task<List<BiomHeroPool>> ObterPoolAsync(Guid biomaId) =>
        await db.BiomHeroPools
            .Include(p => p.Heroi)
            .Where(p => p.BiomeId == biomaId)
            .ToListAsync();

    public async Task<List<Bioma>> ListarTodosAsync() =>
        await db.Biomas
            .Include(b => b.Pool)
            .ThenInclude(p => p.Heroi)
            .OrderBy(b => b.AndarInicio)
            .ToListAsync();
}
