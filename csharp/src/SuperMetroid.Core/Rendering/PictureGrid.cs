using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A grid of equal squares each showing an installed picture or nothing (#1467), as an area map shows its squares: row by
/// row from the top-left square, which lies on the grid's origin; outside its squares the grid shows nothing and does not
/// repeat (#1555). Each square's picture is its part drawn behind and its part drawn in front (#1634). Equal to another showing the same pictures the same way in every square, so an unchanged map keeps the
/// backend's packing.
/// </summary>
public sealed class PictureGrid : IEquatable<PictureGrid>
{
    /// <summary>Pixels along each side of a square.</summary>
    public const int CellPixels = ScreenGeometry.BackgroundTileSizePixels;

    /// <summary>Keeps the grid's cells.</summary>
    /// <param name="size">The grid's width and height in area-map squares.</param>
    /// <param name="cells">Each square's picture, or none, row by row.</param>
    /// <exception cref="ArgumentOutOfRangeException">The grid has no squares along an axis.</exception>
    /// <exception cref="ArgumentException">The squares do not fill the grid, or a picture is not one square in size.</exception>
    public PictureGrid(Extent16<MapTiles> size, ImmutableArray<GridPicture?> cells)
    {
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.X);
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.Y);
        if (cells.IsDefault)
            throw new ArgumentException("A picture grid needs its cells.", nameof(cells));
        Ensure.LengthEqual(cells.AsSpan(), size.X * size.Y);
        foreach (GridPicture? cell in cells)
        {
            if (cell is not { } shown)
                continue;
            // A cell showing nothing is none; a cell's picture is one square, both its parts alike (#1634).
            if (Ensure.NotNull(shown.Picture).Size is not { } size)
                throw new ArgumentException("A grid cell's picture shows nothing; such a cell shows none.", nameof(cells));
            if (size.X != CellPixels || size.Y != CellPixels)
                throw new ArgumentException($"A grid cell's picture is {size.X}x{size.Y}, not one {CellPixels}-pixel square.", nameof(cells));
        }
        Size = size;
        Cells = cells;
    }

    /// <summary>The grid's width and height in area-map squares.</summary>
    public Extent16<MapTiles> Size { get; }

    /// <summary>Each square's picture, or none, row by row.</summary>
    public ImmutableArray<GridPicture?> Cells { get; }

    /// <inheritdoc />
    public bool Equals(PictureGrid? other)
    {
        if (other is null || other.Size != Size)
            return false;
        for (int index = 0; index < Cells.Length; index++)
        {
            if (!Same(Cells[index], other.Cells[index]))
                return false;
        }
        return true;
    }

    /// <summary>Whether two cells show the same picture's parts the same way, or both show nothing.</summary>
    private static bool Same(GridPicture? mine, GridPicture? theirs) => mine is { } shown
        ? theirs is { } other && shown.Picture == other.Picture && shown.Mirroring == other.Mirroring
        : theirs is null;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PictureGrid);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Size, Cells.Length);
}
