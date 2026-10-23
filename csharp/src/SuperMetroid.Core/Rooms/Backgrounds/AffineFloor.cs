using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A Mode 7 floor (#1403): its 128 by 128 map of tiles and the tiles' pre-coloured pictures. A frontend scene's floor is
/// read from its memory image when the frame is captured; a Ceres room's is its map as the frame shows it.
/// </summary>
public sealed class AffineFloor : IEquatable<AffineFloor>
{
    /// <summary>Tiles along each side of the map.</summary>
    public const int Side = 128;

    /// <summary>Tiles in the map.</summary>
    public const int CellCount = Side * Side;

    /// <summary>Holds a floor's map and tiles.</summary>
    /// <exception cref="ArgumentException">The cells do not fill the map.</exception>
    public AffineFloor(ImmutableArray<AffineFloorCell> cells, Mode7TilePictures tiles)
    {
        Ensure.LengthEqual(cells.AsSpan(), CellCount);
        Cells = cells;
        Tiles = Ensure.NotNull(tiles);
    }

    /// <summary>The map's cells, row by row from the top-left corner.</summary>
    public ImmutableArray<AffineFloorCell> Cells { get; }

    /// <summary>What each tile the cells name shows.</summary>
    public Mode7TilePictures Tiles { get; }

    /// <summary>The tile at <paramref name="cell"/>, column and row 0 through 127.</summary>
    public int TileAt(Position16<TilemapCells> cell) =>
        Cells[Ensure.BetweenInclusive(0, Side - 1, cell.Y) * Side + Ensure.BetweenInclusive(0, Side - 1, cell.X)].Tile;

    /// <summary>The floor whose map is <paramref name="tiles"/>' row-major tile numbers.</summary>
    /// <exception cref="ArgumentException">The tile numbers do not fill the map.</exception>
    public static AffineFloor Of(ReadOnlySpan<byte> map, Mode7TilePictures tiles)
    {
        Ensure.LengthEqual(map, CellCount);
        var cells = ImmutableArray.CreateBuilder<AffineFloorCell>(CellCount);
        foreach (byte tile in map)
            cells.Add(new AffineFloorCell(tile));
        return new AffineFloor(cells.MoveToImmutable(), tiles);
    }

    /// <summary>The floor a frontend scene's memory image <paramref name="image"/> maps, showing <paramref name="tiles"/>.</summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    [PpuLayoutBoundary("capture: Mode 7's tile map, the low byte of each of the first 16384 VRAM words")]
    public static AffineFloor Capture(ReadOnlySpan<byte> image, Mode7TilePictures tiles)
    {
        Ensure.LengthEqual(image, SnesPpuLayout.VramByteCount);
        var cells = ImmutableArray.CreateBuilder<AffineFloorCell>(CellCount);
        for (int cell = 0; cell < CellCount; cell++)
            cells.Add(new AffineFloorCell(image[cell * sizeof(ushort)]));
        return new AffineFloor(cells.MoveToImmutable(), tiles);
    }

    /// <inheritdoc />
    public bool Equals(AffineFloor? other) =>
        other is not null && ReferenceEquals(Tiles, other.Tiles) && Cells.AsSpan().SequenceEqual(other.Cells.AsSpan());

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AffineFloor);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Tiles, Cells.Length);
}
