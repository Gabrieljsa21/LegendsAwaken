using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class TreinarCommand
    {
        private readonly DiscordSocketClient _client;

        public TreinarCommand(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            // TODO:
            // - Obter o her�i selecionado pelo jogador
            // - Validar se o her�i pode iniciar treinamento (n�o estar em outro treino)
            // - Permitir escolher tipo de treino (Atributo, habilidade, etc)
            // - Iniciar o treinamento e definir dura��o, resultados esperados
            // - Atualizar dados do her�i no banco
            // - Responder com confirma��o e detalhes do treinamento

            await command.RespondAsync("Treinamento iniciado! (l�gica a implementar)", ephemeral: true);
        }
    }
}
