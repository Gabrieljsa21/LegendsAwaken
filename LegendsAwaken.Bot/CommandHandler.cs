using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot
{
    /// <summary>
    /// Responsável por registrar e tratar comandos de barra (slash commands) no Discord.
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
        /// <param name="logger">Logger para registrar informações e erros.</param>
        /// <param name="guildId">ID do servidor onde os comandos serão registrados.</param>
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
            // Conecta os eventos que lidam com slash commands e com a inicialização do bot
            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            _client.Ready += OnReadyAsync;
        }

        /// <summary>
        /// Evento disparado quando o bot está pronto (conectado).
        /// Registra os comandos de barra no servidor específico.
        /// </summary>
        private async Task OnReadyAsync()
        {
            _logger.LogInformation("Bot está pronto!");

            var guild = _client.GetGuild(_guildId);

            if (guild != null)
            {
                var commands = new[]
                {
                    new SlashCommandBuilder().WithName("invocar").WithDescription("Invocar um herói do gacha"),
                    new SlashCommandBuilder().WithName("treinar").WithDescription("Treinar um herói"),
                    new SlashCommandBuilder().WithName("subir_andar").WithDescription("Subir um andar da torre"),
                    new SlashCommandBuilder().WithName("ver_heroi").WithDescription("Ver detalhes de um herói")
                    // Você pode adicionar mais comandos aqui conforme necessário
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
        /// Lida com comandos recebidos pelo usuário.
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
                        await command.RespondAsync("Comando não reconhecido.", ephemeral: true);
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
            // Aqui será implementada a lógica do gacha
            return command.RespondAsync("Invocação iniciada! (Implementar lógica)", ephemeral: true);
        }

        /// <summary>
        /// Lida com o comando /treinar.
        /// </summary>
        private Task HandleTreinarCommand(SocketSlashCommand command)
        {
            // Aqui será implementada a lógica de treino
            return command.RespondAsync("Treinamento iniciado! (Implementar lógica)", ephemeral: true);
        }

        /// <summary>
        /// Lida com o comando /subir_andar.
        /// </summary>
        private Task HandleSubirAndarCommand(SocketSlashCommand command)
        {
            // Aqui será implementada a lógica de subir na torre
            return command.RespondAsync("Subindo andar! (Implementar lógica)", ephemeral: true);
        }

        /// <summary>
        /// Lida com o comando /ver_heroi.
        /// </summary>
        private Task HandleVerHeroiCommand(SocketSlashCommand command)
        {
            // Aqui será implementada a lógica de exibição do herói
            return command.RespondAsync("Mostrando herói! (Implementar lógica)", ephemeral: true);
        }
    }
}
