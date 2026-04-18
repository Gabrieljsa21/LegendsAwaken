using Discord;
using LegendsAwaken.Application.Config;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Bot.Panels;

public static class ContratoPanel
{
    public static Embed CriarEmbed(Contrato? arquetipo, Contrato? nomeado)
    {
        var builder = new EmbedBuilder()
            .WithTitle("Contratos Ativos")
            .WithColor(Color.Blue);

        if (arquetipo is not null)
            builder.AddField("Arquetipo",
                $"{arquetipo.Arquetipo} (+{ContractConfig.ArchetypeBonus * 100:0}% fragmentos)", inline: true);
        else
            builder.AddField("Arquetipo", "Nenhum ativo", inline: true);

        if (nomeado is not null)
        {
            var restante = nomeado.ExpiraEm.HasValue
                ? $"Expira em {(nomeado.ExpiraEm.Value - DateTime.UtcNow).TotalHours:0.0}h"
                : "Sem expiracao";
            builder.AddField("Foco Nomeado",
                $"{nomeado.Heroi?.Nome ?? "?"} (+{ContractConfig.NamedBonus * 100:0}%) - {restante}", inline: true);
        }
        else
        {
            builder.AddField("Foco Nomeado", "Nenhum ativo", inline: true);
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentes()
    {
        var select = new SelectMenuBuilder()
            .WithCustomId("contrato_arquetipo")
            .WithPlaceholder("Mudar arquetipo...")
            .AddOption("Combate",  Profissao.Guerreiro.ToString())
            .AddOption("Coleta",   Profissao.Agricultor.ToString())
            .AddOption("Producao", Profissao.Ferreiro.ToString());

        return new ComponentBuilder()
            .WithSelectMenu(select)
            .WithButton("Remover Foco Nomeado", "contrato_remover_nomeado", ButtonStyle.Danger, row: 1)
            .Build();
    }
}
