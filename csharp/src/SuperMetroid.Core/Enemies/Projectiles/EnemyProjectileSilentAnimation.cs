using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation whose typed timeline raises no events (#2571): frames, holds and repeats only. It has no cue
/// runner, since no event can arise from it; its play passes no reaction to carry one out.
/// </summary>
/// <typeparam name="TSegment">The family's segment identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
internal abstract class EnemyProjectileSilentAnimation<TSegment, TFrame> : EnemyProjectileTimelineAnimation<TSegment, TFrame, NoAnimationCue>
    where TSegment : struct
    where TFrame : struct
{
    /// <summary>Starts <paramref name="animation"/> before its first play.</summary>
    private protected EnemyProjectileSilentAnimation(Animation<TSegment> animation)
        : base(animation)
    {
    }

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame. The timeline raises no event,
    /// so nothing of the pass's <paramref name="context"/> is acted on.
    /// </summary>
    private protected sealed override IUnpausedHidingAnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        ref Animation<TSegment> animation, ref AnimationPlace place) =>
        Segments.PlayToNextFrame(ref animation, ref place, static (_, _) => null);
}
