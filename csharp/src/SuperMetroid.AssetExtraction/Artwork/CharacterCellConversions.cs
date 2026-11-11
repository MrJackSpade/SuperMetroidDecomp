using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Turns a decoded character grid's cells into the pixels the game places pictures at: a cell's corner lies one
/// character's width per column right of the grid's origin and one character's height per row below it.
/// </summary>
public static class CharacterCellConversions
{
    /// <summary>Pixels across and down one character.</summary>
    public const int CellPixels = 8;

    /// <summary>The pixel of <typeparamref name="TSpace"/> the cell's upper-left corner lies on, the grid's origin at that space's origin.</summary>
    /// <typeparam name="TSpace">The pixel space the grid is shown in.</typeparam>
    /// <param name="cell">The cell.</param>
    public static Position16<TSpace> CornerPixel<TSpace>(this Position16<CharacterCells> cell) where TSpace : ISpatialSpace =>
        new(cell.X * CellPixels, cell.Y * CellPixels);

    /// <summary>The pixel offset of <typeparamref name="TSpace"/> spanning <paramref name="offset"/>'s cells.</summary>
    /// <typeparam name="TSpace">The pixel space the grid is shown in.</typeparam>
    /// <param name="offset">The offset in cells.</param>
    public static Offset16<TSpace> PixelOffset<TSpace>(this Offset16<CharacterCells> offset) where TSpace : ISpatialSpace =>
        new(offset.X * CellPixels, offset.Y * CellPixels);
}
