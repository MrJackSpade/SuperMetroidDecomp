namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Named presentation sources for the fixed Ceres escape transfer metadata.</summary>
internal static class CeresEscapeTileArtworkDefinitions
{
    /// <summary>Escape timer warning-text characters at $B7:DA00-E2FF.</summary>
    internal static readonly CeresEscapeTileSheetDefinition WarningText =
        new(CeresEscapeTileRomData.WarningTextSource,
            CeresEscapeTileRomData.WarningTextByteCount, "ceres-escape-warning-tiles.png");

    /// <summary>Ceres escape door characters at $B0:BA00-BFFF.</summary>
    internal static readonly CeresEscapeTileSheetDefinition Doors =
        new(CeresEscapeTileRomData.DoorSource,
            CeresEscapeTileRomData.DoorByteCount, "ceres-escape-door-tiles.png");

    /// <summary>Enumerates the warning-text page before the door page in native transfer order.</summary>
    internal static PageSequence All => default;

    /// <summary>Provides the two fixed Ceres escape tile pages without allocating a collection.</summary>
    internal readonly struct PageSequence : IReadOnlyList<CeresEscapeTileSheetDefinition>
    {
        /// <summary>Both warning text and door art must be installed.</summary>
        public int Count => 2;
        /// <summary>Alias for the fixed page count used by installation validation.</summary>
        public int Length => Count;
        /// <summary>Selects warning text first and door art second; rejects other indices.</summary>
        public CeresEscapeTileSheetDefinition this[int index] => index switch
        {
            0 => WarningText,
            1 => Doors,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Visits the two pages in their transfer order.</summary>
        public IEnumerator<CeresEscapeTileSheetDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        /// <summary>Exposes the same page ordering through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Accepts only a positive byte range wholly inside one of the two Ceres escape DMA pages.</summary>
    internal static bool Contains(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeTileSheetDefinition page in All)
        {
            int offset = sourceAddress - page.SourceAddress;
            if (offset >= 0 && byteCount > 0 && offset <= page.ByteCount - byteCount)
                return true;
        }
        return false;
    }
}
