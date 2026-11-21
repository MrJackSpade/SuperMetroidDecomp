namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The artwork resources a room state's graphics set selects.</summary>
public static partial class RoomTilesets
{
    /// <summary>The graphics sets in the order the tilesets are listed (#2567).</summary>
    private static readonly IReadOnlyList<RoomGraphicsSet> GraphicsSetOrder = Ensure.EveryMemberOnce<RoomGraphicsSet>(
    [
        RoomGraphicsSet.UpperCrateria,
        RoomGraphicsSet.RedCrateria,
        RoomGraphicsSet.LowerCrateria,
        RoomGraphicsSet.OldTourian,
        RoomGraphicsSet.WreckedShipPowerOn,
        RoomGraphicsSet.WreckedShipPowerOff,
        RoomGraphicsSet.GreenBlueBrinstar,
        RoomGraphicsSet.RedBrinstarKraid,
        RoomGraphicsSet.StatuesHall,
        RoomGraphicsSet.HeatedNorfair,
        RoomGraphicsSet.UnheatedNorfair,
        RoomGraphicsSet.SandlessMaridia,
        RoomGraphicsSet.SandyMaridia,
        RoomGraphicsSet.Tourian,
        RoomGraphicsSet.MotherBrain,
        RoomGraphicsSet.BlueCeres,
        RoomGraphicsSet.WhiteCeres,
        RoomGraphicsSet.BlueCeresElevator,
        RoomGraphicsSet.WhiteCeresElevator,
        RoomGraphicsSet.BlueCeresRidley,
        RoomGraphicsSet.WhiteCeresRidley,
        RoomGraphicsSet.MapStatues,
        RoomGraphicsSet.WreckedShipMapPowerOff,
        RoomGraphicsSet.BlueRefill,
        RoomGraphicsSet.YellowRefill,
        RoomGraphicsSet.SaveStation,
        RoomGraphicsSet.Kraid,
        RoomGraphicsSet.Crocomire,
        RoomGraphicsSet.Draygon,
    ]);

    /// <summary>The tileset of every room graphics set, one per <see cref="RoomGraphicsSet"/>.</summary>
    public static IReadOnlyList<TilesetDefinition> All { get; } =
        [.. GraphicsSetOrder.Select(graphicsSet => new TilesetDefinition(graphicsSet))];

    /// <summary>The tileset definition of the room state's graphics set, which names the set's pictures.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="graphicsSet"/> is not a room graphics set.</exception>
    public static TilesetDefinition Get(RoomGraphicsSet graphicsSet) =>
        Enum.IsDefined(graphicsSet)
            ? new TilesetDefinition(graphicsSet)
            : throw new ArgumentOutOfRangeException(nameof(graphicsSet), graphicsSet, "Not a room graphics set.");
}
