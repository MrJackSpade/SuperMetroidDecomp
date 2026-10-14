namespace SuperMetroid.Core.Game;

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

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    private protected abstract ushort PresentationKey(TFrame frame);

    /// <summary>Plays the timeline, publishing each cue to the family's registered handler.</summary>
    private protected sealed override bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out ushort presentationKey)
    {
        int position = Position;
        AnimationAdvance<TFrame> advance = Timeline.Advance(ref position, cue => handlers.PublishCue(projectile, samus, cue));
        Position = position;
        duration = advance.Duration;
        presentationKey = advance.Held ? (ushort)0 : PresentationKey(advance.Frame);
        return !advance.Held;
    }
}
