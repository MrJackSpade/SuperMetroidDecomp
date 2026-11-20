namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>How a Spark starts and how long it waits (#1472, #1519).</summary>
/// <param name="Start">The animation and behaviour it starts with, one of <see cref="SparkMovementDefinitions"/>'s three.</param>
/// <param name="BaseActionTime">The updates it waits between actions.</param>
internal sealed record SparkSpawnSettings(SparkMovementDefinition Start, int BaseActionTime) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
