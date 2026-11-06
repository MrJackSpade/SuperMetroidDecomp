namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which surface a crawler starts on, which side of its block it holds, and how fast it crawls (#1471, #1472).</summary>
/// <param name="Orientation">The surface it starts on.</param>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity.</param>
/// <param name="SpeedIndex">Its crawl speed, 0 through 65535, or the selector that keeps its velocity.</param>
/// <param name="SurfaceFamily">The family whose animations it switches between at corners, where the species does not fix one.</param>
internal sealed record CrawlerSpawnSettings(CrawlerSurfaceOrientation Orientation, CrawlerSurfaceOrientation BlockSide, int SpeedIndex, CrawlerAnimationFamily SurfaceFamily) : EnemySpawnSettings
{
    /// <summary>Its crawl speed, 0 through 65535, or the selector that keeps its velocity.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedIndex);
}
