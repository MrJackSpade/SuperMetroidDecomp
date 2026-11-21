namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="RoomGraphicsSet"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class RoomGraphicsSetKeys
{
    /// <summary>Each RoomGraphicsSet member's installed key.</summary>
    internal static InstalledKeys<RoomGraphicsSet> Installed { get; } = new(new Dictionary<RoomGraphicsSet, string>
    {
        [RoomGraphicsSet.UpperCrateria] = "UpperCrateria",
        [RoomGraphicsSet.RedCrateria] = "RedCrateria",
        [RoomGraphicsSet.LowerCrateria] = "LowerCrateria",
        [RoomGraphicsSet.OldTourian] = "OldTourian",
        [RoomGraphicsSet.WreckedShipPowerOn] = "WreckedShipPowerOn",
        [RoomGraphicsSet.WreckedShipPowerOff] = "WreckedShipPowerOff",
        [RoomGraphicsSet.GreenBlueBrinstar] = "GreenBlueBrinstar",
        [RoomGraphicsSet.RedBrinstarKraid] = "RedBrinstarKraid",
        [RoomGraphicsSet.StatuesHall] = "StatuesHall",
        [RoomGraphicsSet.HeatedNorfair] = "HeatedNorfair",
        [RoomGraphicsSet.UnheatedNorfair] = "UnheatedNorfair",
        [RoomGraphicsSet.SandlessMaridia] = "SandlessMaridia",
        [RoomGraphicsSet.SandyMaridia] = "SandyMaridia",
        [RoomGraphicsSet.Tourian] = "Tourian",
        [RoomGraphicsSet.MotherBrain] = "MotherBrain",
        [RoomGraphicsSet.BlueCeres] = "BlueCeres",
        [RoomGraphicsSet.WhiteCeres] = "WhiteCeres",
        [RoomGraphicsSet.BlueCeresElevator] = "BlueCeresElevator",
        [RoomGraphicsSet.WhiteCeresElevator] = "WhiteCeresElevator",
        [RoomGraphicsSet.BlueCeresRidley] = "BlueCeresRidley",
        [RoomGraphicsSet.WhiteCeresRidley] = "WhiteCeresRidley",
        [RoomGraphicsSet.MapStatues] = "MapStatues",
        [RoomGraphicsSet.WreckedShipMapPowerOff] = "WreckedShipMapPowerOff",
        [RoomGraphicsSet.BlueRefill] = "BlueRefill",
        [RoomGraphicsSet.YellowRefill] = "YellowRefill",
        [RoomGraphicsSet.SaveStation] = "SaveStation",
        [RoomGraphicsSet.Kraid] = "Kraid",
        [RoomGraphicsSet.Crocomire] = "Crocomire",
        [RoomGraphicsSet.Draygon] = "Draygon",
    });
}
