using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class ArenaCommand
    {
        private readonly ArenaService _arenaService;
        private readonly HeroiService _heroiService;

        public ArenaCommand(ArenaService arenaService, HeroiService heroiService)
        {
            _arenaService = arenaService;
            _heroiService = heroiService;
        }

        public async Task ExecutarAsync(SocketSlashCommand command)
        {
            var acao = (string)command.Data.Options.First(o => o.Name == "acao").Value;

            switch (acao)
            {
                case "desafio":
                    await DesafioAsync(command);
                    break;
                default:
                    await command.RespondAsync("Ação inválida.", ephemeral: true);
                    break;
            }
        }

        // /arena desafio — uses top 5 heroes by level as the party
        private async Task DesafioAsync(SocketSlashCommand command)
        {
            var herois = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
            if (!herois.Any())
            {
                await command.RespondAsync("Você ainda não tem heróis. Use /colecao para ver sua coleção.", ephemeral: true);
                return;
            }

            // Top 5 heroes by level (higher rarity tie-break)
            var party = herois
                .OrderByDescending(h => h.Nivel)
                .ThenByDescending(h => (int)h.Raridade)
                .Take(5)
                .ToList();

            var (resultado, erro) = await _arenaService.DesafioOndasAsync(command.User.Id, party);

            if (erro != null)
            {
                await command.RespondAsync($"❌ {erro}", ephemeral: true);
                return;
            }

            if (resultado == null)
            {
                await command.RespondAsync("Erro desconhecido ao processar o desafio.", ephemeral: true);
                return;
            }

            var nomeParty = string.Join(", ", party.Select(h => h.Nome));
            var embed = new EmbedBuilder()
                .WithTitle("⚔️ Arena — Desafio de Ondas")
                .WithDescription(
                    $"**Party:** {nomeParty}\n\n" +
                    $"🌊 Ondas sobrevividas: **{resultado.OndasSobrevividas}**\n" +
                    $"⭐ XP total concedido: **{resultado.XpTotal}** (dividido entre heróis)\n" +
                    $"💰 Ouro ganho pela cidade: **{resultado.OuroTotal}**")
                .WithColor(Color.Orange)
                .Build();

            await command.RespondAsync(embed: embed, ephemeral: true);
        }
    }
}
