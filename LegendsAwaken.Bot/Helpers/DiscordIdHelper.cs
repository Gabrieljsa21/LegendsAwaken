using System;
using System.Buffers.Binary;

namespace LegendsAwaken.Bot.Helpers;

public static class DiscordIdHelper
{
    public static Guid ToGuid(ulong discordId)
    {
        var bytes = new byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, discordId);
        return new Guid(bytes);
    }
}
