namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Which surface an orange Zoomer starts on, which side of its block it holds, and how fast it crawls (#1471, #2146): its
/// setup always gives it a speed, the one its placement in the room gives it.
/// </summary>
/// <param name="Orientation">The surface it starts on.</param>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity.</param>
/// <param name="CrawlSpeed">Its crawl speed per update on both axes.</param>
internal sealed partial record OrangeZoomerSpawnSettings(CrawlerSurfaceOrientation Orientation, CrawlerSurfaceOrientation BlockSide, SignedEightEight CrawlSpeed) : EnemySpawnSettings;
