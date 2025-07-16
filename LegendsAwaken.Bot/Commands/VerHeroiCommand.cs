using Discord;
using Discord.WebSocket;
using LegendsAwaken.Bot.Helpers;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class VerHeroiCommand
    {
        private readonly DiscordSocketClient _client;

        public VerHeroiCommand(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            // TODO:
            // - Receber par�metro (ex: heroiId ou nome)
            // - Buscar o her�i do jogador no banco de dados
            // - Formatar as informa��es do her�i (atributos, habilidades, equipamentos, status)
            // - Criar embed usando EmbedHelper
            // - Enviar embed na resposta

            // Exemplo placeholder b�sico:
            var embed = EmbedHelper.BuildBasicEmbed(
                "Her�i: Thalindra Sombrassangue",
                "Ra�a: Elfa Negra\nClasse: Bruxa\nN�vel: 1\nAtributos:\nFor�a: 8\nDestreza: 14\nConstitui��o: 10\nIntelig�ncia: 17\nSabedoria: 13\nCarisma: 12",
                Discord.Color.Purple);

            await command.RespondAsync(embed: embed, ephemeral: true);
        }
    }
}
