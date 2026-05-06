using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class RecursoService(IRecursoEstoqueRepository repo)
{
    public Task AdicionarAsync(Guid usuarioId, string recurso, int quantidade)
        => repo.UpsertAsync(usuarioId, recurso, quantidade);

    public Task<RecursoEstoque?> ObterAsync(Guid usuarioId, string recurso)
        => repo.ObterAsync(usuarioId, recurso);

    public Task<List<RecursoEstoque>> ListarEstoqueAsync(Guid usuarioId)
        => repo.ListarAsync(usuarioId);
}
