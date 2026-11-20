using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class DustExplosionProjectileAnimation(DustExplosionAnimation animation)
    : EnemyProjectileFrameAnimation<DustExplosionAnimation, EnemyProjectileCommonCue>(DustExplosionProjectileAnimations.StartingWith(animation), EnemyProjectileCueRunners.Common<DustExplosionAnimation>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DustExplosionProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<DustExplosionAnimation, Animation<DustExplosionAnimation>>> StartingAt => DustExplosionProjectileAnimations.StartingAt;
}
