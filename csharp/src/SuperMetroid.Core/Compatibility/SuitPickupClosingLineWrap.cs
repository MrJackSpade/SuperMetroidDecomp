namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the suit pickup's closing light line narrows (#1692): every update of the line's dissipation moves its left edge
/// eight columns right and its right edge eight columns left, and an edge carried past either side of the screen's row of
/// 256 columns continues from the other side. The dissipation starts its left edge at the screen's rightmost column and
/// its right edge at column 248, so the first narrowing carries the left edge round to column 7, and the line closes
/// from columns 7 and 240 until the left edge reaches column 112, when the pickup finishes and releases Samus: the wrap
/// sets how many updates pass before Samus moves again, and the edge columns the shinespark's carried acceleration reads
/// on each of them (<see cref="SuitPickupSparkCarryover"/>).
/// </summary>
/// <remarks>
/// <para>
/// Consulted once on every update of the light beam's dissipation, before the pickup tests whether the line has closed.
/// A stateless rule.
/// </para>
/// <para>
/// Evidence: the dissipation ($88:E22B) adds eight to the low byte of the beam's edge word and subtracts eight from its
/// high byte, each within its byte, and advances to the pickup's finish once the low byte reads 112 or more; the shrinking
/// stage ($88:E1BA) leaves that word at $F8FF, the left edge 255 and the right edge 248; the suit pickup carryover check
/// confirms the edges at every update of a Varia transformation.
/// </para>
/// </remarks>
[CompatibilityService("the suit pickup's closing line carries its left edge from the screen's right side round to its left, which sets when the pickup releases Samus")]
public static class SuitPickupClosingLineWrap
{
    /// <summary>The columns of the screen's row the line's edges move within.</summary>
    public const int RowColumns = 256;

    /// <summary>The columns each edge moves inward on every update.</summary>
    private const int ColumnsPerUpdate = 8;

    /// <summary>The left edge after one update's narrowing: eight columns right, from the rightmost column round to the left.</summary>
    /// <param name="left">The line's left edge column, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="left"/> is not a column of the row.</exception>
    public static int NarrowedLeft(int left) =>
        (Ensure.BetweenInclusive(0, RowColumns - 1, left) + ColumnsPerUpdate) % RowColumns;

    /// <summary>The right edge after one update's narrowing: eight columns left, from the leftmost column round to the right.</summary>
    /// <param name="right">The line's right edge column, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="right"/> is not a column of the row.</exception>
    public static int NarrowedRight(int right) =>
        (Ensure.BetweenInclusive(0, RowColumns - 1, right) - ColumnsPerUpdate + RowColumns) % RowColumns;
}
