using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LegendsAwaken.Application.Interfaces;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Commands;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Interactions;
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
        private readonly CidadeBoosterService _cidadeBoosterService;
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
        private readonly TorreExploracaoService _torreExploracaoService;
        private readonly SustentoService _sustentoService;
        private readonly RecursoService _recursoService;
        private readonly JogadorItemService _jogadorItemService;
        private readonly R2ImageService _r2ImageService;
        private readonly InteractionRouter _interactionRouter;

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
            CidadeBoosterService cidadeBoosterService,
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
            TorreExploracaoService torreExploracaoService,
            SustentoService sustentoService,
            RecursoService recursoService,
            JogadorItemService jogadorItemService,
            R2ImageService r2ImageService,
            InteractionRouter interactionRouter)
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
            _cidadeBoosterService = cidadeBoosterService;
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
            _torreExploracaoService = torreExploracaoService;
            _sustentoService = sustentoService;
            _recursoService = recursoService;
            _jogadorItemService = jogadorItemService;
            _r2ImageService = r2ImageService;
            _interactionRouter = interactionRouter;
        }

        public void Initialize()
        {
            _client.SlashCommandExecuted += cmd   => { _ = HandleSlashCommandAsync(cmd);   return Task.CompletedTask; };
            _client.ButtonExecuted       += comp  => { _ = HandleButtonExecutedAsync(comp); return Task.CompletedTask; };
            _client.SelectMenuExecuted   += comp  => { _ = HandleButtonExecutedAsync(comp); return Task.CompletedTask; };
            _client.AutocompleteExecuted += auto  => { _ = HandleAutocompleteAsync(auto);  return Task.CompletedTask; };
            _client.ModalSubmitted       += modal => { _ = HandleModalSubmittedAsync(modal); return Task.CompletedTask; };
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
                        await new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _cidadeService, _torreExploracaoService, _partyService, _recursoService, _jogadorItemService, _logger).ExecutarAsync(command);
                        break;

                    case "herois":
                        await new HeroisCommand(_heroiService, _sustentoService, _partyService, _logger, _heroiConfigRepo, _r2ImageService).ExecutarAsync(command);
                        break;

                    case "cidade":
                        await new CidadeCommand(_cidadeService, _heroiService, _cidadeBoosterService, _logger).ExecutarAsync(command);
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
                        await new BiomaCommand(_biomeService, _torreRepository, _fragmentoRepo, _heroiConfigRepo).ExecutarAsync(command);
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


        private async Task HandleModalSubmittedAsync(SocketModal modal)
        {
            _logger.LogInformation("[Modal] CustomId={CustomId} User={User}", modal.Data.CustomId, modal.User.Username);
            var parts = modal.Data.CustomId.Split('|');
            try
            {
                if (parts[0] == "grupos_nome_modal" && parts.Length >= 2 && Guid.TryParse(parts[1], out var partyId))
                {
                    await new GruposCommand(_partyService, _heroiService, _logger).HandleNomeModalAsync(modal, partyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Modal] Exceção não tratada. CustomId={CustomId}", modal.Data.CustomId);
                try { await modal.RespondAsync("❌ Erro interno.", ephemeral: true); } catch { }
            }
        }

        public async Task HandleButtonExecutedAsync(SocketMessageComponent comp)
        {
            _logger.LogInformation("[Interação] CustomId={CustomId} Tipo={Tipo} User={User}",
                comp.Data.CustomId, comp.Data.Type, comp.User.Username);

            // Route new-style ':' customIds to registered handlers
            try
            {
                if (await _interactionRouter.TryRouteAsync(comp))
                    return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Router] Exceção não tratada. CustomId={CustomId} User={User}",
                    comp.Data.CustomId, comp.User.Username);
                try { await comp.RespondAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                return;
            }

            // Global cancel — kept outside router as a universal escape hatch for all confirmation panels
            if (comp.Data.CustomId == "global:cancelar")
            {
                await comp.UpdateAsync(m =>
                {
                    m.Content = "Ação cancelada.";
                    m.Embed = null;
                    m.Components = new ComponentBuilder().Build();
                });
                return;
            }

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
            if (parts[0] == "herois_ver" || parts[0] == "herois_atualizar" || parts[0] == "herois_toggle_inativo" ||
                parts[0] == "herois_colecao" || parts[0] == "herois_treinar" || parts[0] == "herois_treinar_heroi" ||
                parts[0] == "herois_grupos")
            {
                var heroisCmd = new HeroisCommand(_heroiService, _sustentoService, _partyService, _logger, _heroiConfigRepo, _r2ImageService);
                try
                {
                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "herois_ver")
                        { await heroisCmd.HandleVerDetalhesAsync(comp); return; }

                    if (parts[0] == "herois_atualizar")
                        { await heroisCmd.HandleAtualizarAsync(comp); return; }

                    if (parts[0] == "herois_toggle_inativo" && parts.Length >= 2 && Guid.TryParse(parts[1], out var toggleHeroiId))
                        { await heroisCmd.HandleToggleInativoAsync(comp, toggleHeroiId); return; }

                    if (parts[0] == "herois_grupos")
                        { await heroisCmd.HandleGruposAsync(comp); return; }

                    if (parts[0] == "herois_colecao")
                    {
                        await new ColecaoCommand(_heroiConfigRepo, _heroiDesbloqueadoRepo, _fragmentoRepo, _recruitmentService)
                            .MostrarAsync(comp);
                        return;
                    }

                    if (parts[0] == "herois_treinar")
                    {
                        await comp.DeferAsync(ephemeral: true);
                        var hList = await _heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
                        if (!hList.Any()) { await comp.FollowupAsync("Nenhum herói disponível para treinar.", ephemeral: true); return; }
                        var sel = new SelectMenuBuilder()
                            .WithCustomId("herois_treinar_heroi")
                            .WithPlaceholder("Escolha o herói para treinar...")
                            .WithMinValues(1).WithMaxValues(1);
                        foreach (var h in hList.OrderBy(h => h.Nome).Take(25))
                            sel.AddOption(h.Nome, h.Id.ToString(), $"Nv {h.Nivel}");
                        await comp.FollowupAsync(
                            "⚔️ Treinar herói (custo: 100 Ouro + 10 Comida | cooldown: 4h):",
                            components: new ComponentBuilder().WithSelectMenu(sel).Build(),
                            ephemeral: true);
                        return;
                    }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "herois_treinar_heroi")
                    {
                        var heroiIdStr = comp.Data.Values.FirstOrDefault();
                        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var treinarId))
                        { await comp.UpdateAsync(m => { m.Content = "Herói inválido."; m.Components = null; }); return; }
                        var hAll  = await _heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
                        var heroi = hAll.FirstOrDefault(h => h.Id == treinarId);
                        var res   = await _arenaService.TreinarAsync(comp.User.Id, treinarId);
                        if (res.Erro != null)
                        { await comp.UpdateAsync(m => { m.Content = $"❌ {res.Erro}"; m.Components = null; }); return; }
                        var msg = $"✅ **{heroi?.Nome ?? "Herói"}** treinou e ganhou **{res.XpGanho} XP**!";
                        if (res.NiveisGanhos > 0) msg += $" (+{res.NiveisGanhos} nível!)";
                        msg += "\n*Custo: 100 Ouro + 10 Comida | Cooldown: 4h*";
                        await comp.UpdateAsync(m => { m.Content = msg; m.Components = null; });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Herois] Exceção não tratada. CustomId={CustomId} User={User}",
                        comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // ————— Grupos buttons/select menus —————
            if (parts[0].StartsWith("grupos_"))
            {
                var gruposCmd = new GruposCommand(_partyService, _heroiService, _logger);
                try
                {
                    if (parts[0] == "grupos_lista")
                        { await gruposCmd.HandleListaAsync(comp); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "grupos_ver_sel")
                        { await gruposCmd.HandleVerSelAsync(comp); return; }

                    if (parts[0] == "grupos_criar")
                        { await gruposCmd.HandleCriarAsync(comp); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "grupos_criar_sel")
                        { await gruposCmd.HandleCriarSelAsync(comp); return; }

                    if (parts[0] == "grupos_recomendado")
                        { await gruposCmd.HandleRecomendadoAsync(comp); return; }

                    if (parts[0] == "grupos_ver" && parts.Length >= 2 && Guid.TryParse(parts[1], out var verPartyId))
                        { await gruposCmd.HandleVerAsync(comp, verPartyId); return; }

                    if (parts[0] == "grupos_add_sel" && parts.Length >= 2 && Guid.TryParse(parts[1], out var addSelPartyId))
                        { await gruposCmd.HandleAddSelAsync(comp, addSelPartyId); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "grupos_add" && parts.Length >= 2 && Guid.TryParse(parts[1], out var addPartyId))
                        { await gruposCmd.HandleAddAsync(comp, addPartyId); return; }

                    if (parts[0] == "grupos_rem_sel" && parts.Length >= 2 && Guid.TryParse(parts[1], out var remSelPartyId))
                        { await gruposCmd.HandleRemSelAsync(comp, remSelPartyId); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "grupos_rem" && parts.Length >= 2 && Guid.TryParse(parts[1], out var remPartyId))
                        { await gruposCmd.HandleRemAsync(comp, remPartyId); return; }

                    if (parts[0] == "grupos_nome_toggle" && parts.Length >= 2 && Guid.TryParse(parts[1], out var togglePartyId))
                        { await gruposCmd.HandleNomeToggleAsync(comp, togglePartyId); return; }

                    if (parts[0] == "grupos_nome_editar" && parts.Length >= 2 && Guid.TryParse(parts[1], out var editarPartyId))
                        { await gruposCmd.HandleNomeEditarAsync(comp, editarPartyId); return; }

                    if (parts[0] == "grupos_deletar" && parts.Length >= 2 && Guid.TryParse(parts[1], out var deletarPartyId))
                        { await gruposCmd.HandleDeletarAsync(comp, deletarPartyId); return; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Grupos] Exceção não tratada. CustomId={CustomId} User={User}",
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

            // ————— Bioma buttons/select menus —————
            if (parts[0] == "bioma_atualizar" || parts[0] == "torre_bioma" ||
                parts[0] == "bioma_lista" || parts[0] == "bioma_fechar" ||
                parts[0] == "bioma_sel" || parts[0] == "bioma_ver_colecao")
            {
                var biomaCmd = new BiomaCommand(_biomeService, _torreRepository, _fragmentoRepo, _heroiConfigRepo);
                try
                {
                    if (parts[0] == "torre_bioma" || parts[0] == "bioma_atualizar")
                        { await biomaCmd.MostrarListaAsync(comp); return; }

                    if (parts[0] == "bioma_lista")
                        { await biomaCmd.VoltarListaAsync(comp); return; }

                    if (parts[0] == "bioma_fechar")
                        { await comp.UpdateAsync(m => { m.Content = "Fechado."; m.Embed = null; m.Components = new ComponentBuilder().Build(); }); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "bioma_sel")
                        { await biomaCmd.MostrarDetalheAsync(comp); return; }

                    if (parts[0] == "bioma_ver_colecao")
                    {
                        await new ColecaoCommand(_heroiConfigRepo, _heroiDesbloqueadoRepo, _fragmentoRepo, _recruitmentService)
                            .MostrarAsync(comp);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Bioma] Exceção não tratada. CustomId={CustomId} User={User}",
                        comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // ————— Torre Modo Operação buttons/select menus —————
            if (parts[0] == "torre_modo_operacao" || parts[0].StartsWith("torre_op_"))
            {
                var torreCmd = new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _cidadeService, _torreExploracaoService, _partyService, _recursoService, _jogadorItemService, _logger);
                try
                {
                    if (parts[0] == "torre_modo_operacao")
                        { await torreCmd.HandleModoOperacaoAsync(comp); return; }

                    if (parts[0] == "torre_op_alocar")
                        { await torreCmd.HandleOpAlocarAsync(comp); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "torre_op_andar_sel")
                        { await torreCmd.HandleOpAndarSelAsync(comp); return; }

                    if (parts[0] == "torre_op_coletar_todas")
                        { await torreCmd.HandleOpColetarTodasAsync(comp); return; }

                    if (parts[0] == "torre_op_remover_sel")
                        { await torreCmd.HandleOpRemoverSelAsync(comp); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "torre_op_remover_andar_sel")
                        { await torreCmd.HandleOpRemoverAndarSelAsync(comp); return; }

                    if (parts[0] == "torre_op_fechar")
                        { await torreCmd.HandleOpFecharAsync(comp); return; }
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
                await new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _cidadeService, _torreExploracaoService, _partyService, _recursoService, _jogadorItemService, _logger).HandleAtualizarAsync(comp);
                return;
            }

            if (parts[0] == "torre_avancar")
            {
                try
                {
                    await new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _cidadeService, _torreExploracaoService, _partyService, _recursoService, _jogadorItemService, _logger).HandleAvancarAsync(comp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Torre] Exceção não tratada. CustomId={CustomId} User={User}", comp.Data.CustomId, comp.User.Username);
                    try { await comp.FollowupAsync("❌ Erro interno. Tente novamente.", ephemeral: true); } catch { }
                }
                return;
            }

            // ————— Torre Exploração buttons/select menus —————
            if (parts[0] == "torre_investigar" || parts[0] == "torre_explorar" ||
                parts[0] == "torre_explorar_confirmar" || parts[0].StartsWith("torre_exp_"))
            {
                var expCmd = new TorreCommand(_torreService, _heroiService, _biomeService, _torreOperacaoService, _cidadeService, _torreExploracaoService, _partyService, _recursoService, _jogadorItemService, _logger);
                try
                {
                    if (parts[0] == "torre_investigar")
                        { await expCmd.HandleInvestigarAsync(comp); return; }

                    if (parts[0] == "torre_explorar")
                        { await expCmd.HandleExplorarAsync(comp); return; }

                    if (parts[0] == "torre_explorar_confirmar" && parts.Length >= 3)
                        { await expCmd.HandleExplorarConfirmarAsync(comp, parts[1], parts[2]); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "torre_exp_grupo_sel")
                        { await expCmd.HandleExpGrupoSelAsync(comp); return; }

                    if (comp.Data.Type == ComponentType.SelectMenu && parts[0] == "torre_exp_booster_sel")
                        { await expCmd.HandleExpBoosterSelAsync(comp); return; }

                    if (parts[0] == "torre_exp_atualizar")
                        { await expCmd.HandleExpAtualizarAsync(comp); return; }

                    if (parts[0] == "torre_exp_coletar")
                        { await expCmd.HandleExpColetarAsync(comp); return; }

                    if (parts[0] == "torre_exp_cancelar")
                        { await expCmd.HandleExpCancelarAsync(comp); return; }

                    if (parts[0] == "torre_exp_cancelar_sel")
                        { await expCmd.HandleExpCancelarSelAsync(comp); return; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TorreExp] Exceção não tratada. CustomId={CustomId} User={User}",
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
            };

            // Remove commands that no longer exist in our list
            try
            {
                var existingCmds = await guild.GetApplicationCommandsAsync();
                var desiredNames  = commands.Select(c => c.Name).ToHashSet();
                foreach (var cmd in existingCmds.Where(c => !desiredNames.Contains(c.Name)))
                {
                    await cmd.DeleteAsync();
                    _logger.LogInformation("Comando /{CommandName} removido.", cmd.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar comandos antigos.");
            }

            foreach (var cmd in commands)
            {
                try
                {
                    await guild.CreateApplicationCommandAsync(cmd.Build());
                    _logger.LogInformation("Comando /{CommandName} registrado.", cmd.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao registrar comando /{CommandName}", cmd.Name);
                }
            }
        }


    }
}
