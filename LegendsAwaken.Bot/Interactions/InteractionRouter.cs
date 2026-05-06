using System.Collections.Concurrent;
using Discord.WebSocket;

namespace LegendsAwaken.Bot.Interactions;

public class InteractionRouter
{
    // ConcurrentDictionary: Register is startup-only, but TryRouteAsync is called from gateway threads.
    private readonly ConcurrentDictionary<string, IInteractionHandler> _handlers = new();

    public void Register(IInteractionHandler handler)
        => _handlers[handler.CustomIdPrefix] = handler;

    /// <summary>Retorna true se o customId usa ':' e o prefix está registrado.</summary>
    public bool CanRoute(string customId)
    {
        var parts = customId.Split(':', 2);
        return parts.Length == 2 && _handlers.ContainsKey(parts[0]);
    }

    public static string[] ParseParts(string customId) => customId.Split(':');

    /// <summary>
    /// Tenta rotear. Retorna true e chama o handler se prefix reconhecido.
    /// Retorna false sem lançar se customId não pertence ao router.
    /// Exceções de HandleAsync propagam para o caller (CommandHandler trata via try/catch).
    /// </summary>
    public async Task<bool> TryRouteAsync(SocketMessageComponent component)
    {
        var customId = component.Data.CustomId;
        if (!CanRoute(customId)) return false;
        var parts = ParseParts(customId);
        await _handlers[parts[0]].HandleAsync(component, parts);
        return true;
    }
}
