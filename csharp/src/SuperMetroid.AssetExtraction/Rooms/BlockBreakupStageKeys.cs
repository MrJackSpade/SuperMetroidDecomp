using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each block breakup stage (<see cref="BlockBreakupStage"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static class BlockBreakupStageKeys
{
    /// <summary>Each BlockBreakupStage member's installed key.</summary>
    internal static InstalledKeys<BlockBreakupStage> Installed { get; } = new(new Dictionary<BlockBreakupStage, string>
    {
        [BlockBreakupStage.Rock] = "rock",
        [BlockBreakupStage.Pieces] = "pieces",
        [BlockBreakupStage.Dust] = "dust",
        [BlockBreakupStage.Gone] = "gone",
    });
}
