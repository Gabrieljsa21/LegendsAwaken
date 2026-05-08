using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class TorreEventoRepository(LegendsAwakenDbContext db) : ITorreEventoRepository
{
    public async Task AdicionarAsync(TorreEvento evento)
    {
        db.TorreEventos.Add(evento);
        await db.SaveChangesAsync();
    }

    public async Task<TorreEvento?> ObterAtivoAsync(Guid exploracaoId) =>
        await db.TorreEventos
            .FirstOrDefaultAsync(e => e.ExploracaoId == exploracaoId
                                   && e.Status == Domain.Enum.EventoStatus.Ativo);

    public async Task AtualizarAsync(TorreEvento evento)
    {
        db.TorreEventos.Update(evento);
        await db.SaveChangesAsync();
    }

    public async Task AdicionarLogAsync(TorreEventoLog log)
    {
        db.TorreEventoLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<TorreEvento>> ObterExpiradosAsync(DateTime agora) =>
        await db.TorreEventos
            .Include(e => e.Exploracao)
            .Where(e => e.Status == Domain.Enum.EventoStatus.Ativo
                     && e.ExpiraEm.HasValue
                     && e.ExpiraEm.Value < agora)
            .ToListAsync();
}
