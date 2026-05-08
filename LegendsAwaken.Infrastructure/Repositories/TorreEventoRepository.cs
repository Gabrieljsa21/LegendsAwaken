using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class TorreEventoRepository : ITorreEventoRepository
{
    private readonly LegendsAwakenDbContext _ctx;

    public TorreEventoRepository(LegendsAwakenDbContext ctx) => _ctx = ctx;

    public async Task AdicionarAsync(TorreEvento evento)
    {
        _ctx.TorreEventos.Add(evento);
        await _ctx.SaveChangesAsync();
    }

    public async Task<TorreEvento?> ObterAtivoAsync(Guid exploracaoId) =>
        await _ctx.TorreEventos
            .FirstOrDefaultAsync(e => e.ExploracaoId == exploracaoId
                                   && e.Status == Domain.Enum.EventoStatus.Ativo);

    public async Task AtualizarAsync(TorreEvento evento)
    {
        _ctx.TorreEventos.Update(evento);
        await _ctx.SaveChangesAsync();
    }

    public async Task AdicionarLogAsync(TorreEventoLog log)
    {
        _ctx.TorreEventoLogs.Add(log);
        await _ctx.SaveChangesAsync();
    }

    public async Task<List<TorreEvento>> ObterExpiradosAsync(DateTime agora) =>
        await _ctx.TorreEventos
            .Include(e => e.Exploracao)
            .Where(e => e.Status == Domain.Enum.EventoStatus.Ativo
                     && e.ExpiraEm.HasValue
                     && e.ExpiraEm.Value < agora)
            .ToListAsync();
}
