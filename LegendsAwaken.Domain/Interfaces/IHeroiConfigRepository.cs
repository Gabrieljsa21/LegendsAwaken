using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Domain.Interfaces;

public interface IHeroiConfigRepository
{
    Task<HeroiConfig?> ObterPorIdAsync(Guid id);
    Task<HeroiConfig?> ObterPorNomeAsync(string nome);
    Task<List<HeroiConfig>> ListarTodosAsync();
    Task<HeroiUnlockConfig?> ObterUnlockConfigAsync(Guid heroiId);
}
