namespace SuperMetroid.Core.Areas.Escape.Artwork;

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

    /// <summary>The page's native VRAM word destination.</summary>
    public static ushort DestinationWordOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstDestinationWord,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondDestinationWord,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The typed VRAM asset installed for the page.</summary>
    public static VramAssetId AssetOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => VramAssetId.EscapeTimerFirstTiles,
        EscapeTimerTilePage.Second => VramAssetId.EscapeTimerSecondTiles,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>True when a native source address is one of the pages' sources.</summary>
    public static bool IsSource(int sourceAddress)
    {
        foreach (EscapeTimerTilePage page in Pages)
            if (SourceAddressOf(page) == sourceAddress)
                return true;
        return false;
    }

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

    /// <summary>Identifies the page a complete native transfer record uploads.</summary>
    public static bool TryMatch(int sourceAddress, int byteCount, ushort destinationWord, out EscapeTimerTilePage page) =>
        TryMatch(sourceAddress, byteCount, out page) && DestinationWordOf(page) == destinationWord;
}
