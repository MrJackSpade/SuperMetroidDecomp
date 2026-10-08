using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Seven contiguous native scrolling-sky tilemap pages, selected by host artwork.</summary>
public sealed class RoomSkyTilemapCatalog : IInstalledArtworkTransferSource
{
    private readonly byte[] pages;

    /// <summary>Copies and validates the seven scrolling-sky pages in native source order.</summary>
    /// <param name="selectedPages">Exactly seven 32-by-32 compiled BG tilemap pages.</param>
    public RoomSkyTilemapCatalog(IReadOnlyList<RoomBackgroundTilemapAtlas> selectedPages)
    {
        ArgumentNullException.ThrowIfNull(selectedPages);
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

/// <summary>Native bank-$8A scrolling-sky visual pages; pointer arithmetic remains engine-owned.</summary>
public static class RoomSkyTilemapFormat
{
    /// <summary>$8A:B180, first of seven contiguous 32x32 scrolling-sky tilemap pages.</summary>
    public const int FirstSourceAddress = 0x8ab180;
    /// <summary>One native 32x32 page of 16-bit BG tile words.</summary>
    public const int PageByteCount = RoomBackgroundTilemapFormat.BytesPerPage;
    /// <summary>Seven pages through $8A:E97F, including land and ocean sources.</summary>
    public const int PageCount = 7;
    /// <summary>Total contiguous byte length of all seven compiled tilemap pages.</summary>
    public const int TotalByteCount = PageCount * PageByteCount;

    /// <summary>The manifest file that orders the seven editable sky-page resources.</summary>
    public const string ManifestFileName = "scrolling-sky.json";

    /// <summary>Calculates one page's full 24-bit native source identity.</summary>
    /// <param name="page">Zero-based page index from zero through six.</param>
    /// <returns>The page-aligned address in the contiguous bank-$8A range.</returns>
    public static int SourceAddress(int page) => (uint)page < PageCount
        ? FirstSourceAddress + page * PageByteCount
        : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>Builds the editable JSON file name for one scrolling-sky page.</summary>
    /// <param name="page">Zero-based page index from zero through six.</param>
    /// <returns>A file name containing the page's six-digit native source address.</returns>
    public static string FileName(int page) => $"scrolling-sky-{SourceAddress(page):X6}.json";
}
