using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Helpers;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Interfaces;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class BiomaCommand(BiomeService biomeService, ITorreRepository torreRepository)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = DiscordIdHelper.ToGuid(command.User.Id);

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

}
