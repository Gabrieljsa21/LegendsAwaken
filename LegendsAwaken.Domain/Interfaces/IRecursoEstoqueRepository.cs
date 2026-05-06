using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface IRecursoEstoqueRepository
{
    Task EnsureTableAsync();
    Task UpsertAsync(Guid usuarioId, string recurso, int quantidade);
    Task<RecursoEstoque?> ObterAsync(Guid usuarioId, string recurso);
    Task<List<RecursoEstoque>> ListarAsync(Guid usuarioId);
}
