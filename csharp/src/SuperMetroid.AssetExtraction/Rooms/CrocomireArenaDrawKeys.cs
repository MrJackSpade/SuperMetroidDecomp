using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Crocomire arena draw (<see cref="CrocomireArenaDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static class CrocomireArenaDrawKeys
{
    /// <summary>Each CrocomireArenaDraw member's installed key.</summary>
    internal static InstalledKeys<CrocomireArenaDraw> Installed { get; } = new(new Dictionary<CrocomireArenaDraw, string>
    {
        [CrocomireArenaDraw.ClearBridge] = "clear-bridge",
        [CrocomireArenaDraw.CrumbleBridgeBlock] = "crumble-bridge-block",
        [CrocomireArenaDraw.ClearBridgeBlock] = "clear-bridge-block",
        [CrocomireArenaDraw.ClearInvisibleWall] = "clear-invisible-wall",
        [CrocomireArenaDraw.CreateInvisibleWall] = "create-invisible-wall",
    });
}
