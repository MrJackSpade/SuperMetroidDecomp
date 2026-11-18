namespace SuperMetroid.Core.Animation;

/// <summary>
/// An owner's animation played one update at a time (#2359), as the menus, the maps, the title and the ending play their
/// pictures: the frame it shows, the updates that frame has left and where its play stands. Each update counts the shown
/// frame's updates down, and once they run out plays on to the next frame, raising the events it passes, and shows that
/// frame for its own duration. Once the animation rests, on a held frame or where it holds or stops, it shows its last frame
/// for good and counts no more.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed class AnimationPlayer<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>The segments the animation plays.</summary>
    private AnimationSegments<TSegment, TFrame, TEvent> segments;

    /// <summary>Starts <paramref name="animation"/> of <paramref name="segments"/>, showing nothing until its first update.</summary>
    private AnimationPlayer(AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation)
    {
        this.segments = Ensure.NotNull(segments);
        Playback = new(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>The animation playing and where its play stands.</summary>
    internal AnimationPlayback<TSegment> Playback { get; private set; }

    /// <summary>The frame shown, or null before the animation shows its first.</summary>
    internal TFrame? Shown { get; private set; }

    /// <summary>The updates the shown frame, or the pause, has left before the play moves on.</summary>
    internal int UpdatesLeft { get; private set; } = 1;

    /// <summary>Whether the animation rests for good: on a held frame, or past its end where it holds or stops.</summary>
    internal bool IsResting { get; private set; }

    /// <summary>Whether the animation has stopped: it played its last phase and ends there, showing nothing more.</summary>
    internal bool HasStopped { get; private set; }

    /// <summary>The frame shown, which an animation that has shown its first always has.</summary>
    /// <exception cref="InvalidOperationException">The animation has shown no frame yet.</exception>
    internal TFrame Showing => Shown ?? throw new InvalidOperationException("The animation has shown no frame yet.");

    /// <summary>
    /// Starts <paramref name="animation"/> showing nothing: its first update plays it to its first frame, as an animation
    /// whose first beat the owner's next update reads.
    /// </summary>
    internal static AnimationPlayer<TSegment, TFrame, TEvent> Starting(AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation) =>
        new(segments, animation);

    /// <summary>
    /// Starts <paramref name="animation"/> showing nothing until its <paramref name="updates"/>th update, which plays it to
    /// its first frame (#2387), as Samus's death flash waits before its first shade.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal static AnimationPlayer<TSegment, TFrame, TEvent> StartingAfter(
        AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation, int updates)
    {
        var player = new AnimationPlayer<TSegment, TFrame, TEvent>(segments, animation);
        player.UpdatesLeft = updates >= 1
            ? updates
            : throw new ArgumentOutOfRangeException(nameof(updates), updates, "An animation shows its first frame on its first update or later.");
        return player;
    }

    /// <summary>
    /// Starts <paramref name="animation"/> on its first frame, shown at once for <paramref name="firstFrameUpdates"/> updates
    /// instead of its own duration: the menus' and maps' pictures show their first frame until their first update moves them
    /// on, and the pause selector's first frame shows for its installed opening hold.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal static AnimationPlayer<TSegment, TFrame, TEvent> StartShowing(
        AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation, int firstFrameUpdates)
    {
        var player = new AnimationPlayer<TSegment, TFrame, TEvent>(segments, animation);
        player.ShowFirstFrame(firstFrameUpdates);
        return player;
    }

    /// <summary>Starts <paramref name="animation"/> again showing nothing, as <see cref="Starting"/> does.</summary>
    internal void Start(Animation<TSegment> animation)
    {
        Playback = new(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        Shown = null;
        UpdatesLeft = 1;
        IsResting = false;
        HasStopped = false;
    }

    /// <summary>Starts <paramref name="animation"/> again on its first frame, as <see cref="StartShowing"/> does.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal void Show(Animation<TSegment> animation, int firstFrameUpdates)
    {
        Start(animation);
        ShowFirstFrame(firstFrameUpdates);
    }

    /// <summary>Runs one update of an animation that raises no event.</summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal void Step() => Step(raised => throw new InvalidOperationException($"The animation raised {raised} with no owner to act on it."));

    /// <summary>
    /// Runs one update: counts the shown frame's updates down and, once they run out, plays on to the next frame, raising the
    /// events it passes in <paramref name="raise"/>, and shows it for its duration. A resting animation does nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal void Step(Action<TEvent> raise)
    {
        Ensure.NotNull(raise);
        if (IsResting || --UpdatesLeft > 0)
            return;
        _ = PlayOn(raise);
    }

    /// <summary>
    /// Runs one update of an animation that raises no event, as <see cref="Step()"/> does, and returns the frame the update
    /// shows anew (#2387): the next frame once the shown one has run its updates, even when it is the same frame again, or
    /// null while the shown frame holds, while the play pauses and once it rests. An owner that copies each frame it
    /// reaches, as Samus's colourings copy their shades, copies what this returns.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal TFrame? StepToNewFrame() =>
        IsResting || --UpdatesLeft > 0
            ? null
            : PlayOn(raised => throw new InvalidOperationException($"The animation raised {raised} with no owner to act on it."));

    /// <summary>
    /// Plays the animation's segments as <paramref name="rebound"/> holds them from now on, keeping where the play stands,
    /// the shown frame's updates left and whether it rests (#2359): an owner rebinding edited installed timing keeps its
    /// animation running. A place past the rebound phase's last beat stands at its end, so the next frame is the one the
    /// animation plays on to from there; the shown frame is the rebound segment's at the place it stands after.
    /// </summary>
    internal void Rebind(AnimationSegments<TSegment, TFrame, TEvent> rebound, Animation<TSegment> animation)
    {
        segments = Ensure.NotNull(rebound);
        Ensure.NotNull(animation);
        AnimationPlace place = Playback.Place;
        if (IsResting || animation.Phases.Count == 0)
        {
            Playback = new(animation, place);
            return;
        }
        int phase = Math.Min(place.Phase, animation.Phases.Count - 1);
        AnimationPhase<TSegment> playing = animation.Phases[phase];
        int beat = Math.Min(place.Beat, playing.BeatCount);
        place = new AnimationPlace(phase, beat).WithPlaysLeft(Math.Min(place.PlaysLeft, playing.IsRepeated ? playing.Plays : 0));
        Playback = new(animation, place);
        if (beat > 0 && rebound.Beats(playing.Segment)[playing.FirstBeat + beat - 1].ShownFrame is { } shown)
            Shown = shown;
    }

    /// <summary>Plays the animation to its first frame and shows it for <paramref name="firstFrameUpdates"/> updates.</summary>
    private void ShowFirstFrame(int firstFrameUpdates)
    {
        if (firstFrameUpdates < 1)
            throw new ArgumentOutOfRangeException(nameof(firstFrameUpdates), firstFrameUpdates, "A first frame shows for at least one update.");
        _ = PlayOn(raised => throw new InvalidOperationException($"The animation raised {raised} before its first frame."));
        if (Shown is null)
            throw new InvalidOperationException("The animation shows no frame to start on.");
        if (!IsResting)
            UpdatesLeft = firstFrameUpdates;
    }

    /// <summary>Plays the animation on to its next frame, pause or rest, and returns the frame it shows anew, if any.</summary>
    private TFrame? PlayOn(Action<TEvent> raise)
    {
        AnimationPlace place = Playback.Place;
        AnimationShown<TSegment, TFrame> next = segments.PlayToNextFrame(Playback.Animation, ref place, raise);
        Playback = Playback with { Place = place };
        switch (next)
        {
            case AnimationShown<TSegment, TFrame>.Frame frame:
                Shown = frame.Shown;
                UpdatesLeft = frame.Duration;
                return frame.Shown;
            case AnimationShown<TSegment, TFrame>.Paused pause:
                UpdatesLeft = pause.Duration;
                return null;
            case AnimationShown<TSegment, TFrame>.HeldFrame held:
                Shown = held.Shown;
                IsResting = true;
                return held.Shown;
            case AnimationShown<TSegment, TFrame>.Held:
                IsResting = true;
                return null;
            case AnimationShown<TSegment, TFrame>.Ended:
                IsResting = true;
                HasStopped = true;
                return null;
            default:
                throw new InvalidOperationException($"The animation played on to {next}.");
        }
    }
}
