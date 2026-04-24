using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class TorreCommand(
    TorreService torreService,
    HeroiService heroiService,
    BiomeService biomeService,
    TorreOperacaoService operacaoService,
    ILogger? logger = null)
{
    private void Log(string msg)                  => logger?.LogInformation("[Torre] {Msg}", msg);
    private void LogErr(Exception ex, string ctx) => logger?.LogError(ex, "[Torre] ERRO em {Ctx}", ctx);

    // ── /torre — painel público ──────────────────────────────────────────────────

    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        Log($"/torre — user={command.User.Username}");
        await command.DeferAsync();

        var usuarioId = DiscordIdHelper.ToGuid(command.User.Id);
        var (embed, comps) = await BuildPanelAsync(command.User.Id);
        await command.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });

        // Poll for pending operation notification
        var pendente = await operacaoService.VerificarPendenteAsync(usuarioId);
        if (pendente != null)
            await command.FollowupAsync(TorreModoOperacaoPanel.CriarNotificacaoTexto(pendente), ephemeral: true);

        Log("Painel público enviado");
    }

    // ── Button: torre_avancar ────────────────────────────────────────────────────

    public async Task HandleAvancarAsync(SocketMessageComponent comp)
    {
        Log($"Avancar — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var herois    = await heroiService.ObterHeroisPorUsuarioAsync(comp.User.Id);

        if (!herois.Any())
        {
            await comp.FollowupAsync("Você não tem heróis para avançar.", ephemeral: true);
            return;
        }

        try
        {
            var andar = await torreService.ObterAndarAtualAsync(usuarioId)
                        ?? await torreService.InicializarPrimeiroAndarAsync(usuarioId);

            Log($"Avancar: andar={andar.Numero} poder={herois.Sum(h => h.Nivel) * 5} dificuldade={andar.NivelDificuldade}");

            var resultado = await torreService.TentarAvancarAsync(usuarioId, herois);

            if (!resultado.Passou)
            {
                await comp.FollowupAsync(
                    $"❌ **Poder insuficiente!**\n" +
                    $"Poder do time: **{resultado.PoderTime}** | Dificuldade: **{resultado.NivelDificuldade}**\n" +
                    $"Treine seus heróis para avançar.",
                    ephemeral: true);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"✅ **Andar {resultado.AndarAnterior} concluído!**");
            sb.AppendLine($"💰 Ouro: +{resultado.Resultado!.OuroGanho} | ⭐ XP: +{resultado.Resultado.XpConcedido}");

            if (resultado.Resultado.NiveisGanhosPorHeroi.Any())
                sb.AppendLine("🆙 " + string.Join(", ", resultado.Resultado.NiveisGanhosPorHeroi
                    .Select(kv => $"**{kv.Key}** +{kv.Value} nível")));

            if (resultado.Resultado.Fragmentos.Any())
                sb.AppendLine("💎 Fragmentos: " + string.Join(", ", resultado.Resultado.Fragmentos
                    .Select(f => f.HeroiNome)));

            if (resultado.Resultado.NovoBioma != null)
                sb.AppendLine($"🗺️ Novo bioma: **{resultado.Resultado.NovoBioma.Nome}**!");

            if (resultado.Resultado.HeroiDesbloqueado != null)
                sb.AppendLine($"🌟 Herói desbloqueado: **{resultado.Resultado.HeroiDesbloqueado.Nome}**!");

            sb.AppendLine($"\n🗼 Agora no **Andar {resultado.AndarAnterior + 1}**");

            await comp.FollowupAsync(sb.ToString(), ephemeral: true);
        }
        catch (Exception ex)
        {
            LogErr(ex, $"Avancar user={comp.User.Username}");
            await comp.FollowupAsync("❌ Erro interno ao avançar. Veja o log.", ephemeral: true);
        }
    }

    // ── Button: torre_atualizar ──────────────────────────────────────────────────

    public async Task HandleAtualizarAsync(SocketMessageComponent comp)
    {
        Log($"Atualizar — user={comp.User.Username}");
        await comp.DeferAsync();
        var (embed, comps) = await BuildPanelAsync(comp.User.Id);
        await comp.ModifyOriginalResponseAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    // ── Button: torre_modo_operacao ──────────────────────────────────────────────

    public async Task HandleModoOperacaoAsync(SocketMessageComponent comp)
    {
        Log($"ModoOperacao — user={comp.User.Username}");
        await comp.DeferAsync(ephemeral: true);

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);

        // Check for concluded operation awaiting collection
        var pendente = await operacaoService.VerificarPendenteAsync(usuarioId);
        if (pendente != null)
        {
            var (e, c) = TorreModoOperacaoPanel.CriarColeta(pendente);
            await comp.FollowupAsync(embed: e, components: c, ephemeral: true);
            return;
        }

        // Check for active (still running) operation
        var ativa = await operacaoService.ObterAtivaAsync(usuarioId);
        if (ativa != null)
        {
            var (e, c) = TorreModoOperacaoPanel.CriarStatusAtivo(ativa);
            await comp.FollowupAsync(embed: e, components: c, ephemeral: true);
            return;
        }

        // No operation — show floor selector
        int andarAtual = await operacaoService.ObterAndarAtualNumeroAsync(usuarioId);
        if (andarAtual <= 1)
        {
            var (e, c) = TorreModoOperacaoPanel.CriarSemAndares();
            await comp.FollowupAsync(embed: e, components: c, ephemeral: true);
            return;
        }

        var (embed2, comps2) = TorreModoOperacaoPanel.CriarSeletorAndar(andarAtual);
        await comp.FollowupAsync(embed: embed2, components: comps2, ephemeral: true);
    }

    // ── Select: torre_op_andar ───────────────────────────────────────────────────

    public async Task HandleOpAndarAsync(SocketMessageComponent comp)
    {
        var valorStr = comp.Data.Values.FirstOrDefault();
        if (valorStr == null || !int.TryParse(valorStr, out int andar))
        {
            await comp.UpdateAsync(m => { m.Content = "Andar inválido."; m.Embed = null; m.Components = null; });
            return;
        }

        Log($"OpAndar — andar={andar} user={comp.User.Username}");
        var (embed, comps) = TorreModoOperacaoPanel.CriarSeletorObjetivo(andar);
        await comp.UpdateAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
    }

    // ── Button: torre_op_objetivo|{andar}|{objetivo} ─────────────────────────────

    public async Task HandleOpObjetivoAsync(SocketMessageComponent comp, int andar, string objetivoStr)
    {
        if (!Enum.TryParse<ObjetivoOperacao>(objetivoStr, out var objetivo))
        {
            await comp.UpdateAsync(m => { m.Content = "Objetivo inválido."; m.Embed = null; m.Components = null; });
            return;
        }

        Log($"OpObjetivo — andar={andar} objetivo={objetivo} user={comp.User.Username}");
        var (embed, comps) = TorreModoOperacaoPanel.CriarSeletorRisco(andar, objetivo);
        await comp.UpdateAsync(m => { m.Content = null; m.Embed = embed; m.Components = comps; });
    }

    // ── Button: torre_op_risco|{andar}|{objetivo}|{risco} ────────────────────────

    public async Task HandleOpRiscoAsync(SocketMessageComponent comp, int andar, string objetivoStr, string riscoStr)
    {
        if (!Enum.TryParse<ObjetivoOperacao>(objetivoStr, out var objetivo) ||
            !Enum.TryParse<PerfilRisco>(riscoStr, out var risco))
        {
            await comp.UpdateAsync(m => { m.Content = "Parâmetros inválidos."; m.Embed = null; m.Components = null; });
            return;
        }

        Log($"OpRisco — andar={andar} obj={objetivo} risco={risco} user={comp.User.Username}");

        try
        {
            var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
            var op = await operacaoService.IniciarAsync(usuarioId, andar, objetivo, risco);

            int horas = op.DuracaoHoras;
            var objStr = objetivo == ObjetivoOperacao.FarmRecurso ? "🌾 Farm Recurso" : "🗺️ Exploração Leve";
            var riscoLabel = risco switch
            {
                PerfilRisco.Seguro     => "🛡️ Seguro",
                PerfilRisco.Balanceado => "⚖️ Balanceado",
                PerfilRisco.Agressivo  => "⚔️ Agressivo",
                _                      => risco.ToString()
            };

            await comp.UpdateAsync(m =>
            {
                m.Content = $"✅ **Operação iniciada!**\n" +
                            $"Andar **{andar}** | {objStr} | {riscoLabel}\n" +
                            $"Duração: **{horas}h** — Retorne em {op.IniciadoEm.AddHours(horas):HH:mm} UTC";
                m.Embed      = null;
                m.Components = null;
            });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"OpRisco andar={andar} user={comp.User.Username}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro ao iniciar operação."; m.Embed = null; m.Components = null; });
        }
    }

    // ── Button: torre_op_coletar ─────────────────────────────────────────────────

    public async Task HandleOpColetarAsync(SocketMessageComponent comp)
    {
        Log($"OpColetar — user={comp.User.Username}");

        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var pendente = await operacaoService.VerificarPendenteAsync(usuarioId);

        if (pendente == null)
        {
            await comp.UpdateAsync(m => { m.Content = "Nenhuma operação pronta para coletar."; m.Embed = null; m.Components = null; });
            return;
        }

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("✅ **Recompensas coletadas!**");
            sb.AppendLine($"💰 Ouro: +{pendente.ResultadoOuro ?? 0}");
            if (pendente.ResultadoRecursoNome != null)
                sb.AppendLine($"📦 {pendente.ResultadoRecursoNome}: +{pendente.ResultadoRecursoQtd ?? 0}");

            await operacaoService.ColetarAsync(pendente, comp.User.Id);

            await comp.UpdateAsync(m => { m.Content = sb.ToString().TrimEnd(); m.Embed = null; m.Components = null; });
        }
        catch (Exception ex)
        {
            LogErr(ex, $"OpColetar user={comp.User.Username}");
            await comp.UpdateAsync(m => { m.Content = "❌ Erro ao coletar recompensas."; m.Embed = null; m.Components = null; });
        }
    }

    // ── Button: torre_op_cancelar_ativo ─────────────────────────────────────────

    public async Task HandleOpCancelarAtivoAsync(SocketMessageComponent comp)
    {
        Log($"OpCancelarAtivo — user={comp.User.Username}");
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);
        var ativa = await operacaoService.ObterAtivaAsync(usuarioId);

        if (ativa != null)
            await operacaoService.CancelarAsync(ativa);

        await comp.UpdateAsync(m => { m.Content = "Operação cancelada."; m.Embed = null; m.Components = null; });
    }

    // ── Button: torre_op_cancelar ────────────────────────────────────────────────

    public async Task HandleOpCancelarAsync(SocketMessageComponent comp)
    {
        await comp.UpdateAsync(m => { m.Content = "Fechado."; m.Embed = null; m.Components = new ComponentBuilder().Build(); });
    }

    // ── Helper ───────────────────────────────────────────────────────────────────

    private async Task<(Embed embed, MessageComponent comps)> BuildPanelAsync(ulong usuarioId)
    {
        var guid  = DiscordIdHelper.ToGuid(usuarioId);
        var andar = await torreService.ObterAndarAtualAsync(guid)
                    ?? await torreService.InicializarPrimeiroAndarAsync(guid);

        var bioma = await biomeService.ObterBiomaPorAndarAsync(andar.Numero);
        return (TorrePanel.CriarEmbed(andar, bioma), TorrePanel.CriarComponentes());
    }
}
