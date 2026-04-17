using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Domain.Interfaces;

public interface IHeroiDesbloqueadoRepository
{
    Task<bool> JaDesbloqueadoAsync(Guid usuarioId, Guid heroiId);
    Task SalvarAsync(HeroiDesbloqueado desbloqueado);
    Task<List<HeroiDesbloqueado>> ListarPorUsuarioAsync(Guid usuarioId);
}
