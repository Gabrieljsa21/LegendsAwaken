using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface IHeroiPericiaRepository
{
    Task<List<HeroiPericia>> ObterPorHeroiAsync(Guid heroiId);
    Task<List<HeroiPericia>> ObterPorUsuarioAsync(ulong usuarioId);
    Task AdicionarMuitosAsync(IEnumerable<HeroiPericia> pericias);
    Task AtualizarAsync(HeroiPericia pericia);
}
