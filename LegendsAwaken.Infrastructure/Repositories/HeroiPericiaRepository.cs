using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class HeroiPericiaRepository(LegendsAwakenDbContext db) : IHeroiPericiaRepository
{
    public async Task<List<HeroiPericia>> ObterPorHeroiAsync(Guid heroiId)
        => await db.HeroisPericias.Where(p => p.HeroiId == heroiId).ToListAsync();

    public async Task<List<HeroiPericia>> ObterPorHeroisAsync(IEnumerable<Guid> heroiIds)
    {
        var ids = heroiIds.ToList();
        return await db.HeroisPericias
            .Where(p => ids.Contains(p.HeroiId))
            .ToListAsync();
    }

    public async Task<List<HeroiPericia>> ObterPorUsuarioAsync(ulong usuarioId)
        => await db.HeroisPericias
            .Include(p => p.Heroi)
            .Where(p => p.Heroi.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task AdicionarMuitosAsync(IEnumerable<HeroiPericia> pericias)
    {
        await db.HeroisPericias.AddRangeAsync(pericias);
        await db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(HeroiPericia pericia)
    {
        db.HeroisPericias.Update(pericia);
        await db.SaveChangesAsync();
    }
}
