namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="Bearing"/> that only verification reads.</summary>
internal static class BearingConstants
{
    /// <summary>The bearing pointing straight along <paramref name="direction"/>: a whole number of eighths of the turn clockwise from up.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not a compass direction.</exception>
    public static Bearing Toward(CompassDirection direction)
    {
        int eighths = Array.IndexOf(Bearing.DirectionsClockwise, direction);
        return eighths >= 0
            ? new(eighths * (RotationAngle.StepsPerTurn / Bearing.DirectionsClockwise.Length))
            : throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Undefined {nameof(CompassDirection)}.");
    }
}
