using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

public class CombatCommand
{
    private readonly HeroiService _heroiService;
    private readonly CombatService _combatService;

    public CombatCommand(HeroiService heroSvc, CombatService combatSvc)
    {
        _heroiService = heroSvc;
        _combatService = combatSvc;
    }

    public async Task ExecutarAsync(SocketSlashCommand cmd)
    {
        var userId = cmd.User.Id;
        var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);
        // TODO: carregar inimigos do andar via TorreService
        var inimigos = new List<Inimigo> { /* ... */ };

        var encounter = _combatService.IniciarCombate(herois, inimigos);

        // rodar rounds até terminar
        while (!encounter.IsFinished)
            _combatService.ExecutarRound(encounter);

        var vencedor = encounter.Winner;
        await cmd.RespondAsync($"{vencedor?.Nome} venceu o combate no round {encounter.Round}!", ephemeral: true);
    }
}
