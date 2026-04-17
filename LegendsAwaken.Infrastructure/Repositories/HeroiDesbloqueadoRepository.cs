using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class HeroiDesbloqueadoRepository(LegendsAwakenDbContext db) : IHeroiDesbloqueadoRepository
{
    public async Task<bool> JaDesbloqueadoAsync(Guid usuarioId, Guid heroiId) =>
        await db.HeroisDesbloqueados
            .AnyAsync(h => h.UsuarioId == usuarioId && h.HeroiId == heroiId);

    public async Task SalvarAsync(HeroiDesbloqueado desbloqueado)
    {
        await db.HeroisDesbloqueados.AddAsync(desbloqueado);
        await db.SaveChangesAsync();
    }

    public async Task<List<HeroiDesbloqueado>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await db.HeroisDesbloqueados
            .Include(h => h.Heroi)
            .Where(h => h.UsuarioId == usuarioId)
            .ToListAsync();
}
