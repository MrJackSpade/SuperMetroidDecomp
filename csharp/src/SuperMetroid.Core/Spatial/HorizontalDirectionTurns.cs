namespace SuperMetroid.Core.Spatial;

/// <summary>Turning a horizontal direction around (#2277).</summary>
public static class HorizontalDirectionTurns
{
    /// <summary>The direction opposite <paramref name="direction"/>: left for right and right for left.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not left or right.</exception>
    public static HorizontalDirection Reversed(this HorizontalDirection direction) => direction switch
    {
        HorizontalDirection.Left => HorizontalDirection.Right,
        HorizontalDirection.Right => HorizontalDirection.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a horizontal direction."),
    };
}
