namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Which way a Dachora faces and whether it is one of its speed-boost echoes (#1472).</summary>
/// <param name="Facing">The way it faces.</param>
/// <param name="Role">Whether it is the Dachora or one of its speed-boost echoes.</param>
internal sealed record DachoraSpawnSettings(HorizontalDirection Facing, DachoraRole Role) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
