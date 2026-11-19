using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed key of each <see cref="InstalledBackgroundSet"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class InstalledBackgroundSetKeys
{
    /// <summary>Each InstalledBackgroundSet member's installed key.</summary>
    internal static InstalledKeys<InstalledBackgroundSet> Installed { get; } = new(new Dictionary<InstalledBackgroundSet, string>
    {
        [InstalledBackgroundSet.Hud] = "Hud",
        [InstalledBackgroundSet.HudItemsAndEffects] = "HudItemsAndEffects",
        [InstalledBackgroundSet.TilesFirst] = "TilesFirst",
        [InstalledBackgroundSet.TilesSecond] = "TilesSecond",
        [InstalledBackgroundSet.TilesThird] = "TilesThird",
        [InstalledBackgroundSet.TilesFourth] = "TilesFourth",
        [InstalledBackgroundSet.TilesFifth] = "TilesFifth",
        [InstalledBackgroundSet.TilesSixth] = "TilesSixth",
    });
}
