namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How close below a Boulder Samus must come before it falls (#1471).</summary>
/// <param name="TriggerHeight">The vertical distance, 0 through 255 pixels, Samus must be strictly within.</param>
public sealed record BoulderSpawnSettings(int TriggerHeight) : EnemySpawnSettings
{
    /// <summary>The vertical distance, 0 through 255 pixels, Samus must be strictly within.</summary>
    public int TriggerHeight { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, TriggerHeight);
}
