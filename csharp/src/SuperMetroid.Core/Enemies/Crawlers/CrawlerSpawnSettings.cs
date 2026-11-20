namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which surface a crawler starts on, which side of its block it holds, and how fast it crawls (#1471, #1472, #1519).</summary>
/// <param name="Orientation">The surface it starts on.</param>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity.</param>
/// <param name="CrawlSpeed">
/// Its crawl speed per update on both axes; none keeps the velocity it has and takes the default speed after its first
/// landing.
/// </param>
internal sealed record CrawlerSpawnSettings(CrawlerSurfaceOrientation Orientation, CrawlerSurfaceOrientation BlockSide, SignedEightEight? CrawlSpeed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
