using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

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
}
