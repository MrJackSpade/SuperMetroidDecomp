namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>How a Boyon bounces and how near Samus must come (#1472).</summary>
/// <param name="SpeedMultiplierIndex">Its bounce speed multiplier, 0 through 255.</param>
/// <param name="JumpHeightIndex">Its jump height, 0 through 255.</param>
/// <param name="TriggerDistance">The distance, 0 through 65535 pixels, Samus must come within.</param>
internal sealed record BoyonSpawnSettings(int SpeedMultiplierIndex, int JumpHeightIndex, int TriggerDistance) : EnemySpawnSettings
{
    /// <summary>Its bounce speed multiplier, 0 through 255.</summary>
    public int SpeedMultiplierIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedMultiplierIndex);

    /// <summary>Its jump height, 0 through 255.</summary>
    public int JumpHeightIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, JumpHeightIndex);

    /// <summary>The distance, 0 through 65535 pixels, Samus must come within.</summary>
    public int TriggerDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, TriggerDistance);
}
