namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which side of its block a Viola holds, which is also the surface it starts on, and how fast it crawls (#1472, #1519).</summary>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity and names its starting surface.</param>
/// <param name="CrawlSpeed">
/// Its crawl speed per update on both axes; none keeps the velocity it has and takes the default speed after its first
/// landing.
/// </param>
internal sealed record ViolaSpawnSettings(CrawlerSurfaceOrientation BlockSide, SignedEightEight? CrawlSpeed) : CrawlerStartSettings
{
    /// <inheritdoc/>
    /// <remarks>A Viola starts on the surface of the block side it holds; its placement names no surface of its own.</remarks>
    internal override CrawlerSurfaceOrientation StartingSurface => BlockSide;

    /// <inheritdoc/>
    internal override CrawlerSurfaceOrientation HeldBlockSide => BlockSide;

    /// <inheritdoc/>
    internal override SignedEightEight? StartingCrawlSpeed => CrawlSpeed;
}
