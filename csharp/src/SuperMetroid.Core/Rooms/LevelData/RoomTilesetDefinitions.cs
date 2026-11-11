namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Graphics-theme dispatch to independently installed room artwork resources.</summary>
public static class RoomTilesetDefinitions
{
    /// <summary>The number of room graphics themes: one per <see cref="RoomGraphicsSet"/>.</summary>
    public const int Count = 29;

    /// <summary>The theme of each retail graphics set, in graphics-set order.</summary>
    private static readonly RoomGraphicsSet[] GraphicsSetThemes =
    [
        RoomGraphicsSet.UpperCrateria, RoomGraphicsSet.RedCrateria, RoomGraphicsSet.LowerCrateria, RoomGraphicsSet.OldTourian, RoomGraphicsSet.WreckedShipPowerOn,
        RoomGraphicsSet.WreckedShipPowerOff, RoomGraphicsSet.GreenBlueBrinstar, RoomGraphicsSet.RedBrinstarKraid, RoomGraphicsSet.StatuesHall, RoomGraphicsSet.HeatedNorfair,
        RoomGraphicsSet.UnheatedNorfair, RoomGraphicsSet.SandlessMaridia, RoomGraphicsSet.SandyMaridia, RoomGraphicsSet.Tourian, RoomGraphicsSet.MotherBrain, RoomGraphicsSet.BlueCeres,
        RoomGraphicsSet.WhiteCeres, RoomGraphicsSet.BlueCeresElevator, RoomGraphicsSet.WhiteCeresElevator, RoomGraphicsSet.BlueCeresRidley, RoomGraphicsSet.WhiteCeresRidley,
        RoomGraphicsSet.MapStatues, RoomGraphicsSet.WreckedShipMapPowerOff, RoomGraphicsSet.BlueRefill, RoomGraphicsSet.YellowRefill, RoomGraphicsSet.SaveStation, RoomGraphicsSet.Kraid,
        RoomGraphicsSet.Crocomire, RoomGraphicsSet.Draygon,
    ];

    /// <summary>The tileset of each retail graphics set, in graphics-set order.</summary>
    internal static IReadOnlyList<TilesetDefinition> Tilesets { get; } =
        [.. GraphicsSetThemes.Select(theme => new TilesetDefinition(theme))];
}
