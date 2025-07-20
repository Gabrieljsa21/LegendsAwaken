using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities
{
    public class Equipamentos
    {
        public int Id { get; set; }
        public string? Arma { get; set; }
        public string? Armadura { get; set; }
        public List<string> Acessorios { get; set; } = new();
    }
}
