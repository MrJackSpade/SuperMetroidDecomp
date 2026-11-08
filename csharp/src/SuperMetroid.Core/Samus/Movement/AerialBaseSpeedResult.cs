namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Value and carry returned by the deceleration-disallowed base-speed calculation.</summary>
[IndependentFlag("whether the speed reached its maximum, a fact of the step")]
public readonly record struct AerialBaseSpeedResult(UnsignedSixteenSixteen Speed, bool ReachedMaximum);
