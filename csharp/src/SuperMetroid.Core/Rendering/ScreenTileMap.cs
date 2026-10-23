using SuperMetroid.Core.Rooms.Backgrounds;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend tile layer's cells (#1403), read from the screen's tilemap when the frame was captured: each cell shows one
/// of the layer's tiles, row by row from the map's top-left corner. A scrolled position wraps at the map's edges.
/// </summary>
public sealed class ScreenTileMap : IBgSource, IEquatable<ScreenTileMap>
{
    /// <summary>Holds a map's cells.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The map has no cells along an axis.</exception>
    /// <exception cref="ArgumentException">The cells do not fill the map.</exception>
    public ScreenTileMap(ScreenLayerPictures tiles, Extent16<TilemapCells> size, ImmutableArray<ScreenTileCell> cells)
    {
        Tiles = Ensure.NotNull(tiles);
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.X);
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.Y);
        Ensure.LengthEqual(cells.AsSpan(), size.X * size.Y);
        Size = size;
        Cells = cells;
    }

    /// <summary>The tiles the cells show.</summary>
    public ScreenLayerPictures Tiles { get; }

    /// <summary>The map's size in cells.</summary>
    public Extent16<TilemapCells> Size { get; }

    /// <summary>The cells, row by row from the top-left corner.</summary>
    public ImmutableArray<ScreenTileCell> Cells { get; }

    /// <summary>
    /// Reads a <paramref name="size"/> map of cells from the tilemap at <paramref name="tilemapWord"/> in the screen's
    /// memory image <paramref name="image"/>, each showing one of <paramref name="tiles"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    [PpuLayoutBoundary("capture: a frontend layer's tilemap words, read from the screen's memory image into cells")]
    public static ScreenTileMap Capture(ReadOnlySpan<byte> image, int tilemapWord, Extent16<TilemapCells> size, ScreenLayerPictures tiles)
    {
        Ensure.LengthEqual(image, SnesPpuLayout.VramByteCount);
        var geometry = new BgTilemapGeometry(size);
        var cells = ImmutableArray.CreateBuilder<ScreenTileCell>(size.X * size.Y);
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int offset = SnesVramAddressing.ByteOffsetOfWord(geometry.EntryWordAddress(VramWordRange.Require(tilemapWord),
                    new Position16<TilemapCells>((ushort)x, (ushort)y)));
                var word = new SnesBgTilemapWord(SnesLittleEndian.Word(image[offset], image[offset + 1]));
                cells.Add(new ScreenTileCell(word.CharacterIndex, word.PaletteIndex, word.FlipHorizontally, word.FlipVertically, word.HasPriority));
            }
        }
        return new ScreenTileMap(tiles, size, cells.MoveToImmutable());
    }

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        const int tile = ScreenTileSheet.TilePixels;
        int x = scrolled.X % (Size.X * tile), y = scrolled.Y % (Size.Y * tile);
        sample = Tiles.Sample(Cells[y / tile * Size.X + x / tile], new Position16<CharacterPixels>((ushort)(x % tile), (ushort)(y % tile)));
        return true;
    }

    /// <inheritdoc />
    public bool Equals(ScreenTileMap? other) =>
        other is not null && Tiles.Equals(other.Tiles) && Size == other.Size && Cells.AsSpan().SequenceEqual(other.Cells.AsSpan());

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenTileMap);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Tiles, Size, Cells.Length);
}
