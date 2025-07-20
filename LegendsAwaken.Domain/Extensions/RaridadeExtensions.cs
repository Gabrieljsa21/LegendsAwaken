// em LegendsAwaken.Domain.Extensions/RaridadeExtensions.cs
using System;
using System.Linq;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Extensions
{
    public static class RaridadeExtensions
    {
        /// <summary>
        /// Retorna um string contendo '⭐' repetido conforme o valor da raridade.
        /// Ex: Estrela3 → "⭐⭐⭐"
        /// </summary>
        public static string ToStars(this Raridade raridade)
        {
            return string.Concat(Enumerable.Repeat("⭐", (int)raridade));
        }
    }
}
