using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Seven contiguous native scrolling-sky tilemap pages, selected by host artwork.</summary>
public sealed class RoomSkyTilemapCatalog : IInstalledArtworkTransferSource
{
    /// <summary>Owned contiguous bytes of the seven selected bank-$8A sky pages.</summary>
    private readonly byte[] pages;

    /// <summary>The land rows, built when first shown.</summary>
    private SkyTilemap? land;

    /// <summary>The ocean rows, built when first shown.</summary>
    private SkyTilemap? ocean;

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

    /// <summary>The sky rows the land or ocean chunk table selects from these pages (#1347).</summary>
    public SkyTilemap Tilemap(ScrollingSkyChunkTable variant) => variant switch
    {
        ScrollingSkyChunkTable.Land => land ??= new SkyTilemap(this, variant),
        ScrollingSkyChunkTable.Ocean => ocean ??= new SkyTilemap(this, variant),
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown scrolling-sky chunk table."),
    };

    /// <summary>The tile words of one row of one sky page, as little-endian bytes.</summary>
    /// <param name="page">The page, 0 through 6.</param>
    /// <param name="row">The row within the page, 0 through 31.</param>
    internal ReadOnlySpan<byte> Row(int page, int row)
    {
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.PageCount - 1, page);
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.RowsPerPage - 1, row);
        return pages.AsSpan(page * RoomSkyTilemapFormat.PageByteCount + row * RoomSkyTilemapFormat.RowByteCount,
            RoomSkyTilemapFormat.RowByteCount);
    }

    /// <summary>Resolves a whole aligned page from installed artwork.</summary>
    /// <param name="sourceAddress">Full 24-bit bank-$8A source address within the contiguous seven-page range.</param>
    /// <param name="byteCount">One complete page.</param>
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
        // Room setup uploads whole pages; the sky's rows are drawn from the pages directly (#1347).
        if (offset % RoomSkyTilemapFormat.PageByteCount != 0 || byteCount != RoomSkyTilemapFormat.PageByteCount)
            throw new InvalidDataException(
                $"Scrolling-sky transfer ${sourceAddress:X6}+${byteCount:X} is not an aligned page.");
        data = pages.AsMemory(offset, byteCount);
        return true;
    }
}
