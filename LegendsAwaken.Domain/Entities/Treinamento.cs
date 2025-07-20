using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities
{
    public class Treinamento
    {
        public required string Tipo { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public required string ResultadoEsperado { get; set; }
    }
}
