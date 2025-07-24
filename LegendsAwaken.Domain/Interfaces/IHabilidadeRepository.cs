using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface IHabilidadeRepository
    {
        Task<List<Habilidade>> ObterTodasAsync();
        Task<Habilidade?> ObterPorIdAsync(string id);
        Task AdicionarAsync(Habilidade habilidade);
        Task AtualizarAsync(Habilidade habilidade);
        Task RemoverAsync(string id);
    }
}
