namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which surface an orange Zoomer starts on, which side of its block it holds, and how fast it crawls (#2554).</summary>
/// <param name="Orientation">The surface it starts on.</param>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity.</param>
/// <param name="CrawlSpeed">Its crawl speed per update on both axes.</param>
internal sealed record OrangeZoomerSpawnSettings(CrawlerSurfaceOrientation Orientation, CrawlerSurfaceOrientation BlockSide,
    SignedEightEight CrawlSpeed) : EnemySpawnSettings;
