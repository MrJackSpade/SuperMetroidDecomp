namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Which of its eight crawling directions a Yard starts in, and how fast it crawls (#1471, #1472, #1519).</summary>
/// <param name="Direction">The direction it starts crawling in.</param>
/// <param name="CrawlSpeed">Its idle crawl speed in pixels per update, the magnitude its direction signs on each axis.</param>
internal sealed record YardSpawnSettings(YardDirection Direction, SignedEightEight CrawlSpeed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
