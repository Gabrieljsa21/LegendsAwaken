using Discord;
using Discord.WebSocket;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Services;

public class NotificacaoService(
    DiscordSocketClient discord,
    IUsuarioNotificacaoRepository notifRepo,
    ILogger<NotificacaoService> logger) : INotificacaoService
{
    public async Task NotificarEventoCheckpointAsync(ulong discordUserId, TorreEvento evento)
    {
        var prefs = await notifRepo.ObterAsync(discordUserId);

        if (prefs is { NotificacoesAtivas: false }) return;
        if (prefs?.Preferencia == Domain.Enum.NotificacaoPreferencia.Desativado) return;
        if (prefs?.Preferencia == Domain.Enum.NotificacaoPreferencia.ApenasConclusao) return;

        var embed = new EmbedBuilder()
            .WithTitle($"⚠️ Exploração pausada — Checkpoint {evento.ProgressoNoCheckpoint}%")
            .WithDescription($"**{evento.EventoKey.Replace("_", " ")}**\nUse `/torre` para ver suas opções.")
            .WithColor(Color.Orange)
            .WithFooter($"🗼 Andar {evento.AndarOrigem}")
            .Build();

        try
        {
            if (prefs?.CanalPreferido.HasValue == true)
            {
                var canal = discord.GetChannel(prefs.CanalPreferido.Value) as IMessageChannel;
                if (canal != null)
                {
                    await canal.SendMessageAsync($"<@{discordUserId}>", embed: embed);
                    return;
                }
            }

            var user = await discord.GetUserAsync(discordUserId);
            if (user != null)
            {
                var dm = await user.CreateDMChannelAsync();
                await dm.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao notificar usuário {DiscordId} — DM bloqueada ou canal inválido.", discordUserId);
        }
    }
}
