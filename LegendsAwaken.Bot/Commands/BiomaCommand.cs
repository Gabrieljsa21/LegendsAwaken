using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class BiomaCommand(BiomeService biomeService, ITorreRepository torreRepository)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = ToGuid(command.User.Id);

        var andar = await torreRepository.ObterAndarPorUsuarioAsync(usuarioId);
        int andarAtual = andar?.Numero ?? 1;

        var bioma = await biomeService.ObterBiomaPorAndarAsync(andarAtual);
        if (bioma is null)
        {
            await command.ModifyOriginalResponseAsync(m => m.Content = "Bioma nao encontrado para o andar atual.");
            return;
        }

        var pool  = await biomeService.ObterPoolDoBiomaAsync(bioma.Id);
        var embed = BiomaPanel.CriarEmbed(bioma, pool, andarAtual);
        var comps = BiomaPanel.CriarComponentes();

        await command.ModifyOriginalResponseAsync(m =>
        {
            m.Embed      = embed;
            m.Components = comps;
        });
    }

    private static Guid ToGuid(ulong discordId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(discordId).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
