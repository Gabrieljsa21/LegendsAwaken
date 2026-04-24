using Discord;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class InventarioPanel
{
    private static string QualEmoji(Qualidade q) => q switch
    {
        Qualidade.Bom         => "🟢",
        Qualidade.Raro        => "🔵",
        Qualidade.Excepcional => "🟣",
        Qualidade.Mestre      => "🟡",
        _                     => "⚪"
    };

    private static string SlotNome(SlotEquipamento s) => s switch
    {
        SlotEquipamento.Arma      => "⚔️ Armas",
        SlotEquipamento.Armadura  => "🛡️ Armaduras",
        SlotEquipamento.Acessorio => "💍 Acessórios",
        _                         => s.ToString()
    };

    public static Embed CriarEmbed(List<Item> itens, Dictionary<Guid, string> heroiNomes)
    {
        var builder = new EmbedBuilder()
            .WithTitle("🎒 Inventário")
            .WithColor(Color.DarkGrey);

        if (!itens.Any())
        {
            builder.WithDescription("Nenhum item no inventário. Craft itens com `/crafting`.");
            builder.WithFooter("0 itens");
            return builder.Build();
        }

        foreach (var slot in Enum.GetValues<SlotEquipamento>())
        {
            var grupo = itens.Where(i => i.Slot == slot).ToList();
            if (!grupo.Any()) continue;

            var sb = new StringBuilder();
            foreach (var item in grupo)
            {
                var bonusStr = item.Bonus.Any()
                    ? string.Join(", ", item.Bonus.Select(b => $"+{b.Valor} {b.Atributo}"))
                    : "sem bônus";
                var equipStr = item.EstaEquipado && item.HeroiEquipadoId.HasValue
                    ? heroiNomes.TryGetValue(item.HeroiEquipadoId.Value, out var nome)
                        ? $" 📌 {nome}"
                        : " 📌 ?"
                    : "";
                sb.AppendLine($"{QualEmoji(item.Qualidade)} **{item.Nome}**{equipStr}");
                sb.AppendLine($"  └ {bonusStr}");
            }

            builder.AddField(SlotNome(slot), sb.ToString(), inline: false);
        }

        builder.WithFooter($"Total: {itens.Count} item(s)");
        return builder.Build();
    }

    public static MessageComponent CriarComponentes(List<Item> itens)
    {
        var builder = new ComponentBuilder();

        if (itens.Any())
        {
            var select = new SelectMenuBuilder()
                .WithCustomId("inventario_item")
                .WithPlaceholder("Gerenciar item...")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var item in itens.Take(25))
            {
                var equipTag = item.EstaEquipado ? " 📌" : "";
                select.AddOption(
                    $"{QualEmoji(item.Qualidade)} {item.Nome}{equipTag}",
                    item.Id.ToString(),
                    $"{item.Slot} | {item.Qualidade}");
            }

            builder.WithSelectMenu(select);
        }

        builder.WithButton("🔄", "inventario_atualizar", ButtonStyle.Secondary);
        return builder.Build();
    }

    public static Embed CriarEmbedItem(Item item, string? heroiNome)
    {
        var cor = item.Qualidade switch
        {
            Qualidade.Mestre      => Color.Gold,
            Qualidade.Excepcional => Color.Purple,
            Qualidade.Raro        => Color.Blue,
            Qualidade.Bom         => Color.Green,
            _                     => Color.LightGrey
        };

        var bonusStr = item.Bonus.Any()
            ? string.Join("\n", item.Bonus.Select(b => $"+{b.Valor} {b.Atributo}"))
            : "Sem bônus";

        var equipStr = item.EstaEquipado
            ? $"📌 Equipado em **{heroiNome ?? "?"}**"
            : "🆓 Livre";

        return new EmbedBuilder()
            .WithTitle($"{QualEmoji(item.Qualidade)} {item.Nome}")
            .WithColor(cor)
            .AddField("Slot",      item.Slot.ToString(),      inline: true)
            .AddField("Qualidade", item.Qualidade.ToString(), inline: true)
            .AddField("Status",    equipStr,                  inline: false)
            .AddField("Bônus",     bonusStr,                  inline: false)
            .Build();
    }

    public static MessageComponent CriarComponentesItem(Guid itemId, bool estaEquipado)
    {
        var builder = new ComponentBuilder();
        if (estaEquipado)
            builder.WithButton("🔓 Desequipar", $"inventario_desequipar|{itemId}", ButtonStyle.Danger);
        else
            builder.WithButton("⚔️ Equipar em Herói", $"inventario_iniciar_equipar|{itemId}", ButtonStyle.Success);
        return builder.Build();
    }
}
