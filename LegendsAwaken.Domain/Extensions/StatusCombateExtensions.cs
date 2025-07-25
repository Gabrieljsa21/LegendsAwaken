using LegendsAwaken.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Extensions
{
    public static class StatusCombateExtensions
    {
        /// <summary>
        /// Constrói um StatusCombate usando as fórmulas:
        /// VidaMaxima = Vitalidade * 10
        /// ManaMaxima = Inteligencia * 5
        /// </summary>
        public static StatusCombate FromAtributos(this AtributosBase atr)
            => new StatusCombate
            {
                VidaMaxima = atr.Vitalidade * 10,
                VidaAtual = atr.Vitalidade * 10,
                ManaMaxima = atr.Inteligencia * 5,
                ManaAtual = atr.Inteligencia * 5
            };
    }
}
