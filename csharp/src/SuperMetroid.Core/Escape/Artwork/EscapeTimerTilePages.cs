namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Native transfer records of the escape-timer character pages.</summary>
public static class EscapeTimerTilePages
{
    /// <summary>Both native timer transfer pages in their upload order.</summary>
    private static readonly EscapeTimerTilePage[] Pages = Enum.GetValues<EscapeTimerTilePage>();

    /// <summary>The page's native long source address.</summary>
    public static int SourceAddressOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileRomData.FirstSourceAddress,
        EscapeTimerTilePage.Second => EscapeTimerTileRomData.SecondSourceAddress,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The page's native upload size in bytes.</summary>
    public static ushort ByteCountOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstByteCount,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondByteCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The OBJ character the page begins at.</summary>
    public static int FirstCharacterOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstCharacter,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondCharacter,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The typed VRAM asset installed for the page.</summary>
    public static VramAssetId AssetOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => VramAssetId.EscapeTimerFirstTiles,
        EscapeTimerTilePage.Second => VramAssetId.EscapeTimerSecondTiles,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>Identifies the page a native source/size descriptor uploads.</summary>
    public static bool TryMatch(int sourceAddress, int byteCount, out EscapeTimerTilePage page)
    {
        foreach (EscapeTimerTilePage candidate in Pages)
        {
            if (SourceAddressOf(candidate) == sourceAddress && ByteCountOf(candidate) == byteCount)
            {
                page = candidate;
                return true;
            }
        }
        page = default;
        return false;
    }
}
