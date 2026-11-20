namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>What a Hibashi placement is (#1472, #1519, #2342): its flame or the collision part the flame moves and sizes.</summary>
internal abstract record HibashiSpawnSettings : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
