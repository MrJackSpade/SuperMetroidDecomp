using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="DoorHeaderSources"/> that only verification reads.</summary>
internal static class DoorHeaderSourcesConstants
{
    /// <summary>The record pointer of the door named like <paramref name="door"/>.</summary>
    public static ushort PointerOf(DoorDefinition door)
    {
        Ensure.NotNull(door);
        // Two door blocks of the Fast Pillars Setup room lead through one door the cartridge stored twice; its first record serves.
        return DoorHeaderSourceDefinitions.ByPointer.Where(pair => pair.Value.Name == door.Name).Min(pair => pair.Key);
    }
}
