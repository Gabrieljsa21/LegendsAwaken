using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Helpers;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Models.Banner;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LegendsAwaken.Application.Services.HeroiService;

namespace LegendsAwaken.Bot.Commands
{
    public class InvocarCommand
    {
        private readonly HeroiService _heroiService;
        private readonly BannerHistoricoService _histService;
        private readonly BannerService _bannerService;
        private readonly GachaService _gachaService;
        private readonly RacaService _racaService;

        public InvocarCommand(
            HeroiService heroiService,
            BannerHistoricoService histService,
            BannerService bannerService,
            GachaService gachaService,
            RacaService racaService)
        {
            _heroiService = heroiService;
            _histService = histService;
            _bannerService = bannerService;
            _gachaService = gachaService;
            _racaService = racaService;
        }

        public async Task ExecutarAsync(SocketSlashCommand command)
        {
            // 🔍 Buscar todos os banners disponíveis
            var banners = _bannerService.ObterTodosBanners().ToList(); // CONVERSÃO AQUI
            var options = banners
                .Select(b => new ApplicationCommandOptionChoiceProperties
                {
                    Name = b.Nome,
                    Value = b.Id
                }).ToList();

            if (banners.Count == 1)
            {
                // Só um banner, invocar direto
                await EnviarMenuInvocacao(command, banners[0]);
            }
            else
            {
                // Múltiplos banners — pedir escolha
                var embed = new EmbedBuilder()
                    .WithTitle("Selecione um banner para invocar")
                    .WithDescription("Use o menu abaixo para escolher em qual banner deseja invocar.")
                    .Build();

                var menu = new SelectMenuBuilder()
                    .WithCustomId("select_banner")
                    .WithPlaceholder("Escolha um banner")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                foreach (var b in banners)
                    menu.AddOption(b.Nome, b.Id);

                var comp = new ComponentBuilder().WithSelectMenu(menu);

                await command.RespondAsync(embed: embed, components: comp.Build(), ephemeral: true);
            }
        }

        public async Task EnviarMenuInvocacao(SocketSlashCommand command, BannerConfiguracao banner)
        {
            int usado = await _histService.ObterContadorAsync(command.User.Id, banner.Id);
            var embed = GerarEmbedInvocacao(banner, usado);
            var componentes = GerarComponentesMenu(banner.Id);

            await command.RespondAsync(embed: embed, components: componentes, ephemeral: true);
        }

        public async Task EnviarMenuInvocacao(SocketMessageComponent comp, BannerConfiguracao banner)
        {
            int usado = await _histService.ObterContadorAsync(comp.User.Id, banner.Id);
            var embed = GerarEmbedInvocacao(banner, usado);
            var componentes = GerarComponentesMenu(banner.Id);

            await comp.UpdateAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = componentes;
            });
        }

        private Embed GerarEmbedInvocacao(BannerConfiguracao banner, int usado)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"✨ {banner.Nome}")
                .WithDescription("🎯 Chances de invocação por raridade e raça")
                .AddField("🎲 Rolls feitos", $"{usado} / {banner.PityMaximo}", inline: false);

            foreach (var raridade in Enum.GetValues<Raridade>())
            {
                if (banner.RaridadeChances.TryGetValue(raridade, out var chanceRaridade))
                {
                    string textoRacas = banner.RacaPorRaridade.TryGetValue(raridade, out var racas)
                        ? string.Join("\n", racas.Select(r => $"• {r.Key}: {r.Value}%"))
                        : "Nenhuma raça listada.";

                    embedBuilder.AddField(
                        $"{raridade.ToStars()} — {chanceRaridade}%",
                        textoRacas,
                        inline: true);
                }
            }

            return embedBuilder.Build();
        }

        private MessageComponent GerarComponentesMenu(string bannerId)
        {
            return new ComponentBuilder()
                .WithButton("1️⃣ Roll x1", $"roll|1|{bannerId}", ButtonStyle.Primary)
                .WithButton("🔟➕1 Roll x11", $"roll|11|{bannerId}", ButtonStyle.Success)
                .Build();
        }


        public async Task ExecutarRollAsync(SocketMessageComponent compEvt)
        {

            var parts = compEvt.Data.CustomId.Split('|');
            if (parts[0] != "roll") return;

            int quantidade = int.Parse(parts[1]);

            if (quantidade != 1 && quantidade != 11)
            {
                await compEvt.FollowupAsync("Quantidade inválida de rolls.", ephemeral: true);
                return;
            }

            string bannerId = parts[2];
            var banner = _bannerService.ObterBannerPorId(bannerId);
            if (banner == null)
            {
                await compEvt.FollowupAsync("Banner não encontrado.", ephemeral: true);
                return;
            }


            await compEvt.DeferAsync();
            List<Raca> todasRacas = await _racaService.ObterTodasIdsAsync();
            var resultados = new List<(Raca raca, Raridade raridade, string nomeHeroi)>();

            int rollsAntes = await _histService.ObterContadorAsync(compEvt.User.Id, bannerId);
            int rollsAtual = rollsAntes;

            for (int i = 0; i < quantidade; i++)
            {
                Raridade raridade = _gachaService.SortearRaridade(banner, rollsAtual);
                Raca raca = _gachaService.SortearRaca(raridade, todasRacas);

                var generator = new NomeGenerator();
                var nomeGerado = generator.GerarNome(raca);

                var heroi = await _heroiService.CriarHeroiAsync(
                    usuarioId: compEvt.User.Id,
                    nome: nomeGerado,
                    raridade: raridade,
                    raca: raca,
                    antecedente: "Soldado", // TODO
                    afinidade: new List<HeroiAfinidadeElemental>()
                );

                if (raridade == Raridade.Estrela4)
                {
                    await _histService.ResetarHistoricoAsync(compEvt.User.Id, bannerId);
                    rollsAtual = 0;
                }
                else
                {
                    await _histService.IncrementarInvocacaoAsync(compEvt.User.Id, bannerId);
                    rollsAtual++;
                }

                resultados.Add((raca, raridade, heroi.Nome));
            }

            var embed = new EmbedBuilder()
                .WithTitle($"🎯 {banner.Nome} — Resultado da invocação x{quantidade}")
                .WithDescription(
                    string.Join("\n", resultados.Select((r, i) =>
                        $"{i + 1}. {(r.raridade == Raridade.Estrela4 ? "✨ " : "")}{r.nomeHeroi} — **{r.raca}** ({r.raridade.ToStars()})"))
                  + $"\n\n🎯 Progresso do pity: **{rollsAtual}** rolagens desde último especial.")
                .Build();

            var comp = new ComponentBuilder()
                .WithButton("1️⃣ Roll x1", $"roll|1|{bannerId}", ButtonStyle.Primary)
                .WithButton("🔟➕1 Roll x11", $"roll|11|{bannerId}", ButtonStyle.Success);

            await compEvt.FollowupAsync(
                text: null,
                embeds: new[] { embed },
                components: comp.Build(),
                ephemeral: false // deixa visível para todos
            );
        }

    }
}
