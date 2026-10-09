namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous four-bit character page used by the Ceres escape DMA lists.</summary>
/// <param name="SourceAddress">Starting cartridge address of the page's character bytes.</param>
/// <param name="ByteCount">Number of bytes in the contiguous page.</param>
/// <param name="FileName">PNG asset name used to load the editable page artwork.</param>
internal readonly record struct CeresEscapeTileSheetDefinition(
    int SourceAddress, int ByteCount, string FileName);

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

    /// <summary>Provides the warning-text and door character pages in their fixed transfer order.</summary>
    internal static PageSequence All => default;

    /// <summary>Enumerates the fixed warning-text page followed by the Ceres door page.</summary>
    internal readonly struct PageSequence : IReadOnlyList<CeresEscapeTileSheetDefinition>
    {
        /// <summary>Gets the number of defined Ceres escape character pages.</summary>
        public int Count => 2;
        /// <summary>Gets the page count for callers that use sequence length terminology.</summary>
        public int Length => Count;
        /// <summary>Gets the warning-text or door page at the requested position.</summary>
        /// <param name="index">Zero for warning text or one for doors.</param>
        /// <returns>The page definition at <paramref name="index"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">The index is not zero or one.</exception>
        public CeresEscapeTileSheetDefinition this[int index] => index switch
        {
            0 => WarningText,
            1 => Doors,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Iterates over the warning-text page and then the door page.</summary>
        /// <returns>An enumerator over the two fixed page definitions.</returns>
        public IEnumerator<CeresEscapeTileSheetDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Checks whether a positive byte range lies wholly within either defined page.</summary>
    /// <param name="sourceAddress">Starting cartridge address of the requested range.</param>
    /// <param name="byteCount">Number of bytes that the range spans.</param>
    /// <returns><see langword="true"/> when the complete range fits in one page; otherwise, <see langword="false"/>.</returns>
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

/// <summary>
/// Editable Ceres escape warning and door characters. The native transfer order,
/// sizes and destinations remain compiled gameplay/presentation metadata.
/// </summary>
public sealed class CeresEscapeTileArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresEscapeTileArtwork-v1", content =>
        {
            content.Append("pages", pages.Length);
            foreach (RoomCharacterAtlas page in pages)
                content.Append("tiles", page.Transfer.Span);
        });

    /// <summary>Decoded character pages, stored in the same order as the fixed transfer definitions.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Creates an editable artwork set after validating that every defined page is present at its expected size.</summary>
    /// <param name="pages">Decoded character atlases ordered as warning text, then doors.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pages"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidDataException">The page count or a page's transfer size does not match its definition.</exception>
    internal CeresEscapeTileArtwork(RoomCharacterAtlas[] pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        if (pages.Length != CeresEscapeTileArtworkDefinitions.All.Length)
            throw new InvalidDataException("Ceres escape artwork requires both tile pages.");
        for (int index = 0; index < pages.Length; index++)
        {
            if (pages[index] is null || pages[index].Transfer.Length !=
                CeresEscapeTileArtworkDefinitions.All[index].ByteCount)
                throw new InvalidDataException(
                    $"Ceres escape tile page {index} has the wrong size.");
        }
        this.pages = [.. pages];
    }

    /// <summary>Resolves a cartridge character range to its editable bytes when the range belongs to a defined page.</summary>
    /// <param name="sourceAddress">Starting cartridge address of the requested character range.</param>
    /// <param name="byteCount">Number of bytes requested from the page.</param>
    /// <param name="characters">Receives the matching slice, or an empty value when no page contains the range.</param>
    /// <returns><see langword="true"/> if the entire positive range is contained in one page; otherwise, <see langword="false"/>.</returns>
    internal bool TryResolve(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> characters)
    {
        for (int index = 0; index < pages.Length; index++)
        {
            CeresEscapeTileSheetDefinition page =
                CeresEscapeTileArtworkDefinitions.All[index];
            int offset = sourceAddress - page.SourceAddress;
            if (offset < 0 || byteCount <= 0 ||
                offset > page.ByteCount - byteCount)
                continue;
            characters = pages[index].Transfer.Slice(offset, byteCount);
            return true;
        }
        characters = default;
        return false;
    }
}
