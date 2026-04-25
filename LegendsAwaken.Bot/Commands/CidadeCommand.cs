using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class CidadeCommand(CidadeService cidadeService, HeroiService heroiService, CidadeBoosterService boosterService, ILogger? logger = null)
{
    private void Log(string msg)                       => logger?.LogInformation("[Cidade] {Msg}", msg);
    private void LogWarn(string msg)                   => logger?.LogWarning("[Cidade] {Msg}", msg);
    private void LogErr(Exception ex, string ctx)      => logger?.LogError(ex, "[Cidade] ERRO em {Ctx}", ctx);

    // ── /cidade — painel público ─────────────────────────────────────────────────

    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        Log($"/cidade — user={command.User.Username}");
        await command.DeferAsync();
        var (embed, comps) = await BuildPanelAsync(command.User.Id);
        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
        Log("Painel público enviado");
    }

    // ── Coletar ──────────────────────────────────────────────────────────────────

    public async Task HandleColetarAsync(SocketMessageComponent comp)
    {
        Log($"Coletar — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        if (await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id) == null)
        {
            LogWarn("Cidade não encontrada ao coletar");
            await comp.FollowupAsync("Você ainda não tem uma cidade.", ephemeral: true);
            return;
        }

        var (_, produzido) = await cidadeService.ColetarProducaoAsync(comp.User.Id);
        var total = produzido.Comida + produzido.Madeira + produzido.Pedra + produzido.Erva + produzido.Ouro;
        Log($"Coleta: total={total}");

        if (total == 0)
        {
            await comp.FollowupAsync("Nenhuma produção para coletar. Aloque heróis ou aguarde mais tempo.", ephemeral: true);
            return;
        }

        var linhas = new[]
        {
            produzido.Comida  > 0 ? $"🌾 Comida: +{produzido.Comida}"   : null,
            produzido.Madeira > 0 ? $"🪵 Madeira: +{produzido.Madeira}" : null,
            produzido.Pedra   > 0 ? $"⛏️ Pedra: +{produzido.Pedra}"     : null,
            produzido.Erva    > 0 ? $"🌿 Erva: +{produzido.Erva}"       : null,
            produzido.Ouro    > 0 ? $"💰 Ouro: +{produzido.Ouro}"       : null,
        }.Where(l => l != null);

        await comp.FollowupAsync($"✅ **Coleta realizada!**\n{string.Join("\n", linhas)}", ephemeral: true);
    }

    // ── Alocar em Node de Recurso ─────────────────────────────────────────────────

    public async Task HandleAlocarNodeAsync(SocketMessageComponent comp)
    {
        Log($"AlocarNode (botão) — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var herois   = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
        var cidade   = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        var alocados = await ObterHeroisAlocadosAsync(cidade);

        var disponiveis = herois.Where(h => !alocados.Contains(h.Id)).ToList();
        Log($"AlocarNode: {herois.Count} heróis, {disponiveis.Count} disponíveis");

        if (!disponiveis.Any())
        {
            await comp.FollowupAsync("Nenhum herói disponível para alocar.", ephemeral: true);
            return;
        }

        var select = new SelectMenuBuilder()
            .WithCustomId("cidade_alocar_heroi_para_node")
            .WithPlaceholder("Escolha o herói...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var h in disponiveis.OrderBy(h => h.Nome).Take(25))
            select.AddOption(h.Nome, h.Id.ToString());

        await comp.FollowupAsync(
            "Escolha o herói para coletar recursos:",
            components: new ComponentBuilder().WithSelectMenu(select).Build(),
            ephemeral: true);
        Log("AlocarNode: select de herói enviado");
    }

    public async Task HandleHeroiParaNodeAsync(SocketMessageComponent comp)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"HeroiParaNode — heroiId={heroiIdStr} user={comp.User.Username}");

        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var heroiIdParsed))
        {
            LogWarn($"HeroiParaNode: heroiId inválido '{heroiIdStr}'");
            await comp.UpdateAsync(m => { m.Content = "Herói inválido."; m.Components = null; });
            return;
        }

        var heroi  = await heroiService.ObterHeroiPorIdAsync(heroiIdParsed);
        var cidade = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);

        var select = new SelectMenuBuilder()
            .WithCustomId($"cidade_node_para_heroi|{heroiIdStr}")
            .WithPlaceholder("Escolha o node de recurso...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var node in Enum.GetValues<TipoResourceNode>().OrderBy(n => n.ToString()))
        {
            if (!ResourceNodeConfig.BaseRates.TryGetValue(node, out var rate)) continue;

            double bonus = heroi?.Profissao.HasValue == true &&
                ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, heroi.Profissao!.Value), out var b) ? b : 0.0;
            double taxaHeroi = rate.basePorHora * (1.0 + bonus);

            double totalAtual = 0;
            if (cidade != null)
            {
                foreach (var t in cidade.Trabalhadores.Where(t => t.ResourceNode == node))
                {
                    var worker = await heroiService.ObterHeroiPorIdAsync(t.HeroiId);
                    if (worker == null) continue;
                    double wb = worker.Profissao.HasValue &&
                        ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, worker.Profissao!.Value), out var wbv) ? wbv : 0.0;
                    totalAtual += rate.basePorHora * (1.0 + wb);
                }
            }

            var icone = ResourceNodeConfig.Icone(rate.recurso);
            var profStr      = bonus > 0 ? $" +{bonus * 100:F0}%" : "";
            var totalComHeroi = totalAtual + taxaHeroi;
            var label        = $"{node}: Produção Atual {totalComHeroi:F1} {icone}/h (+{taxaHeroi:F1}/h{profStr})";
            select.AddOption(label.Length > 100 ? label[..97] + "…" : label, node.ToString());
        }

        await comp.UpdateAsync(m =>
        {
            m.Content    = "Escolha o node de recurso:";
            m.Components = new ComponentBuilder().WithSelectMenu(select).Build();
        });
        Log("HeroiParaNode: select de node enviado");
    }

    public async Task HandleNodeParaHeroiAsync(SocketMessageComponent comp, Guid heroiId)
    {
        var nodeStr = comp.Data.Values.FirstOrDefault();
        Log($"NodeParaHeroi — heroiId={heroiId} node={nodeStr} user={comp.User.Username}");

        if (nodeStr == null || !Enum.TryParse<TipoResourceNode>(nodeStr, out var node))
        {
            LogWarn($"NodeParaHeroi: node inválido '{nodeStr}'");
            await comp.UpdateAsync(m => { m.Content = "Node inválido."; m.Components = null; });
            return;
        }

        try
        {
            Log($"NodeParaHeroi: chamando AlocarRecursoAsync({node})");
            await cidadeService.AlocarRecursoAsync(comp.User.Id, heroiId, node);
            var heroi = await heroiService.ObterHeroiPorIdAsync(heroiId);
            var nomeHeroi = heroi?.Nome ?? heroiId.ToString();
            ResourceNodeConfig.BaseRates.TryGetValue(node, out var rate);
            var taxaStr = rate != default ? $" — {rate.basePorHora} {rate.recurso}/h" : "";
            var msg = $"✅ **{nomeHeroi}** alocado em **{node}**{taxaStr}!";
            Log($"NodeParaHeroi: sucesso — {msg}");
            await comp.UpdateAsync(m => { m.Content = msg; m.Components = null; });
        }
        catch (InvalidOperationException ex)
        {
            LogWarn($"NodeParaHeroi: regra de negócio — {ex.Message}");
            await comp.UpdateAsync(m => { m.Content = $"❌ {ex.Message}"; m.Components = null; });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"NodeParaHeroi heroiId={heroiId} node={node}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao alocar. Veja o log."; m.Components = null; });
        }
    }

    // ── Alocar em Prédio ──────────────────────────────────────────────────────────

    public async Task HandleAlocarPredioAsync(SocketMessageComponent comp)
    {
        Log($"AlocarPredio (botão) — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var cidade = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        if (cidade == null || !cidade.Construcoes.Any())
        {
            LogWarn("AlocarPredio: sem prédios");
            await comp.FollowupAsync("Você não tem prédios construídos.", ephemeral: true);
            return;
        }

        var herois      = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
        var alocados    = await ObterHeroisAlocadosAsync(cidade);
        var disponiveis = herois.Where(h => !alocados.Contains(h.Id)).ToList();
        Log($"AlocarPredio: {disponiveis.Count} heróis disponíveis");

        if (!disponiveis.Any())
        {
            await comp.FollowupAsync("Nenhum herói disponível para alocar.", ephemeral: true);
            return;
        }

        var select = new SelectMenuBuilder()
            .WithCustomId("cidade_alocar_heroi_para_predio")
            .WithPlaceholder("Escolha o herói...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var h in disponiveis.OrderBy(h => h.Nome).Take(25))
            select.AddOption(h.Nome, h.Id.ToString());

        await comp.FollowupAsync(
            "Escolha o herói para alocar em um prédio:",
            components: new ComponentBuilder().WithSelectMenu(select).Build(),
            ephemeral: true);
        Log("AlocarPredio: select de herói enviado");
    }

    public async Task HandleHeroiParaPredioAsync(SocketMessageComponent comp)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"HeroiParaPredio — heroiId={heroiIdStr} user={comp.User.Username}");

        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out _))
        {
            LogWarn($"HeroiParaPredio: heroiId inválido '{heroiIdStr}'");
            await comp.UpdateAsync(m => { m.Content = "Herói inválido."; m.Components = null; });
            return;
        }

        var cidade = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        if (cidade == null || !cidade.Construcoes.Any())
        {
            LogWarn("HeroiParaPredio: sem prédios");
            await comp.UpdateAsync(m => { m.Content = "Nenhum prédio disponível."; m.Components = null; });
            return;
        }

        var select = new SelectMenuBuilder()
            .WithCustomId($"cidade_predio_para_heroi|{heroiIdStr}")
            .WithPlaceholder("Escolha o prédio e tipo de slot...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var c in cidade.Construcoes.OrderBy(c => c.Nome))
        {
            if (!PredioConfig.Slots.TryGetValue((c.TipoPredio, c.Nivel), out var def)) continue;
            var slots      = await cidadeService.ObterSlotsPorPredioAsync(c.Id);
            var respUsados = slots.Count(s => s.SlotTipo == SlotTipo.Responsabilidade);
            var opUsados   = slots.Count(s => s.SlotTipo == SlotTipo.Operacao);

            if (respUsados < def.NumResponsabilidade)
                select.AddOption($"{c.Nome} ({respUsados}/{def.NumResponsabilidade}) — Responsável", $"{c.Id}|Responsabilidade");
            if (def.NumOperacao > 0 && opUsados < def.NumOperacao)
                select.AddOption($"{c.Nome} ({opUsados}/{def.NumOperacao}) — Operador", $"{c.Id}|Operacao");
        }

        if (select.Options.Count == 0)
        {
            LogWarn("HeroiParaPredio: sem slots disponíveis");
            await comp.UpdateAsync(m => { m.Content = "Todos os slots dos prédios estão ocupados."; m.Components = null; });
            return;
        }

        Log($"HeroiParaPredio: {select.Options.Count} opções de slot");
        await comp.UpdateAsync(m =>
        {
            m.Content    = "Escolha o prédio e tipo de alocação:";
            m.Components = new ComponentBuilder().WithSelectMenu(select).Build();
        });
    }

    public async Task HandlePredioParaHeroiAsync(SocketMessageComponent comp, Guid heroiId)
    {
        var valorStr = comp.Data.Values.FirstOrDefault();
        Log($"PredioParaHeroi — heroiId={heroiId} valor={valorStr} user={comp.User.Username}");

        if (valorStr == null)
        {
            LogWarn("PredioParaHeroi: valor nulo");
            await comp.UpdateAsync(m => { m.Content = "Seleção inválida."; m.Components = null; });
            return;
        }

        var partes = valorStr.Split('|');
        if (partes.Length != 2 ||
            !Guid.TryParse(partes[0], out var construcaoId) ||
            !Enum.TryParse<SlotTipo>(partes[1], out var slotTipo))
        {
            LogWarn($"PredioParaHeroi: parse falhou '{valorStr}'");
            await comp.UpdateAsync(m => { m.Content = "Seleção inválida."; m.Components = null; });
            return;
        }

        try
        {
            Log($"PredioParaHeroi: chamando AlocarSlotPredioAsync({construcaoId}, {slotTipo})");
            var erro = await cidadeService.AlocarSlotPredioAsync(comp.User.Id, heroiId, construcaoId, slotTipo);
            Log($"PredioParaHeroi: resultado={erro ?? "OK"}");

            string msg;
            if (erro == null)
            {
                var heroi    = await heroiService.ObterHeroiPorIdAsync(heroiId);
                var nomeHeroi = heroi?.Nome ?? heroiId.ToString();
                var cidade    = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
                var nomePredio = cidade?.Construcoes.FirstOrDefault(c => c.Id == construcaoId)?.Nome ?? construcaoId.ToString();
                var tipoSlot   = slotTipo == SlotTipo.Responsabilidade ? "Responsável" : "Operador";
                msg = $"✅ **{nomeHeroi}** alocado em **{nomePredio}** como **{tipoSlot}**!";
            }
            else
            {
                msg = $"❌ {erro}";
            }

            await comp.UpdateAsync(m => { m.Content = msg; m.Components = null; });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"PredioParaHeroi heroiId={heroiId} construcao={construcaoId} slot={slotTipo}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao alocar. Veja o log."; m.Components = null; });
        }
    }

    // ── Desalocar ─────────────────────────────────────────────────────────────────

    public async Task HandleDesalocarAsync(SocketMessageComponent comp)
    {
        Log($"Desalocar (botão) — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var herois  = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
        var cidade  = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        if (cidade == null)
        {
            LogWarn("Desalocar: cidade não encontrada");
            await comp.FollowupAsync("Você ainda não tem uma cidade.", ephemeral: true);
            return;
        }

        var heroiPorId    = herois.ToDictionary(h => h.Id);
        var localizacaoMap = new Dictionary<Guid, string>();

        var alocadosNode  = cidade.Trabalhadores.Select(t => t.HeroiId).ToList();
        foreach (var t in cidade.Trabalhadores)
            localizacaoMap[t.HeroiId] = t.ResourceNode.HasValue ? $"[{t.ResourceNode.Value}]" : "[Node]";

        var alocadosPredio = new List<Guid>();
        foreach (var c in cidade.Construcoes)
        {
            var slots = await cidadeService.ObterSlotsPorPredioAsync(c.Id);
            alocadosPredio.AddRange(slots.Select(s => s.HeroiId));
            foreach (var s in slots) localizacaoMap[s.HeroiId] = $"[{c.Nome}]";
        }

        var todos = alocadosNode.Concat(alocadosPredio).Distinct().ToList();
        Log($"Desalocar: {todos.Count} heróis alocados ({alocadosNode.Count} node, {alocadosPredio.Count} prédio)");

        if (!todos.Any())
        {
            await comp.FollowupAsync("Nenhum herói alocado no momento.", ephemeral: true);
            return;
        }

        var select = new SelectMenuBuilder()
            .WithCustomId("cidade_desalocar_heroi")
            .WithPlaceholder("Escolha o herói para desalocar...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var id in todos.OrderBy(id => heroiPorId.TryGetValue(id, out var h2) ? h2.Nome : "").Take(25))
            if (heroiPorId.TryGetValue(id, out var h))
            {
                var loc   = localizacaoMap.GetValueOrDefault(id, "");
                var label = loc.Length > 0 ? $"{loc} {h.Nome}" : h.Nome;
                select.AddOption(label, id.ToString());
            }

        await comp.FollowupAsync(
            "Escolha o herói para desalocar:",
            components: new ComponentBuilder().WithSelectMenu(select).Build(),
            ephemeral: true);
    }

    public async Task HandleDesalocarHeroiAsync(SocketMessageComponent comp)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"DesalocarHeroi — heroiId={heroiIdStr} user={comp.User.Username}");

        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var heroiId))
        {
            LogWarn($"DesalocarHeroi: heroiId inválido '{heroiIdStr}'");
            await comp.UpdateAsync(m => { m.Content = "Herói inválido."; m.Components = null; });
            return;
        }

        try
        {
            // Capture location before desallocating
            var heroi     = await heroiService.ObterHeroiPorIdAsync(heroiId);
            var nomeHeroi = heroi?.Nome ?? heroiId.ToString();

            var cidade      = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
            string localizacao = "desconhecido";
            if (cidade != null)
            {
                var trabalhador = cidade.Trabalhadores.FirstOrDefault(t => t.HeroiId == heroiId);
                if (trabalhador?.ResourceNode != null)
                {
                    localizacao = $"node **{trabalhador.ResourceNode}**";
                }
                else
                {
                    foreach (var c in cidade.Construcoes)
                    {
                        var slots = await cidadeService.ObterSlotsPorPredioAsync(c.Id);
                        var slot  = slots.FirstOrDefault(s => s.HeroiId == heroiId);
                        if (slot != null)
                        {
                            var tipoSlot = slot.SlotTipo == SlotTipo.Responsabilidade ? "Responsável" : "Operador";
                            localizacao = $"**{c.Nome}** ({tipoSlot})";
                            break;
                        }
                    }
                }
            }

            var erro = await cidadeService.DesalocarHeroiAsync(comp.User.Id, heroiId);
            Log($"DesalocarHeroi: resultado={erro ?? "OK"}");

            await comp.UpdateAsync(m =>
            {
                m.Content    = erro == null
                    ? $"✅ **{nomeHeroi}** desalocado de {localizacao}!"
                    : $"❌ {erro}";
                m.Components = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"DesalocarHeroi heroiId={heroiId}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao desalocar. Veja o log."; m.Components = null; });
        }
    }

    // ── Construir ─────────────────────────────────────────────────────────────────

    public async Task HandleConstruirAsync(SocketMessageComponent comp)
    {
        Log($"Construir (botão) — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var cidade  = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        var jaBuilt = cidade?.Construcoes.Select(c => c.TipoPredio).ToHashSet() ?? [];

        var select = new SelectMenuBuilder()
            .WithCustomId("cidade_construir_predio")
            .WithPlaceholder("Escolha o prédio para construir...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var (tipo, custo) in PredioConfig.CustosConstrucao.OrderBy(kvp => kvp.Key.ToString()))
        {
            if (jaBuilt.Contains(tipo)) continue;
            var partesCusto = new List<string>();
            if (custo.Ouro    > 0) partesCusto.Add($"{custo.Ouro}💰");
            if (custo.Madeira > 0) partesCusto.Add($"{custo.Madeira}🪵");
            if (custo.Pedra   > 0) partesCusto.Add($"{custo.Pedra}⛏️");
            if (custo.Comida  > 0) partesCusto.Add($"{custo.Comida}🌾");
            select.AddOption($"{tipo} — {string.Join(" ", partesCusto)}", tipo.ToString());
        }

        Log($"Construir: {select.Options.Count} prédios disponíveis");

        if (select.Options.Count == 0)
        {
            await comp.FollowupAsync("Todos os prédios já foram construídos!", ephemeral: true);
            return;
        }

        await comp.FollowupAsync(
            "Escolha o prédio para construir:",
            components: new ComponentBuilder().WithSelectMenu(select).Build(),
            ephemeral: true);
    }

    public async Task HandleConstruirPredioAsync(SocketMessageComponent comp)
    {
        var predioStr = comp.Data.Values.FirstOrDefault();
        Log($"ConstruirPredio — predio={predioStr} user={comp.User.Username}");

        if (predioStr == null || !Enum.TryParse<TipoPredio>(predioStr, out var tipoPredio))
        {
            LogWarn($"ConstruirPredio: parse falhou '{predioStr}'");
            await comp.UpdateAsync(m => { m.Content = "Prédio inválido."; m.Components = null; });
            return;
        }

        try
        {
            if (await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id) == null)
                await cidadeService.CriarCidadeAsync("Minha Cidade", comp.User.Id);

            Log($"ConstruirPredio: chamando ConstruirPredioAsync({tipoPredio})");
            var erro = await cidadeService.ConstruirPredioAsync(comp.User.Id, tipoPredio);
            Log($"ConstruirPredio: resultado={erro ?? "OK"}");

            string msg;
            if (erro == null)
            {
                msg = $"🏗️ **{tipoPredio}** construída!";
                if (PredioConfig.Slots.TryGetValue((tipoPredio, 1), out var def))
                {
                    var slotStr = $"{def.NumResponsabilidade} Resp";
                    if (def.NumOperacao > 0) slotStr += $" + {def.NumOperacao} Op";
                    msg += $" Slots: {slotStr}.";
                    if (def.BaseProdPorHora > 0)
                        msg += $" Prod: {def.BaseProdPorHora} {PredioConfig.RecursoProducao.GetValueOrDefault(tipoPredio, "?")}/h.";
                }
            }
            else
            {
                msg = $"❌ {erro}";
            }

            await comp.UpdateAsync(m => { m.Content = msg; m.Components = null; });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"ConstruirPredio predio={tipoPredio}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao construir. Veja o log."; m.Components = null; });
        }
    }

    // ── Booster ───────────────────────────────────────────────────────────────────

    public async Task HandleBoosterAsync(SocketMessageComponent comp)
    {
        Log($"Booster (botão) — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var inventario = await boosterService.ObterInventarioAsync(comp.User.Id);
        var boosterAtivo = await boosterService.ObterAtivoAsync(comp.User.Id);

        var sb = new System.Text.StringBuilder();
        if (boosterAtivo != null)
        {
            var restante = boosterAtivo.ExpiraEm - DateTime.UtcNow;
            var restStr = restante.TotalHours >= 1
                ? $"{(int)restante.TotalHours}h {restante.Minutes}m"
                : $"{(int)restante.TotalMinutes}m";
            sb.AppendLine($"**Booster ativo:** {CidadeBoosterService.IconeBooster(boosterAtivo.Tipo)} {CidadeBoosterService.NomeBooster(boosterAtivo.Tipo)}");
            sb.AppendLine($"Efeito: {CidadeBoosterService.DescricaoBooster(boosterAtivo.Tipo)}  ⏱️ {restStr} restantes");
        }
        else
        {
            sb.AppendLine("*Nenhum booster ativo no momento.*");
        }

        if (!inventario.Any())
        {
            sb.AppendLine("\n*Inventário de boosters vazio. Obtenha boosters via crafting.*");
            await comp.FollowupAsync(sb.ToString(), ephemeral: true);
            return;
        }

        sb.AppendLine();
        sb.AppendLine("**Inventário:**");
        foreach (var (tipo, qtd) in inventario)
            sb.AppendLine($"{CidadeBoosterService.IconeBooster(tipo)} {CidadeBoosterService.NomeBooster(tipo)} ×{qtd} — {CidadeBoosterService.DescricaoBooster(tipo)}");

        var menu = new SelectMenuBuilder()
            .WithCustomId("cidade_booster_ativar")
            .WithPlaceholder("Ativar um booster...");

        foreach (var (tipo, qtd) in inventario.OrderBy(b => CidadeBoosterService.NomeBooster(b.Tipo)))
            menu.AddOption(
                $"{CidadeBoosterService.NomeBooster(tipo)} ×{qtd}",
                tipo.ToString(),
                CidadeBoosterService.DescricaoBooster(tipo),
                new Discord.Emoji(CidadeBoosterService.IconeBooster(tipo)));

        await comp.FollowupAsync(
            sb.ToString(),
            components: new ComponentBuilder().WithSelectMenu(menu).Build(),
            ephemeral: true);
    }

    public async Task HandleBoosterAtivarAsync(SocketMessageComponent comp)
    {
        var tipoStr = comp.Data.Values.FirstOrDefault();
        Log($"BoosterAtivar — tipo={tipoStr} user={comp.User.Username}");

        if (tipoStr == null || !Enum.TryParse<TipoBoosterCidade>(tipoStr, out var tipo))
        {
            await comp.UpdateAsync(m => { m.Content = "Booster inválido."; m.Components = null; });
            return;
        }

        var (sucesso, erro) = await boosterService.AtivarAsync(comp.User.Id, tipo);
        if (!sucesso)
        {
            await comp.UpdateAsync(m => { m.Content = $"❌ {erro}"; m.Components = null; });
            return;
        }

        await comp.UpdateAsync(m =>
        {
            m.Content    = $"✅ **{CidadeBoosterService.NomeBooster(tipo)}** ativado! {CidadeBoosterService.DescricaoBooster(tipo)}";
            m.Components = null;
        });
    }

    // ── Atualizar painel ──────────────────────────────────────────────────────────

    public async Task HandleAtualizarAsync(SocketMessageComponent comp)
    {
        Log($"Atualizar — user={comp.User.Username}");
        await comp.DeferAsync();
        var (embed, comps) = await BuildPanelAsync(comp.User.Id);
        await comp.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private async Task<HashSet<Guid>> ObterHeroisAlocadosAsync(Cidade? cidade)
    {
        var alocados = cidade?.Trabalhadores.Select(t => t.HeroiId).ToHashSet() ?? new HashSet<Guid>();
        if (cidade != null)
            foreach (var c in cidade.Construcoes)
            {
                var slots = await cidadeService.ObterSlotsPorPredioAsync(c.Id);
                foreach (var s in slots) alocados.Add(s.HeroiId);
            }
        return alocados;
    }

    private async Task<(Embed embed, MessageComponent comps)> BuildPanelAsync(ulong usuarioId)
    {
        var cidade = await cidadeService.ObterCidadePorUsuarioAsync(usuarioId)
                     ?? await cidadeService.CriarCidadeAsync("Minha Cidade", usuarioId);

        var herois     = await heroiService.ObterHeroisPorUsuarioAsync(usuarioId);
        var heroiPorId = herois.ToDictionary(h => h.Id);

        var slotsPorConstrucao = new Dictionary<Guid, List<SlotOcupacao>>();
        foreach (var c in cidade.Construcoes)
            slotsPorConstrucao[c.Id] = await cidadeService.ObterSlotsPorPredioAsync(c.Id);

        var boosterAtivo = await boosterService.ObterAtivoAsync(usuarioId);

        return (CidadePanel.CriarEmbed(cidade, heroiPorId, slotsPorConstrucao, boosterAtivo), CidadePanel.CriarComponentes());
    }
}
