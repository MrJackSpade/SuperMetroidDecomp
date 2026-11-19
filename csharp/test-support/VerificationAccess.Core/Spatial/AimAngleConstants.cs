namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The cartridge's aim words as <see cref="TrigonometryAngle"/> values (#2555), which only verification decodes: the
/// original counts an aim in steps clockwise from straight up, the game in steps anticlockwise from right.
/// </summary>
internal static class AimAngleConstants
{
    /// <summary>The steps of a whole turn.</summary>
    private const int StepsPerTurn = 256;

    /// <summary>The aim the cartridge's word of <paramref name="steps"/> steps clockwise from up names.</summary>
    public static TrigonometryAngle FromClockwiseSteps(int steps) => TrigonometryAngle.Wrapped(TrigonometryAngle.QuarterTurn.RawValue - steps);

    /// <summary>The cartridge's word for <paramref name="aim"/>: its steps clockwise from up, 0 through 255.</summary>
    public static int ClockwiseSteps(TrigonometryAngle aim) => (TrigonometryAngle.QuarterTurn.RawValue - aim.RawValue + StepsPerTurn) % StepsPerTurn;

    /// <summary>The aim pointing straight along <paramref name="direction"/>: a whole number of eighths of the turn clockwise from up.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not a compass direction.</exception>
    public static TrigonometryAngle Toward(CompassDirection direction)
    {
        int eighths = Array.IndexOf(TrigonometryAngle.DirectionsClockwise, direction);
        return eighths >= 0
            ? FromClockwiseSteps(eighths * (StepsPerTurn / TrigonometryAngle.DirectionsClockwise.Length))
            : throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Undefined {nameof(CompassDirection)}.");
    }
}
