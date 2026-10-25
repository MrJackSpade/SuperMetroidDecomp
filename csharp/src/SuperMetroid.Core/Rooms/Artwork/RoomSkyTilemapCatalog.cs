using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Seven contiguous native scrolling-sky tilemap pages, selected by host artwork.</summary>
public sealed class RoomSkyTilemapCatalog
{
    /// <summary>Owned contiguous bytes of the seven selected sky pages, in scrolling order.</summary>
    private readonly byte[] pages;

    /// <summary>The land rows, built when first shown.</summary>
    private SkyTilemap? land;

    /// <summary>The ocean rows, built when first shown.</summary>
    private SkyTilemap? ocean;

    /// <summary>Copies and validates the seven scrolling-sky pages in scrolling order.</summary>
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
            if (atlas.Bytes.Length != RoomSkyTilemapFormat.PageByteCount)
                throw new InvalidDataException($"Scrolling sky page {index} has the wrong length.");
            atlas.Bytes.Span.CopyTo(pages.AsSpan(index * RoomSkyTilemapFormat.PageByteCount));
        }
    }

    /// <summary>SHA-256 of all seven selected sky pages in their scrolling order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomSkyTilemapCatalog),
        content => content.Append("pages", pages));

    /// <summary>The sky rows the land or ocean chunk table selects from these pages (#1347).</summary>
    public SkyTilemap Tilemap(ScrollingSkyChunkTable variant) => variant switch
    {
        ScrollingSkyChunkTable.Land => land ??= new SkyTilemap(pages, variant),
        ScrollingSkyChunkTable.Ocean => ocean ??= new SkyTilemap(pages, variant),
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown scrolling-sky chunk table."),
    };

    /// <summary>The tile words of one row of one sky page within <paramref name="pages"/>, as little-endian bytes.</summary>
    /// <param name="pages">The seven sky pages in scrolling order.</param>
    /// <param name="page">The page, 0 through 6.</param>
    /// <param name="row">The row within the page, 0 through 31.</param>
    internal static ReadOnlySpan<byte> Row(ReadOnlySpan<byte> pages, int page, int row)
    {
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.PageCount - 1, page);
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.RowsPerPage - 1, row);
        return pages.Slice(page * RoomSkyTilemapFormat.PageByteCount + row * RoomSkyTilemapFormat.RowByteCount,
            RoomSkyTilemapFormat.RowByteCount);
    }

    /// <summary>The installed tile words of sky page <paramref name="page"/>, which a library background copies whole.</summary>
    /// <param name="page">The page, 0 through 6.</param>
    /// <param name="byteCount">The bytes the copy moves: one complete page.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a sky page.</exception>
    /// <exception cref="InvalidDataException"><paramref name="byteCount"/> is not one page.</exception>
    public ReadOnlyMemory<byte> Page(int page, int byteCount)
    {
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.PageCount - 1, page);
        // Room setup copies whole pages; the sky's rows are drawn from the pages directly (#1347).
        if (byteCount != RoomSkyTilemapFormat.PageByteCount)
            throw new InvalidDataException($"A scrolling-sky page copy of {byteCount} bytes is not one whole page.");
        return pages.AsMemory(page * RoomSkyTilemapFormat.PageByteCount, byteCount);
    }
}
