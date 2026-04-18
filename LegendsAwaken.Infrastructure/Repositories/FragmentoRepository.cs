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
        FragmentoProgresso? existente = progresso.TipoFragmento == TipoFragmento.Heroi && progresso.HeroiId.HasValue
            ? await db.FragmentosProgresso.FirstOrDefaultAsync(f => f.UsuarioId == progresso.UsuarioId && f.HeroiId == progresso.HeroiId)
            : progresso.TipoFragmento == TipoFragmento.Arquetipo && progresso.Arquetipo.HasValue
                ? await db.FragmentosProgresso.FirstOrDefaultAsync(f => f.UsuarioId == progresso.UsuarioId && f.Arquetipo == progresso.Arquetipo)
                : null;

        if (existente is not null)
        {
            existente.Quantidade = progresso.Quantidade;
            existente.AtualizadoEm = progresso.AtualizadoEm;
            db.FragmentosProgresso.Update(existente);
        }
        else
        {
            await db.FragmentosProgresso.AddAsync(progresso);
        }
        await db.SaveChangesAsync();
    }

    public async Task<List<FragmentoProgresso>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await db.FragmentosProgresso
            .Include(f => f.Heroi)
            .Where(f => f.UsuarioId == usuarioId)
            .ToListAsync();
}
