namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One horizontal speed record for a movement type: per-frame acceleration, speed cap and deceleration.</summary>
/// <param name="Acceleration">Whole and fractional pixels per frame added to base speed while accelerating.</param>
/// <param name="MaximumSpeed">Whole and fractional pixels per frame that accelerated base speed is capped at.</param>
/// <param name="Deceleration">Whole and fractional pixels per frame removed from base speed while decelerating.</param>
public readonly record struct SpeedTableEntry(
    SignedSixteenSixteen Acceleration,
    SignedSixteenSixteen MaximumSpeed,
    SignedSixteenSixteen Deceleration)
{
}
