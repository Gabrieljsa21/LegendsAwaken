using Discord;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Fragmento;
using System;
using System.Collections.Generic;
using System.Linq;
using StatusExploracao = LegendsAwaken.Domain.Enum.StatusExploracao;

namespace LegendsAwaken.Bot.Panels;

public static class TorrePanel
{
    public static Embed CriarEmbed(TorreAndar andar, Bioma? bioma = null, TorreExploracao? exploracao = null,
        IReadOnlyList<string>? flagsAtivas = null)
    {
        var (tipoStr, cor) = andar.Tipo switch
        {
            TipoAndar.BossDificil => ("⚔️⚔️⚔️ Boss Difícil", Color.DarkRed),
            TipoAndar.BossMedio   => ("⚔️⚔️ Boss Médio",     Color.Orange),
            TipoAndar.BossFacil   => ("⚔️ Boss Fácil",       Color.Gold),
            _                     => ("🏰 Normal",            Color.Blue),
        };

        var statusStr = andar.ObjetivoCumprido ? "✅ Concluído" : "⏳ Pendente";

        var builder = new EmbedBuilder()
            .WithTitle($"🗼 Torre — Andar {andar.Numero}")
            .WithColor(cor)
            .AddField("Tipo",        tipoStr,                           inline: true)
            .AddField("Dificuldade", $"⚡ {andar.NivelDificuldade}",   inline: true)
            .AddField("Status",      statusStr,                         inline: true);

        if (bioma != null)
            builder.AddField("Bioma", $"🗺️ {bioma.Nome}", inline: false);

        if (exploracao != null)
        {
            string expStr;
            if (exploracao.Status == StatusExploracao.Ativa)
            {
                var eta = ComputarETA(exploracao);
                expStr = $"⚔️ Em progresso: {(int)exploracao.Progresso}% | ⏱️ ETA: {eta}";
            }
            else
            {
                expStr = exploracao.Status switch
                {
                    StatusExploracao.Concluida => "✅ Concluída — colete as recompensas!",
                    StatusExploracao.Falha     => "💀 Derrota — colete o loot dos checkpoints!",
                    _                          => ""
                };
            }
            if (!string.IsNullOrEmpty(expStr))
                builder.AddField("Exploração", expStr, inline: false);
        }

        var arcoDef = TorreArcoConfig.ObterArcoPorAndar(andar.Numero);
        if (arcoDef is not null)
        {
            var andarDef = TorreArcoConfig.ObterAndar(andar.Numero);
            builder.AddField($"📖 {arcoDef.Nome} — Andar {andar.Numero}",
                andarDef?.NarrativaDisplay ?? "...");

            if (andarDef?.ObjetivoSecundario is { } sec)
            {
                var cumprido = flagsAtivas?.Contains(sec.FlagNome) == true;
                var secTexto = cumprido
                    ? $"{sec.Descricao}\n✅ *Efeito aplicado: {sec.EfeitoDescricao}*"
                    : sec.Descricao;
                builder.AddField("🎯 Objetivo Secundário", secTexto);
            }
        }

        if (flagsAtivas is { Count: > 0 })
        {
            var listaFlags = string.Join(", ", flagsAtivas.Select(f => $"`{f}`"));
            builder.AddField("🏴 Flags do Arco", listaFlags);
        }

        builder.WithFooter("Use 🔍 Investigar para ver a chance de vitória antes de explorar");

        return builder.Build();
    }

    public static MessageComponent CriarComponentes(bool temExploracao = false)
    {
        var cb = new ComponentBuilder();

        if (temExploracao)
            cb.WithButton("⚔️ Ver Exploração", "torre_explorar",        ButtonStyle.Primary);
        else
            cb.WithButton("🔍 Investigar",     "torre_investigar",      ButtonStyle.Secondary)
              .WithButton("⚔️ Explorar",        "torre_explorar",        ButtonStyle.Success);

        cb.WithButton("🗺️ Bioma",         "torre_bioma",         ButtonStyle.Secondary)
          .WithButton("🏭 Modo Operação", "torre_modo_operacao", ButtonStyle.Primary)
          .WithButton("🔄",               "torre_atualizar",     ButtonStyle.Secondary);

        return cb.Build();
    }

    private static string ComputarETA(TorreExploracao exp)
    {
        if (exp.Progresso <= 0) return "Calculando...";
        var elapsedMin = (DateTime.UtcNow - exp.IniciadoEm).TotalMinutes;
        if (elapsedMin < 0.5) return "Calculando...";
        var ratePerMin   = exp.Progresso / elapsedMin;
        var remainingMin = (100.0 - exp.Progresso) / ratePerMin;
        if (remainingMin <= 0) return "iminente";
        return remainingMin >= 60
            ? $"~{(int)(remainingMin / 60)}h {(int)(remainingMin % 60)}m"
            : $"~{(int)Math.Ceiling(remainingMin)}m";
    }
}
