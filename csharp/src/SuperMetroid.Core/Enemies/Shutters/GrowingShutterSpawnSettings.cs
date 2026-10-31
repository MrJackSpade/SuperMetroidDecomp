namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for, which way it grows and how fast (#1471, #1472).</summary>
/// <param name="Wait">What it waits for.</param>
/// <param name="Growth">The way it grows.</param>
/// <param name="ActivationDistance">The distance, 0 through 65535 pixels, Samus must come within.</param>
/// <param name="GrowthSpeedIndex">Its growth speed, 0 through 65535.</param>
internal sealed record GrowingShutterSpawnSettings(GrowingShutterWait Wait, GrowingShutterGrowth Growth, int ActivationDistance, int GrowthSpeedIndex) : EnemySpawnSettings
{
    /// <summary>The distance, 0 through 65535 pixels, Samus must come within.</summary>
    public int ActivationDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ActivationDistance);

    /// <summary>Its growth speed, 0 through 65535.</summary>
    public int GrowthSpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, GrowthSpeedIndex);
}
