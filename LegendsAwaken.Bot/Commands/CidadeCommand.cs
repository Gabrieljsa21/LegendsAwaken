using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class CidadeCommand
    {
        private readonly CidadeService _cidadeService;
        private readonly HeroiService _heroiService;

        public CidadeCommand(CidadeService cidadeService, HeroiService heroiService)
        {
            _cidadeService = cidadeService;
            _heroiService = heroiService;
        }

        public async Task ExecutarAsync(SocketSlashCommand command)
        {
            var acao       = (string)command.Data.Options.First(o => o.Name == "acao").Value;
            var nomeHeroi  = command.Data.Options.FirstOrDefault(o => o.Name == "heroi")?.Value as string;
            var nomePredio = command.Data.Options.FirstOrDefault(o => o.Name == "predio")?.Value as string;
            var nomeSlot   = command.Data.Options.FirstOrDefault(o => o.Name == "slot_tipo")?.Value as string;
            var nomeNode   = command.Data.Options.FirstOrDefault(o => o.Name == "node")?.Value as string;

            switch (acao)
            {
                case "ver":
                    await VerAsync(command);
                    break;
                case "coletar":
                    await ColetarAsync(command);
                    break;
                case "alocar":
                    // Backward-compat: alocar without node defaults to Campo
                    await AlocarRecursoAsync(command, nomeHeroi, "Campo");
                    break;
                case "alocar_recurso":
                    await AlocarRecursoAsync(command, nomeHeroi, nomeNode);
                    break;
                case "alocar_predio":
                    await AlocarPredioAsync(command, nomeHeroi, nomePredio, nomeSlot);
                    break;
                case "desalocar":
                    await DesalocarAsync(command, nomeHeroi);
                    break;
                case "construir":
                    await ConstruirAsync(command, nomePredio);
                    break;
                default:
                    await command.RespondAsync("Ação inválida.", ephemeral: true);
                    break;
            }
        }

        // ── /cidade ver ─────────────────────────────────────────────────────────────

        private async Task VerAsync(SocketSlashCommand command)
        {
            var cidade = await _cidadeService.ObterCidadePorUsuarioAsync(command.User.Id)
                         ?? await _cidadeService.CriarCidadeAsync("Minha Cidade", command.User.Id);

            var herois    = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
            var heroiPorId = herois.ToDictionary(h => h.Id);

            var sb = new StringBuilder();

            // Recursos
            sb.AppendLine("**Recursos:**");
            sb.AppendLine($"🌾 Comida: {cidade.Recursos.Comida}");
            sb.AppendLine($"🪵 Madeira: {cidade.Recursos.Madeira}");
            sb.AppendLine($"⛏️ Pedra: {cidade.Recursos.Pedra}");
            sb.AppendLine($"🌿 Erva: {cidade.Recursos.Erva}");
            sb.AppendLine($"💰 Ouro: {cidade.Recursos.Ouro}");

            // Coletores (ResourceNode workers)
            var coletores = cidade.Trabalhadores.Where(t => t.ResourceNode.HasValue).ToList();
            sb.AppendLine();
            sb.AppendLine("**Coletores:**");
            if (coletores.Any())
            {
                foreach (var t in coletores)
                {
                    if (!heroiPorId.TryGetValue(t.HeroiId, out var h)) continue;
                    var node = t.ResourceNode!.Value;
                    if (!ResourceNodeConfig.BaseRates.TryGetValue(node, out var rate)) continue;
                    double bonus = h.Profissao.HasValue &&
                        ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, h.Profissao.Value), out var b) ? b : 0.0;
                    double porHora = rate.basePorHora * (1.0 + bonus);
                    sb.AppendLine($"• {h.Nome} → {node} ({porHora:F1} {rate.recurso}/h)");
                }
            }
            else
            {
                sb.AppendLine("*Nenhum coletor. Use `/cidade alocar_recurso` para designar heróis.*");
            }

            // Prédios
            sb.AppendLine();
            if (cidade.Construcoes.Any())
            {
                sb.AppendLine("**Prédios:**");
                foreach (var c in cidade.Construcoes.OrderBy(c => c.Nome))
                {
                    if (!PredioConfig.Slots.TryGetValue((c.TipoPredio, c.Nivel), out var def)) continue;
                    var slots  = await _cidadeService.ObterSlotsPorPredioAsync(c.Id);
                    var resp   = slots.Count(s => s.SlotTipo == SlotTipo.Responsabilidade);
                    var op     = slots.Count(s => s.SlotTipo == SlotTipo.Operacao);
                    var slotInfo = $"Resp {resp}/{def.NumResponsabilidade}";
                    if (def.NumOperacao > 0) slotInfo += $" | Op {op}/{def.NumOperacao}";
                    var prodInfo = def.BaseProdPorHora > 0
                        ? $" — {def.BaseProdPorHora} {PredioConfig.RecursoProducao.GetValueOrDefault(c.TipoPredio, "?")}/h"
                        : "";
                    sb.AppendLine($"• **{c.Nome}** Nv{c.Nivel} [{slotInfo}]{prodInfo}");
                    foreach (var s in slots)
                    {
                        if (heroiPorId.TryGetValue(s.HeroiId, out var h))
                            sb.AppendLine($"  └ {h.Nome} ({s.SlotTipo})");
                    }
                }
            }
            else
            {
                sb.AppendLine("**Prédios:** *Nenhum ainda. Use `/cidade construir` para construir.*");
            }

            // Humor da cidade
            var humores = cidade.Trabalhadores
                .Where(t => heroiPorId.ContainsKey(t.HeroiId))
                .Select(t => (double)heroiPorId[t.HeroiId].Humor)
                .ToList();
            double humorCidade = humores.Count > 0 ? humores.Average() : 50.0;
            var horasAcumuladas = Math.Min((DateTime.UtcNow - cidade.UltimaColeta).TotalHours, 24.0);
            sb.AppendLine();
            sb.AppendLine($"😊 Humor: {humorCidade:F0}/100  |  ⏱️ Produção: {horasAcumuladas:F1}h acumuladas");

            var embed = new EmbedBuilder()
                .WithTitle($"🏰 {cidade.Nome}  —  Nível {cidade.Nivel}")
                .WithDescription(sb.ToString())
                .WithColor(Color.Green)
                .Build();

            await command.RespondAsync(embed: embed, ephemeral: true);
        }

        // ── /cidade coletar ─────────────────────────────────────────────────────────

        private async Task ColetarAsync(SocketSlashCommand command)
        {
            var cidade = await _cidadeService.ObterCidadePorUsuarioAsync(command.User.Id);
            if (cidade == null)
            {
                await command.RespondAsync("Você ainda não tem uma cidade. Use `/cidade ver` para criar uma.", ephemeral: true);
                return;
            }

            var (cidadeAtualizada, produzido) = await _cidadeService.ColetarProducaoAsync(command.User.Id);

            var total = produzido.Comida + produzido.Madeira + produzido.Pedra + produzido.Erva + produzido.Ouro;
            if (total == 0)
            {
                await command.RespondAsync("Nenhuma produção para coletar. Aloque heróis ou aguarde mais tempo.", ephemeral: true);
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

            var embed = new EmbedBuilder()
                .WithTitle($"✅ Coleta — {cidadeAtualizada.Nome}")
                .WithDescription(string.Join("\n", linhas))
                .WithColor(Color.Gold)
                .Build();

            await command.RespondAsync(embed: embed, ephemeral: true);
        }

        // ── /cidade alocar_recurso ──────────────────────────────────────────────────

        private async Task AlocarRecursoAsync(SocketSlashCommand command, string? nomeHeroi, string? nomeNode)
        {
            if (string.IsNullOrWhiteSpace(nomeHeroi))
            {
                await command.RespondAsync("Informe o nome do herói.", ephemeral: true);
                return;
            }

            if (!Enum.TryParse<TipoResourceNode>(nomeNode ?? "Campo", true, out var node))
            {
                await command.RespondAsync(
                    $"Node '{nomeNode}' inválido. Opções: {string.Join(", ", Enum.GetNames<TipoResourceNode>())}.",
                    ephemeral: true);
                return;
            }

            var herois = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
            var heroi  = herois.FirstOrDefault(h => h.Nome.Equals(nomeHeroi, StringComparison.OrdinalIgnoreCase));
            if (heroi == null)
            {
                await command.RespondAsync($"Herói '{nomeHeroi}' não encontrado.", ephemeral: true);
                return;
            }

            if (await _cidadeService.ObterCidadePorUsuarioAsync(command.User.Id) == null)
                await _cidadeService.CriarCidadeAsync("Minha Cidade", command.User.Id);

            try
            {
                await _cidadeService.AlocarRecursoAsync(command.User.Id, heroi.Id, node);
                if (ResourceNodeConfig.BaseRates.TryGetValue(node, out var rate))
                {
                    double bonus = heroi.Profissao.HasValue &&
                        ResourceNodeConfig.ProfissaoBonus.TryGetValue((node, heroi.Profissao.Value), out var b) ? b : 0.0;
                    double porHora = rate.basePorHora * (1.0 + bonus);
                    await command.RespondAsync(
                        $"**{heroi.Nome}** alocado em **{node}** → {porHora:F1} {rate.recurso}/h.",
                        ephemeral: true);
                }
                else
                {
                    await command.RespondAsync($"**{heroi.Nome}** alocado em **{node}**.", ephemeral: true);
                }
            }
            catch (InvalidOperationException ex)
            {
                await command.RespondAsync(ex.Message, ephemeral: true);
            }
        }

        // ── /cidade alocar_predio ──────────────────────────────────────────────────

        private async Task AlocarPredioAsync(SocketSlashCommand command, string? nomeHeroi, string? nomePredio, string? nomeSlot)
        {
            if (string.IsNullOrWhiteSpace(nomeHeroi))
            {
                await command.RespondAsync("Informe o nome do herói.", ephemeral: true);
                return;
            }

            if (!Enum.TryParse<TipoPredio>(nomePredio, true, out var predio))
            {
                await command.RespondAsync(
                    $"Prédio '{nomePredio}' inválido. Opções: {string.Join(", ", Enum.GetNames<TipoPredio>())}.",
                    ephemeral: true);
                return;
            }

            if (!Enum.TryParse<SlotTipo>(nomeSlot ?? "Operacao", true, out var slotTipo))
            {
                await command.RespondAsync(
                    "Slot inválido. Opções: Responsabilidade, Operacao.",
                    ephemeral: true);
                return;
            }

            var herois = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
            var heroi  = herois.FirstOrDefault(h => h.Nome.Equals(nomeHeroi, StringComparison.OrdinalIgnoreCase));
            if (heroi == null)
            {
                await command.RespondAsync($"Herói '{nomeHeroi}' não encontrado.", ephemeral: true);
                return;
            }

            var erro = await _cidadeService.AlocarSlotPredioAsync(command.User.Id, heroi.Id, predio, slotTipo);
            if (erro != null)
                await command.RespondAsync($"❌ {erro}", ephemeral: true);
            else
                await command.RespondAsync(
                    $"**{heroi.Nome}** alocado em **{predio}** como **{slotTipo}**.",
                    ephemeral: true);
        }

        // ── /cidade desalocar ──────────────────────────────────────────────────────

        private async Task DesalocarAsync(SocketSlashCommand command, string? nomeHeroi)
        {
            if (string.IsNullOrWhiteSpace(nomeHeroi))
            {
                await command.RespondAsync("Informe o nome do herói.", ephemeral: true);
                return;
            }

            var herois = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
            var heroi  = herois.FirstOrDefault(h => h.Nome.Equals(nomeHeroi, StringComparison.OrdinalIgnoreCase));
            if (heroi == null)
            {
                await command.RespondAsync($"Herói '{nomeHeroi}' não encontrado.", ephemeral: true);
                return;
            }

            var erro = await _cidadeService.DesalocarHeroiAsync(command.User.Id, heroi.Id);
            if (erro != null)
                await command.RespondAsync($"❌ {erro}", ephemeral: true);
            else
                await command.RespondAsync($"**{heroi.Nome}** foi desalocado.", ephemeral: true);
        }

        // ── /cidade construir ──────────────────────────────────────────────────────

        private async Task ConstruirAsync(SocketSlashCommand command, string? nomePredio)
        {
            if (!Enum.TryParse<TipoPredio>(nomePredio, true, out var predio))
            {
                await command.RespondAsync(
                    $"Prédio inválido. Opções: {string.Join(", ", Enum.GetNames<TipoPredio>())}.",
                    ephemeral: true);
                return;
            }

            if (!PredioConfig.CustosConstrucao.TryGetValue(predio, out var custo))
            {
                await command.RespondAsync("Prédio sem configuração de custo.", ephemeral: true);
                return;
            }

            var erro = await _cidadeService.ConstruirPredioAsync(command.User.Id, predio);
            if (erro != null)
            {
                await command.RespondAsync($"❌ {erro}", ephemeral: true);
                return;
            }

            var custoPartes = new List<string>();
            if (custo.Ouro    > 0) custoPartes.Add($"{custo.Ouro} Ouro");
            if (custo.Madeira > 0) custoPartes.Add($"{custo.Madeira} Madeira");
            if (custo.Pedra   > 0) custoPartes.Add($"{custo.Pedra} Pedra");
            if (custo.Comida  > 0) custoPartes.Add($"{custo.Comida} Comida");

            var detalhes = string.Empty;
            if (PredioConfig.Slots.TryGetValue((predio, 1), out var def))
            {
                var slots = $"{def.NumResponsabilidade} Responsável";
                if (def.NumOperacao > 0) slots += $" + {def.NumOperacao} Operadores";
                detalhes += $"\n🧑‍🔧 Slots: {slots}";
                if (def.BaseProdPorHora > 0)
                    detalhes += $"\n📦 Produção base: {def.BaseProdPorHora} {PredioConfig.RecursoProducao.GetValueOrDefault(predio, "?")}/h";
            }

            await command.RespondAsync(
                $"🏗️ **{predio}** construída! Custo: {string.Join(", ", custoPartes)}.{detalhes}",
                ephemeral: true);
        }
    }
}
