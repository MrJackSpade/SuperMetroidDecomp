using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class DustExplosionProjectileAnimation(DustExplosionAnimation animation)
    : EnemyProjectileFrameAnimation<DustExplosionAnimation, EnemyProjectileCommonCue>(DustExplosionProjectileAnimations.StartingWith(animation))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DustExplosionProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DustExplosionAnimation, Animation<DustExplosionAnimation>> StartingAt => DustExplosionProjectileAnimations.StartingAt;
}
