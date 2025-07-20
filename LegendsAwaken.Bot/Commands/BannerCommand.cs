using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Enum;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    internal class BannerCommand
    {
        private readonly BannerService _bannerService;
        private readonly BannerHistoricoService _historicoService;

        public BannerCommand(BannerService bannerService, BannerHistoricoService historicoService)
        {
            _bannerService = bannerService;
            _historicoService = historicoService;
        }

        public async Task ExecutarAsync(SocketSlashCommand command)
        {
            var banners = _bannerService.ObterTodosBanners().ToList();

            if (!banners.Any())
            {
                await command.RespondAsync("Nenhum banner disponível.", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("📜 Banners disponíveis")
                .WithDescription("Veja abaixo os banners e o pity atual em cada um.")
                .WithColor(Color.Blue);

            foreach (var banner in banners)
            {
                int usado = await _historicoService.ObterContadorAsync(command.User.Id, banner.Id);
                embedBuilder.AddField(
                    $"{banner.Nome} (ID: `{banner.Id}`)",
                    $"🎲 Rolls feitos: {usado} / {banner.PityMaximo}",
                    inline: false);
            }

            // Criar dropdown com os banners
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId("select_banner_roll")
                .WithPlaceholder("Escolha um banner para invocar")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var banner in banners)
            {
                selectMenu.AddOption(banner.Nome, banner.Id);
            }

            var component = new ComponentBuilder()
                .WithSelectMenu(selectMenu);

            await command.RespondAsync(embed: embedBuilder.Build(), components: component.Build(), ephemeral: true);
        }

    }
}
