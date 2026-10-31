namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Which part of a Magdollite a placement is, and how its head rises (#1472).</summary>
/// <param name="Part">The part.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, the head rises at.</param>
/// <param name="Range">The horizontal distance, 0 through 255 pixels, Samus must come within.</param>
internal sealed record MagdolliteSpawnSettings(MagdollitePart Part, int SpeedIndex, int Range) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, the head rises at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>The horizontal distance, 0 through 255 pixels, Samus must come within.</summary>
    public int Range { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Range);
}
