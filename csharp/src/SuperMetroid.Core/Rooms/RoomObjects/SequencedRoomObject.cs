using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that plays authored animations of its frames (#1554), on the shared animation player (#2480): the door
/// caps, the eye doors, the downward gate, the elevator platform, the n00b tube and the Brinstar plants. On each update an
/// object that waits for something (a shot, an opened door) first reacts to it in its own update, then this one runs:
/// unless the object rests, its animation counts down the updates its shown frame stays and, when they run out, shows its
/// next frame, raising the events it passes on the way, or, after the last frame, ends and the object decides what comes
/// next. An animation the object starts while it updates shows its first frame on that same update, as the game's
/// objects continued into their next sequence on the same update. An object just placed decides on its first update, as
/// does one told to end on its next (#2558). An object whose ended animation is followed by nothing rests on the frame it
/// last showed until what it waits for starts another.
/// </summary>
/// <typeparam name="TSegment">The object's segment identity.</typeparam>
/// <typeparam name="TFrame">What one of its frames draws.</typeparam>
/// <typeparam name="TEvent">What the object does as its animation passes an event.</typeparam>
internal abstract class SequencedRoomObject<TSegment, TFrame, TEvent> : RoomObjectBehaviour
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>The segments the object's animations play.</summary>
    private readonly AnimationSegments<TSegment, TFrame, TEvent> _segments;

    /// <summary>
    /// What the object does on its next update (#2558): decides what comes next, plays its animation, or rests on the frame
    /// it last showed.
    /// </summary>
    private SequenceProgress _progress = SequenceProgress.Decides;

    /// <summary>Places the object, which decides what it plays on its first update.</summary>
    /// <param name="segments">The segments the object's animations play.</param>
    private protected SequencedRoomObject(AnimationSegments<TSegment, TFrame, TEvent> segments) =>
        _segments = Ensure.NotNull(segments);

    /// <summary>What a sequenced object does on its next update.</summary>
    private abstract class SequenceProgress
    {
        /// <summary>The object decides what comes next: just placed, or told to end its animation.</summary>
        internal static SequenceProgress Decides { get; } = new Deciding();

        /// <summary>The object's animation has ended with nothing to follow: it rests until what it waits for wakes it.</summary>
        internal static SequenceProgress Rests { get; } = new Resting();

        /// <summary>The object decides what comes next.</summary>
        private sealed class Deciding : SequenceProgress;

        /// <summary>The object rests.</summary>
        private sealed class Resting : SequenceProgress;
    }

    /// <summary>The object plays its animation on <paramref name="player"/>.</summary>
    /// <param name="player">The animation playing.</param>
    private sealed class Playing(AnimationPlayer<TSegment, TFrame, TEvent> player) : SequenceProgress
    {
        /// <summary>The animation playing.</summary>
        internal AnimationPlayer<TSegment, TFrame, TEvent> Player { get; } = player;
    }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (_progress is Playing playing)
            Step(playing.Player, place, context);
        else if (_progress == SequenceProgress.Decides)
            Decide(place, context);
    }

    /// <summary>Decides what follows the animation that just ended, on the update its last frame's updates ran out.</summary>
    /// <param name="place">The object's place.</param>
    /// <param name="context">The update.</param>
    private protected abstract void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context);

    /// <summary>Draws <paramref name="frame"/> at the object's place.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="place">The object's place.</param>
    /// <param name="context">The update.</param>
    private protected abstract void Show(TFrame frame, RoomObjectPlace place, RoomObjectStepContext context);

    /// <summary>Carries out <paramref name="raised"/>, which the animation passes just before the frame after it shows.</summary>
    /// <param name="raised">The event.</param>
    /// <param name="place">The object's place.</param>
    /// <param name="context">The update.</param>
    /// <exception cref="InvalidOperationException">The object's animations raise no event.</exception>
    private protected virtual void Raise(TEvent raised, RoomObjectPlace place, RoomObjectStepContext context) =>
        throw new InvalidOperationException($"{GetType().Name} at block {place.Block} raised {raised}, but its animations raise no event.");

    /// <summary>Plays <paramref name="animation"/> from its start, its first frame showing on this update.</summary>
    /// <exception cref="InvalidOperationException">The animation shows no frame on its first update.</exception>
    private protected void Play(Animation<TSegment> animation, RoomObjectPlace place, RoomObjectStepContext context)
    {
        AnimationPlayer<TSegment, TFrame, TEvent> player = Start(animation);
        if (!ShowNextFrame(player, place, context))
            throw new InvalidOperationException("A room object's animation shows a frame as it starts.");
    }

    /// <summary>Plays <paramref name="animation"/> from its start, its first frame showing on the object's next update.</summary>
    private protected void PlayFromNextUpdate(Animation<TSegment> animation) => _ = Start(animation);

    /// <summary>Ends the object's animation on its next update without showing anything more: it then decides what comes next.</summary>
    private protected void EndOnNextUpdate() => _progress = SequenceProgress.Decides;

    /// <summary>Starts <paramref name="animation"/> playing from its start, showing nothing until its first update.</summary>
    private AnimationPlayer<TSegment, TFrame, TEvent> Start(Animation<TSegment> animation)
    {
        AnimationPlayer<TSegment, TFrame, TEvent> player = AnimationPlayer<TSegment, TFrame, TEvent>.Starting(_segments, animation);
        _progress = new Playing(player);
        return player;
    }

    /// <summary>Decides what follows, resting unless the decision starts another animation.</summary>
    private void Decide(RoomObjectPlace place, RoomObjectStepContext context)
    {
        _progress = SequenceProgress.Rests;
        SequenceEnded(place, context);
    }

    /// <summary>Runs one update of the animation: its next frame once the shown one's updates run out, or its end.</summary>
    private void Step(AnimationPlayer<TSegment, TFrame, TEvent> player, RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (!ShowNextFrame(player, place, context) && player.HasStopped)
            Decide(place, context);
    }

    /// <summary>Counts the shown frame down and shows the next frame its updates run out on, if any.</summary>
    /// <returns>Whether the update showed a frame.</returns>
    private bool ShowNextFrame(AnimationPlayer<TSegment, TFrame, TEvent> player, RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (player.StepToNewFrame(raised => Raise(raised, place, context)) is not { } frame)
            return false;
        Show(frame, place, context);
        return true;
    }
}
