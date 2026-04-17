using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class FragmentoRepository(LegendsAwakenDbContext db) : IFragmentoRepository
{
    public async Task<FragmentoProgresso?> ObterPorHeroiAsync(Guid usuarioId, Guid heroiId) =>
        await db.FragmentosProgresso
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.HeroiId == heroiId);

    public async Task<FragmentoProgresso?> ObterPorArquetipoAsync(Guid usuarioId, Profissao arquetipo) =>
        await db.FragmentosProgresso
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.Arquetipo == arquetipo);

    public async Task UpsertAsync(FragmentoProgresso progresso)
    {
        var existe = await db.FragmentosProgresso.AnyAsync(f => f.Id == progresso.Id);
        if (existe) db.FragmentosProgresso.Update(progresso);
        else await db.FragmentosProgresso.AddAsync(progresso);
        await db.SaveChangesAsync();
    }

    public async Task<List<FragmentoProgresso>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await db.FragmentosProgresso
            .Include(f => f.Heroi)
            .Where(f => f.UsuarioId == usuarioId)
            .ToListAsync();
}
