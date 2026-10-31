namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Which way a Stoke moves and how fast (#1472).</summary>
/// <param name="Direction">The way it moves.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 65535, it moves at.</param>
internal sealed record StokeSpawnSettings(StokeDirection Direction, int SpeedIndex) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 65535, it moves at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);
}
