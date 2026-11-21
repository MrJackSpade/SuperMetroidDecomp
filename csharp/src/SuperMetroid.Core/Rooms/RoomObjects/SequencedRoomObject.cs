using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that plays authored animations of its frames (#1554), on the shared animation player (#2480): the door
/// caps, the eye doors, the downward gate, the elevator platform, the n00b tube and the Brinstar plants. On each update an
/// object that waits for something (a shot, an opened door) first reacts to it in its own update, then this one runs:
/// unless the object rests, its animation counts down the updates its shown frame stays and, when they run out, shows its
/// next frame, raising the events it passes on the way, or, after the last frame, ends and the object decides what comes
/// next. An animation the object starts while it updates shows its first frame on that same update, as the game's
/// objects continued into their next sequence on the same update. An object just placed decides on its first update. An
/// object whose ended animation is followed by nothing rests on the frame it last showed until what it waits for starts
/// another; where its animations stand is its progress (<see cref="SequencedRoomObjectProgress{TSegment, TFrame, TEvent}"/>).
/// </summary>
/// <typeparam name="TSegment">The object's segment identity.</typeparam>
/// <typeparam name="TFrame">What one of its frames draws.</typeparam>
/// <typeparam name="TEvent">What the object does as its animation passes an event.</typeparam>
internal abstract partial class SequencedRoomObject<TSegment, TFrame, TEvent> : RoomObjectBehaviour
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>The segments the object's animations play.</summary>
    private readonly AnimationSegments<TSegment, TFrame, TEvent> _segments;

    /// <summary>Where the object's animations stand: placed, it decides on its first update.</summary>
    internal SequencedRoomObjectProgress<TSegment, TFrame, TEvent> Progress { get; private set; } = SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Deciding.OnNextUpdate;

    /// <summary>Places the object playing animations of <paramref name="segments"/>, deciding what it plays on its first update.</summary>
    /// <param name="segments">The segments the object's animations play.</param>
    private protected SequencedRoomObject(AnimationSegments<TSegment, TFrame, TEvent> segments) =>
        _segments = Ensure.NotNull(segments);

    /// <summary>Whether the object rests: its animation has ended with nothing to follow, until what it waits for wakes it.</summary>
    private protected bool Resting => _progress is SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Resting
        or SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.RestingUnshown;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        switch (Progress)
        {
            case SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Resting or SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.RestingUnshown:
                return;
            case SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Deciding:
                Decide(null, place, context);
                return;
            case SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Playing playing:
                if (!ShowNextFrame(playing.Player, place, context) && playing.Player.HasStopped)
                    Decide(playing.Player.Shown, place, context);
                return;
            default:
                throw new InvalidOperationException($"Undefined room object progress {Progress}.");
        }
    }

    /// <summary>
    /// Decides what follows the object's ended animation (<see cref="SequenceEnded"/>): the object rests on
    /// <paramref name="lastShown"/>, or showing nothing, unless the decision starts another animation or another decision.
    /// </summary>
    /// <param name="lastShown">The frame the ended animation last showed; none when it showed none, or nothing played.</param>
    /// <param name="place">The object's place.</param>
    /// <param name="context">The update.</param>
    private void Decide(TFrame? lastShown, RoomObjectPlace place, RoomObjectStepContext context)
    {
        Progress = lastShown is { } shown
            ? new SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Resting(shown)
            : SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.RestingUnshown.Instance;
        SequenceEnded(place, context);
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

    /// <summary>
    /// What carries out an event the animation passes just before the frame after it shows, given the object's place and
    /// the update; none for an object whose animations raise no event (<see cref="SilentSequencedRoomObject{TSegment, TFrame}"/>),
    /// which plays them silently.
    /// </summary>
    private protected virtual Action<TEvent, RoomObjectPlace, RoomObjectStepContext>? Raiser => null;

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

    /// <summary>Ends the object's animation, its next update deciding what follows without showing anything more.</summary>
    private protected void EndOnNextUpdate() => Progress = SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Deciding.OnNextUpdate;

    /// <summary>Starts <paramref name="animation"/> playing from its start, its first frame due on the object's next step.</summary>
    private AnimationPlayer<TSegment, TFrame, TEvent> Start(Animation<TSegment> animation)
    {
        var playing = new SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Playing(AnimationPlayer<TSegment, TFrame, TEvent>.Starting(_segments, animation));
        Progress = playing;
        return playing.Player;
    }

    /// <summary>Counts <paramref name="player"/>'s shown frame down and shows the next frame its updates run out on, if any.</summary>
    /// <returns>Whether the update showed a frame.</returns>
    private bool ShowNextFrame(AnimationPlayer<TSegment, TFrame, TEvent> player, RoomObjectPlace place, RoomObjectStepContext context)
    {
        TFrame? shown = Raiser is { } raise
            ? player.StepToNewFrame(raised => raise(raised, place, context))
            : player.StepToNewFrame();
        if (shown is not { } frame)
            return false;
        Show(frame, place, context);
        return true;
    }
}
