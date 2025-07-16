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
            // - Receber parâmetro (ex: heroiId ou nome)
            // - Buscar o herói do jogador no banco de dados
            // - Formatar as informações do herói (atributos, habilidades, equipamentos, status)
            // - Criar embed usando EmbedHelper
            // - Enviar embed na resposta

            // Exemplo placeholder básico:
            var embed = EmbedHelper.BuildBasicEmbed(
                "Herói: Thalindra Sombrassangue",
                "Raça: Elfa Negra\nClasse: Bruxa\nNível: 1\nAtributos:\nForça: 8\nDestreza: 14\nConstituição: 10\nInteligência: 17\nSabedoria: 13\nCarisma: 12",
                Discord.Color.Purple);

            await command.RespondAsync(embed: embed, ephemeral: true);
        }
    }
}
