using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface IItemRepository
    {
        Task<Item?> ObterPorIdAsync(Guid id);
        Task<List<Item>> ObterPorProprietarioAsync(ulong usuarioId);
        Task<List<Item>> ObterEquipadosPorHeroiAsync(Guid heroiId);
        Task AdicionarAsync(Item item);
        Task AtualizarAsync(Item item);
    }
}
