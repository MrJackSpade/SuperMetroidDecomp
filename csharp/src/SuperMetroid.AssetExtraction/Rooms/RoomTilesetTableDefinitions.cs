using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The block table and background palette each retail graphics set's rooms are built from (#1616): the import's
/// knowledge, which the rooms' pictures are composed from. Gameplay names a tileset by its graphics set alone.
/// </summary>
internal static class RoomTilesetTableDefinitions
{
    /// <summary>The compressed block table each room theme's blocks use.</summary>
    internal static readonly Dictionary<RoomGraphicsSet, RoomBlockSet> BlockSources = new()
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
    internal static readonly Dictionary<RoomGraphicsSet, RoomPaletteSet> PaletteSources = new()
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
}
