using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each shot-block shape (<see cref="BlockArrangement"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class BlockArrangementKeys
{
    /// <summary>Each BlockArrangement member's installed key.</summary>
    internal static InstalledKeys<BlockArrangement> Installed { get; } = new(new Dictionary<BlockArrangement, string>
    {
        [BlockArrangement.Single] = "single",
        [BlockArrangement.Horizontal] = "horizontal",
        [BlockArrangement.Vertical] = "vertical",
        [BlockArrangement.Square] = "square",
    });
}
