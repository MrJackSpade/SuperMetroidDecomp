namespace SuperMetroid.Core.Game;

/// <summary>The body point whose block a <c>$94:9B60</c> inside reaction is running for.</summary>
public enum SamusInsideBlockPoint
{
    /// <summary><c>InsideBlockReactionSamusPoint</c> = 0: one pixel above the bottom boundary.</summary>
    Bottom,

    /// <summary><c>InsideBlockReactionSamusPoint</c> = 1: Samus's Y position.</summary>
    Center,

    /// <summary><c>InsideBlockReactionSamusPoint</c> = 2: Y position minus the Y radius.</summary>
    Top,
}

/// <summary>
/// The Y sample sequence of <c>SamusBlockInsideHandling</c> ($94:9B60). Every inside
/// reaction pass walks this one sequence so the native row-skip rules stay in one place.
/// </summary>
public static class SamusInsideBlockSamplePoints
{
    /// <summary>Invokes <paramref name="visit"/> for each sampled point in native order.</summary>
    public static void Visit(ushort yPosition, ushort yRadius, Action<ushort, SamusInsideBlockPoint> visit)
    {
        ArgumentNullException.ThrowIfNull(visit);
        ushort bottom = unchecked((ushort)(yPosition + yRadius - 1));
        ushort top = unchecked((ushort)(yPosition - yRadius));
        visit(bottom, SamusInsideBlockPoint.Bottom);
        if (((yPosition ^ bottom) & 0xfff0) != 0)
            visit(yPosition, SamusInsideBlockPoint.Center);
        // $94:9BDC-$9BF1 skip the top when it shares the bottom's row, then chain a second
        // XOR onto the first result instead of the top itself: the top is also skipped
        // whenever ((top ^ bottom) & $FFF0) ^ Y has no bits above the low nibble, even if
        // the top lies in a different row from both.
        int topBottomRows = (top ^ bottom) & 0xfff0;
        if (topBottomRows != 0 && ((topBottomRows ^ yPosition) & 0xfff0) != 0)
            visit(top, SamusInsideBlockPoint.Top);
    }
}
