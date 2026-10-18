using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between the bank-$8F room-header pointers the cartridge stores and the compiled rooms
/// they address; the simulation knows only the named rooms.
/// </summary>
public static class RoomHeaderSources
{
    /// <summary>The compiled room whose header is at <paramref name="pointer"/>.</summary>
    public static RoomHeaderDefinition Room(ushort pointer) => RoomHeaderSourceDefinitions.ByPointer.TryGetValue(pointer, out RoomHeaderDefinition? found)
        ? found
        : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not one of the 262 retail room headers.");

    /// <summary>Whether <paramref name="pointer"/> is exactly one of the compiled room headers.</summary>
    public static bool Contains(ushort pointer) => RoomHeaderSourceDefinitions.ByPointer.ContainsKey(pointer);

    /// <summary>The header pointer of the room named like <paramref name="room"/>.</summary>
    public static ushort PointerOf(RoomHeaderDefinition room)
    {
        Ensure.NotNull(room);
        return RoomHeaderSourceDefinitions.ByPointer.Single(pair => pair.Value.Name == room.Name).Key;
    }
}
