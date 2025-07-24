using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Commands;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot
{
    /// <summary>
    /// Responsável por registrar e tratar comandos de barra (slash commands) e componentes no Discord.
    /// </summary>
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<CommandHandler> _logger;
        private readonly ulong _guildId;
        private readonly HeroiService _heroiService;
        private readonly GeracaoDeDadosService _geracaoDeDadosService;
        private readonly BannerService _bannerService;
        private readonly BannerHistoricoService _bannerHistoricoService;
        private readonly UsuarioService _usuarioService;
        private readonly GachaService _gachaService;
        private readonly RacaService _racaService;
        private readonly AtributoBonusService _atributoBonusService;

        public CommandHandler(
            DiscordSocketClient client,
            ILogger<CommandHandler> logger,
            ulong guildId,
            HeroiService heroiService,
            GeracaoDeDadosService geracaoDeDadosService,
            BannerService bannerService,
            BannerHistoricoService bannerHistoricoService,
            UsuarioService usuarioService,
            GachaService gachaService,
            RacaService racaService,
            AtributoBonusService atributoBonusService)
        {
            _client = client;
            _logger = logger;
            _guildId = guildId;
            _heroiService = heroiService;
            _geracaoDeDadosService = geracaoDeDadosService;
            _bannerService = bannerService;
            _bannerHistoricoService = bannerHistoricoService;
            _usuarioService = usuarioService;
            _gachaService = gachaService;
            _racaService = racaService;
            _atributoBonusService = atributoBonusService;
        }

        public void Initialize()
        {
            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            _client.ButtonExecuted += HandleButtonExecutedAsync;
            _client.AutocompleteExecuted += HandleAutocompleteAsync;
            _client.Ready += OnReadyAsync;
            _client.SelectMenuExecuted += HandleSelectMenuExecutedAsync;

        }

        private async Task OnReadyAsync()
        {
            _logger.LogInformation("Bot está pronto!");
            var guild = _client.GetGuild(_guildId);
            if (guild == null) return;

            var choices = _bannerService.ObterTodosBanners()
                .Select(b => new ApplicationCommandOptionChoiceProperties
                {
                    Name = b.Nome,
                    Value = b.Id
                })
                .ToList();

            var commands = new[]
            {
                new SlashCommandBuilder()
                    .WithName("banners")
                    .WithDescription("Mostra todos os banners disponíveis e seu pity"),

                new SlashCommandBuilder()
                    .WithName("invocar")
                    .WithDescription("Invoca em um banner específico")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("banner")
                        .WithDescription("ID do banner")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true)),

                new SlashCommandBuilder().WithName("treinar").WithDescription("Treinar um herói"),
                new SlashCommandBuilder().WithName("subir_andar").WithDescription("Subir um andar da torre"),
                new SlashCommandBuilder()
                    .WithName("ver_heroi")
                    .WithDescription("Ver detalhes de um herói")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("nome")
                        .WithDescription("Nome do herói")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true))

            };

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

        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            _logger.LogInformation($"Comando recebido: /{command.CommandName} de {command.User.Username}");
            await _usuarioService.ObterOuCriarAsync(command.User);

            try
            {
                switch (command.CommandName)
                {
                    case "invocar":
                        await new InvocarCommand(
                            _heroiService,
                            _bannerHistoricoService,
                            _bannerService,
                            _gachaService,
                            _racaService)
                        .ExecutarAsync(command);
                        break;

                    case "banners":
                        await new BannerCommand(_bannerService, _bannerHistoricoService)
                            .ExecutarAsync(command);
                        break;

                    case "treinar":
                        await command.RespondAsync("Treinamento iniciado! (Implementar lógica)", ephemeral: true);
                        break;

                    case "subir_andar":
                        await command.RespondAsync("Subindo andar! (Implementar lógica)", ephemeral: true);
                        break;

                    case "ver_heroi":
                        var nomeHeroi = (string)command.Data.Options.FirstOrDefault(o => o.Name == "nome")?.Value;
                        var verHeroiCmd = new VerHeroiCommand(_heroiService);
                        await verHeroiCmd.ExecutarAsync(command, nomeHeroi);
                        break;



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

        private async Task HandleAutocompleteAsync(SocketAutocompleteInteraction auto)
        {
            if (auto.Data.CommandName == "invocar" && auto.Data.Options.First().Name == "banner")
            {
                var query = auto.Data.Options.First().Value as string ?? string.Empty;
                var suggestions = _bannerService.ObterTodosBanners()
                    .Select(b => b.Id)
                    .Where(id => id.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(id => new AutocompleteResult(id, id))
                    .Take(25);

                await auto.RespondAsync(suggestions);
            }
            else if (auto.Data.CommandName == "ver_heroi" && auto.Data.Options.First().Name == "nome")
            {
                var userId = auto.User.Id;

                var query = auto.Data.Options.First().Value as string ?? string.Empty;

                // Buscar heróis do usuário que começam com o texto digitado
                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);

                var sugestões = herois
                    .Where(h => h.Nome.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new AutocompleteResult(h.Nome, h.Nome))
                    .Take(25);

                await auto.RespondAsync(sugestões);
            }
        }


        public async Task HandleButtonExecutedAsync(SocketMessageComponent comp)
        {
            var parts = comp.Data.CustomId.Split('|');
            if (parts.Length != 3 || parts[0] != "roll")
                return;

            int quantidade = int.Parse(parts[1]);
            string bannerId = parts[2];

            var invocar = new InvocarCommand(
                _heroiService,
                _bannerHistoricoService,
                _bannerService,
                _gachaService,
                _racaService);

            await invocar.ExecutarRollAsync(comp);
        }

        private async Task HandleSelectMenuExecutedAsync(SocketMessageComponent comp)
        {
            if (comp.Data.CustomId != "select_banner_roll") return;

            var selectedBannerId = comp.Data.Values.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(selectedBannerId))
            {
                await comp.RespondAsync("Nenhum banner selecionado.", ephemeral: true);
                return;
            }

            var banner = _bannerService.ObterBannerPorId(selectedBannerId);
            if (banner == null)
            {
                await comp.RespondAsync("Banner não encontrado.", ephemeral: true);
                return;
            }

            // ✅ Reutiliza o mesmo método já criado para exibir detalhes + botões
            await new InvocarCommand(
                _heroiService,
                _bannerHistoricoService,
                _bannerService,
                _gachaService,
                _racaService)
            .EnviarMenuInvocacao(comp, banner);
        }

    }
}
