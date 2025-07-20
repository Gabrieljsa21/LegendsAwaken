using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Banner;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories
{
    public class BannerHistoricoRepository : IBannerHistoricoRepository
    {
        private readonly LegendsAwakenDbContext _context;

        public BannerHistoricoRepository(LegendsAwakenDbContext context)
        {
            _context = context;
        }

        public async Task<BannerHistorico?> ObterPorUsuarioEbannerAsync(ulong usuarioId, string bannerId)
        {
            return await _context.BannerHistorico
                .FirstOrDefaultAsync(bh => bh.UsuarioId == usuarioId && bh.BannerId == bannerId);
        }

        public async Task AdicionarAsync(BannerHistorico bannerHistorico)
        {
            await _context.BannerHistorico.AddAsync(bannerHistorico);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(BannerHistorico bannerHistorico)
        {
            _context.BannerHistorico.Update(bannerHistorico);
            await _context.SaveChangesAsync();
        }
    }
}
