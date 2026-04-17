using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Domain.Interfaces;

public interface IBiomaRepository
{
    Task<Bioma?> ObterPorAndarAsync(int andar);
    Task<List<BiomHeroPool>> ObterPoolAsync(Guid biomaId);
    Task<List<Bioma>> ListarTodosAsync();
}
