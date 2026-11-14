namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled staircase used by Kraid's private body/projectile collision path.</summary>
public static class KraidBodyContour
{
    /// <summary>First bottom boundary of Kraid's body hitbox contour.</summary>
    private const int FirstBottomBoundary = 1023;

    /// <summary>
    /// Each left edge is paired
    /// with the next ledge's top boundary. The final signed minimum terminates
    /// the contour for every signed relative Y coordinate.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the complete table and both
    /// wrapped comparisons. The first bottom comparison wraps to
    /// nonnegative for Y=-32768..-31746, selecting -48 before the ordinary staircase.
    /// Remaining inputs select the first inclusive top boundary. Preserve this
    /// wrapped comparison, rather than replacing it with an ordered signed one.
    /// </remarks>
    /// <param name="relativeY">The signed vertical distance from Kraid's body, -32768 through 32767.</param>
    public static int LeftEdge(int relativeY) => relativeY switch
    {
        <= short.MinValue + FirstBottomBoundary - 1 => -48,
        >= 0 => -48,
        >= -32 => -32,
        >= -48 => -24,
        >= -80 => -8,
        >= -112 => 0,
        _ => 8,
    };
}
