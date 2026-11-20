using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation whose beats show the family's projectile frames outright and whose only cues are the shared ones
/// (<see cref="EnemyProjectileCommonCue"/>), which the engine carries out for every kind (#2554).
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
internal abstract class EnemyProjectileCommonFrameAnimation<TSegment> : EnemyProjectileFrameAnimation<TSegment, EnemyProjectileCommonCue>
    where TSegment : struct
{
    /// <summary>Starts <paramref name="animation"/> before its first play, the engine carrying out its shared cues.</summary>
    /// <param name="animation">The animation.</param>
    private protected EnemyProjectileCommonFrameAnimation(Animation<TSegment> animation)
        : base(animation, EnemyProjectileCueRunners.Common<TSegment>())
    {
    }
}
