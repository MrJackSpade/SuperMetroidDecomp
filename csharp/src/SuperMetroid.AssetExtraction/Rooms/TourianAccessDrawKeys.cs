using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Tourian access floor draw (<see cref="TourianAccessDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class TourianAccessDrawKeys
{
    /// <summary>Each TourianAccessDraw member's installed key.</summary>
    internal static InstalledKeys<TourianAccessDraw> Installed { get; } = new(new Dictionary<TourianAccessDraw, string>
    {
        [TourianAccessDraw.EmptyRow] = "crumble-empty-row",
        [TourianAccessDraw.Clear] = "clear-six-rows",
    });
}
