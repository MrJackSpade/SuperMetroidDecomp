using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Crocomire arena draw (<see cref="CrocomireArenaMutation"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class CrocomireArenaDrawKeys
{
    /// <summary>Each CrocomireArenaMutation member's installed key.</summary>
    internal static InstalledKeys<CrocomireArenaMutation> Installed { get; } = new(new Dictionary<CrocomireArenaMutation, string>
    {
        [CrocomireArenaMutation.ClearBridge] = "clear-bridge",
        [CrocomireArenaMutation.CrumbleBridgeBlock] = "crumble-bridge-block",
        [CrocomireArenaMutation.ClearBridgeBlock] = "clear-bridge-block",
        [CrocomireArenaMutation.ClearInvisibleWall] = "clear-invisible-wall",
        [CrocomireArenaMutation.CreateInvisibleWall] = "create-invisible-wall",
    });
}
