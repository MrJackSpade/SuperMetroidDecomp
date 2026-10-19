using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scrolling sky's tile rows by vertical world position (#1347). The cartridge streams two pairs of rows a frame
/// into a circular BG2 tilemap (<c>$88:AFA3</c>); each visible row holds the row its chunk table selects for that
/// position, so the sky is drawn from the rows directly and nothing is streamed.
/// </summary>
public sealed class SkyTilemap : IBgTileSource
{
    /// <summary>Tile columns in a row; the sky repeats every 256 pixels across.</summary>
    public const int Columns = 32;

    /// <summary>Rows over the 2048-pixel span the chunk positions cover before they repeat.</summary>
    public const int Rows = 256;

    /// <summary>Rows in one chunk: the 32 rows of one tilemap page.</summary>
    private const int RowsPerChunk = 32;

    /// <summary>Bytes of one row: 32 tile words.</summary>
    private const int RowBytes = Columns * sizeof(ushort);

    /// <summary>The vertical position bits that select a row; positions repeat beyond them.</summary>
    private const int PositionMask = Rows * SnesPpuLayout.BackgroundTileSizePixels - 1;

    /// <summary>The tile words of every row, row-major.</summary>
    private readonly ushort[] words = new ushort[Rows * Columns];

    /// <summary>Whether each row selects a sky page; the ocean table's last chunks select none.</summary>
    private readonly bool[] shown = new bool[Rows];

    /// <summary>Reads each row from the page its chunk selects in <paramref name="variant"/>'s table.</summary>
    /// <param name="pages">The installed sky pages.</param>
    /// <param name="variant">The land or ocean chunk table.</param>
    public SkyTilemap(RoomSkyTilemapCatalog pages, ScrollingSkyChunkTable variant)
    {
        Ensure.NotNull(pages);
        Variant = variant;
        for (int row = 0; row < Rows; row++)
        {
            if (!ScrollingSkyChunkDefinitions.TryGetPage(variant, row / RowsPerChunk, out ushort page))
                continue;
            ReadOnlySpan<byte> bytes = pages.Read(RoomFxRomData.Banks.Tilemaps | (page + row % RowsPerChunk * RowBytes), RowBytes);
            for (int column = 0; column < Columns; column++)
                words[row * Columns + column] = SnesLittleEndian.Word(bytes[column * 2], bytes[column * 2 + 1]);
            shown[row] = true;
        }
    }

    /// <summary>The land or ocean chunk table the rows follow.</summary>
    public ScrollingSkyChunkTable Variant { get; }

    /// <summary>The tile words of every row, row-major; a row <see cref="RowShown"/> rejects holds zeros.</summary>
    public ReadOnlySpan<ushort> Words => words;

    /// <summary>Whether <paramref name="row"/> selects a sky page.</summary>
    public bool RowShown(int row) => shown[row];

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
    {
        int row = (scrolled.Y & PositionMask) / SnesPpuLayout.BackgroundTileSizePixels;
        if (!shown[row])
        {
            sample = default;
            return false;
        }
        int column = scrolled.X / SnesPpuLayout.BackgroundTileSizePixels % Columns;
        sample = BgTileSample.At(new SnesBgTilemapWord(words[row * Columns + column]), scrolled);
        return true;
    }
}
