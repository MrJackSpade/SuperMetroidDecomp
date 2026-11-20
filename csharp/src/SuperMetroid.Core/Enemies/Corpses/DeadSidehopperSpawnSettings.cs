namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which Tourian Sidehopper placement a dead Sidehopper is (#1519).</summary>
/// <param name="Layout">The placement.</param>
internal sealed record DeadSidehopperSpawnSettings(DeadSidehopperLayout Layout) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
