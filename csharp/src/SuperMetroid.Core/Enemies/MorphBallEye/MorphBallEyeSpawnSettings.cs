namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Which part of a morph-ball eye a placement is (#1472, #1519): the eye's body or its beam mount, each its own settings
/// (#2474), so the body carries only its facing and the mount only its side.
/// </summary>
internal abstract record MorphBallEyeSpawnSettings : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
