namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>A morph-ball eye body's placement: the way it faces.</summary>
/// <param name="Facing">The way it faces while it waits.</param>
internal sealed record MorphBallEyeBodySpawnSettings(HorizontalDirection Facing) : MorphBallEyeSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
