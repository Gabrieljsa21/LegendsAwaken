using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Bot.Commands;

public class HeroisCommand(HeroiService heroiService, SustentoService sustentoService, ILogger? logger = null)
{
    private void Log(string msg)                  => logger?.LogInformation("[Herois] {Msg}", msg);
    private void LogErr(Exception ex, string ctx) => logger?.LogError(ex, "[Herois] ERRO em {Ctx}", ctx);

    // ── /herois — painel público ─────────────────────────────────────────────────

    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        Log($"/herois — user={command.User.Username}");
        await command.DeferAsync();
        var (embed, comps) = await BuildPanelAsync(command.User.Id);
        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── Select: herois_ver ───────────────────────────────────────────────────────

    public async Task HandleVerDetalhesAsync(SocketMessageComponent comp)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"VerDetalhes — heroiId={heroiIdStr} user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var heroiId))
        {
            await comp.FollowupAsync("Herói inválido.", ephemeral: true);
            return;
        }

        try
        {
            var herois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
            var heroi  = herois.FirstOrDefault(h => h.Id == heroiId);

            if (heroi == null)
            {
                await comp.FollowupAsync("Herói não encontrado.", ephemeral: true);
                return;
            }

            await comp.FollowupAsync(
                embed: HeroisPanel.CriarEmbedDetalhe(heroi),
                components: HeroisPanel.CriarComponentesDetalhe(heroi),
                ephemeral: true);
        }
        catch (Exception ex)
        {
            LogErr(ex, $"VerDetalhes heroiId={heroiId}");
            await comp.FollowupAsync("❌ Erro interno ao buscar detalhes.", ephemeral: true);
        }
    }

    // ── Toggle Inativo ───────────────────────────────────────────────────────────

    public async Task HandleToggleInativoAsync(SocketMessageComponent comp, Guid heroiId)
    {
        Log($"ToggleInativo — heroiId={heroiId} user={comp.User.Username}");
        try
        {
            await sustentoService.ToggleInativoAsync(heroiId);
            var herois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
            var heroi  = herois.FirstOrDefault(h => h.Id == heroiId);

            if (heroi == null)
            {
                await comp.UpdateAsync(m => { m.Content = "Herói não encontrado."; m.Components = null; });
                return;
            }

            await comp.UpdateAsync(m =>
            {
                m.Embed      = HeroisPanel.CriarEmbedDetalhe(heroi);
                m.Components = HeroisPanel.CriarComponentesDetalhe(heroi);
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"ToggleInativo heroiId={heroiId}");
            await comp.FollowupAsync("❌ Erro interno ao alterar sustento.", ephemeral: true);
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
        var herois = await heroiService.ObterHeroisPorUsuarioAsync(usuarioId);
        return (HeroisPanel.CriarEmbed(herois), HeroisPanel.CriarComponentes(herois));
    }
}
