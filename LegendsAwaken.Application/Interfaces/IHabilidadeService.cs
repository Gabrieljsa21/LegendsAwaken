using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Interfaces
{
    public interface IHabilidadeService
    {
        Task<List<Habilidade>> ObterTodasAsync();
        // outros métodos
    }
}
