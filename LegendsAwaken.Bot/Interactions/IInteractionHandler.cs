using Discord.WebSocket;

namespace LegendsAwaken.Bot.Interactions;

public interface IInteractionHandler
{
    /// <summary>Prefixo do sistema, ex: "cidade". Deve ser único por handler.</summary>
    string CustomIdPrefix { get; }

    /// <summary>
    /// Chamado pelo InteractionRouter quando customId segue a convenção nova: <c>sistema:acao[:param1:param2]</c>.
    /// <c>parts = customId.Split(':')</c> — parts[0] é o prefix, parts[1] é a ação, parts[2+] são parâmetros opcionais.
    /// Nota: customIds legados usam '_' e '|' como separadores e NÃO são roteados por esta interface.
    /// </summary>
    Task HandleAsync(SocketMessageComponent component, string[] parts);
}
