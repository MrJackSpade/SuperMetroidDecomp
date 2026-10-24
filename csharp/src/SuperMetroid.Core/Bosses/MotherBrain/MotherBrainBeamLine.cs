namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One scanline of Mother Brain's rainbow beam: the inclusive left and right screen columns its colour adds across. A
/// left column right of the right column means the beam misses the line.
/// </summary>
/// <param name="Left">The inclusive left screen column, 0 through 255.</param>
/// <param name="Right">The inclusive right screen column, 0 through 255.</param>
public readonly record struct MotherBrainBeamLine(byte Left, byte Right)
{
    /// <summary>A line the beam misses: its left column is the rightmost and its right column the leftmost.</summary>
    public static MotherBrainBeamLine None { get; } = new(byte.MaxValue, 0);

    /// <summary>Whether the beam's colour shows on this line.</summary>
    public bool Crosses => Left <= Right;
}
