using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface IPartyRepository
    {
        Task<Party> CriarAsync(Party party);
        Task<List<Party>> ObterPartiesPorUsuarioAsync(ulong userId);
        Task<Party?> ObterPorIdAsync(Guid partyId);
        Task AdicionarHeroiAsync(Guid partyId, Guid heroiId);
        Task RemoverHeroiAsync(Guid partyId, Guid heroiId);
        Task SalvarAsync();
    }
}
