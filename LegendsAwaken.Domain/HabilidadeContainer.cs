using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain
{
    public class HabilidadeContainer
    {
        public List<Habilidade> Ativas { get; set; } = new();
        public List<Habilidade> Passivas { get; set; } = new();
    }
}
