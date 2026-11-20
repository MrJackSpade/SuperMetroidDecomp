using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The animation of a dust cloud, explosion, smoke or bubbles.</summary>
internal sealed class DustExplosionProjectileAnimation(DustExplosionAnimation animation)
    : EnemyProjectileFrameAnimation<DustExplosionAnimation, EnemyProjectileCommonCue>(DustExplosionProjectileAnimations.StartingWith(animation), EnemyProjectileCueRunners.Common<DustExplosionAnimation>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DustExplosionProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<DustExplosionAnimation, Animation<DustExplosionAnimation>>> StartingAt => DustExplosionProjectileAnimations.StartingAt;
}
