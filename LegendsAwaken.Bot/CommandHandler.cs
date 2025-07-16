using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot
{
    /// <summary>
    /// Respons�vel por registrar e tratar comandos de barra (slash commands) no Discord.
    /// </summary>
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<CommandHandler> _logger;
        private readonly ulong _guildId;

        /// <summary>
        /// Construtor da classe CommandHandler.
        /// </summary>
        /// <param name="client">Cliente do Discord usado para registrar comandos e escutar eventos.</param>
        /// <param name="logger">Logger para registrar informa��es e erros.</param>
        /// <param name="guildId">ID do servidor onde os comandos ser�o registrados.</param>
        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, ulong guildId)
        {
            _client = client;
            _logger = logger;
            _guildId = guildId;
        }

        /// <summary>
        /// Inicializa o handler conectando eventos do Discord.
        /// </summary>
        public void Initialize()
        {
            // Conecta os eventos que lidam com slash commands e com a inicializa��o do bot
            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            _client.Ready += OnReadyAsync;
        }

        /// <summary>
        /// Evento disparado quando o bot est� pronto (conectado).
        /// Registra os comandos de barra no servidor espec�fico.
        /// </summary>
        private async Task OnReadyAsync()
        {
            _logger.LogInformation("Bot est� pronto!");

            var guild = _client.GetGuild(_guildId);

            if (guild != null)
            {
                var commands = new[]
                {
                    new SlashCommandBuilder().WithName("invocar").WithDescription("Invocar um her�i do gacha"),
                    new SlashCommandBuilder().WithName("treinar").WithDescription("Treinar um her�i"),
                    new SlashCommandBuilder().WithName("subir_andar").WithDescription("Subir um andar da torre"),
                    new SlashCommandBuilder().WithName("ver_heroi").WithDescription("Ver detalhes de um her�i")
                    // Voc� pode adicionar mais comandos aqui conforme necess�rio
                };

                // Registra cada comando no servidor
                foreach (var cmd in commands)
                {
                    try
                    {
                        await guild.CreateApplicationCommandAsync(cmd.Build());
                        _logger.LogInformation($"Comando /{cmd.Name} registrado no servidor.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao registrar comando /{cmd.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Lida com comandos recebidos pelo usu�rio.
        /// Encaminha para o handler correto com base no nome do comando.
        /// </summary>
        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            _logger.LogInformation($"Comando recebido: /{command.CommandName} de {command.User.Username}");

            try
            {
                switch (command.CommandName)
                {
                    case "invocar":
                        await HandleInvocarCommand(command);
                        break;

                    case "treinar":
                        await HandleTreinarCommand(command);
                        break;

                    case "subir_andar":
                        await HandleSubirAndarCommand(command);
                        break;

                    case "ver_heroi":
                        await HandleVerHeroiCommand(command);
                        break;

                    // Outros comandos podem ser adicionados aqui

                    default:
                        await command.RespondAsync("Comando n�o reconhecido.", ephemeral: true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar comando /{command.CommandName}");
                await command.RespondAsync("Ocorreu um erro ao processar seu comando.", ephemeral: true);
            }
        }

        /// <summary>
        /// Lida com o comando /invocar.
        /// </summary>
        private Task HandleInvocarCommand(SocketSlashCommand command)
        {
            // Aqui ser� implementada a l�gica do gacha
            return command.RespondAsync("Invoca��o iniciada! (Implementar l�gica)", ephemeral: true);
        }

        /// <summary>
        /// Lida com o comando /treinar.
        /// </summary>
        private Task HandleTreinarCommand(SocketSlashCommand command)
        {
            // Aqui ser� implementada a l�gica de treino
            return command.RespondAsync("Treinamento iniciado! (Implementar l�gica)", ephemeral: true);
        }

        /// <summary>
        /// Lida com o comando /subir_andar.
        /// </summary>
        private Task HandleSubirAndarCommand(SocketSlashCommand command)
        {
            // Aqui ser� implementada a l�gica de subir na torre
            return command.RespondAsync("Subindo andar! (Implementar l�gica)", ephemeral: true);
        }

        /// <summary>
        /// Lida com o comando /ver_heroi.
        /// </summary>
        private Task HandleVerHeroiCommand(SocketSlashCommand command)
        {
            // Aqui ser� implementada a l�gica de exibi��o do her�i
            return command.RespondAsync("Mostrando her�i! (Implementar l�gica)", ephemeral: true);
        }
    }
}
