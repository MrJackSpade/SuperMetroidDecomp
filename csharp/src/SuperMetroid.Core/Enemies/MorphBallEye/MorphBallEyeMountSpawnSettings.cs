namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>A morph-ball eye's beam mount's placement: the side of the eye's centre it sits on.</summary>
/// <param name="Side">The side of the eye's centre the mount sits on.</param>
internal sealed record MorphBallEyeMountSpawnSettings(MorphBallEyeMountSide Side) : MorphBallEyeSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
