namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which surface a crawler starts on, which side of its block it holds, and how fast it crawls (#1471, #1472, #1519).</summary>
/// <param name="Orientation">The surface it starts on.</param>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity.</param>
/// <param name="CrawlSpeed">
/// Its crawl speed on both axes, in 1/256 pixels per update; none keeps the velocity it has and takes the default speed
/// after its first landing.
/// </param>
/// <param name="SurfaceFamily">The family whose animations it switches between at corners, where the species does not fix one.</param>
internal sealed record CrawlerSpawnSettings(CrawlerSurfaceOrientation Orientation, CrawlerSurfaceOrientation BlockSide, SignedEightEight? CrawlSpeed, CrawlerAnimationFamily SurfaceFamily) : EnemySpawnSettings;
