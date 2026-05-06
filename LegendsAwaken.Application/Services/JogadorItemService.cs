using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services;

public class JogadorItemService(IJogadorItemRepository repo)
{
    public Task AdicionarAsync(Guid usuarioId, AndarItemDef def, int quantidade = 1)
        => repo.UpsertAsync(usuarioId, def.Id, def.Nome, def.Tipo, def.Icone, def.Efeito, quantidade);

    public Task<List<JogadorItem>> ListarAsync(Guid usuarioId)
        => repo.ListarAsync(usuarioId);

    public Task<JogadorItem?> ObterPorConfigAsync(Guid usuarioId, string itemConfigId)
        => repo.ObterPorConfigAsync(usuarioId, itemConfigId);
}
