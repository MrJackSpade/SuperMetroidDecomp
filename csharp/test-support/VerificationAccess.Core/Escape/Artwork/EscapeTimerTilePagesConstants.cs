namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Cartridge values and helpers of <see cref="EscapeTimerTilePages"/> that only verification reads.</summary>
internal static class EscapeTimerTilePagesConstants
{
    /// <summary>Both native timer transfer pages in their upload order.</summary>
    internal static readonly EscapeTimerTilePage[] Pages = Enum.GetValues<EscapeTimerTilePage>();
    /// <summary>The page's native long source address.</summary>
    public static int SourceAddressOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileRomData.FirstSourceAddress,
        EscapeTimerTilePage.Second => EscapeTimerTileRomDataConstants.SecondSourceAddress,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };
    /// <summary>Identifies the page a native source/size descriptor uploads.</summary>
    public static bool TryMatch(int sourceAddress, int byteCount, out EscapeTimerTilePage page)
    {
        foreach (EscapeTimerTilePage candidate in Pages)
        {
            if (SourceAddressOf(candidate) == sourceAddress && EscapeTimerTilePages.ByteCountOf(candidate) == byteCount)
            {
                page = candidate;
                return true;
            }
        }
        page = default;
        return false;
    }
}
