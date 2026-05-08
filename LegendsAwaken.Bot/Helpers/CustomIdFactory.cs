using System;

namespace LegendsAwaken.Bot.Helpers;

public static class CustomIdFactory
{
    public const string EventoEscolhaPrefix = "torre_evento_escolha";

    public static string EventoEscolha(Guid eventoId, string opcaoKey) =>
        $"{EventoEscolhaPrefix}:{eventoId}:{opcaoKey}";
}
