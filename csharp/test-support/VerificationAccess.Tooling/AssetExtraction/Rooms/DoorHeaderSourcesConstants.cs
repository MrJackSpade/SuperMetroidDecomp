using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="DoorHeaderSources"/> that only verification reads.</summary>
internal static class DoorHeaderSourcesConstants
{
    /// <summary>The record pointer of the door named like <paramref name="door"/>.</summary>
    public static ushort PointerOf(CartridgeDoorHeader door)
    {
        Ensure.NotNull(door);
        return DoorHeaderSourceDefinitions.ByPointer.Single(pair => pair.Value.Name == door.Name).Key;
    }
}
