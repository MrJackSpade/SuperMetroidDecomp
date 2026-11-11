namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The base speed the deceleration-disallowed calculation produces, and whether it reached its maximum.</summary>
[IndependentFlag("whether the speed reached its maximum, a fact of the step")]
public readonly record struct AerialBaseSpeedResult(UnsignedSixteenSixteen Speed, bool ReachedMaximum);
