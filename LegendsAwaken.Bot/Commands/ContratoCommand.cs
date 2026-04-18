using Discord.WebSocket;
using LegendsAwaken.Application.Services;
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
        var usuarioId = ToGuid(command.User.Id);

        var arquetipo = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Arquetipo);
        var nomeado   = await contratoRepository.ObterAtivoAsync(usuarioId, TipoContrato.Nomeado);

        var embed = ContratoPanel.CriarEmbed(arquetipo, nomeado);
        var comps = ContratoPanel.CriarComponentes();

        await command.ModifyOriginalResponseAsync(m =>
        {
            m.Embed      = embed;
            m.Components = comps;
        });
    }

    public async Task HandleArquetipoAsync(SocketMessageComponent interaction, Profissao arquetipo)
    {
        await interaction.DeferAsync(ephemeral: true);
        var usuarioId = ToGuid(interaction.User.Id);
        await contractService.AtivarContratoArquetipoAsync(usuarioId, arquetipo);
        await interaction.FollowupAsync($"Contrato de arquetipo **{arquetipo}** ativado.", ephemeral: true);
    }

    private static Guid ToGuid(ulong discordId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(discordId).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
