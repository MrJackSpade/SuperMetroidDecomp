namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>A GRipper's patrol speed and starting direction (#1471).</summary>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, the GRipper patrols at.</param>
/// <param name="Direction">The direction it starts moving in.</param>
public sealed record GRipperSpawnSettings(int SpeedIndex, GRipperStartDirection Direction) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, the GRipper patrols at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);
}
