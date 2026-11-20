using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that plays authored animations of its frames (#1554), on the shared animation player (#2480): the door
/// caps, the eye doors, the downward gate, the elevator platform, the n00b tube and the Brinstar plants. On each update an
/// object that waits for something (a shot, an opened door) first reacts to it in its own update, then this one runs:
/// unless the object rests, its animation counts down the updates its shown frame stays and, when they run out, shows its
/// next frame, raising the events it passes on the way, or, after the last frame, ends and the object decides what comes
/// next. An animation the object starts while it updates shows its first frame on that same update, as the game's
/// objects continued into their next sequence on the same update. An object just placed decides on its first update,
/// ending its empty first animation there. An object whose ended animation is followed by nothing rests on the frame it
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
    /// <summary>The animation that plays nothing: it is over on its first update.</summary>
    private static readonly Animation<TSegment> PlaysNothing = new([], AnimationEnd.Stops);

    /// <summary>The object's animation playing.</summary>
    private readonly AnimationPlayer<TSegment, TFrame, TEvent> _player;

    /// <summary>Starts the object playing nothing of <paramref name="segments"/>, so its first update ends it at once.</summary>
    /// <param name="segments">The segments the object's animations play.</param>
    private protected SequencedRoomObject(AnimationSegments<TSegment, TFrame, TEvent> segments) =>
        _player = AnimationPlayer<TSegment, TFrame, TEvent>.Starting(segments, PlaysNothing);

    /// <summary>Whether the object rests: its animation has ended with nothing to follow, until what it waits for wakes it.</summary>
    private protected bool Resting => _player.HasStopped;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (Resting)
            return;
        Step(place, context);
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
        _player.Start(animation);
        if (!ShowNextFrame(place, context))
            throw new InvalidOperationException("A room object's animation shows a frame as it starts.");
    }

    /// <summary>Plays <paramref name="animation"/> from its start, its first frame showing on the object's next update.</summary>
    private protected void PlayFromNextUpdate(Animation<TSegment> animation) => _player.Start(animation);

    /// <summary>Ends the object's animation on its next update without showing anything more.</summary>
    private protected void EndOnNextUpdate() => _player.Start(PlaysNothing);

    /// <summary>Runs one update of the animation: its next frame once the shown one's updates run out, or its end.</summary>
    private void Step(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (!ShowNextFrame(place, context) && _player.HasStopped)
            SequenceEnded(place, context);
    }

    /// <summary>Counts the shown frame down and shows the next frame its updates run out on, if any.</summary>
    /// <returns>Whether the update showed a frame.</returns>
    private bool ShowNextFrame(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (_player.StepToNewFrame(raised => Raise(raised, place, context)) is not { } frame)
            return false;
        Show(frame, place, context);
        return true;
    }
}
