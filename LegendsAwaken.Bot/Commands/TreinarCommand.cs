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
            // - Obter o herói selecionado pelo jogador
            // - Validar se o herói pode iniciar treinamento (não estar em outro treino)
            // - Permitir escolher tipo de treino (Atributo, habilidade, etc)
            // - Iniciar o treinamento e definir duração, resultados esperados
            // - Atualizar dados do herói no banco
            // - Responder com confirmação e detalhes do treinamento

            await command.RespondAsync("Treinamento iniciado! (lógica a implementar)", ephemeral: true);
        }
    }
}
