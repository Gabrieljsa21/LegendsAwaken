using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface ITorreEventoRepository
{
    Task AdicionarAsync(TorreEvento evento);
    Task<TorreEvento?> ObterAtivoAsync(Guid exploracaoId);
    Task AtualizarAsync(TorreEvento evento);
    Task AdicionarLogAsync(TorreEventoLog log);
    Task<List<TorreEvento>> ObterExpiradosAsync(DateTime agora);
}
