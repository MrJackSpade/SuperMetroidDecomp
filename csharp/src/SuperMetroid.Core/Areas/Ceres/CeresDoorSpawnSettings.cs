using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which way an ordinary Ceres door a placement places faces (#1472, #1989).</summary>
/// <param name="Facing">The way the door faces, which selects its facing's animations.</param>
internal sealed record CeresDoorSpawnSettings(HorizontalDirection Facing) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
