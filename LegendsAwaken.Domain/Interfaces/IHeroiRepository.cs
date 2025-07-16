using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface IHeroiRepository
    {
        Task<Heroi?> ObterPorIdAsync(Guid heroiId);
        Task<List<Heroi>> ObterPorUsuarioIdAsync(ulong usuarioId);
        Task<List<Heroi>> ObterTodosAsync();
        Task AdicionarAsync(Heroi heroi);
        Task AtualizarAsync(Heroi heroi);
    }
}
