namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values and helpers of <see cref="SamusVerticalVelocity"/> that only verification reads.</summary>
internal static class SamusVerticalVelocityConstants
{
    /// <summary>
    /// The velocity of <paramref name="speed"/> heading <paramref name="direction"/>, as a state recorded in those two parts
    /// gives it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is undefined.</exception>
    public static SamusVerticalVelocity Of(UnsignedSixteenSixteen speed, SamusVerticalDirection direction) => new global::SuperMetroid.Core.Samus.Movement.SamusVerticalVelocity(speed, direction);
}
