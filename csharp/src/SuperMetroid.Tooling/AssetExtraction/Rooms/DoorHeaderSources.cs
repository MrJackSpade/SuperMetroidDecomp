using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between the bank-$83 door-record pointers the cartridge stores and the compiled doors
/// they address; the simulation knows only the named doors.
/// </summary>
public static class DoorHeaderSources
{
    /// <summary>The compiled door whose record is at <paramref name="pointer"/>.</summary>
    public static CartridgeDoorHeader Door(ushort pointer) => DoorHeaderSourceDefinitions.ByPointer.TryGetValue(pointer, out CartridgeDoorHeader? found)
        ? found
        : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not one of the 597 physical retail door records.");

    /// <summary>The record pointer of the door named like <paramref name="door"/>.</summary>
    public static ushort PointerOf(CartridgeDoorHeader door)
    {
        Ensure.NotNull(door);
        return DoorHeaderSourceDefinitions.ByPointer.Single(pair => pair.Value.Name == door.Name).Key;
    }
}
