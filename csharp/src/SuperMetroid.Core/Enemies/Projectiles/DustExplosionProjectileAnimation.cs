using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The animation of a dust cloud, explosion, smoke or bubbles.</summary>
internal sealed class DustExplosionProjectileAnimation(DustExplosionAnimation animation)
    : EnemyProjectileFrameAnimation<DustExplosionAnimation, EnemyProjectileCommonCue>(animation, DustExplosionProjectileAnimations.StartingWith(animation), EnemyProjectileCueRunners.Common<DustExplosionAnimation>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DustExplosionProjectileAnimations.Segments;
}
