namespace SuperMetroid.Core.Enemies;

/// <summary>Whether a lava jumper or Rio follows the enemy before it rather than leads (#1472).</summary>
/// <param name="Role">Whether it leads or follows the enemy before it.</param>
internal sealed record EnemyFollowerSpawnSettings(EnemyFollowerRole Role) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
