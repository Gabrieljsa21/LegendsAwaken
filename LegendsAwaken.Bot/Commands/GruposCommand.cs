using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class GruposCommand(PartyService partyService, HeroiService heroiService, ILogger? logger = null)
{
    private void Log(string msg)                  => logger?.LogInformation("[Grupos] {Msg}", msg);
    private void LogErr(Exception ex, string ctx) => logger?.LogError(ex, "[Grupos] ERRO em {Ctx}", ctx);

    // ── Entry: herois_grupos button (creates new ephemeral) ──────────────────

    public async Task AbrirAsync(SocketMessageComponent comp)
    {
        Log($"Abrir — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);
        try
        {
            var grupos = await partyService.ObterPartiesUsuarioAsync(comp.User.Id);
            await comp.FollowupAsync(
                embed: GruposPanel.CriarEmbedLista(grupos),
                components: GruposPanel.CriarComponentesLista(grupos),
                ephemeral: true);
        }
        catch (Exception ex)
        {
            LogErr(ex, "AbrirAsync");
            await comp.FollowupAsync("❌ Erro ao carregar grupos.", ephemeral: true);
        }
    }

    // ── Refresh list (grupos_lista) ───────────────────────────────────────────

    public async Task HandleListaAsync(SocketMessageComponent comp)
    {
        Log($"Lista — user={comp.User.Username}");
        try
        {
            var grupos = await partyService.ObterPartiesUsuarioAsync(comp.User.Id);
            await comp.UpdateAsync(m =>
            {
                m.Embed      = GruposPanel.CriarEmbedLista(grupos);
                m.Components = GruposPanel.CriarComponentesLista(grupos);
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleListaAsync");
            await SafeFollowupAsync(comp, "❌ Erro ao carregar grupos.");
        }
    }

    // ── View group from select (grupos_ver_sel) ───────────────────────────────

    public async Task HandleVerSelAsync(SocketMessageComponent comp)
    {
        var partyIdStr = comp.Data.Values.FirstOrDefault();
        Log($"VerSel — partyId={partyIdStr} user={comp.User.Username}");
        if (partyIdStr == null || !Guid.TryParse(partyIdStr, out var partyId))
        {
            await comp.UpdateAsync(m => { m.Content = "Grupo inválido."; m.Components = null; });
            return;
        }
        await MostrarDetalheAsync(comp, partyId);
    }

    // ── View group from button (grupos_ver|{partyId}) ─────────────────────────

    public async Task HandleVerAsync(SocketMessageComponent comp, Guid partyId)
    {
        Log($"Ver — partyId={partyId} user={comp.User.Username}");
        await MostrarDetalheAsync(comp, partyId);
    }

    // ── Show creation hero selector (grupos_criar) ────────────────────────────

    public async Task HandleCriarAsync(SocketMessageComponent comp)
    {
        Log($"Criar — user={comp.User.Username}");
        try
        {
            var herois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
            if (!herois.Any())
            {
                await comp.UpdateAsync(m =>
                {
                    m.Embed      = new EmbedBuilder().WithTitle("👥 Grupos").WithDescription("Você não tem heróis.").WithColor(Color.Red).Build();
                    m.Components = new ComponentBuilder().WithButton("← Voltar", "grupos_lista", ButtonStyle.Secondary).Build();
                });
                return;
            }

            var (embed, comps) = GruposPanel.CriarSeletorCriacao(herois);
            await comp.UpdateAsync(m => { m.Embed = embed; m.Components = comps; });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleCriarAsync");
            await SafeFollowupAsync(comp, "❌ Erro ao carregar heróis.");
        }
    }

    // ── Heroes selected for creation (grupos_criar_sel) ───────────────────────

    public async Task HandleCriarSelAsync(SocketMessageComponent comp)
    {
        Log($"CriarSel — user={comp.User.Username}");
        var heroisIds = comp.Data.Values
            .Where(v => Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v))
            .ToList();

        if (!heroisIds.Any())
        {
            await comp.UpdateAsync(m => { m.Content = "Selecione pelo menos um herói."; });
            return;
        }

        await comp.DeferAsync();
        try
        {
            var todosHerois  = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
            var selecionados = todosHerois.Where(h => heroisIds.Contains(h.Id)).ToList();
            var grupo        = await partyService.CriarComHeroisAsync(comp.User.Id, selecionados);

            await comp.ModifyOriginalResponseAsync(m =>
            {
                m.Embed      = GruposPanel.CriarEmbedDetalhe(grupo);
                m.Components = GruposPanel.CriarComponentesDetalhe(grupo);
                m.Content    = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleCriarSelAsync");
            await SafeFollowupAsync(comp, $"❌ {ex.Message}");
        }
    }

    // ── Auto-create recommended group (grupos_recomendado) ────────────────────

    public async Task HandleRecomendadoAsync(SocketMessageComponent comp)
    {
        Log($"Recomendado — user={comp.User.Username}");
        await comp.DeferAsync();
        try
        {
            var herois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
            var grupo  = await partyService.CriarRecomendadaAsync(comp.User.Id, herois);

            await comp.ModifyOriginalResponseAsync(m =>
            {
                m.Embed      = GruposPanel.CriarEmbedDetalhe(grupo);
                m.Components = GruposPanel.CriarComponentesDetalhe(grupo);
                m.Content    = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleRecomendadoAsync");
            await SafeFollowupAsync(comp, $"❌ {ex.Message}");
        }
    }

    // ── Show add-hero selector (grupos_add_sel|{partyId}) ─────────────────────

    public async Task HandleAddSelAsync(SocketMessageComponent comp, Guid partyId)
    {
        Log($"AddSel — partyId={partyId} user={comp.User.Username}");
        try
        {
            var grupo = await partyService.ObterPorIdAsync(partyId);
            if (grupo == null) { await comp.UpdateAsync(m => { m.Content = "Grupo não encontrado."; m.Components = null; }); return; }

            var todosHerois = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);
            var jaNaGrupo   = grupo.Membros.Select(m => m.HeroiId).ToHashSet();
            var disponiveis = todosHerois.Where(h => !jaNaGrupo.Contains(h.Id)).ToList();

            if (!disponiveis.Any())
            {
                await comp.UpdateAsync(m =>
                {
                    m.Embed      = GruposPanel.CriarEmbedDetalhe(grupo);
                    m.Components = GruposPanel.CriarComponentesDetalhe(grupo);
                    m.Content    = "Todos os heróis já estão neste grupo.";
                });
                return;
            }

            var (embed, comps) = GruposPanel.CriarSeletorAddHeroi(grupo, disponiveis);
            await comp.UpdateAsync(m => { m.Embed = embed; m.Components = comps; m.Content = null; });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleAddSelAsync");
            await SafeFollowupAsync(comp, "❌ Erro ao carregar heróis.");
        }
    }

    // ── Hero chosen to add (grupos_add|{partyId}) ─────────────────────────────

    public async Task HandleAddAsync(SocketMessageComponent comp, Guid partyId)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"Add — partyId={partyId} heroiId={heroiIdStr} user={comp.User.Username}");
        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var heroiId))
        {
            await comp.UpdateAsync(m => { m.Content = "Herói inválido."; });
            return;
        }

        await comp.DeferAsync();
        try
        {
            await partyService.AdicionarHeroiComNomeAutoAsync(partyId, heroiId);
            await MostrarDetalheDeferredAsync(comp, partyId);
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleAddAsync");
            await SafeFollowupAsync(comp, $"❌ {ex.Message}");
        }
    }

    // ── Show remove-hero selector (grupos_rem_sel|{partyId}) ──────────────────

    public async Task HandleRemSelAsync(SocketMessageComponent comp, Guid partyId)
    {
        Log($"RemSel — partyId={partyId} user={comp.User.Username}");
        try
        {
            var grupo = await partyService.ObterPorIdAsync(partyId);
            if (grupo == null) { await comp.UpdateAsync(m => { m.Content = "Grupo não encontrado."; m.Components = null; }); return; }

            var (embed, comps) = GruposPanel.CriarSeletorRemHeroi(grupo);
            await comp.UpdateAsync(m => { m.Embed = embed; m.Components = comps; m.Content = null; });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleRemSelAsync");
            await SafeFollowupAsync(comp, "❌ Erro ao carregar membros.");
        }
    }

    // ── Hero chosen to remove (grupos_rem|{partyId}) ──────────────────────────

    public async Task HandleRemAsync(SocketMessageComponent comp, Guid partyId)
    {
        var heroiIdStr = comp.Data.Values.FirstOrDefault();
        Log($"Rem — partyId={partyId} heroiId={heroiIdStr} user={comp.User.Username}");
        if (heroiIdStr == null || !Guid.TryParse(heroiIdStr, out var heroiId))
        {
            await comp.UpdateAsync(m => { m.Content = "Herói inválido."; });
            return;
        }

        await comp.DeferAsync();
        try
        {
            await partyService.RemoverHeroiComNomeAutoAsync(partyId, heroiId);
            await MostrarDetalheDeferredAsync(comp, partyId);
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleRemAsync");
            await SafeFollowupAsync(comp, $"❌ {ex.Message}");
        }
    }

    // ── Toggle auto/manual name (grupos_nome_toggle|{partyId}) ───────────────

    public async Task HandleNomeToggleAsync(SocketMessageComponent comp, Guid partyId)
    {
        Log($"NomeToggle — partyId={partyId} user={comp.User.Username}");
        await comp.DeferAsync();
        try
        {
            await partyService.ToggleModoNomeAsync(partyId);
            await MostrarDetalheDeferredAsync(comp, partyId);
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleNomeToggleAsync");
            await SafeFollowupAsync(comp, $"❌ {ex.Message}");
        }
    }

    // ── Open name-edit modal (grupos_nome_editar|{partyId}) ───────────────────

    public async Task HandleNomeEditarAsync(SocketMessageComponent comp, Guid partyId)
    {
        Log($"NomeEditar — partyId={partyId} user={comp.User.Username}");
        var grupo     = await partyService.ObterPorIdAsync(partyId);
        var nomeAtual = grupo?.Nome ?? "";
        const int maxLen = 64;

        var modal = new ModalBuilder()
            .WithTitle("Editar Nome do Grupo")
            .WithCustomId($"grupos_nome_modal|{partyId}")
            .AddTextInput("Nome", "nome_input",
                placeholder: "Ex: Time de Exploração",
                required: true,
                maxLength: maxLen,
                value: nomeAtual.Length > maxLen ? nomeAtual[..maxLen] : nomeAtual)
            .Build();

        await comp.RespondWithModalAsync(modal);
    }

    // ── Modal submitted — save new name (grupos_nome_modal|{partyId}) ─────────

    public async Task HandleNomeModalAsync(SocketModal modal, Guid partyId)
    {
        var novoNome = modal.Data.Components
            .FirstOrDefault(c => c.CustomId == "nome_input")?.Value ?? "";

        Log($"NomeModal — partyId={partyId} nome='{novoNome}' user={modal.User.Username}");

        await modal.DeferAsync(ephemeral: true);

        if (string.IsNullOrWhiteSpace(novoNome))
        {
            await modal.FollowupAsync("❌ O nome não pode ficar vazio.", ephemeral: true);
            return;
        }

        try
        {
            await partyService.AtualizarNomeManualAsync(partyId, novoNome);
            var grupo = await partyService.ObterPorIdAsync(partyId);
            if (grupo == null) { await modal.FollowupAsync("Grupo não encontrado.", ephemeral: true); return; }

            await modal.FollowupAsync(
                embed: GruposPanel.CriarEmbedDetalhe(grupo),
                components: GruposPanel.CriarComponentesDetalhe(grupo),
                ephemeral: true);
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleNomeModalAsync");
            await modal.FollowupAsync($"❌ {ex.Message}", ephemeral: true);
        }
    }

    // ── Delete group (grupos_deletar|{partyId}) ───────────────────────────────

    public async Task HandleDeletarAsync(SocketMessageComponent comp, Guid partyId)
    {
        Log($"Deletar — partyId={partyId} user={comp.User.Username}");
        await comp.DeferAsync();
        try
        {
            await partyService.DeletarAsync(partyId, comp.User.Id);
            var grupos = await partyService.ObterPartiesUsuarioAsync(comp.User.Id);
            await comp.ModifyOriginalResponseAsync(m =>
            {
                m.Embed      = GruposPanel.CriarEmbedLista(grupos);
                m.Components = GruposPanel.CriarComponentesLista(grupos);
                m.Content    = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, "HandleDeletarAsync");
            await SafeFollowupAsync(comp, $"❌ {ex.Message}");
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task MostrarDetalheAsync(SocketMessageComponent comp, Guid partyId)
    {
        var grupo = await partyService.ObterPorIdAsync(partyId);
        if (grupo == null)
        {
            await comp.UpdateAsync(m => { m.Content = "Grupo não encontrado."; m.Components = null; });
            return;
        }

        await comp.UpdateAsync(m =>
        {
            m.Embed      = GruposPanel.CriarEmbedDetalhe(grupo);
            m.Components = GruposPanel.CriarComponentesDetalhe(grupo);
            m.Content    = null;
        });
    }

    private async Task MostrarDetalheDeferredAsync(SocketMessageComponent comp, Guid partyId)
    {
        var grupo = await partyService.ObterPorIdAsync(partyId);
        if (grupo == null)
        {
            await comp.ModifyOriginalResponseAsync(m => { m.Content = "Grupo não encontrado."; m.Components = new ComponentBuilder().Build(); m.Embed = null; });
            return;
        }

        await comp.ModifyOriginalResponseAsync(m =>
        {
            m.Embed      = GruposPanel.CriarEmbedDetalhe(grupo);
            m.Components = GruposPanel.CriarComponentesDetalhe(grupo);
            m.Content    = null;
        });
    }

    private static async Task SafeFollowupAsync(SocketMessageComponent comp, string message)
    {
        try { await comp.FollowupAsync(message, ephemeral: true); } catch { }
    }
}
