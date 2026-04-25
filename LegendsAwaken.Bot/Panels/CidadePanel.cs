using Discord;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class CidadePanel
{
    public static Embed CriarEmbed(
        Cidade cidade,
        Dictionary<Guid, Heroi> heroiPorId,
        Dictionary<Guid, List<SlotOcupacao>> slotsPorConstrucao,
        CidadeBoosterAtivo? boosterAtivo = null)
    {
        var sb = new StringBuilder();

        // ── Recursos ──────────────────────────────────────────────────────────────
        sb.AppendLine("**Recursos**");
        sb.AppendLine($"🌾 {cidade.Recursos.Comida} Comida  🪵 {cidade.Recursos.Madeira} Madeira  ⛏️ {cidade.Recursos.Pedra} Pedra");
        sb.AppendLine($"🌿 {cidade.Recursos.Erva} Erva  💰 {cidade.Recursos.Ouro} Ouro");

        // ── Heróis disponíveis ────────────────────────────────────────────────────
        var alocadosIds = new HashSet<Guid>(cidade.Trabalhadores.Select(t => t.HeroiId));
        foreach (var slots in slotsPorConstrucao.Values)
            foreach (var s in slots) alocadosIds.Add(s.HeroiId);
        int disponiveis = heroiPorId.Count - alocadosIds.Count;
        sb.AppendLine();
        sb.AppendLine($"👥 **Heróis:** {disponiveis} disponíveis / {heroiPorId.Count} total");

        // ── Coletores (grouped by node) ───────────────────────────────────────────
        var coletores = cidade.Trabalhadores.Where(t => t.ResourceNode.HasValue).ToList();
        sb.AppendLine();
        sb.AppendLine("**Coletores**");
        if (coletores.Any())
        {
            var porNode = coletores
                .GroupBy(t => t.ResourceNode!.Value)
                .OrderBy(g => g.Key.ToString());

            foreach (var grupo in porNode)
            {
                var node = grupo.Key;
                if (!ResourceNodeConfig.BaseRates.TryGetValue(node, out var rate)) continue;

                double totalPorHora = 0;
                foreach (var t in grupo)
                {
                    if (!heroiPorId.TryGetValue(t.HeroiId, out var hh)) continue;
                    double bv = hh.Profissao.HasValue &&
                        ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, hh.Profissao!.Value), out var bx) ? bx : 0.0;
                    totalPorHora += rate.basePorHora * (1.0 + bv);
                }

                var icone = ResourceNodeConfig.Icone(rate.recurso);
                sb.AppendLine($"• **{node}** — {totalPorHora:F1} {icone}/h");

                foreach (var t in grupo)
                {
                    if (!heroiPorId.TryGetValue(t.HeroiId, out var h)) continue;
                    double bonus = h.Profissao.HasValue &&
                        ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, h.Profissao!.Value), out var b) ? b : 0.0;
                    double taxa = rate.basePorHora * (1.0 + bonus);
                    var profStr = bonus > 0 ? $" +{bonus * 100:F0}%" : "";
                    sb.AppendLine($"  └ {h.Nome} ({taxa:F1} {icone}/h{profStr})");
                }
            }
        }
        else
        {
            sb.AppendLine("*Nenhum coletor alocado.*");
        }

        // ── Prédios ───────────────────────────────────────────────────────────────
        sb.AppendLine();
        if (cidade.Construcoes.Any())
        {
            sb.AppendLine("**Prédios**");
            foreach (var c in cidade.Construcoes.OrderBy(c => c.Nome))
            {
                if (!PredioConfig.Slots.TryGetValue((c.TipoPredio, c.Nivel), out var def)) continue;
                var slots   = slotsPorConstrucao.GetValueOrDefault(c.Id) ?? [];
                var resp    = slots.Count(s => s.SlotTipo == SlotTipo.Responsabilidade);
                var op      = slots.Count(s => s.SlotTipo == SlotTipo.Operacao);
                var slotStr = $"Resp {resp}/{def.NumResponsabilidade}";
                if (def.NumOperacao > 0) slotStr += $" | Op {op}/{def.NumOperacao}";
                var prodStr = def.BaseProdPorHora > 0
                    ? $" — {def.BaseProdPorHora} {PredioConfig.RecursoProducao.GetValueOrDefault(c.TipoPredio, "?")}/h"
                    : "";
                sb.AppendLine($"• **{c.Nome}** Nv{c.Nivel} [{slotStr}]{prodStr}");
                foreach (var s in slots.Where(s => heroiPorId.ContainsKey(s.HeroiId)))
                    sb.AppendLine($"  └ {heroiPorId[s.HeroiId].Nome} ({s.SlotTipo})");
            }
        }
        else
        {
            sb.AppendLine("**Prédios** — *Nenhum ainda.*");
        }

        // ── Humor e acúmulo ───────────────────────────────────────────────────────
        var humores = cidade.Trabalhadores
            .Where(t => heroiPorId.ContainsKey(t.HeroiId))
            .Select(t => (double)heroiPorId[t.HeroiId].Humor).ToList();
        double humor = humores.Count > 0 ? humores.Average() : 50.0;
        var horas = Math.Min((DateTime.UtcNow - cidade.UltimaColeta).TotalHours, 24.0);
        sb.AppendLine();
        sb.AppendLine($"😊 Humor: {humor:F0}/100  |  ⏱️ Acúmulo: {horas:F1}h");

        // ── Sustento ─────────────────────────────────────────────────────────────
        var heroisList = heroiPorId.Values.ToList();
        var (consumoH, horasRest, estadoSust) = SustentoService.ObterResumo(cidade, heroisList);
        var sustIcon = estadoSust switch
        {
            EstadoSustento.Instavel  => "⚠️",
            EstadoSustento.Degradado => "🔴",
            _                        => "✅"
        };
        var horasRestStr = horasRest == double.MaxValue ? "∞" : $"{horasRest:F1}h";
        sb.AppendLine();
        sb.AppendLine($"**Sustento** {sustIcon}  {consumoH} 🌾/h | Estoque: {cidade.Recursos.Comida} | ~{horasRestStr} restantes");

        // ── Booster ───────────────────────────────────────────────────────────────
        if (boosterAtivo != null)
        {
            var restante = boosterAtivo.ExpiraEm - DateTime.UtcNow;
            var restStr = restante.TotalMinutes < 1
                ? "expirando"
                : restante.TotalHours >= 1
                    ? $"{(int)restante.TotalHours}h {restante.Minutes}m"
                    : $"{(int)restante.TotalMinutes}m";
            sb.AppendLine();
            sb.AppendLine($"🧪 **Booster:** {CidadeBoosterService.IconeBooster(boosterAtivo.Tipo)} {CidadeBoosterService.NomeBooster(boosterAtivo.Tipo)} — {CidadeBoosterService.DescricaoBooster(boosterAtivo.Tipo)} ⏱️ {restStr}");
        }

        return new EmbedBuilder()
            .WithTitle($"🏰 {cidade.Nome}  —  Nível {cidade.Nivel}")
            .WithDescription(sb.ToString())
            .WithColor(Color.Green)
            .Build();
    }

    public static MessageComponent CriarComponentes()
        => new ComponentBuilder()
            .WithButton("Coletar",       "cidade_coletar",       ButtonStyle.Success)
            .WithButton("Alocar Node",   "cidade_alocar_node",   ButtonStyle.Primary)
            .WithButton("Alocar Prédio", "cidade_alocar_predio", ButtonStyle.Primary)
            .WithButton("Desalocar",     "cidade_desalocar",     ButtonStyle.Danger)
            .WithButton("Construir",     "cidade_construir",     ButtonStyle.Secondary)
            .WithButton("🧪 Booster",    "cidade_booster",       ButtonStyle.Secondary)
            .WithButton("🔄",            "cidade_atualizar",     ButtonStyle.Secondary)
            .Build();
}
