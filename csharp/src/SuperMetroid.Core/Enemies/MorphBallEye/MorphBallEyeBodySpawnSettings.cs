namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>A morph-ball eye body's placement: the way it faces.</summary>
/// <param name="Facing">The way it faces while it waits.</param>
internal sealed record MorphBallEyeBodySpawnSettings(HorizontalDirection Facing) : MorphBallEyeSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
