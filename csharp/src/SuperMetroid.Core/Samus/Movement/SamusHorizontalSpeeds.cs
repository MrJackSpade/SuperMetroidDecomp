namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One movement's horizontal speeds: how fast Samus speeds up, the speed she is held to and how fast she slows.</summary>
/// <param name="Acceleration">Whole and fractional pixels an update added to her base speed while she accelerates.</param>
/// <param name="MaximumSpeed">Whole and fractional pixels an update her accelerated base speed is held to.</param>
/// <param name="Deceleration">Whole and fractional pixels an update taken from her base speed while she slows.</param>
public readonly record struct SamusHorizontalSpeeds(
    SignedSixteenSixteen Acceleration,
    SignedSixteenSixteen MaximumSpeed,
    SignedSixteenSixteen Deceleration)
{
}
