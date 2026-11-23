using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation whose beats show the family's projectile frames outright (#1623), so each frame it reaches is
/// the one it presents.
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TEvent">The family's events.</typeparam>
internal abstract partial class EnemyProjectileFrameAnimation<TSegment, TEvent> : EnemyProjectileTypedAnimation<TSegment, EnemyProjectileFrameId, TEvent>
    where TSegment : struct
    where TEvent : notnull
{
    /// <summary>Starts <paramref name="animation"/> before its first play, its events carried out by <paramref name="cues"/>.</summary>
    private protected EnemyProjectileFrameAnimation(TSegment startsAt, Animation<TSegment> animation, EnemyProjectileCueRunner<TSegment, TEvent> cues)
        : base(startsAt, animation, cues)
    {
    }

    /// <inheritdoc/>
    private protected sealed override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}
