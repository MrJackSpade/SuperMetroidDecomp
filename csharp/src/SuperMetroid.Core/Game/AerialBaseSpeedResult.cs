namespace SuperMetroid.Core.Game;

/// <summary>Value and 65816 carry returned by <c>$90:9B1F</c>.</summary>
public readonly record struct AerialBaseSpeedResult(uint Speed, bool ReachedMaximum);
