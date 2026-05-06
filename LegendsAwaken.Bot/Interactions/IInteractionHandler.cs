using Discord.WebSocket;

namespace LegendsAwaken.Bot.Interactions;

public interface IInteractionHandler
{
    /// <summary>Prefixo do sistema, ex: "cidade". Deve ser único por handler.</summary>
    string CustomIdPrefix { get; }

    /// <summary>
    /// Chamado pelo InteractionRouter quando customId começa com CustomIdPrefix.
    /// parts = customId.Split(':') — parts[0] é o prefix, parts[1] é a ação.
    /// </summary>
    Task HandleAsync(SocketMessageComponent component, string[] parts);
}
