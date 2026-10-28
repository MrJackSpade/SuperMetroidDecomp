using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scrolling sky's tile rows by vertical world position (#1347). The cartridge streams two pairs of rows a frame
/// into a circular BG2 tilemap (<c>$88:AFA3</c>); each visible row holds the row its chunk table selects for that
/// position, so the sky is drawn from the rows directly and nothing is streamed.
/// </summary>
public sealed class SkyTilemap
{
    /// <summary>Tile columns in a row; the sky repeats every 256 pixels across.</summary>
    public const int Columns = 32;

    /// <summary>Rows over the 2048-pixel span the chunk positions cover before they repeat.</summary>
    public const int Rows = 256;

    /// <summary>Rows in one chunk: the 32 rows of one tilemap page.</summary>
    private const int RowsPerChunk = 32;

    /// <summary>The tiles of every row, row-major.</summary>
    private readonly BackgroundTile[] tiles = new BackgroundTile[Rows * Columns];

    /// <summary>Whether each row selects a sky page; the ocean table's last chunks select none.</summary>
    private readonly bool[] shown = new bool[Rows];

    /// <summary>Reads each row from the page its chunk selects in <paramref name="variant"/>'s table.</summary>
    /// <param name="pages">The installed sky pages.</param>
    /// <param name="variant">The land or ocean chunk table.</param>
    public SkyTilemap(ReadOnlySpan<BackgroundTile> pages, ScrollingSkyChunkTable variant)
    {
        Variant = variant;
        for (int row = 0; row < Rows; row++)
        {
            if (!ScrollingSkyChunks.TryGetPage(variant, row / RowsPerChunk, out int page))
                continue;
            RoomSkyTilemapCatalog.Row(pages, page, row % RowsPerChunk).CopyTo(tiles.AsSpan(row * Columns, Columns));
            shown[row] = true;
        }
    }

    /// <summary>The land or ocean chunk table the rows follow.</summary>
    public ScrollingSkyChunkTable Variant { get; }

    /// <summary>The tiles of every row, row-major; a row <see cref="RowShown"/> rejects holds blank tiles.</summary>
    public ReadOnlySpan<BackgroundTile> Tiles => tiles;

    /// <summary>Whether <paramref name="row"/> selects a sky page.</summary>
    public bool RowShown(int row) => shown[row];
}
