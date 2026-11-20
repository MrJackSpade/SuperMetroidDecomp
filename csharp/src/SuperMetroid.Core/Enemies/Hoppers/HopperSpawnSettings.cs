namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The surface a hopper's placement mounts it on (#1472, #2338).</summary>
/// <param name="Surface">The floor or the ceiling.</param>
internal sealed record HopperSpawnSettings(HopperSurface Surface) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
