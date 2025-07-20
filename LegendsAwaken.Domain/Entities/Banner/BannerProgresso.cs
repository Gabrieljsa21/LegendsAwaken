using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities.Banner
{
    public class BannerProgresso
    {
        public string BannerId { get; set; } = default!;
        public ulong UsuarioId { get; set; }
        public int QuantidadeRolls { get; set; } = 0;

        // Campos para lógica dinâmica
        public int ChanceHumano { get; set; } = 90;
        public int[] OutrasChances { get; set; } = new int[5]; // ordem: Bestial, Anão, Elfo, Draconato, Fada
        public int ProximoIndexCrescente { get; set; } = 0;
    }

}
