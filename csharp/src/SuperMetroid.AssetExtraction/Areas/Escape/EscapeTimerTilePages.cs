using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Transfer records of the escape-timer character pages.</summary>
public static class EscapeTimerTilePages
{
    /// <summary>The typed tile asset installed for the page.</summary>
    public static InstalledArtworkId AssetOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => InstalledArtworkId.EscapeTimerFirstTiles,
        EscapeTimerTilePage.Second => InstalledArtworkId.EscapeTimerSecondTiles,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>Each timer page, by the typed tile asset installed for it.</summary>
    private static readonly Dictionary<InstalledArtworkId, EscapeTimerTilePage> PagesByAsset =
        Enum.GetValues<EscapeTimerTilePage>().ToDictionary(AssetOf);

    /// <summary>The timer page <paramref name="asset"/> is installed for, when it is one.</summary>
    public static bool TryPageOf(InstalledArtworkId asset, out EscapeTimerTilePage page) => PagesByAsset.TryGetValue(asset, out page);
}
