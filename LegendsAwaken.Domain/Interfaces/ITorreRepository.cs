using LegendsAwaken.Domain.Entities;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface ITorreRepository
    {
        Task<TorreAndar?> ObterAndarPorUsuarioAsync(Guid usuarioId);
        Task<TorreAndar?> ObterPorIdAsync(Guid andarId);
        Task AdicionarAsync(TorreAndar andar);
        Task AtualizarAsync(TorreAndar andar);
    }
}
