namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="RotationAngle"/> that only verification reads.</summary>
internal static class RotationAngleConstants2
{
    /// <summary>The angle <paramref name="halfSteps"/> half steps round from zero, 0 through 511: a whole or a half step.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> lies outside one turn.</exception>
    public static RotationAngle FromHalfSteps(int halfSteps) =>
        RotationAngle.FromSteps(Ensure.BetweenInclusive(0, 2 * RotationAngle.StepsPerTurn - 1, halfSteps) / 2, halfSteps % 2 * (RotationAngle.FractionsPerStep / 2));
}
