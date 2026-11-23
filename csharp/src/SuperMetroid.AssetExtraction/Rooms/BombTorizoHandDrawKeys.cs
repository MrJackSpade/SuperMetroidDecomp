using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Bomb Torizo hand draw (<see cref="BombTorizoHandDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class BombTorizoHandDrawKeys
{
    /// <summary>Each BombTorizoHandDraw member's installed key.</summary>
    internal static InstalledKeys<BombTorizoHandDraw> Installed { get; } = new(new Dictionary<BombTorizoHandDraw, string>
    {
        [BombTorizoHandDraw.Intact] = "intact",
        [BombTorizoHandDraw.Cleared] = "cleared",
    });
}
