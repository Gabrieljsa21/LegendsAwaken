using Discord;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class GruposPanel
{
    // ── List panel ────────────────────────────────────────────────────────────

    public static Embed CriarEmbedLista(List<Party> grupos)
    {
        var builder = new EmbedBuilder()
            .WithTitle("👥 Grupos de Heróis")
            .WithColor(Color.Blue);

        if (!grupos.Any())
        {
            builder.WithDescription(
                "Você ainda não tem grupos criados.\n" +
                "Use **➕ Criar** para montar uma formação ou **⭐ Recomendado** para uma composição automática.");
            return builder.Build();
        }

        var sb = new StringBuilder();
        foreach (var g in grupos.OrderBy(g => g.Nome))
        {
            var ps   = g.Membros.Any() ? (int)HeroPowerScoreService.CalcularParty(g.Membros.Select(m => m.Heroi).ToList()) : 0;
            var modo = g.NomeModoManual ? "🔒" : "🔄";
            sb.AppendLine($"{modo} **{g.Nome}** — {g.Membros.Count} herói(s) | PS {ps}");
        }

        builder.WithDescription(sb.ToString());
        builder.WithFooter($"{grupos.Count} grupo(s)");
        return builder.Build();
    }

    public static MessageComponent CriarComponentesLista(List<Party> grupos)
    {
        var cb = new ComponentBuilder();

        if (grupos.Any())
        {
            var select = new SelectMenuBuilder()
                .WithCustomId("grupos_ver_sel")
                .WithPlaceholder("Selecionar grupo para gerenciar...")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var g in grupos.OrderBy(g => g.Nome).Take(25))
            {
                var ps    = g.Membros.Any() ? (int)HeroPowerScoreService.CalcularParty(g.Membros.Select(m => m.Heroi).ToList()) : 0;
                var label = g.Nome.Length > 25 ? g.Nome[..22] + "..." : g.Nome;
                select.AddOption(label, g.Id.ToString(), $"{g.Membros.Count} heróis | PS {ps}");
            }

            cb.WithSelectMenu(select, row: 0);
        }

        cb.WithButton("➕ Criar",      "grupos_criar",       ButtonStyle.Success,   row: 1);
        cb.WithButton("⭐ Recomendado", "grupos_recomendado", ButtonStyle.Primary,   row: 1);
        cb.WithButton("🔄 Atualizar",   "grupos_lista",       ButtonStyle.Secondary, row: 1);
        return cb.Build();
    }

    // ── Detail panel ──────────────────────────────────────────────────────────

    public static Embed CriarEmbedDetalhe(Party grupo)
    {
        var membros  = grupo.Membros.Select(m => m.Heroi).ToList();
        var totalPS  = membros.Any() ? (int)HeroPowerScoreService.CalcularParty(membros) : 0;
        var modoStr  = grupo.NomeModoManual ? "🔒 Nome fixo" : "🔄 Nome automático";

        var builder = new EmbedBuilder()
            .WithTitle(grupo.Nome)
            .WithColor(Color.DarkBlue)
            .WithFooter($"PS Total: {totalPS} | {modoStr}");

        if (!membros.Any())
        {
            builder.WithDescription("Grupo vazio. Adicione heróis para começar.");
            return builder.Build();
        }

        var sb = new StringBuilder();
        foreach (var h in membros)
        {
            var ps    = (int)HeroPowerScoreService.Calcular(h);
            var func  = IconeFuncao(h.Funcao);
            var stars = new string('⭐', (int)h.Raridade);
            sb.AppendLine($"{func} {stars} **{h.Nome}** — Nv {h.Nivel} | PS {ps}");
        }

        builder.WithDescription(sb.ToString());
        return builder.Build();
    }

    public static MessageComponent CriarComponentesDetalhe(Party grupo)
    {
        var podeAdd = grupo.Membros.Count < 5;
        var podeRem = grupo.Membros.Any();

        var cb = new ComponentBuilder();

        cb.WithButton("➕ Herói",   $"grupos_add_sel|{grupo.Id}",   podeAdd ? ButtonStyle.Success   : ButtonStyle.Secondary, row: 0, disabled: !podeAdd);
        cb.WithButton("➖ Herói",   $"grupos_rem_sel|{grupo.Id}",   podeRem ? ButtonStyle.Danger    : ButtonStyle.Secondary, row: 0, disabled: !podeRem);
        cb.WithButton("✏️ Nome",    $"grupos_nome_editar|{grupo.Id}", ButtonStyle.Secondary,          row: 0);
        cb.WithButton("🗑️ Deletar", $"grupos_deletar|{grupo.Id}",   ButtonStyle.Danger,              row: 0);

        var toggleLabel = grupo.NomeModoManual ? "🔄 Auto-nome" : "🔒 Fixar Nome";
        cb.WithButton(toggleLabel, $"grupos_nome_toggle|{grupo.Id}", ButtonStyle.Secondary, row: 1);
        cb.WithButton("← Voltar",  "grupos_lista",                   ButtonStyle.Secondary, row: 1);

        return cb.Build();
    }

    // ── Create selector ───────────────────────────────────────────────────────

    public static (Embed embed, MessageComponent components) CriarSeletorCriacao(List<Heroi> herois)
    {
        var embed = new EmbedBuilder()
            .WithTitle("➕ Criar Grupo")
            .WithDescription("Selecione de 1 a 5 heróis para compor o grupo.\nO nome será gerado automaticamente.")
            .WithColor(Color.Green)
            .Build();

        var select = new SelectMenuBuilder()
            .WithCustomId("grupos_criar_sel")
            .WithPlaceholder("Escolha os heróis...")
            .WithMinValues(1)
            .WithMaxValues(Math.Min(5, herois.Count));

        foreach (var h in herois.OrderByDescending(HeroPowerScoreService.Calcular).Take(25))
        {
            var ps    = (int)HeroPowerScoreService.Calcular(h);
            var func  = IconeFuncao(h.Funcao);
            var stars = new string('⭐', (int)h.Raridade);
            var label = $"{func} {stars} {h.Nome}".Length > 25
                ? $"{func} {h.Nome} (Nv{h.Nivel})"
                : $"{func} {stars} {h.Nome}";
            label = label.Length > 25 ? label[..25] : label;
            select.AddOption(label, h.Id.ToString(), $"Nv {h.Nivel} | PS {ps}");
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(select, row: 0)
            .WithButton("← Voltar", "grupos_lista", ButtonStyle.Secondary, row: 1)
            .Build();

        return (embed, comps);
    }

    // ── Add hero selector ─────────────────────────────────────────────────────

    public static (Embed embed, MessageComponent components) CriarSeletorAddHeroi(Party grupo, List<Heroi> heroisDisponiveis)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"➕ Adicionar Herói — {grupo.Nome}")
            .WithDescription("Selecione o herói para adicionar ao grupo.")
            .WithColor(Color.Green)
            .Build();

        var select = new SelectMenuBuilder()
            .WithCustomId($"grupos_add|{grupo.Id}")
            .WithPlaceholder("Escolha o herói...")
            .WithMinValues(1)
            .WithMaxValues(1);

        foreach (var h in heroisDisponiveis.OrderBy(h => h.Nome).Take(25))
        {
            var ps    = (int)HeroPowerScoreService.Calcular(h);
            var func  = IconeFuncao(h.Funcao);
            var label = $"{func} {h.Nome} (Nv{h.Nivel})";
            label = label.Length > 25 ? label[..25] : label;
            select.AddOption(label, h.Id.ToString(), $"PS {ps}");
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(select, row: 0)
            .WithButton("← Voltar", $"grupos_ver|{grupo.Id}", ButtonStyle.Secondary, row: 1)
            .Build();

        return (embed, comps);
    }

    // ── Remove hero selector ──────────────────────────────────────────────────

    public static (Embed embed, MessageComponent components) CriarSeletorRemHeroi(Party grupo)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"➖ Remover Herói — {grupo.Nome}")
            .WithDescription("Selecione o herói para remover do grupo.")
            .WithColor(Color.Orange)
            .Build();

        var select = new SelectMenuBuilder()
            .WithCustomId($"grupos_rem|{grupo.Id}")
            .WithPlaceholder("Escolha o herói...")
            .WithMinValues(1)
            .WithMaxValues(1);

        foreach (var membro in grupo.Membros.OrderBy(m => m.Heroi.Nome))
        {
            var h     = membro.Heroi;
            var ps    = (int)HeroPowerScoreService.Calcular(h);
            var func  = IconeFuncao(h.Funcao);
            var label = $"{func} {h.Nome} (Nv{h.Nivel})";
            label = label.Length > 25 ? label[..25] : label;
            select.AddOption(label, h.Id.ToString(), $"PS {ps}");
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(select, row: 0)
            .WithButton("← Voltar", $"grupos_ver|{grupo.Id}", ButtonStyle.Secondary, row: 1)
            .Build();

        return (embed, comps);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string IconeFuncao(FuncaoTatica? funcao) => funcao switch
    {
        FuncaoTatica.Frente       => "🛡️",
        FuncaoTatica.Curandeiro   => "💚",
        FuncaoTatica.Suporte      => "🤝",
        FuncaoTatica.Controle     => "🔮",
        FuncaoTatica.LongoAlcance => "🏹",
        _                         => "⚔️",
    };
}
