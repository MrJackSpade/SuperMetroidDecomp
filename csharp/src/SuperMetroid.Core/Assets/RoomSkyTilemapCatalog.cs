using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Seven contiguous native scrolling-sky tilemap pages, selected by host artwork.</summary>
public sealed class RoomSkyTilemapCatalog : IInstalledArtworkTransferSource
{
    /// <summary>Owned contiguous bytes of the seven selected bank-$8A sky pages.</summary>
    private readonly byte[] pages;

    /// <summary>Copies and validates the seven scrolling-sky pages in native source order.</summary>
    /// <param name="selectedPages">Exactly seven 32-by-32 compiled BG tilemap pages.</param>
    public RoomSkyTilemapCatalog(IReadOnlyList<RoomBackgroundTilemapAtlas> selectedPages)
    {
        Ensure.NotNull(selectedPages);
        if (selectedPages.Count != RoomSkyTilemapFormat.PageCount)
            throw new InvalidDataException(
                $"Scrolling sky requires {RoomSkyTilemapFormat.PageCount} ordered pages.");
        pages = new byte[RoomSkyTilemapFormat.TotalByteCount];
        for (int index = 0; index < selectedPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas atlas = selectedPages[index]
                ?? throw new InvalidDataException($"Scrolling sky page {index} is missing.");
            if (atlas.Transfer.Length != RoomSkyTilemapFormat.PageByteCount)
                throw new InvalidDataException($"Scrolling sky page {index} has the wrong length.");
            atlas.Transfer.Span.CopyTo(pages.AsSpan(index * RoomSkyTilemapFormat.PageByteCount));
        }
    }

    /// <summary>SHA-256 of all seven selected sky pages in their scrolling order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomSkyTilemapCatalog),
        content => content.Append("pages", pages));

    /// <summary>Resolves a whole aligned page or one even-addressed native scrolling row from installed artwork.</summary>
    /// <param name="sourceAddress">Full 24-bit bank-$8A source address within the contiguous seven-page range.</param>
    /// <param name="byteCount">One complete page or the native scrolling-row transfer length.</param>
    /// <param name="data">Receives the matching immutable slice when the address belongs to this catalog.</param>
    /// <returns><see langword="true"/> when the source lies within the installed sky range; otherwise <see langword="false"/>.</returns>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        int offset = sourceAddress - RoomSkyTilemapFormat.FirstSourceAddress;
        if ((uint)offset >= pages.Length)
        {
            data = default;
            return false;
        }
        // Room setup uploads whole pages; the bank-$88 scrolling-sky main
        // routine subsequently streams four 64-byte rows per frame. Its native
        // top-of-room pointer overread can start mid-page (e.g. $8A:B526), so
        // demanding page alignment here incorrectly falls through to ROM DMA.
        bool wholePage = offset % RoomSkyTilemapFormat.PageByteCount == 0 &&
            byteCount == RoomSkyTilemapFormat.PageByteCount;
        bool scrollingRow = (sourceAddress & 1) == 0 &&
            byteCount == RoomFxRomData.ScrollingSky.TilemapRowByteCount;
        if ((!wholePage && !scrollingRow) || offset + byteCount > pages.Length)
            throw new InvalidDataException(
                $"Scrolling-sky transfer ${sourceAddress:X6}+${byteCount:X} is neither an aligned page nor a complete scrolling row.");
        data = pages.AsMemory(offset, byteCount);
        return true;
    }
}
