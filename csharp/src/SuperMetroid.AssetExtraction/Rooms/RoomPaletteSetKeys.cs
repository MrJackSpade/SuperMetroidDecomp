using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed key of each <see cref="RoomPaletteSet"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class RoomPaletteSetKeys
{
    /// <summary>Each RoomPaletteSet member's installed key.</summary>
    internal static InstalledKeys<RoomPaletteSet> Installed { get; } = new(new Dictionary<RoomPaletteSet, string>
    {
        [RoomPaletteSet.UpperCrateria] = "UpperCrateria",
        [RoomPaletteSet.RedCrateria] = "RedCrateria",
        [RoomPaletteSet.LowerCrateria] = "LowerCrateria",
        [RoomPaletteSet.OldTourian] = "OldTourian",
        [RoomPaletteSet.WreckedShipPowerOn] = "WreckedShipPowerOn",
        [RoomPaletteSet.WreckedShipPowerOff] = "WreckedShipPowerOff",
        [RoomPaletteSet.GreenBlueBrinstar] = "GreenBlueBrinstar",
        [RoomPaletteSet.RedBrinstarKraid] = "RedBrinstarKraid",
        [RoomPaletteSet.StatuesHall] = "StatuesHall",
        [RoomPaletteSet.HeatedNorfair] = "HeatedNorfair",
        [RoomPaletteSet.UnheatedNorfair] = "UnheatedNorfair",
        [RoomPaletteSet.SandlessMaridia] = "SandlessMaridia",
        [RoomPaletteSet.SandyMaridia] = "SandyMaridia",
        [RoomPaletteSet.Tourian] = "Tourian",
        [RoomPaletteSet.MotherBrain] = "MotherBrain",
        [RoomPaletteSet.BlueCeres] = "BlueCeres",
        [RoomPaletteSet.WhiteCeres] = "WhiteCeres",
        [RoomPaletteSet.MapStatues] = "MapStatues",
        [RoomPaletteSet.WreckedShipMapPowerOff] = "WreckedShipMapPowerOff",
        [RoomPaletteSet.BlueRefill] = "BlueRefill",
        [RoomPaletteSet.YellowRefill] = "YellowRefill",
        [RoomPaletteSet.SaveStation] = "SaveStation",
        [RoomPaletteSet.Kraid] = "Kraid",
        [RoomPaletteSet.Crocomire] = "Crocomire",
        [RoomPaletteSet.Draygon] = "Draygon",
    });
}
