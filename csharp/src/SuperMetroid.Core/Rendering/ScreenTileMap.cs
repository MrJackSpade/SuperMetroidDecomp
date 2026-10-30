using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend tile layer's cells (#1403), read from the screen's tilemap when the frame was captured: each cell shows one
/// of the layer's tiles, row by row from the map's top-left corner. A scrolled position wraps at the map's edges.
/// </summary>
public sealed class ScreenTileMap : IEquatable<ScreenTileMap>
{

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

    /// <inheritdoc />
    public bool Equals(ScreenTileMap? other) =>
        other is not null && Tiles.Equals(other.Tiles) && Size == other.Size && Cells.AsSpan().SequenceEqual(other.Cells.AsSpan());

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ScreenTileMap);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Tiles, Size, Cells.Length);
}
