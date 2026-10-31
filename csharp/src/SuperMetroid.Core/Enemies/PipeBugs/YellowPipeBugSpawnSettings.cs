namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Which way a yellow pipe bug flies and how fast (#1472).</summary>
/// <param name="Facing">The way it flies.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 8191, it flies at.</param>
internal sealed record YellowPipeBugSpawnSettings(HorizontalDirection Facing, int SpeedIndex) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 8191, it flies at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);
}
