using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Extensions
{
    public static class RacaExtensions
    {
        public static bool EhEspecial(this Raca raca)
        {
            return raca is Raca.Fada or Raca.Draconato or Raca.Bestial or Raca.Anao or Raca.Elfo;
        }
    }
}
