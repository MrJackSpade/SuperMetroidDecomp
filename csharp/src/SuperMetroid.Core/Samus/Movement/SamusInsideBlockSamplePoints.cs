namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The Y sample sequence of Samus's inside-block handling. Every inside
/// reaction pass walks this one sequence so the row-skip rules stay in one place.
/// </summary>
public static class SamusInsideBlockSamplePoints
{
    /// <summary>
    /// Invokes <paramref name="visit"/> for each sampled point in order: the bottom, the centre when it lies on another
    /// block row from the bottom, and the top when <see cref="InsideBlockTopSample"/> samples it.
    /// </summary>
    public static void Visit(int yPosition, int yRadius, Action<int, SamusInsideBlockPoint> visit)
    {
        Ensure.NotNull(visit);
        int bottom = CoordinateAxis.Wrap(yPosition + yRadius - 1);
        int top = CoordinateAxis.Wrap(yPosition - yRadius);
        visit(bottom, SamusInsideBlockPoint.Bottom);
        if (RoomGridConversions.BlockStart(yPosition) != RoomGridConversions.BlockStart(bottom))
            visit(yPosition, SamusInsideBlockPoint.Center);
        if (InsideBlockTopSample.IsSampled(top, bottom, yPosition))
            visit(top, SamusInsideBlockPoint.Top);
    }
}
