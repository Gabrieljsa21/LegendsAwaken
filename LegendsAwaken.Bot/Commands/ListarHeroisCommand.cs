using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Extensions;

namespace LegendsAwaken.Bot.Commands
{
    public class ListarHeroisCommand
    {
        private readonly HeroiService _heroiService;
        public const int PageSize = 25; // quantos heróis por página

        public ListarHeroisCommand(HeroiService heroiService)
            => _heroiService = heroiService;

        public async Task ExecutarAsync(SocketSlashCommand cmd)
        {
            // 1) lê filtros e página inicial (sempre 1)
            //var profissao = (string?)cmd.Data.Options
            //    .FirstOrDefault(o => o.Name == "profissao")?.Value;
            var raridade = (int?)(long?)cmd.Data.Options
                .FirstOrDefault(o => o.Name == "raridade")?.Value;
            int page = 1;

            // 2) carrega tudo, aplica filtros
            var todos = await _heroiService.ObterHeroisPorUsuarioAsync(cmd.User.Id);
            var filtrados = todos
                //.Where(h => profissao == null || h.Profissao?.ToString() == profissao)
                .Where(h => raridade == null || (int)h.Raridade == raridade)
                .ToList();

            if (!filtrados.Any())
            {
                await cmd.RespondAsync("Nenhum herói encontrado com esses filtros.", ephemeral: true);
                return;
            }

            // 3) monta embed da página 1
            var embed = BuildEmbed(filtrados, page);
            var comp = new ComponentBuilder()
                .WithButton("◀️", $"listar|{page - 1}|{raridade}", ButtonStyle.Secondary, disabled: true)
                .WithButton("▶️", $"listar|{page + 1}|{raridade}", ButtonStyle.Secondary,
                            disabled: filtrados.Count <= PageSize)
                .Build();

            await cmd.RespondAsync(embed: embed, components: comp, ephemeral: true);
        }

        public Embed BuildEmbed(List<Heroi> herois, int page)
        {
            const int pageSize = PageSize;
            int totalPages = (int)Math.Ceiling((double)herois.Count / pageSize);
            int start = (page - 1) * pageSize;

            var pageHerois = herois.Skip(start).Take(pageSize).ToList();

            // Você pode definir a cor do embed com base na maior raridade da página:
            var cor = pageHerois.Any() switch
            {
                true when pageHerois.Any(h => h.Raridade == Raridade.Estrela5) => Color.Gold,
                true when pageHerois.Any(h => h.Raridade == Raridade.Estrela4) => Color.Purple,
                true when pageHerois.Any(h => h.Raridade == Raridade.Estrela3) => Color.Blue,
                _ => Color.DarkGrey
            };


            var embedBuilder = new EmbedBuilder()
                .WithTitle($"📜 Lista de Heróis (Página {page} de {totalPages})")
                .WithFooter($"Total: {herois.Count} heróis")
                .WithColor(cor);


            foreach (var heroi in pageHerois)
            {
                var estrela = new string('⭐', (int)heroi.Raridade);

                embedBuilder.AddField(
                    $"{estrela} {heroi.Nome}",
                    $"`Nv {heroi.Nivel}` | Raça: **{heroi.Raca}**",
                    inline: false
                );
            }

            return embedBuilder.Build();
        }

    }

}
