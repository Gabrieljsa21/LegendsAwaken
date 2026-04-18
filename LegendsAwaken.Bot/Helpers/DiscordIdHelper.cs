using System;

namespace LegendsAwaken.Bot.Helpers;

public static class DiscordIdHelper
{
    public static Guid ToGuid(ulong discordId)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(discordId).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
