namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Which way a Waver first flies (#1472).</summary>
/// <param name="Facing">The way it first flies.</param>
internal sealed record WaverSpawnSettings(HorizontalDirection Facing) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
