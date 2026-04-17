using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LegendsAwaken.Infrastructure.Repositories;

public class ContratoRepository(LegendsAwakenDbContext db) : IContratoRepository
{
    public async Task<Contrato?> ObterAtivoAsync(Guid usuarioId, TipoContrato tipo) =>
        await db.Contratos
            .Include(c => c.Heroi)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Tipo == tipo && c.Ativo);

    public async Task SalvarAsync(Contrato contrato)
    {
        var existe = await db.Contratos.AnyAsync(c => c.Id == contrato.Id);
        if (existe) db.Contratos.Update(contrato);
        else await db.Contratos.AddAsync(contrato);
        await db.SaveChangesAsync();
    }

    public async Task DesativarAsync(Guid contratoId)
    {
        var contrato = await db.Contratos.FindAsync(contratoId);
        if (contrato is null) return;
        contrato.Ativo = false;
        await db.SaveChangesAsync();
    }

    public async Task<List<Contrato>> ListarAtivosVencidosAsync(DateTime agora) =>
        await db.Contratos
            .Where(c => c.Ativo && c.ExpiraEm.HasValue && c.ExpiraEm.Value <= agora)
            .ToListAsync();
}
