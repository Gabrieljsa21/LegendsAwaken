using Discord;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class TorreModoOperacaoPanel
{
    // ── Main board ───────────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarBoard(
        List<TorreOperacao> ativas,
        List<TorreOperacao> concluidas,
        int andarAtual,
        int maxSlots,
        List<RecursoEstoque>? estoque = null,
        List<JogadorItem>? itens = null,
        EstadoSustento estadoSustento = EstadoSustento.Ativo,
        double horasComidaRestantes = double.MaxValue)
    {
        var sb = new StringBuilder();

        // Sustento warning banner
        if (estadoSustento == EstadoSustento.Degradado)
        {
            sb.AppendLine("🔴 **ATENÇÃO — Heróis Degradados!** Sem comida: novas operações bloqueadas.");
            sb.AppendLine();
        }
        else if (estadoSustento == EstadoSustento.Instavel)
        {
            var hStr = horasComidaRestantes < double.MaxValue
                ? $"~{horasComidaRestantes:F0}h restantes"
                : "estoque baixo";
            sb.AppendLine($"⚠️ **Sustento Instável** — {hStr}. Produza mais Comida no Campo.");
            sb.AppendLine();
        }

        // Slot usage
        int emUso = ativas.Count;
        sb.AppendLine($"**Slots:** {emUso}/{maxSlots} em uso");

        // Active operations
        sb.AppendLine();
        sb.AppendLine("**Operações Ativas**");
        if (ativas.Any())
        {
            foreach (var op in ativas.OrderBy(o => o.AndarNumero))
            {
                var fim       = op.IniciadoEm.AddHours(op.DuracaoHoras);
                var restante  = fim - DateTime.UtcNow;
                var tempoStr  = restante.TotalMinutes < 1
                    ? "⚡ Concluindo..."
                    : restante.TotalHours >= 1
                        ? $"⏳ {(int)restante.TotalHours}h{restante.Minutes:D2}m"
                        : $"⏳ {restante.Minutes}m";

                var (recurso, qtd, icone) = TorreOperacaoConfig.ObterProducao(op.AndarNumero);
                sb.AppendLine($"• Andar **{op.AndarNumero}** — {icone} {recurso} ×{qtd} — {tempoStr}");
            }
        }
        else
        {
            sb.AppendLine("*Nenhuma operação ativa.*");
        }

        // Pending collection
        if (concluidas.Any())
        {
            sb.AppendLine();
            sb.AppendLine("**Prontas para Coletar** ✅");
            foreach (var op in concluidas.OrderBy(o => o.AndarNumero))
            {
                var (recurso, qtd, icone) = TorreOperacaoConfig.ObterProducao(op.AndarNumero);
                if (op.ResultadoOuro > 0)
                    sb.AppendLine($"• Andar **{op.AndarNumero}** — 💰 {op.ResultadoOuro} Ouro");
                else if (op.ResultadoRecursoNome != null)
                    sb.AppendLine($"• Andar **{op.AndarNumero}** — {icone} {op.ResultadoRecursoNome} ×{op.ResultadoRecursoQtd}");
                else
                    sb.AppendLine($"• Andar **{op.AndarNumero}** — {icone} {recurso} ×{qtd}");
            }
        }

        var builder = new EmbedBuilder()
            .WithTitle("🏭 Modo Operação")
            .WithDescription(sb.ToString())
            .WithColor(Color.DarkTeal)
            .WithFooter($"Andares conquistados: 1–{andarAtual - 1} | Duração fixa: {TorreOperacaoConfig.DuracaoHoras}h");

        if (estoque is { Count: > 0 })
        {
            var sbRecursos = new StringBuilder();
            foreach (var r in estoque.Take(8))
                sbRecursos.AppendLine($"**{r.Recurso}** ×{r.Quantidade}");
            builder.AddField("📦 Estoque de Recursos", sbRecursos.ToString().TrimEnd(), inline: true);
        }

        if (itens is { Count: > 0 })
        {
            var sbItens = new StringBuilder();
            foreach (var item in itens.Take(8))
                sbItens.AppendLine($"{item.Icone} **{item.Nome}** ×{item.Quantidade}");
            builder.AddField("🎒 Itens", sbItens.ToString().TrimEnd(), inline: true);
        }

        var embed = builder.Build();

        var cb = new ComponentBuilder();
        if (emUso < maxSlots)
            cb.WithButton("➕ Alocar", "torre_op_alocar", ButtonStyle.Success);
        if (concluidas.Any())
            cb.WithButton("📦 Coletar Tudo", "torre_op_coletar_todas", ButtonStyle.Primary);
        if (ativas.Any())
            cb.WithButton("🗑️ Remover", "torre_op_remover_sel", ButtonStyle.Danger);
        cb.WithButton("✖ Fechar", "torre_op_fechar", ButtonStyle.Secondary);

        return (embed, cb.Build());
    }

    // ── No floors yet ────────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarSemAndares()
    {
        var embed = new EmbedBuilder()
            .WithTitle("🏭 Modo Operação")
            .WithDescription("Você precisa conquistar pelo menos um andar antes de usar o Modo Operação.\nUse **⚔️ Explorar** para progredir na Torre.")
            .WithColor(Color.DarkerGrey)
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("✖ Fechar", "torre_op_fechar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Floor selector (Alocar) ──────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarSeletorAndar(
        int andarAtual, HashSet<int> andaresBloqueados, int maxSlots, int emUso)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🏭 Alocar Operação")
            .WithDescription(
                $"Slots disponíveis: **{maxSlots - emUso}** de {maxSlots}\n" +
                "Selecione um andar conquistado para iniciar uma operação.")
            .WithColor(Color.DarkTeal)
            .Build();

        var menu = new SelectMenuBuilder()
            .WithCustomId("torre_op_andar_sel")
            .WithPlaceholder("Escolher andar...")
            .WithMinValues(1).WithMaxValues(1);

        int inicio = Math.Max(1, andarAtual - 25);
        for (int i = andarAtual - 1; i >= inicio; i--)
        {
            if (andaresBloqueados.Contains(i)) continue;
            var (recurso, qtd, icone) = TorreOperacaoConfig.ObterProducao(i);
            var itemDef = AndarItemConfig.ObterItemDoAndar(i);
            var descricao = itemDef != null
                ? $"{icone} {recurso} ×{qtd} | 🎁 {itemDef.Nome}"
                : $"{icone} {recurso} ×{qtd} por operação";
            menu.AddOption($"Andar {i}", i.ToString(), descricao);
        }

        if (menu.Options.Count == 0)
        {
            var embedVazio = new EmbedBuilder()
                .WithTitle("🏭 Modo Operação")
                .WithDescription("Todos os andares conquistados já têm operação em andamento.")
                .WithColor(Color.DarkerGrey)
                .Build();
            return (embedVazio, new ComponentBuilder().WithButton("✖ Fechar", "torre_op_fechar", ButtonStyle.Secondary).Build());
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(menu)
            .WithButton("✖ Cancelar", "torre_op_fechar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Remove selector ──────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarSeletorRemover(List<TorreOperacao> ativas)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🗑️ Remover Operação")
            .WithDescription("Selecione a operação que deseja cancelar.\n⚠️ Nenhuma recompensa será dada ao cancelar.")
            .WithColor(Color.DarkRed)
            .Build();

        var menu = new SelectMenuBuilder()
            .WithCustomId("torre_op_remover_andar_sel")
            .WithPlaceholder("Escolher operação para cancelar...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var op in ativas.OrderBy(o => o.AndarNumero))
        {
            var fim       = op.IniciadoEm.AddHours(op.DuracaoHoras);
            var restante  = fim - DateTime.UtcNow;
            var tempoStr  = restante.TotalMinutes < 1 ? "concluindo" : $"{(int)restante.TotalMinutes}m rest.";
            var (recurso, _, icone) = TorreOperacaoConfig.ObterProducao(op.AndarNumero);
            menu.AddOption($"Andar {op.AndarNumero} — {recurso}", op.AndarNumero.ToString(), tempoStr);
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(menu)
            .WithButton("✖ Cancelar", "torre_op_fechar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Notification text ────────────────────────────────────────────────────────

    public static string CriarNotificacaoTexto(List<TorreOperacao> concluidas)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📬 **{concluidas.Count} operação(ões) concluída(s)!** Use **🏭 Modo Operação** para coletar:");
        foreach (var op in concluidas.Take(3))
        {
            if (op.ResultadoOuro > 0)
                sb.AppendLine($"• Andar {op.AndarNumero}: 💰 {op.ResultadoOuro} Ouro");
            else if (op.ResultadoRecursoNome != null)
                sb.AppendLine($"• Andar {op.AndarNumero}: {op.ResultadoRecursoNome} ×{op.ResultadoRecursoQtd}");
        }
        return sb.ToString().TrimEnd();
    }

    // Legacy single-op notification (kept for /torre call that still uses the old path)
    public static string CriarNotificacaoTexto(TorreOperacao op)
    {
        var sb = new StringBuilder();
        sb.AppendLine("📬 **Operação concluída!** Clique em 🏭 Modo Operação para coletar:");
        sb.AppendLine($"💰 Ouro: +{op.ResultadoOuro ?? 0}");
        if (op.ResultadoRecursoNome != null)
            sb.AppendLine($"📦 {op.ResultadoRecursoNome}: +{op.ResultadoRecursoQtd ?? 0}");
        return sb.ToString().TrimEnd();
    }
}
