using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed key of each <see cref="RoomBlockSet"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class RoomBlockSetKeys
{
    /// <summary>Each RoomBlockSet member's installed key.</summary>
    internal static InstalledKeys<RoomBlockSet> Installed { get; } = new(new Dictionary<RoomBlockSet, string>
    {
        [RoomBlockSet.UpperCrateria] = "UpperCrateria",
        [RoomBlockSet.LowerCrateria] = "LowerCrateria",
        [RoomBlockSet.WreckedShip] = "WreckedShip",
        [RoomBlockSet.GreenBlueBrinstar] = "GreenBlueBrinstar",
        [RoomBlockSet.RedBrinstarKraidStatuesHall] = "RedBrinstarKraidStatuesHall",
        [RoomBlockSet.Norfair] = "Norfair",
        [RoomBlockSet.SandlessMaridia] = "SandlessMaridia",
        [RoomBlockSet.SandyMaridia] = "SandyMaridia",
        [RoomBlockSet.Tourian] = "Tourian",
        [RoomBlockSet.Ceres] = "Ceres",
        [RoomBlockSet.UtilityRoomStatues] = "UtilityRoomStatues",
        [RoomBlockSet.Kraid] = "Kraid",
        [RoomBlockSet.Crocomire] = "Crocomire",
        [RoomBlockSet.Draygon] = "Draygon",
    });
}
