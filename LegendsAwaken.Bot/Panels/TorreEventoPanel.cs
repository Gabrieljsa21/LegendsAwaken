using Discord;
using LegendsAwaken.Application.Config;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Bot.Panels;

public static class TorreEventoPanel
{
    public static Embed CriarEmbedEscolha(TorreEvento evento, CheckpointEventoConfig config)
    {
        string expiracao = evento.ExpiraEm.HasValue
            ? FormatarExpiracao(evento.ExpiraEm.Value)
            : "Sem expiração";

        return new EmbedBuilder()
            .WithTitle($"🔀 {config.Titulo} — Checkpoint {evento.ProgressoNoCheckpoint}%")
            .WithDescription(config.Descricao)
            .WithColor(Color.DarkOrange)
            .WithFooter($"🗼 Andar {evento.AndarOrigem} | ⏳ Expira em {expiracao}")
            .Build();
    }

    public static MessageComponent CriarComponentesEscolha(TorreEvento evento, CheckpointEventoConfig config)
    {
        var builder = new ComponentBuilder();
        if (config.Opcoes == null) return builder.Build();

        foreach (var opcao in config.Opcoes)
        {
            var style = opcao.RiscoTom switch
            {
                RiscoTom.Seguro    => ButtonStyle.Success,
                RiscoTom.Arriscado => ButtonStyle.Danger,
                _                  => ButtonStyle.Secondary
            };
            builder.WithButton(opcao.TextoExibido, CustomIdFactory.EventoEscolha(evento.Id, opcao.Key), style);
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentesDesabilitados(TorreEvento evento, CheckpointEventoConfig config)
    {
        var builder = new ComponentBuilder();
        if (config.Opcoes == null) return builder.Build();

        foreach (var opcao in config.Opcoes)
        {
            var style = opcao.RiscoTom switch
            {
                RiscoTom.Seguro    => ButtonStyle.Success,
                RiscoTom.Arriscado => ButtonStyle.Danger,
                _                  => ButtonStyle.Secondary
            };
            builder.WithButton(opcao.TextoExibido, CustomIdFactory.EventoEscolha(evento.Id, opcao.Key), style, disabled: true);
        }

        return builder.Build();
    }

    public static Embed CriarEmbedResultado(TorreEvento evento, CheckpointEventoConfig config, string descricaoResultado, int progressoBonus)
    {
        var color = evento.OpcaoKey is "recuar" or "ignorar"
            ? Color.DarkGrey
            : Color.Green;

        return new EmbedBuilder()
            .WithTitle($"✅ {config.Titulo}")
            .WithDescription(descricaoResultado)
            .WithColor(color)
            .AddField("Progresso bônus", progressoBonus > 0 ? $"+{progressoBonus}%" : "Nenhum", inline: true)
            .Build();
    }

    private static string FormatarExpiracao(DateTime expiraEm)
    {
        var restante = expiraEm - DateTime.UtcNow;
        if (restante.TotalDays >= 1)
            return $"{(int)restante.TotalDays}d {restante.Hours}h";
        return $"{restante.Hours}h {restante.Minutes}m";
    }
}
