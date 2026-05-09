using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface ITorreExploracaoRepository
{
    Task EnsureTableAsync();
    Task SalvarAsync(TorreExploracao exploracao);
    Task AtualizarAsync(TorreExploracao exploracao);
    Task<TorreExploracao?> ObterAtivaAsync(Guid usuarioId);
    Task<TorreExploracao?> ObterPendenteAsync(Guid usuarioId); // Status Concluida or Falha (not Coletada)
    Task<List<TorreExploracao>> ObterTodasAtivasAsync();       // All Ativa + AguardandoEscolha across all users
}
