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
    /// <param name="share">How much of a move along it the slope lets through, in 256ths; 256 is all of it.</param>
    /// <param name="holdsMiddle">Whether the slope holds things up by their middle, for movers that ask for slopes to be treated so.</param>
    /// <exception cref="ArgumentOutOfRangeException">The share is not 1 through 256.</exception>
    /// <param name="step">The size of the steps the solid part is cut in, in pixels; 1 for a shape met pixel by pixel.</param>
    public SlopeShape(IReadOnlyList<int> solid, bool fromTop, int share = WholeShare, bool holdsMiddle = false, int step = 1)
    {
        if (step < 1 || step > solid.Count)
            throw new ArgumentOutOfRangeException(nameof(step), step, $"A shape's step is 1 through {solid.Count} pixels.");
        Step = step;
        HoldsMiddle = holdsMiddle;
        if (solid.Any(pixels => pixels < 0 || pixels > solid.Count))
            throw new ArgumentException($"Each column of a slope holds 0 through {solid.Count} solid pixels.", nameof(solid));
        if (share is < 1 or > WholeShare)
            throw new ArgumentOutOfRangeException(nameof(share), share, $"A slope's share is 1 through {WholeShare}.");
        _solid = [.. solid];
        FromTop = fromTop;
        Share = share;
    }

    /// <summary>
    /// Whether the slope holds things up by their middle: to a mover that treats slopes so, only the middle column of
    /// its body meets this slope. A slope that does not is a step or a wall, and meets the whole body.
    /// </summary>
    public bool HoldsMiddle { get; }

    /// <summary>
    /// The size of the steps the solid part is cut in, in pixels, counted from the tile's left and top: a move tried
    /// only where it ends, and stopped by the shape, comes to rest at the start of the step it would have ended in,
    /// whatever lies in the step before. 1 for a shape met pixel by pixel, which a stopped move rests flush against.
    /// </summary>
    public int Step { get; }

    /// <summary>The parts in the whole of a move: a slope's share is counted in these.</summary>
    public const int WholeShare = 256;

    /// <summary>
    /// How much of a move along it the slope lets through, in 256ths: of the player's move across a floor's slope they
    /// walk into, and of a crawler's along the floor or ceiling it is on.
    /// </summary>
    public int Share { get; }

    /// <summary>Whether the solid part hangs from the tile's top.</summary>
    public bool FromTop { get; }

    /// <summary>How many columns the shape has: a tile's width in pixels.</summary>
    public int Columns => _solid.Length;

    /// <summary>How many pixels of a column are solid, the columns counted from the tile's left.</summary>
    public int SolidAt(int column) => _solid[column];

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
