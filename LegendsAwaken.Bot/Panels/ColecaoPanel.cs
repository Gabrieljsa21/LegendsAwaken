using Discord;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class ColecaoPanel
{
    public static Embed CriarEmbed(
        List<HeroiConfig> todosHerois,
        List<HeroiDesbloqueado> desbloqueados,
        List<FragmentoProgresso> progressos,
        List<HeroiUnlockConfig> unlockConfigs)
    {
        var builder = new EmbedBuilder()
            .WithTitle("Sua Coleção")
            .WithColor(Color.Purple);

        // Group by Tag so we stay well under the 25-field Discord limit
        var grupos = todosHerois
            .GroupBy(h => h.Tag ?? "Outros")
            .OrderBy(g => g.Key);

        foreach (var grupo in grupos)
        {
            var sb = new StringBuilder();

            foreach (var heroi in grupo.OrderBy(h => h.NomeCompleto))
            {
                bool desbloqueado = desbloqueados.Any(d => d.HeroiId == heroi.Id);
                var progresso     = progressos.FirstOrDefault(p => p.HeroiId == heroi.Id);
                var unlock        = unlockConfigs.FirstOrDefault(u => u.HeroiId == heroi.Id);

                string estado = desbloqueado ? "+" : "-";
                string info   = desbloqueado
                    ? "Recrutado"
                    : unlock?.TipoUnlock switch
                    {
                        TipoUnlock.Fragmentos   => $"{progresso?.Quantidade ?? 0}/{unlock.QuantidadeFragmentos} frags",
                        TipoUnlock.MarcoTorre    => $"Andar {unlock.AndarMarco}",
                        TipoUnlock.CondicaoUnica => "Condição especial",
                        _ => "?"
                    };

                sb.AppendLine($"{estado} **{heroi.NomeCompleto}** — {info}");
            }

            builder.AddField(grupo.Key, sb.Length > 0 ? sb.ToString() : "—", inline: false);
        }

        builder.WithFooter($"{desbloqueados.Count}/{todosHerois.Count} heróis recrutados");

        return builder.Build();
    }

    public static MessageComponent CriarComponentes(List<HeroiConfig> heroisProntos)
    {
        var builder = new ComponentBuilder();

        if (heroisProntos.Count > 0)
        {
            var select = new SelectMenuBuilder()
                .WithCustomId("colecao_recrutar")
                .WithPlaceholder("Recrutar herói...")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var heroi in heroisProntos.Take(25))
                select.AddOption(heroi.Nome, heroi.Id.ToString());

            builder.WithSelectMenu(select);
        }

        return builder.Build();
    }
}
