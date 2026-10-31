namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The Y sample sequence of <c>SamusBlockInsideHandling</c> ($94:9B60). Every inside
/// reaction pass walks this one sequence so the native row-skip rules stay in one place.
/// </summary>
public static class SamusInsideBlockSamplePoints
{
    /// <summary>Invokes <paramref name="visit"/> for each sampled point in native order.</summary>
    public static void Visit(int yPosition, int yRadius, Action<int, SamusInsideBlockPoint> visit)
    {
        Ensure.NotNull(visit);
        int bottom = CoordinateAxis.Wrap(yPosition + yRadius - 1);
        int top = CoordinateAxis.Wrap(yPosition - yRadius);
        visit(bottom, SamusInsideBlockPoint.Bottom);
        if (((yPosition ^ bottom) & ~15) != 0)
            visit(yPosition, SamusInsideBlockPoint.Center);
        // $94:9BDC-$9BF1 skip the top when it shares the bottom's row, then chain a second
        // XOR onto the first result instead of the top itself: the top is also skipped
        // whenever ((top ^ bottom) & $FFF0) ^ Y has no bits above the low nibble, even if
        // the top lies in a different row from both.
        int topBottomRows = (top ^ bottom) & ~15;
        if (topBottomRows != 0 && ((topBottomRows ^ yPosition) & ~15) != 0)
            visit(top, SamusInsideBlockPoint.Top);
    }
}
