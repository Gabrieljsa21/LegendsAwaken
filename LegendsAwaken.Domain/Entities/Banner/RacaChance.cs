using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities.Banner
{
    public class RacaChance
    {
        public Raca Raca { get; set; }
        public int Chance { get; set; } // Ex: 10 significa 10% se a soma total for 100
    }
}
