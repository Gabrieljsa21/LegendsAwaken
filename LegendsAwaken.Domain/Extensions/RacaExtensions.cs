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
        /// <summary>
        /// Returns true for any race that is not Humano.
        /// Automatically includes new races added to the enum without code changes.
        /// </summary>
        public static bool EhEspecial(this Raca raca) => raca != Raca.Humano;
    }
}
