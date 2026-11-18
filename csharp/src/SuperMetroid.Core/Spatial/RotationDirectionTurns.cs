namespace SuperMetroid.Core.Spatial;

/// <summary>Turning the sense of a rotation around (#2452).</summary>
public static class RotationDirectionTurns
{
    /// <summary>The other way round from <paramref name="direction"/>: counterclockwise for clockwise and clockwise for counterclockwise.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not clockwise or counterclockwise.</exception>
    public static RotationDirection Reversed(this RotationDirection direction) => direction switch
    {
        RotationDirection.Clockwise => RotationDirection.Counterclockwise,
        RotationDirection.Counterclockwise => RotationDirection.Clockwise,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a rotation direction."),
    };
}
