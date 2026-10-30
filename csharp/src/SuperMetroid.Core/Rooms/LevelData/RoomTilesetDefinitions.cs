namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Native graphics-theme dispatch to independently installed room artwork resources.</summary>
public static class RoomTilesetDefinitions
{
    /// <summary>$8F:E7A7..E7DF: graphics themes $00..$1C precede unrelated data.</summary>
    public const int Count = 0x1d;

    /// <summary>The compressed block table each room theme's blocks use.</summary>
    private static readonly Dictionary<RoomGraphicsSet, RoomBlockSet> BlockSources = new()
    {
        [RoomGraphicsSet.UpperCrateria] = RoomBlockSet.UpperCrateria,
        [RoomGraphicsSet.RedCrateria] = RoomBlockSet.UpperCrateria,
        [RoomGraphicsSet.LowerCrateria] = RoomBlockSet.LowerCrateria,
        [RoomGraphicsSet.OldTourian] = RoomBlockSet.LowerCrateria,
        [RoomGraphicsSet.WreckedShipPowerOn] = RoomBlockSet.WreckedShip,
        [RoomGraphicsSet.WreckedShipPowerOff] = RoomBlockSet.WreckedShip,
        [RoomGraphicsSet.GreenBlueBrinstar] = RoomBlockSet.GreenBlueBrinstar,
        [RoomGraphicsSet.RedBrinstarKraid] = RoomBlockSet.RedBrinstarKraidStatuesHall,
        [RoomGraphicsSet.StatuesHall] = RoomBlockSet.RedBrinstarKraidStatuesHall,
        [RoomGraphicsSet.HeatedNorfair] = RoomBlockSet.Norfair,
        [RoomGraphicsSet.UnheatedNorfair] = RoomBlockSet.Norfair,
        [RoomGraphicsSet.SandlessMaridia] = RoomBlockSet.SandlessMaridia,
        [RoomGraphicsSet.SandyMaridia] = RoomBlockSet.SandyMaridia,
        [RoomGraphicsSet.Tourian] = RoomBlockSet.Tourian,
        [RoomGraphicsSet.MotherBrain] = RoomBlockSet.Tourian,
        [RoomGraphicsSet.BlueCeres] = RoomBlockSet.Ceres,
        [RoomGraphicsSet.WhiteCeres] = RoomBlockSet.Ceres,
        [RoomGraphicsSet.BlueCeresElevator] = RoomBlockSet.Ceres,
        [RoomGraphicsSet.WhiteCeresElevator] = RoomBlockSet.Ceres,
        [RoomGraphicsSet.BlueCeresRidley] = RoomBlockSet.Ceres,
        [RoomGraphicsSet.WhiteCeresRidley] = RoomBlockSet.Ceres,
        [RoomGraphicsSet.MapStatues] = RoomBlockSet.UtilityRoomStatues,
        [RoomGraphicsSet.WreckedShipMapPowerOff] = RoomBlockSet.UtilityRoomStatues,
        [RoomGraphicsSet.BlueRefill] = RoomBlockSet.UtilityRoomStatues,
        [RoomGraphicsSet.YellowRefill] = RoomBlockSet.UtilityRoomStatues,
        [RoomGraphicsSet.SaveStation] = RoomBlockSet.UtilityRoomStatues,
        [RoomGraphicsSet.Kraid] = RoomBlockSet.Kraid,
        [RoomGraphicsSet.Crocomire] = RoomBlockSet.Crocomire,
        [RoomGraphicsSet.Draygon] = RoomBlockSet.Draygon,
    };

    /// <summary>Each room theme's selected palette resource, including Ceres color variants.</summary>
    private static readonly Dictionary<RoomGraphicsSet, RoomPaletteSet> PaletteSources = new()
    {
        [RoomGraphicsSet.UpperCrateria] = RoomPaletteSet.UpperCrateria,
        [RoomGraphicsSet.RedCrateria] = RoomPaletteSet.RedCrateria,
        [RoomGraphicsSet.LowerCrateria] = RoomPaletteSet.LowerCrateria,
        [RoomGraphicsSet.OldTourian] = RoomPaletteSet.OldTourian,
        [RoomGraphicsSet.WreckedShipPowerOn] = RoomPaletteSet.WreckedShipPowerOn,
        [RoomGraphicsSet.WreckedShipPowerOff] = RoomPaletteSet.WreckedShipPowerOff,
        [RoomGraphicsSet.GreenBlueBrinstar] = RoomPaletteSet.GreenBlueBrinstar,
        [RoomGraphicsSet.RedBrinstarKraid] = RoomPaletteSet.RedBrinstarKraid,
        [RoomGraphicsSet.StatuesHall] = RoomPaletteSet.StatuesHall,
        [RoomGraphicsSet.HeatedNorfair] = RoomPaletteSet.HeatedNorfair,
        [RoomGraphicsSet.UnheatedNorfair] = RoomPaletteSet.UnheatedNorfair,
        [RoomGraphicsSet.SandlessMaridia] = RoomPaletteSet.SandlessMaridia,
        [RoomGraphicsSet.SandyMaridia] = RoomPaletteSet.SandyMaridia,
        [RoomGraphicsSet.Tourian] = RoomPaletteSet.Tourian,
        [RoomGraphicsSet.MotherBrain] = RoomPaletteSet.MotherBrain,
        [RoomGraphicsSet.BlueCeres] = RoomPaletteSet.BlueCeres,
        [RoomGraphicsSet.BlueCeresElevator] = RoomPaletteSet.BlueCeres,
        [RoomGraphicsSet.BlueCeresRidley] = RoomPaletteSet.BlueCeres,
        [RoomGraphicsSet.WhiteCeres] = RoomPaletteSet.WhiteCeres,
        [RoomGraphicsSet.WhiteCeresElevator] = RoomPaletteSet.WhiteCeres,
        [RoomGraphicsSet.WhiteCeresRidley] = RoomPaletteSet.WhiteCeres,
        [RoomGraphicsSet.MapStatues] = RoomPaletteSet.MapStatues,
        [RoomGraphicsSet.WreckedShipMapPowerOff] = RoomPaletteSet.WreckedShipMapPowerOff,
        [RoomGraphicsSet.BlueRefill] = RoomPaletteSet.BlueRefill,
        [RoomGraphicsSet.YellowRefill] = RoomPaletteSet.YellowRefill,
        [RoomGraphicsSet.SaveStation] = RoomPaletteSet.SaveStation,
        [RoomGraphicsSet.Kraid] = RoomPaletteSet.Kraid,
        [RoomGraphicsSet.Crocomire] = RoomPaletteSet.Crocomire,
        [RoomGraphicsSet.Draygon] = RoomPaletteSet.Draygon,
    };

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

    /// <summary>The identity, block and palette resources of each retail graphics set, in graphics-set order.</summary>
    internal static IReadOnlyList<TilesetDefinition> Tilesets { get; } =
        [.. GraphicsSetThemes.Select(theme => new TilesetDefinition(theme, BlockSources[theme], PaletteSources[theme]))];
}
