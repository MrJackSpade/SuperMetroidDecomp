namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Which way an Atomic spins and how fast (#1472).</summary>
/// <param name="Spin">Its spin, 0 through 3.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, it spins at.</param>
internal sealed record AtomicSpawnSettings(int Spin, int SpeedIndex) : EnemySpawnSettings
{
    /// <summary>Its spin, 0 through 3.</summary>
    public int Spin { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Spin);

    /// <summary>The common linear-speed record, 0 through 255, it spins at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);
}
