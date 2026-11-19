using System.Numerics;

namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A speed, distance or turn taken toward a direction (#2569): the one operation that gives a quantity its direction, so a
/// velocity is a signed value built toward its side rather than a size whose sign a test restores.
/// </summary>
public static class TowardDirection
{
    /// <summary>
    /// <paramref name="amount"/> taken toward <paramref name="direction"/>: as it is rightward, toward increasing X, and
    /// reversed leftward.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not left or right.</exception>
    public static T Toward<T>(this T amount, HorizontalDirection direction) where T : IUnaryNegationOperators<T, T> => direction switch
    {
        HorizontalDirection.Right => amount,
        HorizontalDirection.Left => -amount,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a horizontal direction."),
    };

    /// <summary>
    /// <paramref name="amount"/> taken toward <paramref name="direction"/>: as it is downward, toward increasing Y, and
    /// reversed upward.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not up or down.</exception>
    public static T Toward<T>(this T amount, VerticalDirection direction) where T : IUnaryNegationOperators<T, T> => direction switch
    {
        VerticalDirection.Down => amount,
        VerticalDirection.Up => -amount,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a vertical direction."),
    };
}
