using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomHeaderSources"/> that only verification reads.</summary>
internal static class RoomHeaderSourcesConstants
{
    /// <summary>Whether <paramref name="pointer"/> is exactly one of the compiled room headers.</summary>
    public static bool Contains(ushort pointer) => RoomHeaderSourceDefinitions.ByPointer.ContainsKey(pointer);
    /// <summary>The header pointer of the room named like <paramref name="room"/>.</summary>
    public static ushort PointerOf(RoomHeaderDefinition room)
    {
        Ensure.NotNull(room);
        return RoomHeaderSourceDefinitions.ByPointer.Single(pair => pair.Value.Name == room.Name).Key;
    }
}
