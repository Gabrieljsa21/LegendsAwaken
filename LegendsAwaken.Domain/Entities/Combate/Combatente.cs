using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities.Combate
{
    public class Combatente
    {
        public Guid Id { get; init; }
        public string Nome { get; init; } = string.Empty;
        public AtributosBase Atributos { get; set; } = new();
        public StatusCombate Status { get; set; } = new();
        public List<HeroiHabilidade>? Habilidades { get; set; }
        public bool IsHeroi { get; init; }
    }

}
