using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which wall of Ceres Ridley's getaway chamber his getaway places (#1989).</summary>
/// <param name="Side">The side of the chamber the wall stands on.</param>
internal sealed record CeresGetawayChamberWallSpawnSettings(HorizontalDirection Side) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
