using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class TorreCommand(
    TorreService torreService,
    HeroiService heroiService,
    BiomeService biomeService,
    TorreOperacaoService operacaoService,
    CidadeService cidadeService,
    TorreExploracaoService exploracaoService,
    PartyService partyService,
    RecursoService recursoService,
    JogadorItemService itemService,
    ILogger? logger = null)
{
    private void Log(string msg)                  => logger?.LogInformation("[Torre] {Msg}", msg);
    private void LogErr(Exception ex, string ctx) => logger?.LogError(ex, "[Torre] ERRO em {Ctx}", ctx);

    // ── /torre ───────────────────────────────────────────────────────────────

    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        Log($"/torre — user={command.User.Username}");
        await command.DeferAsync();

        var usuarioId = DiscordIdHelper.ToGuid(command.User.Id);
        await exploracaoService.ProcessarAsync(usuarioId);

        var (embed, comps) = await BuildPanelAsync(command.User.Id);
        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });

        var opPendente = await operacaoService.VerificarPendenteAsync(usuarioId);
        if (opPendente != null)
            await command.FollowupAsync(TorreModoOperacaoPanel.CriarNotificacaoTexto(opPendente), ephemeral: true);

        var expPendente = await exploracaoService.ObterPendenteAsync(usuarioId);
        if (expPendente != null)
        {
            var msg = expPendente.Status == StatusExploracao.Concluida
                ? $"✅ Andar **{expPendente.AndarNumero}** concluído! Use **⚔️ Ver Exploração** para coletar."
                : $"💀 Derrota no Andar **{expPendente.AndarNumero}**. Use **⚔️ Ver Exploração** para coletar o loot salvo.";
            await command.FollowupAsync(msg, ephemeral: true);
        }

        Log("Painel público enviado");
    }

    // ── Button: torre_avancar (legacy) ───────────────────────────────────────

    public async Task HandleAvancarAsync(SocketMessageComponent comp)
    {
        await comp.DeferAsync(ephemeral: true);
        await comp.FollowupAsync("⚠️ Use **🔍 Investigar** e **⚔️ Explorar** no painel da Torre.", ephemeral: true);
    }

    // ── Button: torre_atualizar ──────────────────────────────────────────────

    public async Task HandleAtualizarAsync(SocketMessageComponent comp)
    {
        Log($"Atualizar — user={comp.User.Username}");
        await comp.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await exploracaoService.ProcessarAsync(usuarioId);
        var (embed, comps) = await BuildPanelAsync(comp.User.Id);
        await comp.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── Button: torre_modo_operacao ──────────────────────────────────────────────

    public async Task HandleModoOperacaoAsync(SocketMessageComponent comp)
    {
        Log($"ModoOperacao — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await operacaoService.ProcessarTodasAsync(usuarioId);

        int andarAtual = await operacaoService.ObterAndarAtualNumeroAsync(usuarioId);
        if (andarAtual <= 1)
        {
            var (e0, c0) = TorreModoOperacaoPanel.CriarSemAndares();
            await comp.FollowupAsync(embed: e0, components: c0, ephemeral: true);
            return;
        }

        var cidade  = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        var construcoes = cidade?.Construcoes ?? new System.Collections.Generic.List<Construcao>();

        var ativas     = await operacaoService.ListarAtivasAsync(usuarioId);
        var concluidas = await operacaoService.ListarConcluidasAsync(usuarioId);
        int maxSlots   = TorreOperacaoConfig.CalcularMaxSlots(construcoes);

        var estoque = await recursoService.ListarEstoqueAsync(usuarioId);
        var itens   = await itemService.ListarAsync(usuarioId);

        var todosHerois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
        var (_, horasComida, estadoSustento) = SustentoService.ObterResumo(
            cidade ?? new Domain.Entities.Cidade { Nome = "", UsuarioId = comp.User.Id, Recursos = new() },
            todosHerois);

        var (embed, comps) = TorreModoOperacaoPanel.CriarBoard(
            ativas, concluidas, andarAtual, maxSlots,
            estoque, itens, estadoSustento, horasComida);
        await comp.FollowupAsync(embed: embed, components: comps, ephemeral: true);
    }

    // ── Button: torre_op_alocar ──────────────────────────────────────────────────

    public async Task HandleOpAlocarAsync(SocketMessageComponent comp)
    {
        Log($"OpAlocar — user={comp.User.Username}");
        await comp.DeferAsync();

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);

        var cidade      = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        var construcoes = cidade?.Construcoes ?? new System.Collections.Generic.List<Construcao>();

        var ativas   = await operacaoService.ListarAtivasAsync(usuarioId);
        int maxSlots = TorreOperacaoConfig.CalcularMaxSlots(construcoes);

        if (ativas.Count >= maxSlots)
        {
            await comp.ModifyOriginalResponseAsync(m => { m.Content = $"❌ Capacidade máxima de operações atingida ({maxSlots})."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
            return;
        }

        int andarAtual  = await operacaoService.ObterAndarAtualNumeroAsync(usuarioId);
        var andaresBloq = ativas.Select(o => o.AndarNumero).ToHashSet();

        var (embed, comps) = TorreModoOperacaoPanel.CriarSeletorAndar(andarAtual, andaresBloq, maxSlots, ativas.Count);
        await comp.ModifyOriginalResponseAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
    }

    // ── SelectMenu: torre_op_andar_sel ───────────────────────────────────────────

    public async Task HandleOpAndarSelAsync(SocketMessageComponent comp)
    {
        var valorStr = comp.Data.Values.FirstOrDefault();
        if (valorStr == null || !int.TryParse(valorStr, out int andar))
        {
            await comp.UpdateAsync(m => { m.Content = "Andar inválido."; m.Embed = null; m.Components = null; });
            return;
        }

        Log($"OpAndarSel — andar={andar} user={comp.User.Username}");
        await comp.DeferAsync();

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var cidade    = await cidadeService.ObterCidadePorUsuarioAsync(comp.User.Id);
        var construcoes = cidade?.Construcoes ?? new System.Collections.Generic.List<Construcao>();

        var heroisDaOp = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
        if (heroisDaOp.Any(h => h.EstadoSustento == EstadoSustento.Degradado))
        {
            await comp.ModifyOriginalResponseAsync(m =>
            {
                m.Content    = "🔴 Seus heróis estão **degradados** (sem comida). Produza Comida no Campo antes de iniciar novas operações.";
                m.Embed      = null;
                m.Components = new ComponentBuilder().Build();
            });
            return;
        }

        try
        {
            var op = await operacaoService.IniciarAsync(usuarioId, andar, construcoes);
            var (recurso, qtd, icone) = TorreOperacaoConfig.ObterProducao(andar);
            var finishAt = op.IniciadoEm.AddHours(op.DuracaoHoras);
            await comp.ModifyOriginalResponseAsync(m =>
            {
                m.Content = $"✅ Operação iniciada no Andar **{andar}**!\n" +
                            $"{icone} {recurso} ×{qtd} — Coleta em {finishAt:HH:mm} UTC";
                m.Embed      = null;
                m.Components = new ComponentBuilder().Build();
            });
        }
        catch (InvalidOperationException ex)
        {
            await comp.ModifyOriginalResponseAsync(m => { m.Content = $"❌ {ex.Message}"; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"OpAndarSel andar={andar} user={comp.User.Username}");
            await comp.ModifyOriginalResponseAsync(m => { m.Content = "❌ Erro interno ao iniciar operação."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
        }
    }

    // ── Button: torre_op_coletar_todas ───────────────────────────────────────────

    public async Task HandleOpColetarTodasAsync(SocketMessageComponent comp)
    {
        Log($"OpColetarTodas — user={comp.User.Username}");
        await comp.DeferAsync();

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await operacaoService.ProcessarTodasAsync(usuarioId);

        int coletadas = await operacaoService.ColetarTodasAsync(usuarioId, comp.User.Id);

        await comp.ModifyOriginalResponseAsync(m =>
        {
            m.Content    = coletadas > 0 ? $"✅ {coletadas} operação(ões) coletada(s)!" : "Nenhuma operação pronta para coletar.";
            m.Embed      = null;
            m.Components = new ComponentBuilder().Build();
        });
    }

    // ── Button: torre_op_remover_sel ─────────────────────────────────────────────

    public async Task HandleOpRemoverSelAsync(SocketMessageComponent comp)
    {
        Log($"OpRemoverSel — user={comp.User.Username}");
        await comp.DeferAsync();

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var ativas    = await operacaoService.ListarAtivasAsync(usuarioId);

        if (!ativas.Any())
        {
            await comp.ModifyOriginalResponseAsync(m => { m.Content = "Nenhuma operação ativa para remover."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
            return;
        }

        var (embed, comps) = TorreModoOperacaoPanel.CriarSeletorRemover(ativas);
        await comp.ModifyOriginalResponseAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
    }

    // ── SelectMenu: torre_op_remover_andar_sel ───────────────────────────────────

    public async Task HandleOpRemoverAndarSelAsync(SocketMessageComponent comp)
    {
        var valorStr = comp.Data.Values.FirstOrDefault();
        if (valorStr == null || !int.TryParse(valorStr, out int andar))
        {
            await comp.UpdateAsync(m => { m.Content = "Seleção inválida."; m.Embed = null; m.Components = null; });
            return;
        }

        Log($"OpRemoverAndarSel — andar={andar} user={comp.User.Username}");
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await operacaoService.CancelarPorAndarAsync(usuarioId, andar);

        await comp.UpdateAsync(m =>
        {
            m.Content    = $"🗑️ Operação no andar **{andar}** cancelada.";
            m.Embed      = null;
            m.Components = new ComponentBuilder().Build();
        });
    }

    // ── Button: torre_op_fechar ──────────────────────────────────────────────────

    public async Task HandleOpFecharAsync(SocketMessageComponent comp)
    {
        await comp.UpdateAsync(m => { m.Content = "Fechado."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
    }

    // ── Button: torre_investigar ─────────────────────────────────────────────

    public async Task HandleInvestigarAsync(SocketMessageComponent comp)
    {
        Log($"Investigar — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await exploracaoService.ProcessarAsync(usuarioId);

        var herois = (await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id))
            .Where(h => h.EstadoSustento != EstadoSustento.Inativo)
            .ToList();

        if (!herois.Any())
        {
            await comp.FollowupAsync("Você não tem heróis ativos para explorar.", ephemeral: true);
            return;
        }

        var andar = await torreService.ObterAndarAtualAsync(usuarioId)
                    ?? await torreService.InicializarPrimeiroAndarAsync(usuarioId);

        double teamPS    = HeroPowerScoreService.CalcularParty(herois);
        double cdi       = HeroPowerScoreService.CalcularCDI(andar.Numero);
        double ratio     = HeroPowerScoreService.CalcularRatio(teamPS, cdi);
        double winChance = HeroPowerScoreService.CalcularWinChance(ratio);
        string descricao = HeroPowerScoreService.DescricaoWinChance(winChance);

        var boosters = await exploracaoService.ObterBoostersAsync(usuarioId);

        var (embed, comps) = TorreExploracaoPanel.CriarInvestigacao(
            andar.Numero, winChance, teamPS, cdi, descricao, boosters);

        await comp.FollowupAsync(embed: embed, components: comps, ephemeral: true);
    }

    // ── Button: torre_explorar ────────────────────────────────────────────────

    public async Task HandleExplorarAsync(SocketMessageComponent comp)
    {
        Log($"Explorar — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await exploracaoService.ProcessarAsync(usuarioId);

        // Show existing active exploration
        var ativa = await exploracaoService.ObterAtivaAsync(usuarioId);
        if (ativa != null)
        {
            var expIds = ativa.HeroisIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue).Select(g => g!.Value).ToHashSet();
            var heroisAtivos = (await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id))
                .Where(h => expIds.Contains(h.Id))
                .ToList();
            double ps2 = HeroPowerScoreService.CalcularParty(heroisAtivos);
            double cd2 = HeroPowerScoreService.CalcularCDI(ativa.AndarNumero);
            var (e, c) = TorreExploracaoPanel.CriarAtivo(ativa, ps2, cd2);
            await comp.FollowupAsync(embed: e, components: c, ephemeral: true);
            return;
        }

        // Show pending collection panel
        var pendente = await exploracaoService.ObterPendenteAsync(usuarioId);
        if (pendente != null)
        {
            var (e, c) = pendente.Status == StatusExploracao.Concluida
                ? TorreExploracaoPanel.CriarConcluido(pendente)
                : TorreExploracaoPanel.CriarFalha(pendente);
            await comp.FollowupAsync(embed: e, components: c, ephemeral: true);
            return;
        }

        // Load parties for the user
        var parties = await partyService.ObterPartiesUsuarioAsync(comp.User.Id);
        var partiesValidas = parties
            .Where(p => p.Membros != null && p.Membros.Count > 0)
            .ToList();

        if (!partiesValidas.Any())
        {
            await comp.FollowupAsync(
                "Você não tem grupos com heróis. Crie e configure um grupo com /grupo.",
                ephemeral: true);
            return;
        }

        var andar = await torreService.ObterAndarAtualAsync(usuarioId)
                    ?? await torreService.InicializarPrimeiroAndarAsync(usuarioId);

        if (partiesValidas.Count == 1)
        {
            await MostrarConfirmacaoGrupoAsync(comp, usuarioId, andar, partiesValidas[0], isUpdate: false);
            return;
        }

        // Multiple parties — show group selector
        var (embed, comps) = TorreExploracaoPanel.CriarSeletorGrupo(andar.Numero, partiesValidas);
        await comp.FollowupAsync(embed: embed, components: comps, ephemeral: true);
    }

    // ── SelectMenu: torre_exp_grupo_sel ──────────────────────────────────────

    public async Task HandleExpGrupoSelAsync(SocketMessageComponent comp)
    {
        Log($"ExpGrupoSel — user={comp.User.Username}");

        var partyIdStr = comp.Data.Values.FirstOrDefault();
        if (partyIdStr == null || !Guid.TryParse(partyIdStr, out var partyId))
        {
            await comp.UpdateAsync(m => { m.Content = "Grupo inválido."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
            return;
        }

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var parties = await partyService.ObterPartiesUsuarioAsync(comp.User.Id);
        var party = parties.FirstOrDefault(p => p.Id == partyId);

        if (party == null)
        {
            await comp.UpdateAsync(m => { m.Content = "Grupo não encontrado."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
            return;
        }

        var andar = await torreService.ObterAndarAtualAsync(usuarioId)
                    ?? await torreService.InicializarPrimeiroAndarAsync(usuarioId);

        await MostrarConfirmacaoGrupoAsync(comp, usuarioId, andar, party, isUpdate: true);
    }

    // ── Button: torre_explorar_confirmar|{booster}|{partyId} ─────────────────

    public async Task HandleExplorarConfirmarAsync(SocketMessageComponent comp, string boosterStr, string partyId)
    {
        Log($"ExplorarConfirmar — booster={boosterStr} partyId={partyId} user={comp.User.Username}");
        try
        {
            var (exp, teamPS, cdi) = await PrepararInicioAsync(comp.User.Id, boosterStr, partyId);
            var (embed, comps) = TorreExploracaoPanel.CriarAtivo(exp, teamPS, cdi);
            await comp.UpdateAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
        }
        catch (InvalidOperationException ex)
        {
            await comp.UpdateAsync(m => { m.Content = $"❌ {ex.Message}"; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"ExplorarConfirmar user={comp.User.Username}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno ao iniciar exploração."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
        }
    }

    // ── SelectMenu: torre_exp_booster_sel|{partyId} ───────────────────────────

    public async Task HandleExpBoosterSelAsync(SocketMessageComponent comp)
    {
        var parts   = comp.Data.CustomId.Split('|');
        var partyId = parts.Length >= 2 ? parts[1] : "";
        var valor   = comp.Data.Values.FirstOrDefault() ?? "nenhum";

        Log($"BoosterSel — valor={valor} partyId={partyId} user={comp.User.Username}");
        try
        {
            var (exp, teamPS, cdi) = await PrepararInicioAsync(comp.User.Id, valor, partyId);
            var (embed, comps) = TorreExploracaoPanel.CriarAtivo(exp, teamPS, cdi);
            await comp.UpdateAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
        }
        catch (InvalidOperationException ex)
        {
            await comp.UpdateAsync(m => { m.Content = $"❌ {ex.Message}"; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"BoosterSel user={comp.User.Username}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro interno."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
        }
    }

    // ── Button: torre_exp_atualizar ────────────────────────────────────────────

    public async Task HandleExpAtualizarAsync(SocketMessageComponent comp)
    {
        Log($"ExpAtualizar — user={comp.User.Username}");

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await exploracaoService.ProcessarAsync(usuarioId);

        var ativa = await exploracaoService.ObterAtivaAsync(usuarioId);
        if (ativa != null)
        {
            var expIds = ativa.HeroisIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue).Select(g => g!.Value).ToHashSet();
            var herois = (await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id))
                .Where(h => expIds.Contains(h.Id))
                .ToList();
            double teamPS = HeroPowerScoreService.CalcularParty(herois);
            double cdi    = HeroPowerScoreService.CalcularCDI(ativa.AndarNumero);
            var (embed, comps) = TorreExploracaoPanel.CriarAtivo(ativa, teamPS, cdi);
            await comp.UpdateAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
            return;
        }

        var pendente = await exploracaoService.ObterPendenteAsync(usuarioId);
        if (pendente != null)
        {
            var (embed, comps) = pendente.Status == StatusExploracao.Concluida
                ? TorreExploracaoPanel.CriarConcluido(pendente)
                : TorreExploracaoPanel.CriarFalha(pendente);
            await comp.UpdateAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
            return;
        }

        await comp.UpdateAsync(m => { m.Content = "Nenhuma exploração ativa."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
    }

    // ── Button: torre_exp_coletar ──────────────────────────────────────────────

    public async Task HandleExpColetarAsync(SocketMessageComponent comp)
    {
        Log($"ExpColetar — user={comp.User.Username}");

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var exp = await exploracaoService.ColetarAsync(usuarioId, comp.User.Id);

        if (exp == null)
        {
            await comp.UpdateAsync(m => { m.Content = "Nenhuma exploração aguardando coleta."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("✅ **Recompensas coletadas!**");
        if (exp.LootOuro > 0)
            sb.AppendLine($"💰 Ouro: +**{exp.LootOuro}**");
        if (exp.LootFragmentosQtd > 0)
            sb.AppendLine($"💎 Fragmentos: +**{exp.LootFragmentosQtd}**");
        if (exp.LootOuro == 0 && exp.LootFragmentosQtd == 0)
            sb.AppendLine("*Nenhum loot nesta tentativa.*");

        await comp.UpdateAsync(m => { m.Content = sb.ToString().TrimEnd(); m.Embed = null; m.Components = new ComponentBuilder().Build(); });
    }

    // ── Button: torre_exp_cancelar (abandon active exploration) ───────────────

    public async Task HandleExpCancelarAsync(SocketMessageComponent comp)
    {
        Log($"ExpCancelar — user={comp.User.Username}");
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        await exploracaoService.CancelarAsync(usuarioId);
        await comp.UpdateAsync(m => { m.Content = "🏳️ Exploração abandonada."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
    }

    // ── Button: torre_exp_cancelar_sel (close booster selector / confirm) ─────

    public async Task HandleExpCancelarSelAsync(SocketMessageComponent comp)
    {
        await comp.UpdateAsync(m => { m.Content = "Fechado."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task MostrarConfirmacaoGrupoAsync(
        SocketMessageComponent comp,
        Guid usuarioId,
        LegendsAwaken.Domain.Entities.TorreAndar andar,
        Party party,
        bool isUpdate)
    {
        // Load heroes from the party, filter inactive/null
        var membros = party.Membros ?? new List<PartyHero>();
        var heroisList = new List<Heroi>();

        foreach (var membro in membros)
        {
            var heroi = await heroiService.ObterHeroiPorIdAsync(membro.HeroiId);
            if (heroi != null && heroi.EstadoSustento != EstadoSustento.Inativo)
                heroisList.Add(heroi);
        }

        if (!heroisList.Any())
        {
            const string msg = "Todos os heróis do grupo estão inativos ou indisponíveis.";
            if (isUpdate)
                await comp.UpdateAsync(m => { m.Content = msg; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
            else
                await comp.FollowupAsync(msg, ephemeral: true);
            return;
        }

        double teamPS    = HeroPowerScoreService.CalcularParty(heroisList);
        double cdi       = HeroPowerScoreService.CalcularCDI(andar.Numero);
        double ratio     = HeroPowerScoreService.CalcularRatio(teamPS, cdi);
        double winChance = HeroPowerScoreService.CalcularWinChance(ratio);

        var boosters = await exploracaoService.ObterBoostersAsync(usuarioId);

        Embed panelEmbed;
        MessageComponent panelComps;

        if (boosters.Any(b => b.Quantidade > 0))
        {
            (panelEmbed, panelComps) = TorreExploracaoPanel.CriarSeletorBooster(
                andar.Numero, winChance, boosters,
                party.Id.ToString(), party.Nome);
        }
        else
        {
            (panelEmbed, panelComps) = TorreExploracaoPanel.CriarConfirmacao(
                andar.Numero, winChance,
                party.Nome,
                heroisList.Select(h => h.Nome).ToList(),
                party.Id.ToString());
        }

        if (isUpdate)
            await comp.UpdateAsync(m => { m.Content = null; m.Embed = panelEmbed; m.Components = panelComps; });
        else
            await comp.FollowupAsync(embed: panelEmbed, components: panelComps, ephemeral: true);
    }

    private async Task<(Domain.Entities.TorreExploracao exp, double teamPS, double cdi)> PrepararInicioAsync(
        ulong discordId, string boosterStr, string partyIdStr)
    {
        var usuarioId = DiscordIdHelper.ToGuid(discordId);

        TipoBooster? booster = boosterStr is "nenhum" or "" or null
            ? null
            : Enum.TryParse<TipoBooster>(boosterStr, out var b) ? b : null;

        if (!Guid.TryParse(partyIdStr, out var partyId))
            throw new InvalidOperationException("Grupo inválido.");

        var parties = await partyService.ObterPartiesUsuarioAsync(discordId);
        var party   = parties.FirstOrDefault(p => p.Id == partyId);

        if (party == null)
            throw new InvalidOperationException("Grupo não encontrado.");

        var heroisIds = (party.Membros ?? new List<PartyHero>())
            .Select(m => m.HeroiId)
            .ToList();

        if (!heroisIds.Any())
            throw new InvalidOperationException("O grupo está vazio.");

        if (heroisIds.Count > 5)
            throw new InvalidOperationException($"O grupo tem {heroisIds.Count} heróis. Máximo permitido: 5.");

        if (heroisIds.Distinct().Count() != heroisIds.Count)
            throw new InvalidOperationException("O grupo contém heróis duplicados.");

        var heroisValidos = new List<Heroi>();
        foreach (var id in heroisIds)
        {
            var heroi = await heroiService.ObterHeroiPorIdAsync(id);
            if (heroi == null) continue;
            if (heroi.EstadoSustento == EstadoSustento.Inativo)
                throw new InvalidOperationException($"O herói {heroi.Nome} está inativo.");
            heroisValidos.Add(heroi);
        }

        if (!heroisValidos.Any())
            throw new InvalidOperationException("Nenhum herói ativo no grupo.");

        var exp    = await exploracaoService.IniciarAsync(usuarioId, heroisValidos.Select(h => h.Id).ToList(), booster);
        double teamPS = HeroPowerScoreService.CalcularParty(heroisValidos);
        double cdi    = HeroPowerScoreService.CalcularCDI(exp.AndarNumero);
        return (exp, teamPS, cdi);
    }

    private async Task<(Embed embed, MessageComponent comps)> BuildPanelAsync(ulong discordId)
    {
        var guid  = DiscordIdHelper.ToGuid(discordId);
        var andar = await torreService.ObterAndarAtualAsync(guid)
                    ?? await torreService.InicializarPrimeiroAndarAsync(guid);

        var bioma      = await biomeService.ObterBiomaPorAndarAsync(andar.Numero);
        var ativa      = await exploracaoService.ObterAtivaAsync(guid);
        var pendente   = await exploracaoService.ObterPendenteAsync(guid);
        var exploracao = ativa ?? pendente;

        return (TorrePanel.CriarEmbed(andar, bioma, exploracao),
                TorrePanel.CriarComponentes(exploracao != null));
    }
}
