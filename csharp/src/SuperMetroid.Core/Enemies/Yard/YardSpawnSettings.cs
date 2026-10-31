namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Which of its eight crawling directions a Yard starts in, and how fast it crawls (#1471, #1472).</summary>
/// <param name="Direction">The direction it starts crawling in.</param>
/// <param name="SpeedIndex">Its crawl speed, 0 through 65535.</param>
internal sealed record YardSpawnSettings(YardDirection Direction, int SpeedIndex) : EnemySpawnSettings
{
    /// <summary>Its crawl speed, 0 through 65535.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);
}
