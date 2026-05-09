using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Config;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Services;

public class NotificacaoService(
    DiscordSocketClient discord,
    IUsuarioNotificacaoRepository notifRepo,
    ILogger<NotificacaoService> logger) : INotificacaoService
{
    public async Task NotificarEventoCheckpointAsync(ulong channelId, ulong discordUserId, TorreEvento evento)
    {
        var config = CheckpointEventoCatalog.Todos.FirstOrDefault(c => c.Key == evento.EventoKey);

        // For Maior events: post full event panel in exploration channel so player can respond inline
        if (config != null && channelId != 0)
        {
            try
            {
                var canal = discord.GetChannel(channelId) as IMessageChannel
                         ?? await discord.GetChannelAsync(channelId) as IMessageChannel;
                if (canal != null)
                {
                    var embed = TorreEventoPanel.CriarEmbedEscolha(evento, config);
                    var comps = TorreEventoPanel.CriarComponentesEscolha(evento, config);
                    await canal.SendMessageAsync($"<@{discordUserId}>", embed: embed, components: comps);
                    return;
                }
                logger.LogWarning("Canal {ChannelId} não encontrado para notificação de evento.", channelId);
            }
            catch (Discord.Net.HttpException ex)
            {
                logger.LogWarning(ex, "Falha ao postar evento no canal {ChannelId}, tentando DM.", channelId);
            }
        }

        // Fallback: DM — also respects user notification preferences
        var prefs = await notifRepo.ObterAsync(discordUserId);
        if (prefs is { NotificacoesAtivas: false }) return;
        if (prefs?.Preferencia == Domain.Enum.NotificacaoPreferencia.Desativado) return;
        if (prefs?.Preferencia == Domain.Enum.NotificacaoPreferencia.ApenasConclusao) return;

        try
        {
            var user = await discord.GetUserAsync(discordUserId);
            if (user == null) return;
            var dm = await user.CreateDMChannelAsync();

            if (config != null)
            {
                var embed = TorreEventoPanel.CriarEmbedEscolha(evento, config);
                var comps = TorreEventoPanel.CriarComponentesEscolha(evento, config);
                await dm.SendMessageAsync(embed: embed, components: comps);
            }
            else
            {
                var fallbackEmbed = new EmbedBuilder()
                    .WithTitle($"⚠️ Exploração pausada — Checkpoint {evento.ProgressoNoCheckpoint}%")
                    .WithDescription($"Use `/torre` para ver suas opções.")
                    .WithColor(Color.Orange)
                    .WithFooter($"🗼 Andar {evento.AndarOrigem}")
                    .Build();
                await dm.SendMessageAsync(embed: fallbackEmbed);
            }
        }
        catch (Discord.Net.HttpException ex)
        {
            logger.LogWarning(ex, "Falha ao notificar usuário {DiscordId} via DM.", discordUserId);
        }
    }

    public async Task NotificarEventoMenorAsync(
        ulong channelId, ulong discordUserId,
        TorreEvento evento, string titulo, string descricaoResultado, int progressoBonus)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"⚡ {titulo} — Checkpoint {evento.ProgressoNoCheckpoint}%")
            .WithDescription(descricaoResultado)
            .WithColor(Color.Blue)
            .AddField("Progresso bônus", progressoBonus > 0 ? $"+{progressoBonus}%" : "Nenhum", inline: true)
            .WithFooter($"🗼 Andar {evento.AndarOrigem} | Resolvido automaticamente")
            .Build();

        if (channelId != 0)
        {
            try
            {
                var canal = discord.GetChannel(channelId) as IMessageChannel
                         ?? await discord.GetChannelAsync(channelId) as IMessageChannel;
                if (canal != null)
                {
                    await canal.SendMessageAsync($"<@{discordUserId}>", embed: embed);
                    return;
                }
                logger.LogWarning("Canal {ChannelId} não encontrado para evento menor.", channelId);
            }
            catch (Discord.Net.HttpException ex)
            {
                logger.LogWarning(ex, "Falha ao postar evento menor no canal {ChannelId}, tentando DM.", channelId);
            }
        }

        try
        {
            var user = await discord.GetUserAsync(discordUserId);
            if (user == null) return;
            var dm = await user.CreateDMChannelAsync();
            await dm.SendMessageAsync(embed: embed);
        }
        catch (Discord.Net.HttpException ex)
        {
            logger.LogWarning(ex, "Falha ao notificar evento menor via DM para {DiscordId}.", discordUserId);
        }
    }
}
