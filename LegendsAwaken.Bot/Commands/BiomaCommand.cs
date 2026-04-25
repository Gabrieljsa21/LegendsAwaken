using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class BiomaCommand(
    BiomeService biomeService,
    ITorreRepository torreRepository,
    IFragmentoRepository fragmentoRepo,
    IHeroiConfigRepository heroiConfigRepo)
{
    // ── /bioma slash command → public list ───────────────────────────────────────

    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(command.User.Id);
        var (embed, comps) = await BuildListaAsync(usuarioId);
        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── torre_bioma / bioma_atualizar → ephemeral list ───────────────────────────

    public async Task MostrarListaAsync(SocketMessageComponent comp)
    {
        await comp.DeferAsync(ephemeral: true);
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var (embed, comps) = await BuildListaAsync(usuarioId);
        await comp.FollowupAsync(embed: embed, components: comps, ephemeral: true);
    }

    // ── bioma_lista (Voltar button) → in-place update to list ───────────────────

    public async Task VoltarListaAsync(SocketMessageComponent comp)
    {
        await comp.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var (embed, comps) = await BuildListaAsync(usuarioId);
        await comp.ModifyOriginalResponseAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
    }

    // ── bioma_sel SelectMenu → in-place update to detail ────────────────────────

    public async Task MostrarDetalheAsync(SocketMessageComponent comp)
    {
        var biomaIdStr = comp.Data.Values.FirstOrDefault();
        if (!Guid.TryParse(biomaIdStr ?? "", out var biomaId))
        {
            await comp.UpdateAsync(m => { m.Content = "Bioma inválido."; m.Embed = null; m.Components = null; });
            return;
        }

        await comp.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);

        var bioma = await biomeService.ObterPorIdAsync(biomaId);
        if (bioma == null)
        {
            await comp.ModifyOriginalResponseAsync(m => { m.Content = "Bioma não encontrado."; m.Embed = null; m.Components = null; });
            return;
        }

        var andar     = await torreRepository.ObterAndarPorUsuarioAsync(usuarioId);
        int andarAtual = andar?.Numero ?? 1;

        var pool       = await biomeService.ObterPoolDoBiomaAsync(bioma.Id);
        var fragmentos = await fragmentoRepo.ListarPorUsuarioAsync(usuarioId);

        var unlockMap = new Dictionary<Guid, HeroiUnlockConfig?>();
        foreach (var entry in pool)
            unlockMap[entry.HeroiId] = await heroiConfigRepo.ObterUnlockConfigAsync(entry.HeroiId);

        var (embed, comps) = BiomaPanel.CriarDetalhe(bioma, pool, fragmentos, unlockMap, andarAtual);
        await comp.ModifyOriginalResponseAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<(Embed, MessageComponent)> BuildListaAsync(Guid usuarioId)
    {
        var andar     = await torreRepository.ObterAndarPorUsuarioAsync(usuarioId);
        int andarAtual = andar?.Numero ?? 1;
        var biomas    = await biomeService.ListarDescobertosAsync(andarAtual);

        if (biomas.Count == 0)
        {
            var emptyEmbed = new EmbedBuilder()
                .WithTitle("🗺️ Biomas")
                .WithDescription("Nenhum bioma descoberto ainda. Avance na Torre para explorar novos biomas.")
                .WithColor(Color.DarkerGrey)
                .Build();
            var emptyComps = new ComponentBuilder()
                .WithButton("✖ Fechar", "bioma_fechar", ButtonStyle.Secondary)
                .Build();
            return (emptyEmbed, emptyComps);
        }

        return BiomaPanel.CriarLista(biomas, andarAtual);
    }
}
