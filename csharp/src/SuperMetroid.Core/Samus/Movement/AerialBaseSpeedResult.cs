namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Value and 65816 carry returned by <c>$90:9B1F</c>.</summary>
[IndependentFlag("whether the speed reached its maximum, a fact of the step")]
public readonly record struct AerialBaseSpeedResult(uint Speed, bool ReachedMaximum);
