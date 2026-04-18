using Discord;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

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
            .WithTitle("Sua Colecao")
            .WithColor(Color.Purple);

        foreach (var heroi in todosHerois)
        {
            bool desbloqueado = desbloqueados.Any(d => d.HeroiId == heroi.Id);
            var progresso     = progressos.FirstOrDefault(p => p.HeroiId == heroi.Id);
            var unlock        = unlockConfigs.FirstOrDefault(u => u.HeroiId == heroi.Id);

            string estado = desbloqueado ? "+" : "-";
            string barra  = GerarBarra(progresso?.Quantidade ?? 0, unlock?.QuantidadeFragmentos ?? 0);
            string valor  = desbloqueado
                ? "Recrutado"
                : unlock?.TipoUnlock switch
                {
                    TipoUnlock.Fragmentos   => $"{barra} {progresso?.Quantidade ?? 0}/{unlock.QuantidadeFragmentos}",
                    TipoUnlock.MarcoTorre    => $"Andar {unlock.AndarMarco}",
                    TipoUnlock.CondicaoUnica => "Condicao especial",
                    _ => "?"
                };

            builder.AddField(
                $"{estado} {heroi.Nome} ({new string('*', (int)heroi.RaridadeBase)})",
                valor,
                inline: true);
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentes(List<HeroiConfig> heroisProntos)
    {
        var builder = new ComponentBuilder();

        if (heroisProntos.Count > 0)
        {
            var select = new SelectMenuBuilder()
                .WithCustomId("colecao_recrutar")
                .WithPlaceholder("Recrutar heroi...")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var heroi in heroisProntos.Take(25))
                select.AddOption(heroi.Nome, heroi.Id.ToString());

            builder.WithSelectMenu(select);
        }

        return builder.Build();
    }

    private static string GerarBarra(int atual, int maximo)
    {
        if (maximo == 0) return string.Empty;
        int preenchido = (int)Math.Round((double)atual / maximo * 10);
        preenchido = Math.Min(10, preenchido);
        return $"[{new string('#', preenchido).PadRight(10, '.')}]";
    }
}
