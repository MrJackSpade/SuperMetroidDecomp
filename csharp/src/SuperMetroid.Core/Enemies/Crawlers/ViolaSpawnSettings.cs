namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which side of its block a Viola holds, which is also the surface it starts on, and how fast it crawls (#1472).</summary>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity and names its starting surface.</param>
/// <param name="SpeedIndex">Its crawl speed, 0 through 65535, or the selector that keeps its velocity.</param>
/// <param name="SurfaceTableOffset">Its surface-table offset, 0 through 65535.</param>
internal sealed record ViolaSpawnSettings(CrawlerSurfaceOrientation BlockSide, int SpeedIndex, int SurfaceTableOffset) : EnemySpawnSettings
{
    /// <summary>Its crawl speed, 0 through 65535, or the selector that keeps its velocity.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);

    /// <summary>Its surface-table offset, 0 through 65535.</summary>
    public int SurfaceTableOffset { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SurfaceTableOffset);
}
