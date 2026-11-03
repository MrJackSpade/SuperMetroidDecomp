using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where each gameplay background set lies in the cartridge's background colours (#1464, #1512): the import pipeline's
/// private knowledge of the colour image a gameplay background sheet's colour groups are installed from.
/// </summary>
[EncodingOwner("order of gameplay's background colour sets in the cartridge's colour image")]
internal static class InstalledBackgroundSetPositionDefinitions
{
    /// <summary>The background sets in the order the cartridge's colour image holds them, sixteen colours each.</summary>
    internal static readonly InstalledBackgroundSet[] InCartridgeOrder =
    [
        InstalledBackgroundSet.Hud,
        InstalledBackgroundSet.HudItemsAndEffects,
        InstalledBackgroundSet.TilesFirst,
        InstalledBackgroundSet.TilesSecond,
        InstalledBackgroundSet.TilesThird,
        InstalledBackgroundSet.TilesFourth,
        InstalledBackgroundSet.TilesFifth,
        InstalledBackgroundSet.TilesSixth,
    ];
}
