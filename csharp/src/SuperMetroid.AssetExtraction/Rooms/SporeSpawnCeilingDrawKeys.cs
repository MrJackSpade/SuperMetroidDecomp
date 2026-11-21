using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Spore Spawn ceiling frame (<see cref="SporeSpawnCeilingDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class SporeSpawnCeilingDrawKeys
{
    /// <summary>Each SporeSpawnCeilingDraw member's installed key.</summary>
    internal static InstalledKeys<SporeSpawnCeilingDraw> Installed { get; } = new(new Dictionary<SporeSpawnCeilingDraw, string>
    {
        [SporeSpawnCeilingDraw.Clear] = "clear-ceiling",
        [SporeSpawnCeilingDraw.CeilingCracked] = "ceiling-cracked",
        [SporeSpawnCeilingDraw.CeilingCrumbling] = "ceiling-crumbling",
        [SporeSpawnCeilingDraw.CeilingCrumbled] = "ceiling-crumbled",
    });
}
