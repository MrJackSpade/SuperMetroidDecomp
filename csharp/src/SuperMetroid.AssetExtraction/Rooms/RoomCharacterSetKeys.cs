using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed key of each <see cref="RoomCharacterSet"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class RoomCharacterSetKeys
{
    /// <summary>Each RoomCharacterSet member's installed key.</summary>
    internal static InstalledKeys<RoomCharacterSet> Installed { get; } = new(new Dictionary<RoomCharacterSet, string>
    {
        [RoomCharacterSet.UpperCrateria] = "UpperCrateria",
        [RoomCharacterSet.LowerCrateria] = "LowerCrateria",
        [RoomCharacterSet.WreckedShip] = "WreckedShip",
        [RoomCharacterSet.GreenBlueBrinstar] = "GreenBlueBrinstar",
        [RoomCharacterSet.RedBrinstarKraidStatuesHall] = "RedBrinstarKraidStatuesHall",
        [RoomCharacterSet.Norfair] = "Norfair",
        [RoomCharacterSet.SandlessMaridia] = "SandlessMaridia",
        [RoomCharacterSet.SandyMaridia] = "SandyMaridia",
        [RoomCharacterSet.Tourian] = "Tourian",
        [RoomCharacterSet.Ceres] = "Ceres",
        [RoomCharacterSet.CeresElevator] = "CeresElevator",
        [RoomCharacterSet.CeresRidley] = "CeresRidley",
        [RoomCharacterSet.UtilityRoomStatues] = "UtilityRoomStatues",
        [RoomCharacterSet.Kraid] = "Kraid",
        [RoomCharacterSet.Crocomire] = "Crocomire",
        [RoomCharacterSet.Draygon] = "Draygon",
        [RoomCharacterSet.TourianStatueGhost] = "TourianStatueGhost",
    });
}
