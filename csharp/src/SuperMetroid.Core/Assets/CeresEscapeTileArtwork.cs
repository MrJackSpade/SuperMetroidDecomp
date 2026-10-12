namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous four-bit character page used by the Ceres escape DMA lists.</summary>
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

    /// <summary>Gets all.</summary>
    internal static PageSequence All => default;

    /// <summary>Represents page sequence.</summary>
    internal readonly struct PageSequence : IReadOnlyList<CeresEscapeTileSheetDefinition>
    {
        /// <summary>Gets count.</summary>
        public int Count => 2;
        /// <summary>Gets length.</summary>
        public int Length => Count;
        /// <summary>Gets an indexed value.</summary>
        public CeresEscapeTileSheetDefinition this[int index] => index switch
        {
            0 => WarningText,
            1 => Doors,
            _ => throw new IndexOutOfRangeException(),
        };
        /// <summary>Gets enumerator.</summary>
        public IEnumerator<CeresEscapeTileSheetDefinition> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        /// <summary>Gets enumerator.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Contains.</summary>
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

    /// <summary>Stores pages.</summary>
    private readonly RoomCharacterAtlas[] pages;

    /// <summary>Initializes a new ceres escape tile artwork instance.</summary>
    internal CeresEscapeTileArtwork(RoomCharacterAtlas[] pages)
    {
        Ensure.NotNull(pages);
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

    /// <summary>Tries to resolve.</summary>
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
