using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorIdAsync(ulong id);
        Task AdicionarAsync(Usuario usuario);
        Task AtualizarAsync(Usuario usuario);
    }
}
