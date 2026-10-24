namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One scanline of Mother Brain's rainbow beam: the inclusive left and right screen columns its colour adds across. A
/// left column right of the right column means the beam misses the line.
/// </summary>
/// <param name="Left">The inclusive left screen column, 0 through 255.</param>
/// <param name="Right">The inclusive right screen column, 0 through 255.</param>
public readonly record struct MotherBrainBeamLine(int Left, int Right)
{
    /// <summary>The rightmost screen column.</summary>
    private const int LastColumn = 255;

    /// <summary>The inclusive left screen column.</summary>
    public int Left { get; } = Ensure.BetweenInclusive(0, LastColumn, Left);

    /// <summary>The inclusive right screen column.</summary>
    public int Right { get; } = Ensure.BetweenInclusive(0, LastColumn, Right);

    /// <summary>A line the beam misses: its left column is the rightmost and its right column the leftmost.</summary>
    public static MotherBrainBeamLine None { get; } = new(LastColumn, 0);

    /// <summary>Whether the beam's colour shows on this line.</summary>
    public bool Crosses => Left <= Right;
}
