using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Bot.Panels;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands;

public class ColecaoCommand(
    IHeroiConfigRepository heroiConfigRepo,
    IHeroiDesbloqueadoRepository desbloqueadoRepo,
    IFragmentoRepository fragmentoRepo,
    RecruitmentService recruitmentService)
{
    public async Task ExecutarAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();
        var usuarioId = ToGuid(command.User.Id);

        var todosHerois   = await heroiConfigRepo.ListarTodosAsync();
        var desbloqueados = await desbloqueadoRepo.ListarPorUsuarioAsync(usuarioId);
        var progressos    = await fragmentoRepo.ListarPorUsuarioAsync(usuarioId);

        var unlockTasks = todosHerois.Select(h => heroiConfigRepo.ObterUnlockConfigAsync(h.Id));
        var unlockArr   = await Task.WhenAll(unlockTasks);
        var unlockList  = unlockArr.Where(u => u is not null).Select(u => u!).ToList();

        var heroisProntos = todosHerois
            .Where(h =>
            {
                var unlock = unlockList.FirstOrDefault(u => u.HeroiId == h.Id);
                var prog   = progressos.FirstOrDefault(p => p.HeroiId == h.Id);
                return !desbloqueados.Any(d => d.HeroiId == h.Id)
                    && unlock?.TipoUnlock == TipoUnlock.Fragmentos
                    && prog?.Quantidade >= unlock.QuantidadeFragmentos;
            })
            .ToList();

        var embed      = ColecaoPanel.CriarEmbed(todosHerois, desbloqueados, progressos, unlockList);
        var components = ColecaoPanel.CriarComponentes(heroisProntos);

        await command.ModifyOriginalResponseAsync(m =>
        {
            m.Embed      = embed;
            m.Components = components;
        });
    }

    public async Task HandleRecrutarAsync(SocketMessageComponent interaction, Guid heroiId)
    {
        await interaction.DeferAsync(ephemeral: true);
        var usuarioId = ToGuid(interaction.User.Id);
        var resultado = await recruitmentService.TentarRecrutarPorFragmentosAsync(usuarioId, heroiId);
        await interaction.FollowupAsync(resultado.Mensagem, ephemeral: true);
    }

    private static Guid ToGuid(ulong discordId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(discordId).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
