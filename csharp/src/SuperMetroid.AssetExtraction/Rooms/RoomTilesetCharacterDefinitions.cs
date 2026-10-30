using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The room character sheet each retail graphics set uploads after the common room elements (#1466): the import's
/// knowledge, which the rooms' pictures are composed from. Gameplay names a tileset by its graphics set alone.
/// </summary>
internal static class RoomTilesetCharacterDefinitions
{
    /// <summary>The character sheet each graphics set uploads for its rooms' backgrounds.</summary>
    internal static readonly Dictionary<RoomGraphicsSet, RoomCharacterSet> CharacterSources = new()
    {
        [RoomGraphicsSet.UpperCrateria] = RoomCharacterSet.UpperCrateria,
        [RoomGraphicsSet.RedCrateria] = RoomCharacterSet.UpperCrateria,
        [RoomGraphicsSet.LowerCrateria] = RoomCharacterSet.LowerCrateria,
        [RoomGraphicsSet.OldTourian] = RoomCharacterSet.LowerCrateria,
        [RoomGraphicsSet.WreckedShipPowerOn] = RoomCharacterSet.WreckedShip,
        [RoomGraphicsSet.WreckedShipPowerOff] = RoomCharacterSet.WreckedShip,
        [RoomGraphicsSet.GreenBlueBrinstar] = RoomCharacterSet.GreenBlueBrinstar,
        [RoomGraphicsSet.RedBrinstarKraid] = RoomCharacterSet.RedBrinstarKraidStatuesHall,
        [RoomGraphicsSet.StatuesHall] = RoomCharacterSet.RedBrinstarKraidStatuesHall,
        [RoomGraphicsSet.HeatedNorfair] = RoomCharacterSet.Norfair,
        [RoomGraphicsSet.UnheatedNorfair] = RoomCharacterSet.Norfair,
        [RoomGraphicsSet.SandlessMaridia] = RoomCharacterSet.SandlessMaridia,
        [RoomGraphicsSet.SandyMaridia] = RoomCharacterSet.SandyMaridia,
        [RoomGraphicsSet.Tourian] = RoomCharacterSet.Tourian,
        [RoomGraphicsSet.MotherBrain] = RoomCharacterSet.Tourian,
        [RoomGraphicsSet.BlueCeres] = RoomCharacterSet.Ceres,
        [RoomGraphicsSet.WhiteCeres] = RoomCharacterSet.Ceres,
        [RoomGraphicsSet.BlueCeresElevator] = RoomCharacterSet.CeresElevator,
        [RoomGraphicsSet.WhiteCeresElevator] = RoomCharacterSet.CeresElevator,
        [RoomGraphicsSet.BlueCeresRidley] = RoomCharacterSet.CeresRidley,
        [RoomGraphicsSet.WhiteCeresRidley] = RoomCharacterSet.CeresRidley,
        [RoomGraphicsSet.MapStatues] = RoomCharacterSet.UtilityRoomStatues,
        [RoomGraphicsSet.WreckedShipMapPowerOff] = RoomCharacterSet.UtilityRoomStatues,
        [RoomGraphicsSet.BlueRefill] = RoomCharacterSet.UtilityRoomStatues,
        [RoomGraphicsSet.YellowRefill] = RoomCharacterSet.UtilityRoomStatues,
        [RoomGraphicsSet.SaveStation] = RoomCharacterSet.UtilityRoomStatues,
        [RoomGraphicsSet.Kraid] = RoomCharacterSet.Kraid,
        [RoomGraphicsSet.Crocomire] = RoomCharacterSet.Crocomire,
        [RoomGraphicsSet.Draygon] = RoomCharacterSet.Draygon,
    };
}
