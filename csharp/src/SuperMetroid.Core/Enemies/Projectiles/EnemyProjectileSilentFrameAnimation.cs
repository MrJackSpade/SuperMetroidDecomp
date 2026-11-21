using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A silent projectile animation whose frames are projectile frames themselves (#2571).</summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
internal abstract class EnemyProjectileSilentFrameAnimation<TSegment> : EnemyProjectileSilentAnimation<TSegment, EnemyProjectileFrameId>
    where TSegment : struct
{
    /// <summary>Starts <paramref name="animation"/> before its first play.</summary>
    private protected EnemyProjectileSilentFrameAnimation(TSegment startsAt, Animation<TSegment> animation)
        : base(startsAt, animation)
    {
    }

    /// <inheritdoc/>
    private protected sealed override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}
