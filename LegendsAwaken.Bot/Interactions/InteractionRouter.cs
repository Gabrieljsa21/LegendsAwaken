using Discord.WebSocket;

namespace LegendsAwaken.Bot.Interactions;

public class InteractionRouter
{
    private readonly Dictionary<string, IInteractionHandler> _handlers = new();

    public void Register(IInteractionHandler handler)
        => _handlers[handler.CustomIdPrefix] = handler;

    /// <summary>Retorna true se o customId usa ':' e o prefix está registrado.</summary>
    public bool CanRoute(string customId)
    {
        if (!customId.Contains(':')) return false;
        var prefix = customId.Split(':')[0];
        return _handlers.ContainsKey(prefix);
    }

    public static string[] ParseParts(string customId) => customId.Split(':');

    /// <summary>
    /// Tenta rotear. Retorna true e chama o handler se prefix reconhecido.
    /// Retorna false sem lançar se customId não pertence ao router.
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
