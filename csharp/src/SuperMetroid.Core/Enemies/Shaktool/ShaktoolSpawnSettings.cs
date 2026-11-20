namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Which segment of a Shaktool a placement is, and how its chain starts moving (#1472, #1519, #2452).</summary>
/// <param name="Motion">How the chain starts moving; every segment of one chain names the same.</param>
/// <param name="Segment">The segment of the chain it is, one of <see cref="ShaktoolSegmentDefinitions.Segments"/>.</param>
internal sealed record ShaktoolSpawnSettings(ShaktoolChainMotion Motion, ShaktoolSegmentDefinition Segment) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
