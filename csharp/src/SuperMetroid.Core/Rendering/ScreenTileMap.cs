using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend tile layer's cells (#1403), read from the screen's tilemap when the frame was captured: each cell shows one
/// of the layer's tiles, row by row from the map's top-left corner. A scrolled position wraps at the map's edges.
/// </summary>
public sealed class ScreenTileMap : IEquatable<ScreenTileMap>
{
    /// <summary>Cells along each side of one screen of a tilemap; a larger map keeps whole screens one after another.</summary>
    private const int ScreenCells = 32;

    /// <summary>Holds a map's cells.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The map has no cells along an axis.</exception>
    /// <exception cref="ArgumentException">The cells do not fill the map.</exception>
    public ScreenTileMap(ScreenLayerPictures tiles, Extent16<TilemapCells> size, ImmutableArray<BackgroundTile> cells)
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
    public ImmutableArray<BackgroundTile> Cells { get; }

    /// <summary>
    /// Reads a <paramref name="size"/> map of cells from a screen's own tilemap <paramref name="tilemap"/>, each showing one
    /// of <paramref name="tiles"/>. A map wider or taller than one 32-cell screen keeps its screens one after another,
    /// left to right, then top to bottom.
    /// </summary>
    /// <exception cref="ArgumentException">The tilemap does not hold every cell of the map.</exception>
    public static ScreenTileMap Of(ReadOnlySpan<ushort> tilemap, Extent16<TilemapCells> size, ScreenLayerPictures tiles)
    {
        Ensure.LengthEqual(tilemap, size.X * size.Y);
        if (size.X % ScreenCells != 0 || size.Y % ScreenCells != 0)
            throw new ArgumentException($"A {size.X}x{size.Y} map is not whole {ScreenCells}-cell screens.", nameof(size));
        int screensAcross = size.X / ScreenCells;
        var cells = ImmutableArray.CreateBuilder<BackgroundTile>(size.X * size.Y);
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int screen = y / ScreenCells * screensAcross + x / ScreenCells;
                var word = new SnesBgTilemapWord(tilemap[screen * ScreenCells * ScreenCells + y % ScreenCells * ScreenCells + x % ScreenCells]);
                cells.Add(new BackgroundTile(word.CharacterIndex, word.PaletteIndex, word.FlipHorizontally, word.FlipVertically, word.HasPriority));
            }
        }
        return new ScreenTileMap(tiles, size, cells.MoveToImmutable());
    }

    /// <inheritdoc />
    public bool Equals(ScreenTileMap? other) =>
        other is not null && Tiles.Equals(other.Tiles) && Size == other.Size && Cells.AsSpan().SequenceEqual(other.Cells.AsSpan());

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenTileMap);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Tiles, Size, Cells.Length);
}
