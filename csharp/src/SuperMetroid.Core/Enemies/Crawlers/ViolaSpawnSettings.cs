namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which side of its block a Viola holds, which is also the surface it starts on, and how fast it crawls (#1472, #1519).</summary>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity and names its starting surface.</param>
/// <param name="CrawlSpeed">
/// Its crawl speed on both axes, in 1/256 pixels per update; none keeps the velocity it has and takes the default speed
/// after its first landing.
/// </param>
/// <param name="SurfaceFamily">The surface animation family its entry names; the Viola's own initializer always uses its own.</param>
internal sealed record ViolaSpawnSettings(CrawlerSurfaceOrientation BlockSide, SignedEightEight? CrawlSpeed, CrawlerAnimationFamily SurfaceFamily) : EnemySpawnSettings;
