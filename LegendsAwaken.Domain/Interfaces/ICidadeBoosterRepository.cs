using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface ICidadeBoosterRepository
{
    Task EnsureTablesAsync();
    // Inventory
    Task<int> ObterQuantidadeAsync(ulong usuarioId, TipoBoosterCidade tipo);
    Task<List<(TipoBoosterCidade Tipo, int Quantidade)>> ListarInventarioAsync(ulong usuarioId);
    Task AdicionarInventarioAsync(ulong usuarioId, TipoBoosterCidade tipo, int quantidade);
    Task<bool> ConsumirInventarioAsync(ulong usuarioId, TipoBoosterCidade tipo);
    // Active booster
    Task<CidadeBoosterAtivo?> ObterAtivoAsync(ulong usuarioId);
    Task SalvarAtivoAsync(CidadeBoosterAtivo ativo);
    Task DesativarAsync(ulong usuarioId);
}
