namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>How a Cacatac patrols (#1472).</summary>
/// <param name="Direction">The way it starts.</param>
/// <param name="Mounting">Whether it hangs from a ceiling or stands on a floor.</param>
/// <param name="DistanceIndex">Its travel distance, 0 through 255.</param>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, it walks at.</param>
internal sealed record CacatacSpawnSettings(CacatacDirection Direction, CacatacMounting Mounting, int DistanceIndex, int SpeedIndex) : EnemySpawnSettings
{
    /// <summary>Its travel distance, 0 through 255.</summary>
    public int DistanceIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, DistanceIndex);

    /// <summary>The common linear-speed record, 0 through 255, it walks at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);
}
