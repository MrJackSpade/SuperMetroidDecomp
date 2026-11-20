namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which side of its block a Viola holds, which is also the surface it starts on, and how fast it crawls (#1472, #1519).</summary>
/// <param name="BlockSide">The side of its block it holds, which signs its crawl velocity and names its starting surface.</param>
/// <param name="CrawlSpeed">
/// Its crawl speed per update on both axes; none keeps the velocity it has and takes the default speed after its first
/// landing.
/// </param>
internal sealed record ViolaSpawnSettings(CrawlerSurfaceOrientation BlockSide, SignedEightEight? CrawlSpeed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
