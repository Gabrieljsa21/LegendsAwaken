using Discord;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class BiomaPanel
{
    // ── Legacy single-biome embed (kept for backwards compat) ────────────────────

    public static Embed CriarEmbed(Bioma bioma, List<BiomHeroPool> pool, int andarAtual)
    {
        var builder = new EmbedBuilder()
            .WithTitle(bioma.Nome)
            .WithDescription(bioma.Descricao)
            .WithColor(Color.DarkOrange)
            .AddField("Andares", $"{bioma.AndarInicio}–{bioma.AndarFim}", inline: true)
            .AddField("Seu andar", andarAtual.ToString(), inline: true);

        var heroPrincipal = pool.FirstOrDefault(p => p.EHeroPrincipal);
        if (heroPrincipal is not null)
            builder.AddField("Herói Principal", heroPrincipal.Heroi?.Nome ?? "(desconhecido)", inline: false);

        var secundarios = pool
            .Where(p => !p.EHeroPrincipal && p.Heroi is not null)
            .Select(p => p.Heroi.Nome);
        if (secundarios.Any())
            builder.AddField("Heróis do Pool", string.Join(", ", secundarios), inline: false);

        return builder.Build();
    }

    public static MessageComponent CriarComponentes()
    {
        return new ComponentBuilder()
            .WithButton("✖ Fechar", "bioma_fechar", ButtonStyle.Secondary)
            .Build();
    }

    // ── List view ─────────────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarLista(List<Bioma> biomas, int andarAtual)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🗺️ Biomas Descobertos")
            .WithColor(Color.DarkOrange)
            .WithDescription("Selecione um bioma para ver detalhes, progresso e heróis do pool.")
            .WithFooter($"Andar atual: {andarAtual}")
            .Build();

        var menu = new SelectMenuBuilder()
            .WithCustomId("bioma_sel")
            .WithPlaceholder("Escolha um bioma...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var b in biomas.Take(25))
        {
            int total        = b.AndarFim - b.AndarInicio + 1;
            int conquistados = Math.Max(0, Math.Min(andarAtual - b.AndarInicio, total));
            int pct          = total > 0 ? conquistados * 100 / total : 0;
            bool isAtual     = andarAtual >= b.AndarInicio && andarAtual <= b.AndarFim;

            var label = b.Nome.Length > 25 ? b.Nome[..22] + "..." : b.Nome;
            var desc  = isAtual
                ? $"Andares {b.AndarInicio}–{b.AndarFim} | ⚔️ Atual • {conquistados}/{total}"
                : $"Andares {b.AndarInicio}–{b.AndarFim} | ✅ {pct}% completo";
            if (desc.Length > 100) desc = desc[..97] + "...";

            menu.AddOption(label, b.Id.ToString(), desc);
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(menu)
            .WithButton("✖ Fechar", "bioma_fechar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Detail view ───────────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarDetalhe(
        Bioma bioma,
        List<BiomHeroPool> pool,
        List<FragmentoProgresso> fragmentos,
        Dictionary<Guid, HeroiUnlockConfig?> unlockMap,
        int andarAtual)
    {
        int total        = bioma.AndarFim - bioma.AndarInicio + 1;
        int conquistados = Math.Max(0, Math.Min(andarAtual - bioma.AndarInicio, total));
        int pct          = total > 0 ? conquistados * 100 / total : 0;
        bool isAtual     = andarAtual >= bioma.AndarInicio && andarAtual <= bioma.AndarFim;

        int barFilled = total > 0 ? (int)Math.Round((double)conquistados / total * 10) : 0;
        var bar = "[" + new string('█', barFilled) + new string('░', 10 - barFilled) + "]";

        var embed = new EmbedBuilder()
            .WithTitle($"🗺️ {bioma.Nome}")
            .WithDescription(bioma.Descricao)
            .WithColor(Color.DarkOrange)
            .AddField("Andares",   $"{bioma.AndarInicio}–{bioma.AndarFim}",    inline: true)
            .AddField("Status",    isAtual ? "⚔️ Exploração atual" : "✅ Concluído", inline: true)
            .AddField("Progresso", $"{bar} {conquistados}/{total} ({pct}%)",   inline: false);

        if (isAtual)
            embed.AddField("Andar Atual", $"⚔️ Você está no andar **{andarAtual}**", inline: false);

        AddHeroPoolField(embed, pool, fragmentos, unlockMap);

        embed.WithFooter("★ herói principal  •  ? heróis ainda por descobrir");

        var comps = new ComponentBuilder()
            .WithButton("◀ Voltar", "bioma_lista",  ButtonStyle.Secondary)
            .WithButton("✖ Fechar", "bioma_fechar", ButtonStyle.Secondary)
            .Build();

        return (embed.Build(), comps);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private static void AddHeroPoolField(
        EmbedBuilder embed,
        List<BiomHeroPool> pool,
        List<FragmentoProgresso> fragmentos,
        Dictionary<Guid, HeroiUnlockConfig?> unlockMap)
    {
        if (!pool.Any()) return;

        var sb = new StringBuilder();
        int desconhecidos = 0;

        var sorted = pool
            .OrderByDescending(p => p.EHeroPrincipal)
            .ThenBy(p => p.Heroi?.Nome ?? "")
            .ToList();

        foreach (var entry in sorted)
        {
            if (entry.Heroi == null) continue;

            var frag       = fragmentos.FirstOrDefault(f => f.TipoFragmento == TipoFragmento.Heroi && f.HeroiId == entry.HeroiId);
            bool descoberto = frag?.Quantidade > 0;

            if (!entry.EHeroPrincipal && !descoberto)
            {
                desconhecidos++;
                continue;
            }

            unlockMap.TryGetValue(entry.HeroiId, out var unlock);
            int requerido = unlock?.QuantidadeFragmentos ?? 10;
            int coletados = frag?.Quantidade ?? 0;

            string prefixo    = entry.EHeroPrincipal ? "★" : "•";
            string progressoStr = coletados >= requerido
                ? $"**{coletados}/{requerido}** ✅"
                : $"{coletados}/{requerido}";

            sb.AppendLine($"{prefixo} **{entry.Heroi.Nome}** — {progressoStr} fragmentos");
        }

        if (desconhecidos > 0)
            sb.AppendLine($"? *{desconhecidos} herói(s) por descobrir*");

        if (sb.Length > 0)
            embed.AddField("Heróis do Pool", sb.ToString().TrimEnd(), inline: false);
    }
}
