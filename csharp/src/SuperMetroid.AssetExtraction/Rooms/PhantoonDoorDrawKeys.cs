using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Phantoon door frame (<see cref="PhantoonDoorDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class PhantoonDoorDrawKeys
{
    /// <summary>Each PhantoonDoorDraw member's installed key.</summary>
    internal static InstalledKeys<PhantoonDoorDraw> Installed { get; } = new(new Dictionary<PhantoonDoorDraw, string>
    {
        [PhantoonDoorDraw.FightDoor] = "fight-door",
        [PhantoonDoorDraw.RestoredDoor] = "restored-door",
    });
}
