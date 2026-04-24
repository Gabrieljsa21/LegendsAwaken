using Discord;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class TorreModoOperacaoPanel
{
    // ── No eligible floors (player on andar 1) ───────────────────────────────────

    public static (Embed, MessageComponent) CriarSemAndares()
    {
        var embed = new EmbedBuilder()
            .WithTitle("🏭 Modo Operação")
            .WithDescription("Você precisa conquistar pelo menos um andar antes de usar o Modo Operação.\nUse **⚔️ Avançar Andar** para progredir na Torre.")
            .WithColor(Color.DarkerGrey)
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("❌ Fechar", "torre_op_cancelar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Andar selector ───────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarSeletorAndar(int andarAtual)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🏭 Modo Operação")
            .WithDescription("Selecione um andar já conquistado para enviar heróis em operação.")
            .WithColor(Color.DarkTeal)
            .WithFooter($"Andares elegíveis: 1–{andarAtual - 1}")
            .Build();

        var menu = new SelectMenuBuilder()
            .WithCustomId("torre_op_andar")
            .WithPlaceholder("Escolher andar...")
            .WithMinValues(1).WithMaxValues(1);

        int inicio = Math.Max(1, andarAtual - 15);
        for (int i = andarAtual - 1; i >= inicio; i--)
        {
            var recurso = RecursoDoAndar(i);
            var label = recurso != null ? $"Andar {i} — {recurso}" : $"Andar {i}";
            var desc  = recurso != null ? $"Recurso: {recurso}" : "Apenas ouro";
            menu.AddOption(label, i.ToString(), desc);
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(menu)
            .WithButton("❌ Fechar", "torre_op_cancelar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Objetivo selector ────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarSeletorObjetivo(int andar)
    {
        var recurso = RecursoDoAndar(andar);
        var desc = recurso != null
            ? $"Recurso disponível: **{recurso}**\nEscolha o objetivo da operação."
            : "Nenhum recurso exclusivo neste andar.\nEscolha o objetivo da operação.";

        var embed = new EmbedBuilder()
            .WithTitle($"🏭 Modo Operação — Andar {andar}")
            .WithDescription(desc)
            .WithColor(Color.DarkTeal)
            .AddField("🌾 Farm Recurso",    "Duração: 4h | Foca em coleta de ouro e recursos.", inline: true)
            .AddField("🗺️ Exploração Leve", "Duração: 8h | Mais ouro + bônus de XP para heróis.", inline: true)
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("🌾 Farm Recurso",    $"torre_op_objetivo|{andar}|FarmRecurso",    ButtonStyle.Primary)
            .WithButton("🗺️ Exploração Leve", $"torre_op_objetivo|{andar}|ExploracaoLeve", ButtonStyle.Primary)
            .WithButton("❌ Fechar",           "torre_op_cancelar",                          ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Risk selector ────────────────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarSeletorRisco(int andar, ObjetivoOperacao objetivo)
    {
        int horas = objetivo == ObjetivoOperacao.FarmRecurso ? 4 : 8;
        var objStr = objetivo == ObjetivoOperacao.FarmRecurso ? "🌾 Farm Recurso" : "🗺️ Exploração Leve";

        int ouroSeg = (int)(andar * 3 * horas * 0.8);
        int ouroBal = andar * 3 * horas;
        int ouroAgr = (int)(andar * 3 * horas * 1.5);

        var embed = new EmbedBuilder()
            .WithTitle($"🏭 Modo Operação — Andar {andar}")
            .WithDescription($"Objetivo: **{objStr}** | Duração: **{horas}h**\nEscolha o perfil de risco:")
            .WithColor(Color.DarkTeal)
            .AddField("🛡️ Seguro",    $"~{ouroSeg} ouro | Garantido", inline: true)
            .AddField("⚖️ Balanceado", $"~{ouroBal} ouro | Padrão",   inline: true)
            .AddField("⚔️ Agressivo",  $"~{ouroAgr} ouro | Arriscado", inline: true)
            .Build();

        var objStr2 = objetivo.ToString();
        var comps = new ComponentBuilder()
            .WithButton("🛡️ Seguro",    $"torre_op_risco|{andar}|{objStr2}|Seguro",    ButtonStyle.Success)
            .WithButton("⚖️ Balanceado", $"torre_op_risco|{andar}|{objStr2}|Balanceado", ButtonStyle.Primary)
            .WithButton("⚔️ Agressivo",  $"torre_op_risco|{andar}|{objStr2}|Agressivo",  ButtonStyle.Danger)
            .WithButton("❌ Fechar",     "torre_op_cancelar",                             ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Active operation status ──────────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarStatusAtivo(TorreOperacao op)
    {
        var fim      = op.IniciadoEm.AddHours(op.DuracaoHoras);
        var restante = fim - DateTime.UtcNow;
        string restanteStr = restante.TotalMinutes < 1
            ? "Menos de 1 min"
            : restante.TotalHours >= 1
                ? $"{(int)restante.TotalHours}h {restante.Minutes}min"
                : $"{restante.Minutes} min";

        var objStr  = op.Objetivo == ObjetivoOperacao.FarmRecurso ? "🌾 Farm Recurso" : "🗺️ Exploração Leve";
        var riscoStr = op.PerfilRisco switch
        {
            PerfilRisco.Seguro     => "🛡️ Seguro",
            PerfilRisco.Balanceado => "⚖️ Balanceado",
            PerfilRisco.Agressivo  => "⚔️ Agressivo",
            _                      => op.PerfilRisco.ToString()
        };

        var embed = new EmbedBuilder()
            .WithTitle("🏭 Operação em Andamento")
            .WithColor(Color.Blue)
            .AddField("Andar",       op.AndarNumero.ToString(), inline: true)
            .AddField("Objetivo",    objStr,                     inline: true)
            .AddField("Risco",       riscoStr,                   inline: true)
            .AddField("Concluído em", $"⏳ {restanteStr}")
            .WithFooter($"Iniciado às {op.IniciadoEm:HH:mm} UTC")
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("❌ Cancelar Operação", "torre_op_cancelar_ativo", ButtonStyle.Danger)
            .WithButton("✖ Fechar",             "torre_op_cancelar",       ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Concluded: collect rewards ───────────────────────────────────────────────

    public static (Embed, MessageComponent) CriarColeta(TorreOperacao op)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"💰 **Ouro:** +{op.ResultadoOuro ?? 0}");
        if (op.ResultadoRecursoNome != null)
            sb.AppendLine($"📦 **{op.ResultadoRecursoNome}:** +{op.ResultadoRecursoQtd ?? 0}");

        var objStr = op.Objetivo == ObjetivoOperacao.FarmRecurso ? "🌾 Farm Recurso" : "🗺️ Exploração Leve";

        var embed = new EmbedBuilder()
            .WithTitle("🏭 Operação Concluída!")
            .WithDescription(sb.ToString())
            .WithColor(Color.Green)
            .AddField("Andar",    op.AndarNumero.ToString(), inline: true)
            .AddField("Objetivo", objStr,                     inline: true)
            .WithFooter($"Concluído às {op.ConcluidoEm:HH:mm} UTC")
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("📦 Coletar", "torre_op_coletar", ButtonStyle.Success)
            .WithButton("✖ Fechar",   "torre_op_cancelar", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Notification text (shown as ephemeral followup when /torre is opened) ────

    public static string CriarNotificacaoTexto(TorreOperacao op)
    {
        var sb = new StringBuilder();
        sb.AppendLine("📬 **Operação concluída!** Clique em 🏭 Modo Operação para coletar:");
        sb.AppendLine($"💰 Ouro: +{op.ResultadoOuro ?? 0}");
        if (op.ResultadoRecursoNome != null)
            sb.AppendLine($"📦 {op.ResultadoRecursoNome}: +{op.ResultadoRecursoQtd ?? 0}");
        return sb.ToString().TrimEnd();
    }

    // ── Helper ───────────────────────────────────────────────────────────────────

    private static string? RecursoDoAndar(int andar) => andar switch
    {
        >= 25 => "Núcleo Sombrio",
        >= 18 => "Cristal Arcano",
        >= 12 => "Essência Corrompida",
        >= 5  => "Fragmento Rústico",
        _     => null
    };
}
