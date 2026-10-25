namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresEscapeTileCharacters"/> that only verification reads.</summary>
internal static class CeresEscapeTileCharactersConstants
{
    /// <summary>The page range a native escape transfer from <paramref name="sourceAddress"/> uploads, or null outside both pages.</summary>
    /// <param name="sourceAddress">The transfer's native source.</param>
    /// <param name="byteCount">Bytes the transfer moves.</param>
    internal static CeresEscapeTileCharacters? ForTransfer(int sourceAddress, int byteCount)
    {
        for (int page = 0; page < CeresEscapeTileArtworkDefinitions.All.Length; page++)
        {
            int offset = sourceAddress - CeresEscapeNativeSources.PageSource(page);
            if (offset >= 0 && byteCount > 0 && offset <= CeresEscapeTileArtworkDefinitions.All[page].ByteCount - byteCount)
                return new CeresEscapeTileCharacters(page, offset, byteCount);
        }
        return null;
    }
}
