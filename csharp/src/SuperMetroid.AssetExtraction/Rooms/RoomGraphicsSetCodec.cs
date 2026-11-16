using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The cartridge's numbering of the room graphics sets, decoded once at import: a room state's graphics-set byte and
/// the position of each set's entry in the cartridge's tileset table. The game names a room state's graphics set and
/// never sees the number.
/// </summary>
internal static class RoomGraphicsSetCodec
{
    /// <summary>Each graphics set at the position of its number in the cartridge.</summary>
    private static readonly RoomGraphicsSet[] Numbered =
    [
        RoomGraphicsSet.UpperCrateria, RoomGraphicsSet.RedCrateria, RoomGraphicsSet.LowerCrateria, RoomGraphicsSet.OldTourian, RoomGraphicsSet.WreckedShipPowerOn,
        RoomGraphicsSet.WreckedShipPowerOff, RoomGraphicsSet.GreenBlueBrinstar, RoomGraphicsSet.RedBrinstarKraid, RoomGraphicsSet.StatuesHall, RoomGraphicsSet.HeatedNorfair,
        RoomGraphicsSet.UnheatedNorfair, RoomGraphicsSet.SandlessMaridia, RoomGraphicsSet.SandyMaridia, RoomGraphicsSet.Tourian, RoomGraphicsSet.MotherBrain, RoomGraphicsSet.BlueCeres,
        RoomGraphicsSet.WhiteCeres, RoomGraphicsSet.BlueCeresElevator, RoomGraphicsSet.WhiteCeresElevator, RoomGraphicsSet.BlueCeresRidley, RoomGraphicsSet.WhiteCeresRidley,
        RoomGraphicsSet.MapStatues, RoomGraphicsSet.WreckedShipMapPowerOff, RoomGraphicsSet.BlueRefill, RoomGraphicsSet.YellowRefill, RoomGraphicsSet.SaveStation, RoomGraphicsSet.Kraid,
        RoomGraphicsSet.Crocomire, RoomGraphicsSet.Draygon,
    ];

    /// <summary>The number of graphics sets the cartridge numbers, one per <see cref="RoomGraphicsSet"/>.</summary>
    public static int Count => Numbered.Length;

    /// <summary>The graphics set the cartridge numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="number"/> numbers no graphics set.</exception>
    public static RoomGraphicsSet Decode(int number) =>
        (uint)number < (uint)Numbered.Length
            ? Numbered[number]
            : throw new InvalidDataException($"Graphics set number {number} numbers none of the {Numbered.Length} room graphics sets.");

    /// <summary>The number the cartridge gives <paramref name="graphicsSet"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="graphicsSet"/> is not a room graphics set.</exception>
    public static int Encode(RoomGraphicsSet graphicsSet)
    {
        int number = Array.IndexOf(Numbered, graphicsSet);
        return number >= 0 ? number : throw new ArgumentOutOfRangeException(nameof(graphicsSet), graphicsSet, "Not a room graphics set.");
    }
}
