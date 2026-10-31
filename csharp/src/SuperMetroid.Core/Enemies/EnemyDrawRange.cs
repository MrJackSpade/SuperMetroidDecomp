namespace SuperMetroid.Core.Enemies;

/// <summary>Where the enemy draw list draws a component of an extended enemy frame (#1412).</summary>
internal static class EnemyDrawRange
{
    /// <summary>The leftmost or topmost coordinate a drawn component may have.</summary>
    private const int First = -128;

    /// <summary>The rightmost or bottommost coordinate a drawn component may have.</summary>
    private const int Last = 383;

    /// <summary>
    /// Whether a component at <paramref name="origin"/> is drawn: both coordinates, read as signed screen positions,
    /// lie within -128 through 383; a component outside that range is skipped.
    /// </summary>
    internal static bool Contains(Position16<SpritePixels> origin) => Axis(origin.X) && Axis(origin.Y);

    /// <summary>Whether one coordinate, read as a signed screen position, lies within the drawn range.</summary>
    private static bool Axis(int coordinate) => CoordinateAxis.AsSigned(coordinate) is >= First and <= Last;
}
