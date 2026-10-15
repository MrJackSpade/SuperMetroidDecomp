using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;

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

    /// <summary>How <paramref name="frame"/> is drawn.</summary>
    private protected abstract EnemyProjectileFramePresentation Present(TFrame frame);

    /// <summary>Plays the timeline, publishing each cue to the family's registered handler.</summary>
    private protected sealed override bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out EnemyProjectileFramePresentation presentation)
    {
        int position = Position;
        AnimationAdvance<TFrame> advance = Timeline.Advance(ref position, cue => handlers.PublishCue(projectile, samus, cue));
        Position = position;
        duration = advance.Duration;
        presentation = advance.Held ? default : Present(advance.Frame);
        return !advance.Held;
    }
}
