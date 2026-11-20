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

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6426", Justification = "Every projectile animation takes its next frame through this shared step; a silent timeline raises no events, so it needs neither the cue context nor the engine the timelines that raise events hand their cues.")]
    private protected sealed override AnimationShown<TSegment, TFrame> PlayToNextFrame(EnemyProjectileCueContext context,
        EnemyProjectileEngine projectiles, ref Animation<TSegment> animation, ref AnimationPlace place) =>
        Segments.PlayToNextFrame(ref animation, ref place, static (_, _) => null);
}
