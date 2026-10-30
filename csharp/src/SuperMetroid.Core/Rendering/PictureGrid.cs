using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A grid of equal cells each showing an installed picture or nothing (#1467), as an area map shows its cells: row by row
/// from the top-left cell, wrapping at the grid's edges when scrolled. Equal to another showing the same pictures the same
/// way in every cell, so an unchanged map keeps the backend's packing.
/// </summary>
public sealed class PictureGrid : IEquatable<PictureGrid>
{
    /// <summary>Pixels along each side of a cell.</summary>
    public const int CellPixels = ScreenGeometry.BackgroundTileSizePixels;

    /// <summary>Keeps the grid's cells.</summary>
    /// <param name="size">The grid's width and height in cells.</param>
    /// <param name="cells">Each cell's picture, or none, row by row.</param>
    /// <exception cref="ArgumentOutOfRangeException">The grid has no cells along an axis.</exception>
    /// <exception cref="ArgumentException">The cells do not fill the grid, or a picture is not one cell in size.</exception>
    public PictureGrid(Extent16<TilemapCells> size, ImmutableArray<GridPicture?> cells)
    {
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.X);
        Ensure.BetweenInclusive(1, ushort.MaxValue, size.Y);
        if (cells.IsDefault)
            throw new ArgumentException("A picture grid needs its cells.", nameof(cells));
        Ensure.LengthEqual(cells.AsSpan(), size.X * size.Y);
        foreach (GridPicture? cell in cells)
        {
            if (cell is { } shown && (shown.Picture.Size.X != CellPixels || shown.Picture.Size.Y != CellPixels))
                throw new ArgumentException($"A grid cell's picture is {shown.Picture.Size.X}x{shown.Picture.Size.Y}, not one {CellPixels}-pixel cell.", nameof(cells));
        }
        Size = size;
        Cells = cells;
    }

    /// <summary>The grid's width and height in cells.</summary>
    public Extent16<TilemapCells> Size { get; }

    /// <summary>Each cell's picture, or none, row by row.</summary>
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

    /// <summary>Whether two cells show the same picture the same way, or both show nothing.</summary>
    private static bool Same(GridPicture? mine, GridPicture? theirs) => mine is { } shown
        ? theirs is { } other && ReferenceEquals(shown.Picture, other.Picture) && shown.Mirroring == other.Mirroring
        : theirs is null;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PictureGrid);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Size, Cells.Length);
}
