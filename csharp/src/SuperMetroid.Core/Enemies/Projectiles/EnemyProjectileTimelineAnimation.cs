using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that plays one <see cref="AnimationTimeline{TFrame, TCue}"/>: each cue goes to the
/// handler its family registered with <see cref="EnemyProjectileHandlerTable.RegisterCue{TCue}"/>.
/// </summary>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TCue">The family's cue type.</typeparam>
internal abstract class EnemyProjectileTimelineAnimation<TFrame, TCue> : EnemyProjectileAnimation
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The next step of the timeline.</summary>
    internal int Position { get; private set; }

    /// <summary>The timeline this animation plays.</summary>
    private protected abstract AnimationTimeline<TFrame, TCue> Timeline { get; }

    /// <summary>Plays the timeline again from its first step, as reinstalling an instruction list does.</summary>
    private protected void Restart() => Position = 0;

    /// <summary>The projectile frame <paramref name="frame"/> shows.</summary>
    private protected abstract EnemyProjectileFrameId Present(TFrame frame);

    /// <summary>Plays the timeline, publishing each cue to the family's registered handler.</summary>
    private protected sealed override bool TryShowNextFrame(
        EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers, out int duration, out EnemyProjectileFrameId frame)
    {
        int position = Position;
        AnimationAdvance<TFrame> advance = Timeline.Advance(ref position, cue => handlers.PublishCue(context, cue));
        Position = position;
        duration = advance.Duration;
        frame = advance.Held ? default : Present(advance.Frame);
        return !advance.Held;
    }
}
