using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The animation of a dust cloud, explosion, smoke or bubbles.</summary>
internal sealed partial class DustExplosionProjectileAnimation
    : EnemyProjectileFrameAnimation<DustExplosionAnimation, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="DustExplosionProjectileAnimation"/>.</summary>
    /// <param name="animation">animation</param>
    public DustExplosionProjectileAnimation(DustExplosionAnimation animation)
        : base(animation, DustExplosionProjectileAnimations.StartingAt[animation], EnemyProjectileCueRunners.Common<DustExplosionAnimation>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DustExplosionProjectileAnimations.Segments;
}
