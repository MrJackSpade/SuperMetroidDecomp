using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Seven contiguous native scrolling-sky tilemap pages, selected by host artwork.</summary>
public sealed class RoomSkyTilemapCatalog
{
    /// <summary>The tiles of the seven selected sky pages, in scrolling order.</summary>
    private readonly BackgroundTile[] pages;

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
        pages = new BackgroundTile[RoomSkyTilemapFormat.PageCount * RoomSkyTilemapFormat.PageTiles];
        for (int index = 0; index < selectedPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas atlas = selectedPages[index]
                ?? throw new InvalidDataException($"Scrolling sky page {index} is missing.");
            if (atlas.Tiles.Length != RoomSkyTilemapFormat.PageTiles)
                throw new InvalidDataException($"Scrolling sky page {index} has the wrong length.");
            atlas.Tiles.Span.CopyTo(pages.AsSpan(index * RoomSkyTilemapFormat.PageTiles));
        }
    }

    /// <summary>The tiles of all seven selected sky pages in their scrolling order.</summary>
    public ReadOnlyMemory<BackgroundTile> Pages => pages;

    /// <summary>The sky rows the land or ocean chunk table selects from these pages (#1347).</summary>
    public SkyTilemap Tilemap(ScrollingSkyChunkTable variant) => variant switch
    {
        ScrollingSkyChunkTable.Land => land ??= new SkyTilemap(pages, variant),
        ScrollingSkyChunkTable.Ocean => ocean ??= new SkyTilemap(pages, variant),
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown scrolling-sky chunk table."),
    };

    /// <summary>The tiles of one row of one sky page within <paramref name="pages"/>.</summary>
    /// <param name="pages">The seven sky pages in scrolling order.</param>
    /// <param name="page">The page, 0 through 6.</param>
    /// <param name="row">The row within the page, 0 through 31.</param>
    internal static ReadOnlySpan<BackgroundTile> Row(ReadOnlySpan<BackgroundTile> pages, int page, int row)
    {
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.PageCount - 1, page);
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.RowsPerPage - 1, row);
        return pages.Slice(page * RoomSkyTilemapFormat.PageTiles + row * SkyTilemap.Columns, SkyTilemap.Columns);
    }

    /// <summary>The installed tiles of sky page <paramref name="page"/>, which a library background copies whole.</summary>
    /// <param name="page">The page, 0 through 6.</param>
    /// <param name="tileCount">The tiles the copy moves: one complete page.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a sky page.</exception>
    /// <exception cref="InvalidDataException"><paramref name="tileCount"/> is not one page.</exception>
    public ReadOnlyMemory<BackgroundTile> Page(int page, int tileCount)
    {
        Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.PageCount - 1, page);
        // Room setup copies whole pages; the sky's rows are drawn from the pages directly (#1347).
        if (tileCount != RoomSkyTilemapFormat.PageTiles)
            throw new InvalidDataException($"A scrolling-sky page copy of {tileCount} tiles is not one whole page.");
        return pages.AsMemory(page * RoomSkyTilemapFormat.PageTiles, tileCount);
    }
}
