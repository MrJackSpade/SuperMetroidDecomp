namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The suit pickup's light beam's inclusive left and right edges on the screen; only X is meaningful. A left edge right
/// of the right edge lights nothing.
/// </summary>
/// <param name="Left">The inclusive left edge, at a column 0 through 255.</param>
/// <param name="Right">The inclusive right edge, at a column 0 through 255.</param>
public readonly record struct SuitPickupBeamEdges(Position16<ScreenPixels> Left, Position16<ScreenPixels> Right)
{
    /// <summary>The edges at screen columns <paramref name="left"/> and <paramref name="right"/>.</summary>
    public static SuitPickupBeamEdges Columns(int left, int right) =>
        new(Position16<ScreenPixels>.Of(left, 0), Position16<ScreenPixels>.Of(right, 0));
}
