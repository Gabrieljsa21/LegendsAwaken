using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface ISlotOcupacaoRepository
    {
        Task<List<SlotOcupacao>> ObterPorConstrucaoAsync(Guid construcaoId);
        Task<SlotOcupacao?> ObterPorHeroiAsync(Guid heroiId);
        Task AdicionarAsync(SlotOcupacao slot);
        Task RemoverAsync(SlotOcupacao slot);
        Task<List<SlotOcupacao>> ObterPorCidadeAsync(Guid cidadeId); // joins via ConstrucaoId
    }
}
