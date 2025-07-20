using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Entities.Banner
{
    public class BannerHistorico
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ulong UsuarioId { get; set; }
        public string BannerId { get; set; } = "";
        public int QuantidadeInvocacoes { get; set; } = 0;
        public DateTime DataUltimoReset { get; set; } = DateTime.UtcNow;
    }
}
