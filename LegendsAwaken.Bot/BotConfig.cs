namespace LegendsAwaken.Bot
{
    public class BotConfig
    {
        /// <summary>
        /// Token do bot para autentica��o com a API do Discord.
        /// </summary>
        public string DiscordToken { get; set; } = string.Empty;

        /// <summary>
        /// Prefixo dos comandos, se usar comandos com prefixo (ex: "!" ou "/").
        /// No seu caso, est� usando slash commands, ent�o pode deixar vazio ou "/".
        /// </summary>
        public string CommandPrefix { get; set; } = "/";

        /// <summary>
        /// Id do canal padr�o para logs ou mensagens administrativas.
        /// </summary>
        public ulong? LogChannelId { get; set; }

        /// <summary>
        /// Id do canal onde comandos ser�o aceitos prioritariamente (opcional).
        /// </summary>
        public ulong? CommandChannelId { get; set; }

        /// <summary>
        /// Tempo de timeout para respostas interativas (em segundos).
        /// </summary>
        public int InteractionTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Idioma padr�o do bot (ex: "pt-BR", "en-US").
        /// </summary>
        public string DefaultLanguage { get; set; } = "pt-BR";

        // Outras configura��es personalizadas podem ser adicionadas conforme necess�rio.
    }
}
