using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class MiscDustProjectileAnimation(DustExplosionAnimation animation)
    : EnemyProjectileFrameAnimation<DustExplosionAnimation, EnemyProjectileCommonCue>(MiscDustProjectileAnimations.StartingWith(animation))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => MiscDustProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DustExplosionAnimation, Animation<DustExplosionAnimation>> StartingAt => MiscDustProjectileAnimations.StartingAt;
}
