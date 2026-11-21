using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Chozo statue draw (<see cref="ChozoStatueDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class ChozoStatueDrawKeys
{
    /// <summary>Each ChozoStatueDraw member's installed key.</summary>
    internal static InstalledKeys<ChozoStatueDraw> Installed { get; } = new(new Dictionary<ChozoStatueDraw, string>
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] = "lower-norfair-cleared-hand",
        [ChozoStatueDraw.ClearSlopeAccess] = "wrecked-ship-clear-slope-access",
        [ChozoStatueDraw.BlockSlopeAccess] = "wrecked-ship-block-slope-access",
    });
}
