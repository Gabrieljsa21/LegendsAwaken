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
        private readonly CombatService _combatService;
        private readonly PartyService _partyService;
        private readonly CidadeService _cidadeService;
        private readonly CraftingService _craftingService;
        private readonly ArenaService _arenaService;

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
            AtributoBonusService atributoBonusService,
            CombatService combatService,
            PartyService partyService,
            CidadeService cidadeService,
            CraftingService craftingService,
            ArenaService arenaService)
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
            _combatService = combatService;
            _partyService = partyService;
            _cidadeService = cidadeService;
            _craftingService = craftingService;
            _arenaService = arenaService;
        }

        public void Initialize()
        {
            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            _client.ButtonExecuted += HandleButtonExecutedAsync;
            _client.AutocompleteExecuted += HandleAutocompleteAsync;
            _client.Ready += OnReadyAsync;
            _client.SelectMenuExecuted += HandleSelectMenuExecutedAsync;

        }

        private Task OnReadyAsync()
        {
            _logger.LogInformation("Bot está pronto!");
            return Task.CompletedTask;
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
                    {
                        var nomeHeroiTreino = command.Data.Options.FirstOrDefault(o => o.Name == "heroi")?.Value as string;
                        if (string.IsNullOrWhiteSpace(nomeHeroiTreino))
                        {
                            await command.RespondAsync("Informe o nome do herói que deseja treinar.", ephemeral: true);
                            break;
                        }
                        var heroisTreino = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
                        var heroiTreino  = heroisTreino.FirstOrDefault(h => h.Nome.Equals(nomeHeroiTreino, StringComparison.OrdinalIgnoreCase));
                        if (heroiTreino == null)
                        {
                            await command.RespondAsync($"Herói '{nomeHeroiTreino}' não encontrado.", ephemeral: true);
                            break;
                        }
                        var treinoResult = await _arenaService.TreinarAsync(command.User.Id, heroiTreino.Id);
                        if (treinoResult.Erro != null)
                        {
                            await command.RespondAsync($"❌ {treinoResult.Erro}", ephemeral: true);
                            break;
                        }
                        var treinoMsg = $"**{heroiTreino.Nome}** treinou e ganhou **{treinoResult.XpGanho} XP**!";
                        if (treinoResult.NiveisGanhos > 0)
                            treinoMsg += $" (+{treinoResult.NiveisGanhos} nível!)";
                        treinoMsg += "\n*Custo: 100 Ouro + 10 Comida | Cooldown: 4h*";
                        await command.RespondAsync(treinoMsg, ephemeral: true);
                        break;
                    }

                    case "arena":
                        await new ArenaCommand(_arenaService, _heroiService).ExecutarAsync(command);
                        break;

                    case "subir_andar":
                        await new CombatCommand(_heroiService, _combatService).ExecutarAsync(command);
                        break;

                    case "ver_heroi":
                        var nomeHeroi = command.Data.Options.FirstOrDefault(o => o.Name == "nome")?.Value as string ?? string.Empty;
                        var verHeroiCmd = new VerHeroiCommand(_heroiService);
                        await verHeroiCmd.ExecutarAsync(command, nomeHeroi);
                        break;

                    case "listar_herois":
                        var listarCmd = new ListarHeroisCommand(_heroiService);
                        await listarCmd.ExecutarAsync(command);
                        break;

                    case "cidade":
                        await new CidadeCommand(_cidadeService, _heroiService).ExecutarAsync(command);
                        break;

                    case "grupo":
                        var acao = (string)command.Data.Options.First(o => o.Name == "acao").Value;
                        var nomeHeroiGrupo = (string?)command.Data.Options.FirstOrDefault(o => o.Name == "heroi")?.Value;
                        var nomeParty = (string?)command.Data.Options.FirstOrDefault(o => o.Name == "nome_party")?.Value;

                        var parties = await _partyService.ObterPartiesUsuarioAsync(command.User.Id);

                        switch (acao)
                        {
                            case "criar":
                                if (string.IsNullOrWhiteSpace(nomeParty))
                                {
                                    await command.RespondAsync("Você precisa informar o nome da nova party.", ephemeral: true);
                                    break;
                                }

                                var jaExiste = parties.Any(p => p.Nome.Equals(nomeParty, StringComparison.OrdinalIgnoreCase));
                                if (jaExiste)
                                {
                                    await command.RespondAsync($"Você já possui uma party chamada '{nomeParty}'.", ephemeral: true);
                                    break;
                                }

                                await _partyService.CriarPartyAsync(command.User.Id, nomeParty);
                                await command.RespondAsync($"Party '{nomeParty}' criada com sucesso!", ephemeral: true);
                                break;

                            case "ver":
                                if (string.IsNullOrWhiteSpace(nomeParty))
                                {
                                    await command.RespondAsync("Você precisa informar o nome da party que deseja ver.", ephemeral: true);
                                    break;
                                }

                                var partyVer = parties.FirstOrDefault(p => p.Nome.Equals(nomeParty, StringComparison.OrdinalIgnoreCase));
                                if (partyVer == null)
                                {
                                    await command.RespondAsync($"Party '{nomeParty}' não encontrada.", ephemeral: true);
                                    break;
                                }

                                if (partyVer.Membros == null || !partyVer.Membros.Any())
                                {
                                    await command.RespondAsync($"A party '{partyVer.Nome}' está vazia.", ephemeral: true);
                                    break;
                                }

                                var listaMembros = string.Join("\n", partyVer.Membros.Select(m => m.Heroi.Nome));
                                await command.RespondAsync($"**Party '{partyVer.Nome}'**:\n{listaMembros}", ephemeral: true);
                                break;

                            case "adicionar":
                            case "remover":
                                if (string.IsNullOrWhiteSpace(nomeParty))
                                {
                                    await command.RespondAsync("Você precisa informar o nome da party.", ephemeral: true);
                                    break;
                                }

                                var partyEdit = parties.FirstOrDefault(p => p.Nome.Equals(nomeParty, StringComparison.OrdinalIgnoreCase));
                                if (partyEdit == null)
                                {
                                    await command.RespondAsync($"Party '{nomeParty}' não encontrada.", ephemeral: true);
                                    break;
                                }

                                if (string.IsNullOrWhiteSpace(nomeHeroiGrupo))
                                {
                                    await command.RespondAsync("Você precisa informar o nome do herói.", ephemeral: true);
                                    break;
                                }

                                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
                                var heroi = herois.FirstOrDefault(h => h.Nome.Equals(nomeHeroiGrupo, StringComparison.OrdinalIgnoreCase));

                                if (heroi == null)
                                {
                                    await command.RespondAsync($"Herói '{nomeHeroiGrupo}' não encontrado.", ephemeral: true);
                                    break;
                                }

                                if (acao == "adicionar")
                                {
                                    await _partyService.AdicionarHeroiAsync(partyEdit.Id, heroi.Id);
                                    await command.RespondAsync($"Herói '{heroi.Nome}' adicionado à party '{partyEdit.Nome}'!", ephemeral: true);
                                }
                                else
                                {
                                    await _partyService.RemoverHeroiAsync(partyEdit.Id, heroi.Id);
                                    await command.RespondAsync($"Herói '{heroi.Nome}' removido da party '{partyEdit.Nome}'.", ephemeral: true);
                                }
                                break;

                            default:
                                await command.RespondAsync("Ação inválida para o comando /grupo.", ephemeral: true);
                                break;
                        }

                        break;


                    case "crafting":
                        await new CraftingCommand(_craftingService, _heroiService).ExecutarAsync(command);
                        break;

                    case "heroi_equipar":
                        var nomeHeroiEq = command.Data.Options.FirstOrDefault(o => o.Name == "heroi")?.Value as string;
                        var itemIdStr   = command.Data.Options.FirstOrDefault(o => o.Name == "item_id")?.Value as string;
                        if (string.IsNullOrWhiteSpace(nomeHeroiEq) || string.IsNullOrWhiteSpace(itemIdStr))
                        {
                            await command.RespondAsync("Informe o heroi e o ID do item.", ephemeral: true);
                            break;
                        }
                        if (!Guid.TryParse(itemIdStr, out var itemGuid))
                        {
                            await command.RespondAsync("ID do item invalido.", ephemeral: true);
                            break;
                        }
                        var heroisEq = await _heroiService.ObterHeroisPorUsuarioAsync(command.User.Id);
                        var heroiEq = heroisEq.FirstOrDefault(h => h.Nome.Equals(nomeHeroiEq, StringComparison.OrdinalIgnoreCase));
                        if (heroiEq == null)
                        {
                            await command.RespondAsync($"Heroi '{nomeHeroiEq}' nao encontrado.", ephemeral: true);
                            break;
                        }
                        var erroEquipar = await _heroiService.EquiparItemAsync(heroiEq.Id, itemGuid, command.User.Id);
                        await command.RespondAsync(
                            erroEquipar == null ? $"Item equipado em **{heroiEq.Nome}** com sucesso!" : $"Erro: {erroEquipar}",
                            ephemeral: true);
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
            else if(auto.Data.CommandName == "party_add" || auto.Data.CommandName == "party_remove")
            {
                var userId = auto.User.Id;
                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);
                var query = auto.Data.Options.First().Value as string ?? "";
                var sugestoes = herois
                    .Where(h => h.Nome.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new AutocompleteResult(h.Nome, h.Id.ToString()))
                    .Take(25);
                await auto.RespondAsync(sugestoes);
            }
            else if (auto.Data.CommandName == "grupo" && auto.Data.Options.Any(o => o.Name == "heroi"))
            {
                var userId = auto.User.Id;
                var query = auto.Data.Options.First(o => o.Name == "heroi").Value as string ?? "";
                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);
                var sugestoes = herois
                    .Where(h => h.Nome.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new AutocompleteResult(h.Nome, h.Nome))
                    .Take(25);
                await auto.RespondAsync(sugestoes);
            }

            else if (auto.Data.CommandName == "treinar" && auto.Data.Options.Any(o => o.Name == "heroi"))
            {
                var userId = auto.User.Id;
                var query  = auto.Data.Options.First(o => o.Name == "heroi").Value as string ?? "";
                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);
                var sugestoes = herois
                    .Where(h => h.Nome.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new AutocompleteResult(h.Nome, h.Nome))
                    .Take(25);
                await auto.RespondAsync(sugestoes);
            }

            else if ((auto.Data.CommandName == "cidade") && auto.Data.Options.Any(o => o.Name == "heroi"))
            {
                var userId = auto.User.Id;
                var query = auto.Data.Options.FirstOrDefault(o => o.Name == "heroi")?.Value as string ?? "";
                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);
                var sugestoes = herois
                    .Where(h => h.Nome.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new AutocompleteResult(h.Nome, h.Nome))
                    .Take(25);
                await auto.RespondAsync(sugestoes);
            }

            else if (auto.Data.CommandName == "heroi_equipar" && auto.Data.Options.Any(o => o.Name == "heroi"))
            {
                var userId = auto.User.Id;
                var query = auto.Data.Options.First(o => o.Name == "heroi").Value as string ?? "";
                var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);
                var sugestoes = herois
                    .Where(h => h.Nome.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new AutocompleteResult(h.Nome, h.Nome))
                    .Take(25);
                await auto.RespondAsync(sugestoes);
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

            // ————— Roll (se for o botão de roll existente) —————
            if (parts[0] == "roll" && parts.Length == 3)
            {
                int quantidade = int.Parse(parts[1]);
                string bannerId = parts[2];
                await new InvocarCommand(
                    _heroiService,
                    _bannerHistoricoService,
                    _bannerService,
                    _gachaService,
                    _racaService
                ).ExecutarRollAsync(comp);
                return;
            }

            // ————— Paginação do listar_herois —————
            if (parts[0] == "listar" && parts.Length == 3)
            {
                // parts = [ "listar", "<page>", "<raridade?>" ]
                int page = int.Parse(parts[1]);
                int? raridade = string.IsNullOrEmpty(parts[2]) || parts[2] == "0" ? null : (int?)int.Parse(parts[2]);

                var todos = await _heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);

                var filtrados = todos
                    .Where(h => raridade == null || (int)h.Raridade == raridade)
                    .ToList();

                var builder = new ListarHeroisCommand(_heroiService);
                var embed = builder.BuildEmbed(filtrados, page);

                int pageCount = (int)Math.Ceiling(filtrados.Count / (double)ListarHeroisCommand.PageSize);

                var comps = new ComponentBuilder()
                    .WithButton("◀️", $"listar|{page - 1}|{(raridade ?? 0)}", ButtonStyle.Secondary, disabled: page <= 1)
                    .WithButton("▶️", $"listar|{page + 1}|{(raridade ?? 0)}", ButtonStyle.Secondary, disabled: page >= pageCount)
                    .Build();

                await comp.UpdateAsync(msg =>
                {
                    msg.Embed = embed;
                    msg.Components = comps;
                });
            }
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

        public async Task SetupCommandsAsync()
        {
            var guild = _client.GetGuild(_guildId);
            if (guild == null)
                return;

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

                new SlashCommandBuilder()
                    .WithName("treinar")
                    .WithDescription("Treina um herói na Arena (custo: 100 Ouro + 10 Comida, cooldown 4h)")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("heroi")
                        .WithDescription("Nome do herói")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true)),

                new SlashCommandBuilder()
                    .WithName("arena")
                    .WithDescription("Comandos da Arena")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("acao")
                        .WithDescription("O que fazer na Arena")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("desafio", "desafio")),

                new SlashCommandBuilder()
                    .WithName("subir_andar")
                    .WithDescription("Inicia um combate automático contra os inimigos do próximo andar"),

                new SlashCommandBuilder()
                    .WithName("ver_heroi")
                    .WithDescription("Ver detalhes de um herói")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("nome")
                        .WithDescription("Nome do herói")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true)),

                new SlashCommandBuilder()
                    .WithName("listar_herois")
                    .WithDescription("Mostra seus heróis, com paginação e filtros")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("raridade")
                        .WithDescription("Filtrar por raridade")
                        .WithType(ApplicationCommandOptionType.Integer)
                        .WithRequired(false)),

                new SlashCommandBuilder()
                    .WithName("cidade")
                    .WithDescription("Gerencia sua cidade")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("acao")
                        .WithDescription("O que deseja fazer")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("ver", "ver")
                        .AddChoice("coletar", "coletar")
                        .AddChoice("construir", "construir")
                        .AddChoice("alocar_recurso", "alocar_recurso")
                        .AddChoice("alocar_predio", "alocar_predio")
                        .AddChoice("desalocar", "desalocar"))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("heroi")
                        .WithDescription("Nome do herói")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("predio")
                        .WithDescription("Prédio (construir / alocar_predio): Fazenda, Serraria, Mina, Forja, Arena, Guilda")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("Fazenda",  "Fazenda")
                        .AddChoice("Serraria", "Serraria")
                        .AddChoice("Mina",     "Mina")
                        .AddChoice("Forja",    "Forja")
                        .AddChoice("Arena",    "Arena")
                        .AddChoice("Guilda",   "Guilda"))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("node")
                        .WithDescription("Node de recurso (alocar_recurso): Campo, Floresta, Mina, Prado")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("Campo",    "Campo")
                        .AddChoice("Floresta", "Floresta")
                        .AddChoice("Mina",     "Mina")
                        .AddChoice("Prado",    "Prado"))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("slot_tipo")
                        .WithDescription("Tipo de slot (alocar_predio): Responsabilidade ou Operacao")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("Responsabilidade", "Responsabilidade")
                        .AddChoice("Operacao", "Operacao")),

                new SlashCommandBuilder()
                    .WithName("grupo")
                    .WithDescription("Gerencia os grupos de heróis")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("acao")
                        .WithDescription("Escolha o que fazer com o grupo")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("criar", "criar")
                        .AddChoice("ver", "ver")
                        .AddChoice("adicionar", "adicionar")
                        .AddChoice("remover", "remover"))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("nome_party")
                        .WithDescription("Nome do grupo (obrigatório para criar/adicionar/remover)")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("heroi")
                        .WithDescription("Nome do herói (obrigatório para adicionar/remover)")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true)),

                new SlashCommandBuilder()
                    .WithName("crafting")
                    .WithDescription("Sistema de crafting de itens")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("acao")
                        .WithDescription("listar receitas ou fazer item")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .AddChoice("listar", "listar")
                        .AddChoice("fazer", "fazer"))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("receita")
                        .WithDescription("ID da receita (obrigatorio para 'fazer')")
                        .WithRequired(false)
                        .WithType(ApplicationCommandOptionType.String)),

                new SlashCommandBuilder()
                    .WithName("heroi_equipar")
                    .WithDescription("Equipa um item craftado em um heroi")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("heroi")
                        .WithDescription("Nome do heroi")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithAutocomplete(true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("item_id")
                        .WithDescription("ID do item (obtido ao craftar)")
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String))
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


    }
}
