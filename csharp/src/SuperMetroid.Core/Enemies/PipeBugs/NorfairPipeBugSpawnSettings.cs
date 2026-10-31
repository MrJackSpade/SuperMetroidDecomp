namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>How a Norfair pipe bug rises and whether it leads its formation (#1472).</summary>
/// <param name="TriggerWidth">The horizontal distance, 0 through 255 pixels, Samus must come within; zero follows the leader.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, it rises at.</param>
internal sealed record NorfairPipeBugSpawnSettings(int TriggerWidth, int SpeedIndex) : EnemySpawnSettings
{
    /// <summary>The horizontal distance, 0 through 255 pixels, Samus must come within; zero follows the leader.</summary>
    public int TriggerWidth { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, TriggerWidth);

    /// <summary>The common linear-speed record, 0 through 255, it rises at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);
}
