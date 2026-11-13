using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="RoomHeaderSources"/> that only verification reads.</summary>
internal static class RoomHeaderSourcesConstants
{
    /// <summary>Whether <paramref name="pointer"/> is exactly one of the compiled room headers.</summary>
    public static bool Contains(ushort pointer) => RoomHeaderSourceDefinitions.ByPointer.ContainsKey(pointer);
    /// <summary>The header pointer of the room named like <paramref name="room"/>.</summary>
    /// <exception cref="InvalidOperationException">No compiled room header, or more than one, is named like the room.</exception>
    public static ushort PointerOf(RoomDefinition room)
    {
        Ensure.NotNull(room);
        return PointersByName.Value[room.Name].Single();
    }

    /// <summary>The compiled room headers' pointers by room name, read once since checks name a room's pointer per block they report.</summary>
    private static readonly Lazy<ILookup<string, ushort>> PointersByName =
        new(() => RoomHeaderSourceDefinitions.ByPointer.ToLookup(pair => pair.Value.Name, pair => pair.Key, StringComparer.Ordinal));
}
