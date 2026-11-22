namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Where a crawler starts and how fast it crawls, whichever placement settings carry them (#1471, #1472, #1519).</summary>
internal abstract partial record CrawlerStartSettings : EnemySpawnSettings
{
    /// <summary>The surface the crawler starts on.</summary>
    internal abstract CrawlerSurfaceOrientation StartingSurface { get; }

    /// <summary>The side of its block the crawler holds, which signs its crawl velocity.</summary>
    internal abstract CrawlerSurfaceOrientation HeldBlockSide { get; }

    /// <summary>
    /// Its crawl speed per update on both axes; none keeps the velocity it has and takes the default speed after its first
    /// landing.
    /// </summary>
    internal abstract SignedEightEight? StartingCrawlSpeed { get; }
}
