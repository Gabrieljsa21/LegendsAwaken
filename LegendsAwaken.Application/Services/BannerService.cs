using LegendsAwaken.Application.Helpers;
using LegendsAwaken.Bot.Models.Banner;
using LegendsAwaken.Domain.Entities.Banner;
using LegendsAwaken.Infrastructure.Providers;
using System;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services
{
    public class BannerService
    {
        private readonly Dictionary<string, BannerConfiguracao> _bannersFixos;
        private readonly Dictionary<(ulong usuarioId, string bannerId), BannerDinamico> _bannersDinamicos = new();

        public BannerService()
        {
            _bannersFixos = new Dictionary<string, BannerConfiguracao>(StringComparer.OrdinalIgnoreCase)
            {
                { "banner_padrao", BannerConfiguracoesProvider.BannerPadrao }
                // Adicione outros banners fixos aqui
            };
        }

        /// <summary>
        /// Retorna todos os banners configurados (fixos e dinâmicos, se aplicável).
        /// </summary>
        public IEnumerable<BannerConfiguracao> ObterTodosBanners()
        {
            return _bannersFixos.Values;
        }

        /// <summary>
        /// Retorna a configuração de um banner pelo ID ou null se não encontrado.
        /// </summary>
        public BannerConfiguracao? ObterBannerPorId(string bannerId)
        {
            if (string.IsNullOrWhiteSpace(bannerId))
                return null;

            _bannersFixos.TryGetValue(bannerId.Trim(), out var banner);
            return banner;
        }

        // Futuro: métodos para gerenciar BannerDinamico em _bannersDinamicos
    }
}
