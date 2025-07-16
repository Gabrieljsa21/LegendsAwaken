using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class InvocarCommand
    {
        private readonly DiscordSocketClient _client;

        public InvocarCommand(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task HandleAsync(SocketSlashCommand command)
        {
            // Aqui voc� implementaria a l�gica do gacha e invoca��o do her�i
            // Por enquanto, uma resposta simples:

            // Exemplo b�sico: resposta inicial
            await command.RespondAsync("Iniciando invoca��o de her�i... (l�gica a implementar)", ephemeral: true);

            // TODO:
            // - Verificar recursos do jogador (moedas, cristais, etc)
            // - Executar roleta gacha conforme tipo de invoca��o
            // - Criar inst�ncia do her�i invocado com dados base + randomiza��o
            // - Salvar her�i no banco de dados vinculado ao usu�rio
            // - Retornar mensagem com dados do her�i invocado (embed com detalhes)
        }
    }
}
