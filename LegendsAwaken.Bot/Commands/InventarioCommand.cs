using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class InventarioCommand(HeroiService heroiService, ILogger? logger = null)
{
    private void Log(string msg)                  => logger?.LogInformation("[Inventario] {Msg}", msg);
    private void LogErr(Exception ex, string ctx) => logger?.LogError(ex, "[Inventario] ERRO em {Ctx}", ctx);

    // ── /inventario — painel público ─────────────────────────────────────────────

    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        Log($"/inventario — user={command.User.Username}");
        await command.DeferAsync();
        var (embed, comps) = await BuildPanelAsync(command.User.Id);
        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── Select: inventario_item ──────────────────────────────────────────────────

    public async Task HandleGerenciarItemAsync(SocketMessageComponent comp)
    {
        var itemIdStr = comp.Data.Values.FirstOrDefault();
        Log($"GerenciarItem — itemId={itemIdStr} user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        if (itemIdStr == null || !Guid.TryParse(itemIdStr, out var itemId))
        {
            await comp.FollowupAsync("Item inválido.", ephemeral: true);
            return;
        }

        try
        {
            var itens = await heroiService.ObterItensAsync(comp.User.Id);
            var item  = itens.FirstOrDefault(i => i.Id == itemId);
            if (item == null) { await comp.FollowupAsync("Item não encontrado.", ephemeral: true); return; }

            string? heroiNome = null;
            if (item.EstaEquipado && item.HeroiEquipadoId.HasValue)
            {
                var herois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
                heroiNome = herois.FirstOrDefault(h => h.Id == item.HeroiEquipadoId.Value)?.Nome;
            }

            await comp.FollowupAsync(
                embed: InventarioPanel.CriarEmbedItem(item, heroiNome),
                components: InventarioPanel.CriarComponentesItem(itemId, item.EstaEquipado),
                ephemeral: true);
        }
        catch (Exception ex)
        {
            LogErr(ex, $"GerenciarItem itemId={itemId}");
            await comp.FollowupAsync("❌ Erro interno.", ephemeral: true);
        }
    }

    // ── Button: inventario_iniciar_equipar|{itemId} ──────────────────────────────

    public async Task HandleIniciarEquiparAsync(SocketMessageComponent comp, Guid itemId)
    {
        Log($"IniciarEquipar — itemId={itemId} user={comp.User.Username}");

        var herois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
        if (!herois.Any())
        {
            await comp.UpdateAsync(m => { m.Content = "Você não tem heróis para equipar."; m.Components = null; });
            return;
        }

        var select = new SelectMenuBuilder()
            .WithCustomId($"inventario_equipar_heroi|{itemId}")
            .WithPlaceholder("Escolha o herói...")
            .WithMinValues(1).WithMaxValues(1);

        foreach (var h in herois.Take(25))
            select.AddOption($"{h.Nome} (Nv {h.Nivel})", h.Id.ToString(), h.Raca.ToString());

        await comp.UpdateAsync(m =>
        {
            m.Content    = "Escolha o herói para equipar:";
            m.Embed      = null;
            m.Components = new ComponentBuilder().WithSelectMenu(select).Build();
        });
    }

    // ── Select: inventario_equipar_heroi|{itemId} ────────────────────────────────

    public async Task HandleEquiparHeroiAsync(SocketMessageComponent comp, Guid itemId)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"EquiparHeroi — itemId={itemId} heroiId={heroiIdStr} user={comp.User.Username}");

        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var heroiId))
        {
            await comp.UpdateAsync(m => { m.Content = "Herói inválido."; m.Components = null; });
            return;
        }

        try
        {
            var erro = await heroiService.EquiparItemAsync(heroiId, itemId, comp.User.Id);
            await comp.UpdateAsync(m =>
            {
                m.Content    = erro == null ? "✅ Item equipado com sucesso!" : $"❌ {erro}";
                m.Components = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"EquiparHeroi itemId={itemId} heroiId={heroiId}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao equipar."; m.Components = null; });
        }
    }

    // ── Button: inventario_desequipar|{itemId} ───────────────────────────────────

    public async Task HandleDesequiparAsync(SocketMessageComponent comp, Guid itemId)
    {
        Log($"Desequipar — itemId={itemId} user={comp.User.Username}");
        try
        {
            var erro = await heroiService.DesequiparItemAsync(itemId, comp.User.Id);
            await comp.UpdateAsync(m =>
            {
                m.Content    = erro == null ? "✅ Item desequipado!" : $"❌ {erro}";
                m.Embed      = null;
                m.Components = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"Desequipar itemId={itemId}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao desequipar."; m.Embed = null; m.Components = null; });
        }
    }

    // ── Atualizar painel ─────────────────────────────────────────────────────────

    public async Task HandleAtualizarAsync(SocketMessageComponent comp)
    {
        Log($"Atualizar — user={comp.User.Username}");
        await comp.DeferAsync();
        var (embed, comps) = await BuildPanelAsync(comp.User.Id);
        await comp.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── Helper ───────────────────────────────────────────────────────────────────

    private async Task<(Embed embed, MessageComponent comps)> BuildPanelAsync(ulong usuarioId)
    {
        var itens  = await heroiService.ObterItensAsync(usuarioId);
        var herois = await heroiService.ObterHeroisPorUsuarioAsync(usuarioId);
        var heroiNomes = herois.ToDictionary(h => h.Id, h => h.Nome);
        return (InventarioPanel.CriarEmbed(itens, heroiNomes), InventarioPanel.CriarComponentes(itens));
    }
}
