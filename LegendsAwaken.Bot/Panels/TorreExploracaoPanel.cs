using Discord;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class TorreExploracaoPanel
{
    // ── Investigation ────────────────────────────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarInvestigacao(
        int andar,
        double winChance,
        double teamPS,
        double cdi,
        string descricao,
        IList<(TipoBooster Tipo, int Quantidade)> boosters)
    {
        var cor = winChance switch
        {
            > 0.80 => Color.Green,
            > 0.60 => Color.LightOrange,
            > 0.40 => Color.Orange,
            > 0.20 => Color.DarkOrange,
            _      => Color.DarkRed
        };

        var pct = (int)(winChance * 100);
        var barra = BarraProgresso(pct, 10);

        var sb = new StringBuilder();
        sb.AppendLine($"```");
        sb.AppendLine($"Chance de vitória : {pct}% {barra}");
        sb.AppendLine($"Poder do time     : {teamPS:F0}");
        sb.AppendLine($"Dificuldade (CDI) : {cdi:F0}");
        sb.AppendLine($"Avaliação         : {descricao}");
        sb.AppendLine($"```");

        var sb2 = new StringBuilder();
        if (boosters.Any(b => b.Quantidade > 0))
        {
            sb2.AppendLine("**Boosters disponíveis:**");
            foreach (var (tipo, qtd) in boosters.Where(b => b.Quantidade > 0))
                sb2.AppendLine($"{IconeBooster(tipo)} {NomeBooster(tipo)}: ×{qtd}");
        }
        else
        {
            sb2.AppendLine("*Sem boosters disponíveis.*");
        }

        var embed = new EmbedBuilder()
            .WithTitle($"🔍 Investigação — Andar {andar}")
            .WithColor(cor)
            .WithDescription(sb.ToString())
            .AddField("Boosters", sb2.ToString())
            .WithFooter("Clique em ⚔️ Explorar no painel da Torre para iniciar")
            .Build();

        var comps = new ComponentBuilder()
            .Build();

        return (embed, comps);
    }

    // ── Active exploration ───────────────────────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarAtivo(
        TorreExploracao exp,
        double teamPS,
        double cdi)
    {
        var resumo = TorreExploracaoService.ObterResumo(exp, teamPS, cdi);
        int pct    = (int)resumo.Progresso;
        var barra  = BarraProgresso(pct, 20);

        var boosterStr = exp.BoosterAtivo.HasValue
            ? $"{IconeBooster(exp.BoosterAtivo.Value)} {NomeBooster(exp.BoosterAtivo.Value)}"
            : "Nenhum";

        var boosterMult        = exp.BoosterAtivo == TipoBooster.Eficiencia ? 1.20 : 1.0;
        var progressoPorMinuto = Math.Max(0.01, Math.Min(1.5 * resumo.Ratio * boosterMult, 3.0));
        var minutosRestantes   = (100.0 - resumo.Progresso) / progressoPorMinuto;
        var tempoStr = minutosRestantes < 1
            ? "< 1m"
            : minutosRestantes < 60
                ? $"~{(int)Math.Ceiling(minutosRestantes)}m"
                : $"~{(int)minutosRestantes / 60}h{(int)minutosRestantes % 60}m";

        var sb = new StringBuilder();
        sb.AppendLine($"```");
        sb.AppendLine($"Progresso : {pct:D3}% {barra}");
        sb.AppendLine($"Checkpoint: último {resumo.UltimoCheckpoint}% / próximo {resumo.ProximoCheckpoint}%");
        sb.AppendLine($"Ratio     : {resumo.Ratio:F2}x  |  Chance vitória: {(int)(resumo.WinChance*100)}%");
        sb.AppendLine($"Booster   : {boosterStr}");
        sb.AppendLine($"Tempo     : {tempoStr} até conclusão");
        sb.AppendLine($"```");

        if (resumo.LootOuro > 0 || resumo.LootFragmentos > 0)
        {
            sb.AppendLine($"**Loot acumulado:** 💰 {resumo.LootOuro} Ouro" +
                          (resumo.LootFragmentos > 0 ? $" | 💎 {resumo.LootFragmentos} Fragmentos" : ""));
        }

        var embed = new EmbedBuilder()
            .WithTitle($"⚔️ Explorando — Andar {exp.AndarNumero}")
            .WithColor(Color.Blue)
            .WithDescription(sb.ToString())
            .WithFooter("Exploração em andamento — use 🔄 para atualizar")
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("🔄 Atualizar",  "torre_exp_atualizar", ButtonStyle.Secondary)
            .WithButton("🏳️ Abandonar", "torre_exp_cancelar",  ButtonStyle.Danger)
            .Build();

        return (embed, comps);
    }

    // ── Completed ────────────────────────────────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarConcluido(TorreExploracao exp)
    {
        var sb = new StringBuilder();
        sb.AppendLine("✅ **Exploração concluída com sucesso!**");
        sb.AppendLine($"💰 Ouro coletado: **{exp.LootOuro}**");
        if (exp.LootFragmentosQtd > 0)
            sb.AppendLine($"💎 Fragmentos: **{exp.LootFragmentosQtd}**");
        sb.AppendLine($"\n*Recompensas creditadas ao coletar.*");

        var embed = new EmbedBuilder()
            .WithTitle($"🏆 Andar {exp.AndarNumero} — Concluído!")
            .WithColor(Color.Green)
            .WithDescription(sb.ToString())
            .WithFooter("Clique em Coletar para receber as recompensas")
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("📦 Coletar Recompensas", "torre_exp_coletar", ButtonStyle.Success)
            .Build();

        return (embed, comps);
    }

    // ── Failed ───────────────────────────────────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarFalha(TorreExploracao exp)
    {
        var sb = new StringBuilder();
        sb.AppendLine("❌ **Time derrotado!**");
        sb.AppendLine($"Chegou a **{(int)exp.Progresso}%** do andar {exp.AndarNumero}.");
        sb.AppendLine();

        if (exp.LootOuro > 0 || exp.LootFragmentosQtd > 0)
        {
            sb.AppendLine("**Loot salvo até o último checkpoint:**");
            sb.AppendLine($"💰 Ouro: **{exp.LootOuro}**");
            if (exp.LootFragmentosQtd > 0)
                sb.AppendLine($"💎 Fragmentos: **{exp.LootFragmentosQtd}**");
        }
        else
        {
            sb.AppendLine("*Nenhum loot foi salvo (falha antes do primeiro checkpoint).*");
        }

        sb.AppendLine();
        sb.AppendLine("⚠️ Os heróis ficaram feridos. Colete para libertá-los.");

        var embed = new EmbedBuilder()
            .WithTitle($"💀 Andar {exp.AndarNumero} — Derrota")
            .WithColor(Color.DarkRed)
            .WithDescription(sb.ToString())
            .WithFooter("Colete para liberar os heróis e receber o loot dos checkpoints")
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("📦 Coletar e Libertar Heróis", "torre_exp_coletar", ButtonStyle.Primary)
            .Build();

        return (embed, comps);
    }

    // ── Group selector before exploration ────────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarSeletorGrupo(
        int andar, System.Collections.Generic.List<Party> parties)
    {
        // Pre-compute PS for each party once, then sort descending
        var ranked = parties
            .Select(p =>
            {
                var herois = (p.Membros ?? new System.Collections.Generic.List<PartyHero>())
                    .Where(m => m.Heroi != null)
                    .Select(m => m.Heroi)
                    .ToList();
                int totalPS = (int)HeroPowerScoreService.CalcularParty(herois);
                return (Party: p, Herois: herois, TotalPS: totalPS);
            })
            .OrderByDescending(x => x.TotalPS)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Escolha o grupo que vai explorar este andar.");
        sb.AppendLine();

        foreach (var item in ranked)
        {
            sb.AppendLine($"• **{item.Party.Nome}** — PS **{item.TotalPS}**");
            foreach (var h in item.Herois)
                sb.AppendLine($"  └ {h.Nome} — PS {(int)HeroPowerScoreService.Calcular(h)}");
        }

        var embed = new EmbedBuilder()
            .WithTitle($"⚔️ Explorar — Andar {andar}")
            .WithColor(Color.Gold)
            .WithDescription(sb.ToString())
            .WithFooter("Grupos vazios não são exibidos")
            .Build();

        var menu = new SelectMenuBuilder()
            .WithCustomId("torre_exp_grupo_sel")
            .WithPlaceholder("Escolha o grupo...");

        foreach (var item in ranked.Take(25))
        {
            var label = item.Party.Nome.Length > 25 ? item.Party.Nome[..22] + "..." : item.Party.Nome;
            var heroisStr = string.Join(", ", item.Herois.Take(3).Select(h => $"{h.Nome} ({(int)HeroPowerScoreService.Calcular(h)})"));
            var desc = $"PS {item.TotalPS} | {heroisStr}";
            if (desc.Length > 100) desc = desc[..97] + "...";
            menu.AddOption(label, item.Party.Id.ToString(), desc);
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(menu)
            .WithButton("✖ Cancelar", "torre_exp_cancelar_sel", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Booster selection before exploration ────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarSeletorBooster(
        int andar,
        double winChance,
        IList<(TipoBooster Tipo, int Quantidade)> boosters,
        string partyId,
        string partyNome)
    {
        var pct = (int)(winChance * 100);
        var descricao = HeroPowerScoreService.DescricaoWinChance(winChance);

        var sb = new StringBuilder();
        sb.AppendLine($"**Grupo:** {partyNome}");
        sb.AppendLine($"**Andar {andar}** — Chance de vitória: **{pct}%** ({descricao})");
        sb.AppendLine();
        sb.AppendLine("Escolha um booster para usar nesta exploração, ou prossiga sem booster.");

        var embed = new EmbedBuilder()
            .WithTitle("⚔️ Iniciar Exploração")
            .WithColor(Color.Gold)
            .WithDescription(sb.ToString())
            .WithFooter("Boosters são consumidos ao iniciar")
            .Build();

        var menu = new SelectMenuBuilder()
            .WithCustomId($"torre_exp_booster_sel|{partyId}")
            .WithPlaceholder("Escolha um booster (ou sem booster)...")
            .AddOption("Sem Booster", "nenhum", "Explorar sem booster", new Emoji("🗡️"));

        foreach (var (tipo, qtd) in boosters.Where(b => b.Quantidade > 0))
        {
            menu.AddOption(
                $"{NomeBooster(tipo)} ×{qtd}",
                tipo.ToString(),
                DescricaoBooster(tipo),
                new Emoji(IconeBooster(tipo)));
        }

        var comps = new ComponentBuilder()
            .WithSelectMenu(menu)
            .WithButton("✖ Cancelar", "torre_exp_cancelar_sel", ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Confirm exploration (no boosters) ────────────────────────────────────

    public static (Embed embed, MessageComponent comps) CriarConfirmacao(
        int andar, double winChance, string partyNome, List<string> heroisNomes, string partyId)
    {
        var pct       = (int)(winChance * 100);
        var descricao = HeroPowerScoreService.DescricaoWinChance(winChance);
        var heroList  = string.Join(", ", heroisNomes);

        var embed = new EmbedBuilder()
            .WithTitle($"⚔️ Confirmar Exploração — Andar {andar}")
            .WithColor(Color.Gold)
            .WithDescription(
                $"**Grupo:** {partyNome}\n" +
                $"**Heróis:** {heroList}\n" +
                $"Chance de vitória: **{pct}%** ({descricao})\n\n" +
                $"Loot garantido apenas em checkpoints (a cada **25%** de progresso).\n" +
                $"Em caso de derrota, heróis ficam feridos até você coletar.")
            .WithFooter("A exploração avança enquanto você usa outros comandos")
            .Build();

        var comps = new ComponentBuilder()
            .WithButton("✅ Explorar", $"torre_explorar_confirmar|nenhum|{partyId}", ButtonStyle.Success)
            .WithButton("✖ Cancelar",  "torre_exp_cancelar_sel",                     ButtonStyle.Secondary)
            .Build();

        return (embed, comps);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string BarraProgresso(int pct, int tamanho)
    {
        int preenchido = (int)Math.Round((double)pct / 100 * tamanho);
        return "[" + new string('█', preenchido) + new string('░', tamanho - preenchido) + "]";
    }

    public static string IconeBooster(TipoBooster tipo) => tipo switch
    {
        TipoBooster.Progresso  => "🚀",
        TipoBooster.Fragmento  => "💎",
        TipoBooster.Eficiencia => "⚡",
        TipoBooster.XP         => "📚",
        TipoBooster.Ouro       => "💰",
        TipoBooster.Materiais  => "🪨",
        TipoBooster.Checkpoint => "🏁",
        _                      => "📦"
    };

    public static string NomeBooster(TipoBooster tipo) => tipo switch
    {
        TipoBooster.Progresso  => "Booster de Progresso",
        TipoBooster.Fragmento  => "Booster de Fragmentos",
        TipoBooster.Eficiencia => "Booster de Eficiência",
        TipoBooster.XP         => "Booster de XP",
        TipoBooster.Ouro       => "Booster de Ouro",
        TipoBooster.Materiais  => "Booster de Materiais",
        TipoBooster.Checkpoint => "Booster de Checkpoint",
        _                      => "Booster"
    };

    private static string DescricaoBooster(TipoBooster tipo) => tipo switch
    {
        TipoBooster.Progresso  => "Inicia com +10% de progresso",
        TipoBooster.Fragmento  => "+50% chance de fragmentos nos checkpoints",
        TipoBooster.Eficiencia => "+20% velocidade de progresso",
        TipoBooster.XP         => "+20% XP ao completar (não implementado)",
        TipoBooster.Ouro       => "+30% ouro nos checkpoints",
        TipoBooster.Materiais  => "+20% materiais (futuro)",
        TipoBooster.Checkpoint => "Checkpoints a cada 20% em vez de 25%",
        _                      => ""
    };
}
