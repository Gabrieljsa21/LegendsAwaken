using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class ContratoCommand(ContractService contractService, IContratoRepository contratoRepository)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(command.User.Id);

        var arquetT = contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        var nomeadT = contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);
        await Task.WhenAll(arquetT, nomeadT);

        var embed = ContratoPanel.CriarEmbed(arquetT.Result, nomeadT.Result);
        var comps = ContratoPanel.CriarComponentes();

        await command.ModifyOriginalResponseAsync(m =>
        {
            m.Embed      = embed;
            m.Components = comps;
        });
    }

    public async Task MostrarAsync(SocketMessageComponent comp)
    {
        await comp.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(comp.User.Id);

        var arquetT = contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        var nomeadT = contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);
        await Task.WhenAll(arquetT, nomeadT);

        var embed = ContratoPanel.CriarEmbed(arquetT.Result, nomeadT.Result);
        var comps = ContratoPanel.CriarComponentes();

        await comp.UpdateAsync(m => { m.Embed = embed; m.Components = comps; });
    }

    public async Task HandleArquetipoAsync(SocketMessageComponent interaction, Profissao arquetipo)
    {
        await interaction.DeferAsync(ephemeral: true);
        var usuarioId = DiscordIdHelper.ToGuid(interaction.User.Id);
        await contractService.AtivarContratoArquetipoAsync(usuarioId, arquetipo);
        await interaction.FollowupAsync($"Contrato de arquetipo **{arquetipo}** ativado.", ephemeral: true);
    }
}
