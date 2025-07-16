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
            // Aqui você implementaria a lógica do gacha e invocação do herói
            // Por enquanto, uma resposta simples:

            // Exemplo básico: resposta inicial
            await command.RespondAsync("Iniciando invocação de herói... (lógica a implementar)", ephemeral: true);

            // TODO:
            // - Verificar recursos do jogador (moedas, cristais, etc)
            // - Executar roleta gacha conforme tipo de invocação
            // - Criar instância do herói invocado com dados base + randomização
            // - Salvar herói no banco de dados vinculado ao usuário
            // - Retornar mensagem com dados do herói invocado (embed com detalhes)
        }
    }
}
