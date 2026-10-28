namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Native transfer records of the escape-timer character pages.</summary>
public static class EscapeTimerTilePages
{

    /// <summary>The page's native upload size in bytes.</summary>
    public static ushort ByteCountOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstByteCount,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondByteCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>The OBJ character the page begins at.</summary>
    public static int FirstCharacterOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstCharacter,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondCharacter,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>The typed VRAM asset installed for the page.</summary>
    public static InstalledArtworkId AssetOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => InstalledArtworkId.EscapeTimerFirstTiles,
        EscapeTimerTilePage.Second => InstalledArtworkId.EscapeTimerSecondTiles,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };
}
