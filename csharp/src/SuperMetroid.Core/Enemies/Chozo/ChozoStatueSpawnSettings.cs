namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Which of the two Chozo statues a placement is (#1472, #1519).</summary>
/// <param name="Site">The statue.</param>
internal sealed record ChozoStatueSpawnSettings(ChozoStatueSite Site) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
