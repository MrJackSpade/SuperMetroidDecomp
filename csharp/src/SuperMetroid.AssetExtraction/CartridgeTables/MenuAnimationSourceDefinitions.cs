using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the menu missile's and the file-select helmet's frame timers (#1615): the import pipeline's
/// private knowledge of the cartridge, decoded once into the installed menu presentations.
/// </summary>
[EncodingOwner("cartridge addresses of the menu missile's and file-select helmet's frame timers")]
internal static class MenuAnimationSourceDefinitions
{
    /// <summary>$82:BAAA, the menu missile's frame timer reloads, one word for each of its frames.</summary>
    internal const int MissileFrameDurations = 0x82baaa;

    /// <summary>$81:9DF4, the immediate the file-select helmet's frame timer is reloaded with.</summary>
    internal const int HelmetFrameDuration = 0x819df4;
}
