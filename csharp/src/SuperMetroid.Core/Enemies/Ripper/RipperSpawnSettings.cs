namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>How fast a Ripper flies and which way it starts (#1472).</summary>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 8191, it flies at.</param>
/// <param name="Direction">The way it starts.</param>
internal sealed record RipperSpawnSettings(int SpeedIndex, HorizontalDirection Direction) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 8191, it flies at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);
}
