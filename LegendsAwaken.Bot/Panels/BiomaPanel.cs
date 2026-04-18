using Discord;
using LegendsAwaken.Domain.Entities.Fragmento;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Bot.Panels;

public static class BiomaPanel
{
    public static Embed CriarEmbed(Bioma bioma, List<BiomHeroPool> pool, int andarAtual)
    {
        var builder = new EmbedBuilder()
            .WithTitle(bioma.Nome)
            .WithDescription(bioma.Descricao)
            .WithColor(Color.DarkOrange)
            .AddField("Andares", $"{bioma.AndarInicio} - {bioma.AndarFim}", inline: true)
            .AddField("Seu andar", andarAtual.ToString(), inline: true);

        var heroPrincipal = pool.FirstOrDefault(p => p.EHeroPrincipal);
        if (heroPrincipal is not null)
            builder.AddField("Heroi Principal", heroPrincipal.Heroi?.Nome ?? "(desconhecido)", inline: false);

        var secundarios = pool
            .Where(p => !p.EHeroPrincipal && p.Heroi is not null)
            .Select(p => p.Heroi.Nome);

        if (secundarios.Any())
            builder.AddField("Herois do Pool", string.Join(", ", secundarios), inline: false);

        return builder.Build();
    }

    public static MessageComponent CriarComponentes()
    {
        return new ComponentBuilder()
            .WithButton("Ver Colecao", "bioma_ver_colecao", ButtonStyle.Secondary)
            .WithButton("Contratos", "bioma_contratos", ButtonStyle.Primary)
            .Build();
    }
}
