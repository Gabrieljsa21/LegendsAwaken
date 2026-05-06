using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Bot.Commands;

public class HeroisCommand(
    HeroiService heroiService,
    SustentoService sustentoService,
    PartyService? partyService = null,
    ILogger? logger = null,
    IHeroiConfigRepository? heroiConfigRepo = null,
    R2ImageService? r2 = null)
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

            // Tenta buscar a imagem do herói no R2
            Stream? imageStream = null;
            if (r2 != null && heroiConfigRepo != null)
            {
                var config = await heroiConfigRepo.ObterPorNomeAsync(heroi.Nome);
                if (config?.ImageUrl is not null)
                    imageStream = await r2.GetAsync(config.ImageUrl);
            }

            if (imageStream is not null)
            {
                using var ms = new MemoryStream();
                await imageStream.CopyToAsync(ms);
                ms.Position = 0;
                await comp.FollowupWithFileAsync(
                    new FileAttachment(ms, "hero.webp"),
                    embed: HeroisPanel.CriarEmbedDetalhe(heroi, comImagem: true),
                    components: HeroisPanel.CriarComponentesDetalhe(heroi),
                    ephemeral: true);
            }
            else
            {
                await comp.FollowupAsync(
                    embed: HeroisPanel.CriarEmbedDetalhe(heroi),
                    components: HeroisPanel.CriarComponentesDetalhe(heroi),
                    ephemeral: true);
            }
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

    // ── Abrir painel de grupos ───────────────────────────────────────────────────

    public async Task HandleGruposAsync(SocketMessageComponent comp)
    {
        if (partyService == null)
        {
            await comp.DeferAsync(ephemeral: true);
            await comp.FollowupAsync("Sistema de grupos não disponível.", ephemeral: true);
            return;
        }
        await new GruposCommand(partyService, heroiService, logger).AbrirAsync(comp);
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
