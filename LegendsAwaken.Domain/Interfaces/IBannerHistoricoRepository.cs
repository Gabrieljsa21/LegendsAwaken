using LegendsAwaken.Domain.Entities.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces
{
    public interface IBannerHistoricoRepository
    {
        Task<BannerHistorico?> ObterPorUsuarioEbannerAsync(ulong usuarioId, string bannerId);
        Task AdicionarAsync(BannerHistorico bannerHistorico);
        Task AtualizarAsync(BannerHistorico bannerHistorico);
    }
}
