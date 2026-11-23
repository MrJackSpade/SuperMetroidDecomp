namespace Engine.World;

/// <summary>
/// The shape of a sloped tile: for each pixel column, how many pixels of it are solid, measured up from the tile's
/// bottom for a floor or down from its top for a ceiling.
/// </summary>
public sealed class SlopeShape
{
    /// <summary>The solid pixels in each column, left to right.</summary>
    private readonly int[] _solid;

    /// <summary>A slope with the given solid pixels per column.</summary>
    /// <param name="solid">The solid pixels in each column, left to right; one entry per pixel of a tile's width.</param>
    /// <param name="fromTop">Whether the solid part hangs from the tile's top, as a ceiling does.</param>
    /// <exception cref="ArgumentException">A column holds fewer than zero or more than a tile's height of pixels.</exception>
    public SlopeShape(IReadOnlyList<int> solid, bool fromTop)
    {
        if (solid.Any(pixels => pixels < 0 || pixels > solid.Count))
            throw new ArgumentException($"Each column of a slope holds 0 through {solid.Count} solid pixels.", nameof(solid));
        _solid = [.. solid];
        FromTop = fromTop;
    }

    /// <summary>Whether the solid part hangs from the tile's top.</summary>
    public bool FromTop { get; }

    /// <summary>How many columns the shape has: a tile's width in pixels.</summary>
    public int Columns => _solid.Length;

    /// <summary>
    /// Whether any solid pixel lies in the given columns and rows of the tile, both counted from the tile's top-left and
    /// each range ending just past its last column or row.
    /// </summary>
    public bool IsSolidIn(int firstColumn, int endColumn, int firstRow, int endRow)
    {
        for (int column = firstColumn; column < endColumn; column++)
        {
            int solid = _solid[column];
            if (solid == 0)
                continue;
            if (FromTop ? firstRow < solid : endRow > _solid.Length - solid)
                return true;
        }

        return false;
    }
}
