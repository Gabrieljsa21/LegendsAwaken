using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Commands;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using LegendsAwaken.Domain.Interfaces;
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
        private readonly UsuarioService _usuarioService;
        private readonly RacaService _racaService;
        private readonly AtributoBonusService _atributoBonusService;
        private readonly CombatService _combatService;
        private readonly PartyService _partyService;
        private readonly CidadeService _cidadeService;
        private readonly CraftingService _craftingService;
        private readonly ArenaService _arenaService;
        private readonly IHeroiConfigRepository _heroiConfigRepo;
        private readonly IHeroiDesbloqueadoRepository _heroiDesbloqueadoRepo;
        private readonly IFragmentoRepository _fragmentoRepo;
        private readonly RecruitmentService _recruitmentService;
        private readonly BiomeService _biomeService;
        private readonly ContractService _contractService;
        private readonly IContratoRepository _contratoRepository;
        private readonly ITorreRepository _torreRepository;
        private readonly TorreService _torreService;
        private readonly TorreOperacaoService _torreOperacaoService;
        private readonly SustentoService _sustentoService;

        public CommandHandler(
            DiscordSocketClient client,
            ILogger<CommandHandler> logger,
            ulong guildId,
            HeroiService heroiService,
            GeracaoDeDadosService geracaoDeDadosService,
            UsuarioService usuarioService,
            RacaService racaService,
            AtributoBonusService atributoBonusService,
            CombatService combatService,
            PartyService partyService,
            CidadeService cidadeService,
            CraftingService craftingService,
            ArenaService arenaService,
            IHeroiConfigRepository heroiConfigRepo,
            IHeroiDesbloqueadoRepository heroiDesbloqueadoRepo,
            IFragmentoRepository fragmentoRepo,
            RecruitmentService recruitmentService,
            BiomeService biomeService,
            ContractService contractService,
            IContratoRepository contratoRepository,
            ITorreRepository torreRepository,
            TorreService torreService,
            TorreOperacaoService torreOperacaoService,
            SustentoService sustentoService)
        {
            _client = client;
            _logger = logger;
            _guildId = guildId;
            _heroiService = heroiService;
            _geracaoDeDadosService = geracaoDeDadosService;
            _usuarioService = usuarioService;
            _racaService = racaService;
            _atributoBonusService = atributoBonusService;
            _combatService = combatService;
            _partyService = partyService;
            _cidadeService = cidadeService;
            _craftingService = craftingService;
            _arenaService = arenaService;
            _heroiConfigRepo = heroiConfigRepo;
            _heroiDesbloqueadoRepo = heroiDesbloqueadoRepo;
            _fragmentoRepo = fragmentoRepo;
            _recruitmentService = recruitmentService;
            _biomeService = biomeService;
            _contractService = contractService;
            _contratoRepository = contratoRepository;
            _torreRepository = torreRepository;
            _torreService = torreService;
            _torreOperacaoService = torreOperacaoService;
            _sustentoService = sustentoService;
        }

        public void Initialize()
        {
            _client.SlashCommandExecuted += cmd  => { _ = HandleSlashCommandAsync(cmd);  return Task.CompletedTask; };
            _client.ButtonExecuted       += comp => { _ = HandleButtonExecutedAsync(comp); return Task.CompletedTask; };
            _client.SelectMenuExecuted   += comp => { _ = HandleButtonExecutedAsync(comp); return Task.CompletedTask; };
            _client.AutocompleteExecuted += auto => { _ = HandleAutocompleteAsync(auto);  return Task.CompletedTask; };
            _client.Ready += OnReadyAsync;
        }

        private Task OnReadyAsync()
        {
            _logger.LogInformation("Bot está pronto!");
            return Task.CompletedTask;
        }

        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            _logger.LogInformation("Comando /{CommandName} de {Username}", command.CommandName, command.User.Username);
            await _usuarioService.ObterOuCriarAsync(command.User);
            await _sustentoService.ProcessarAsync(command.User.Id);

            try
            {
                switch (command.CommandName)
                {
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

                    case "torre":
                    case "subir_andar":
                        await new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _logger).ExecutarAsync(command);
                        break;

                    case "herois":
                        await new HeroisCommand(_heroiService, _sustentoService, _logger).ExecutarAsync(command);
                        break;

                    case "cidade":
                        await new CidadeCommand(_cidadeService, _heroiService, _logger).ExecutarAsync(command);
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

                    case "colecao":
                        await new ColecaoCommand(_heroiConfigRepo, _heroiDesbloqueadoRepo, _fragmentoRepo, _recruitmentService)
                            .ExecutarAsync(command);
                        break;

                    case "bioma":
                        await new BiomaCommand(_biomeService, _torreRepository).ExecutarAsync(command);
                        break;

                    case "contrato":
                        await new ContratoCommand(_contractService, _contratoRepository).ExecutarAsync(command);
                        break;

                    case "inventario":
                        await new InventarioCommand(_heroiService, _logger).ExecutarAsync(command);
                        break;

                    default:
                        await command.RespondAsync("Comando não reconhecido.", ephemeral: true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar comando /{CommandName}", command.CommandName);
                await command.RespondAsync("Ocorreu um erro ao processar seu comando.", ephemeral: true);
            }
        }

        private async Task HandleAutocompleteAsync(SocketAutocompleteInteraction auto)
        {
            if(auto.Data.CommandName == "party_add" || auto.Data.CommandName == "party_remove")
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

        }


        public async Task HandleButtonExecutedAsync(SocketMessageComponent comp)
        {
            _logger.LogInformation("[Interação] CustomId={CustomId} Tipo={Tipo} User={User}",
                comp.Data.CustomId, comp.Data.Type, comp.User.Username);

            var parts = comp.Data.CustomId.Split('|');

            // ————— Inventário buttons/select menus —————
            if (parts[0].StartsWith("inventario_"))
            {
                var inventarioCmd = new InventarioCommand(_heroiService, _logger);
                try
                {
                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "inventario_item")
                        { await inventarioCmd.HandleGerenciarItemAsync(comp); return; }

                    if (parts[0] == "inventario_iniciar_equipar" && parts.Length >= 2 && Guid.TryParse(parts[1], out var itemEquiparId))
                        { await inventarioCmd.HandleIniciarEquiparAsync(comp, itemEquiparId); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "inventario_equipar_heroi" && parts.Length >= 2 && Guid.TryParse(parts[1], out var itemEqHeroiId))
                        { await inventarioCmd.HandleEquiparHeroiAsync(comp, itemEqHeroiId); return; }

                    if (parts[0] == "inventario_desequipar" && parts.Length >= 2 && Guid.TryParse(parts[1], out var itemDesequiparId))
                        { await inventarioCmd.HandleDesequiparAsync(comp, itemDesequiparId); return; }

                    if (parts[0] == "inventario_atualizar")
                        { await inventarioCmd.HandleAtualizarAsync(comp); return; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Inventario] Exceção não tratada. CustomId={CustomId} User={User}",
                        comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // ————— Heróis buttons/select menus —————
            if (parts[0] == "herois_ver" || parts[0] == "herois_atualizar" || parts[0] == "herois_toggle_inativo")
            {
                var heroisCmd = new HeroisCommand(_heroiService, _sustentoService, _logger);
                try
                {
                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "herois_ver")
                        { await heroisCmd.HandleVerDetalhesAsync(comp); return; }

                    if (parts[0] == "herois_atualizar")
                        { await heroisCmd.HandleAtualizarAsync(comp); return; }

                    if (parts[0] == "herois_toggle_inativo" && parts.Length >= 2 && Guid.TryParse(parts[1], out var toggleHeroiId))
                        { await heroisCmd.HandleToggleInativoAsync(comp, toggleHeroiId); return; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Herois] Exceção não tratada. CustomId={CustomId} User={User}",
                        comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // Select menu: recrutar heroi por fragmentos
            if (comp.Data.Type == ComponentType.SelectMenu && comp.Data.CustomId == "colecao_recrutar")
            {
                var heroiIdStr = comp.Data.Values.FirstOrDefault();
                if (heroiIdStr is not null && Guid.TryParse(heroiIdStr, out var heroiId))
                    await new ColecaoCommand(_heroiConfigRepo, _heroiDesbloqueadoRepo, _fragmentoRepo, _recruitmentService)
                        .HandleRecrutarAsync(comp, heroiId);
                return;
            }

            // Select menu: mudar arquetipo de contrato
            if (comp.Data.Type == ComponentType.SelectMenu && comp.Data.CustomId == "contrato_arquetipo")
            {
                var arquetipoStr = comp.Data.Values.FirstOrDefault();
                if (arquetipoStr is not null && Enum.TryParse<Profissao>(arquetipoStr, out var arquetipo))
                    await new ContratoCommand(_contractService, _contratoRepository)
                        .HandleArquetipoAsync(comp, arquetipo);
                return;
            }

            // Button: bioma -> ver colecao
            if (comp.Data.CustomId == "bioma_ver_colecao")
            {
                await new ColecaoCommand(_heroiConfigRepo, _heroiDesbloqueadoRepo, _fragmentoRepo, _recruitmentService)
                    .MostrarAsync(comp);
                return;
            }

            // Button: bioma -> contratos
            if (comp.Data.CustomId == "bioma_contratos")
            {
                await new ContratoCommand(_contractService, _contratoRepository)
                    .MostrarAsync(comp);
                return;
            }

            // Button: remover foco nomeado
            if (comp.Data.CustomId == "contrato_remover_nomeado")
            {
                await comp.DeferAsync(ephemeral: true);
                var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
                var nomeado = await _contratoRepository.ObterAtivoAsync(usuarioId, LegendsAwaken.Domain.Enum.TipoContrato.Nomeado);
                if (nomeado is not null)
                    await _contratoRepository.DesativarAsync(nomeado.Id);
                await comp.FollowupAsync(nomeado is not null ? "Foco nomeado removido." : "Nenhum foco nomeado ativo.", ephemeral: true);
                return;
            }

            // ————— Torre Modo Operação buttons/select menus —————
            if (parts[0] == "torre_modo_operacao" || parts[0].StartsWith("torre_op_"))
            {
                var torreCmd = new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _logger);
                try
                {
                    if (parts[0] == "torre_modo_operacao")
                        { await torreCmd.HandleModoOperacaoAsync(comp); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "torre_op_andar")
                        { await torreCmd.HandleOpAndarAsync(comp); return; }

                    if (parts[0] == "torre_op_objetivo" && parts.Length >= 3 && int.TryParse(parts[1], out var opAndarObj))
                        { await torreCmd.HandleOpObjetivoAsync(comp, opAndarObj, parts[2]); return; }

                    if (parts[0] == "torre_op_risco" && parts.Length >= 4 && int.TryParse(parts[1], out var opAndarRisco))
                        { await torreCmd.HandleOpRiscoAsync(comp, opAndarRisco, parts[2], parts[3]); return; }

                    if (parts[0] == "torre_op_coletar")
                        { await torreCmd.HandleOpColetarAsync(comp); return; }

                    if (parts[0] == "torre_op_cancelar_ativo")
                        { await torreCmd.HandleOpCancelarAtivoAsync(comp); return; }

                    if (parts[0] == "torre_op_cancelar")
                        { await torreCmd.HandleOpCancelarAsync(comp); return; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TorreOp] Exceção não tratada. CustomId={CustomId} User={User}",
                        comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // ————— Torre buttons —————
            if (parts[0] == "torre_atualizar")
            {
                await new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _logger).HandleAtualizarAsync(comp);
                return;
            }

            if (parts[0] == "torre_avancar")
            {
                try
                {
                    await new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _logger).HandleAvancarAsync(comp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Torre] Exceção não tratada. CustomId={CustomId} User={User}", comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // ————— Cidade buttons/select menus —————
            if (parts[0].StartsWith("cidade_"))
            {
                var cidadeCmd = new CidadeCommand(_cidadeService, _heroiService, _logger);
                try
                {
                    if (parts[0] == "cidade_coletar")
                        { await cidadeCmd.HandleColetarAsync(comp); return; }

                    if (parts[0] == "cidade_alocar_node")
                        { await cidadeCmd.HandleAlocarNodeAsync(comp); return; }

                    if (parts[0] == "cidade_alocar_heroi_para_node")
                        { await cidadeCmd.HandleHeroiParaNodeAsync(comp); return; }

                    if (parts[0] == "cidade_node_para_heroi" && parts.Length >= 2 && Guid.TryParse(parts[1], out var heroiNodeId))
                        { await cidadeCmd.HandleNodeParaHeroiAsync(comp, heroiNodeId); return; }

                    if (parts[0] == "cidade_alocar_predio")
                        { await cidadeCmd.HandleAlocarPredioAsync(comp); return; }

                    if (parts[0] == "cidade_alocar_heroi_para_predio")
                        { await cidadeCmd.HandleHeroiParaPredioAsync(comp); return; }

                    if (parts[0] == "cidade_predio_para_heroi" && parts.Length >= 2 && Guid.TryParse(parts[1], out var heroiPredioId))
                        { await cidadeCmd.HandlePredioParaHeroiAsync(comp, heroiPredioId); return; }

                    if (parts[0] == "cidade_desalocar")
                        { await cidadeCmd.HandleDesalocarAsync(comp); return; }

                    if (parts[0] == "cidade_desalocar_heroi")
                        { await cidadeCmd.HandleDesalocarHeroiAsync(comp); return; }

                    if (parts[0] == "cidade_construir")
                        { await cidadeCmd.HandleConstruirAsync(comp); return; }

                    if (parts[0] == "cidade_construir_predio")
                        { await cidadeCmd.HandleConstruirPredioAsync(comp); return; }

                    if (parts[0] == "cidade_atualizar")
                        { await cidadeCmd.HandleAtualizarAsync(comp); return; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Cidade] Exceção não tratada. CustomId={CustomId} User={User}",
                        comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }
        }


        public async Task SetupCommandsAsync()
        {
            var guild = _client.GetGuild(_guildId);
            if (guild == null)
                return;

            var commands = new[]
            {
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
                    .WithName("torre")
                    .WithDescription("Abre o painel da Torre — veja seu andar atual e avance"),

                new SlashCommandBuilder()
                    .WithName("herois")
                    .WithDescription("Abre o painel com seus heróis"),

                new SlashCommandBuilder()
                    .WithName("cidade")
                    .WithDescription("Abre o painel da sua cidade"),

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
                    .WithName("inventario")
                    .WithDescription("Abre o painel do seu inventário de itens"),

                new SlashCommandBuilder()
                    .WithName("colecao")
                    .WithDescription("Ver sua colecao de herois e progresso de fragmentos"),

                new SlashCommandBuilder()
                    .WithName("bioma")
                    .WithDescription("Ver o bioma atual da Torre com herois disponiveis e contratos"),

                new SlashCommandBuilder()
                    .WithName("contrato")
                    .WithDescription("Gerenciar contratos de drop: arquetipo e foco nomeado")
            };

            foreach (var cmd in commands)
            {
                try
                {
                    await guild.CreateApplicationCommandAsync(cmd.Build());
                    _logger.LogInformation("Comando /{CommandName} registrado no servidor.", cmd.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao registrar comando /{CommandName}", cmd.Name);
                }
            }
        }


    }
}
