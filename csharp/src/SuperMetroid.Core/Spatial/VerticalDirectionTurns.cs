namespace SuperMetroid.Core.Spatial;

/// <summary>Turning a vertical direction around (#2369).</summary>
public static class VerticalDirectionTurns
{
    /// <summary>The direction opposite <paramref name="direction"/>: down for up and up for down.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not up or down.</exception>
    public static VerticalDirection Reversed(this VerticalDirection direction) => direction switch
    {
        VerticalDirection.Up => VerticalDirection.Down,
        VerticalDirection.Down => VerticalDirection.Up,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a vertical direction."),
    };
}
