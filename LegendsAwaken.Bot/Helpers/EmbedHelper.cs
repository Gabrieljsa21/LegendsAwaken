using Discord;
using System;

namespace LegendsAwaken.Bot.Helpers
{
    public static class EmbedHelper
    {
        /// <summary>
        /// Cria um embed básico com título, descrição e cor customizada.
        /// </summary>
        public static Embed BuildBasicEmbed(string title, string description, Color? color = null)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color ?? Color.DarkBlue)
                .WithTimestamp(DateTimeOffset.UtcNow);

            return embedBuilder.Build();
        }

        /// <summary>
        /// Cria um embed para mensagens de erro, em vermelho.
        /// </summary>
        public static Embed BuildErrorEmbed(string errorMessage)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Erro")
                .WithDescription(errorMessage)
                .WithColor(Color.Red)
                .WithTimestamp(DateTimeOffset.UtcNow);

            return embedBuilder.Build();
        }

        /// <summary>
        /// Cria um embed para mensagens de sucesso, em verde.
        /// </summary>
        public static Embed BuildSuccessEmbed(string message)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Sucesso")
                .WithDescription(message)
                .WithColor(Color.Green)
                .WithTimestamp(DateTimeOffset.UtcNow);

            return embedBuilder.Build();
        }
    }
}
