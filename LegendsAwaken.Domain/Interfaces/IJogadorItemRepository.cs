using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface IJogadorItemRepository
{
    Task EnsureTableAsync();
    Task UpsertAsync(Guid usuarioId, string itemConfigId, string nome, TipoItemJogador tipo, string icone, string efeito, int quantidade);
    Task<List<JogadorItem>> ListarAsync(Guid usuarioId);
    Task<JogadorItem?> ObterPorConfigAsync(Guid usuarioId, string itemConfigId);
}
