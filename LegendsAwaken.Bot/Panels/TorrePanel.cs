using Discord;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Fragmento;

namespace LegendsAwaken.Bot.Panels;

public static class TorrePanel
{
    public static Embed CriarEmbed(TorreAndar andar, Bioma? bioma = null)
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

        builder.WithFooter("Poder do time = soma dos níveis × 5");

        return builder.Build();
    }

    public static MessageComponent CriarComponentes()
    {
        return new ComponentBuilder()
            .WithButton("⚔️ Avançar Andar",  "torre_avancar",         ButtonStyle.Success)
            .WithButton("🏭 Modo Operação",   "torre_modo_operacao",   ButtonStyle.Primary)
            .WithButton("🔄",                 "torre_atualizar",       ButtonStyle.Secondary)
            .Build();
    }
}
