namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether Samus's inside-block reactions sample her top point (#1621): only when her top lies on a different block row
/// from her bottom, and the two rows' numbers, combined bit by bit (each bit set where exactly one of them has it), do not
/// give her centre's row. The top is therefore also left unsampled on rows the combination happens to name, even when it
/// lies on a different row from both her centre and her bottom: a top on the room's first row with her centre and bottom
/// on the second, or a top on row 1 with her centre on row 2 and her bottom on row 3, reacts to no block there.
/// </summary>
/// <remarks>
/// Consulted once per inside-block pass (the block reactions after movement and the terrain hazards), after her bottom
/// and centre are sampled. Evidence: the original sampling skips the top when the top and bottom rows match, and then
/// combines the top's and bottom's coordinates with an exclusive or, keeps the row bits, and combines that result with
/// the centre's coordinate instead of the top's own; the inside-block sampling check compares the rule with that
/// combination for every centre row and radius.
/// </remarks>
[CompatibilityService("Samus's inside-block reactions skip her top point when its row and her bottom's, combined bit by bit, give her centre's row")]
public static class InsideBlockTopSample
{
    /// <summary>Pixels along a block's side.</summary>
    private const int BlockPixels = 16;

    /// <summary>
    /// Whether the top point is sampled for a body whose top, bottom and centre lie at the room pixel rows given, each
    /// 0 through 65535.
    /// </summary>
    /// <param name="top">The body's top pixel row.</param>
    /// <param name="bottom">The body's bottom pixel row.</param>
    /// <param name="centre">The body's centre pixel row.</param>
    public static bool IsSampled(int top, int bottom, int centre)
    {
        int topRow = BlockRow(top);
        int bottomRow = BlockRow(bottom);
        return topRow != bottomRow && (topRow ^ bottomRow) != BlockRow(centre);
    }

    /// <summary>The block row holding a room pixel row, 0 through 65535.</summary>
    private static int BlockRow(int pixel) => Ensure.BetweenInclusive(0, ushort.MaxValue, pixel) / BlockPixels;
}
