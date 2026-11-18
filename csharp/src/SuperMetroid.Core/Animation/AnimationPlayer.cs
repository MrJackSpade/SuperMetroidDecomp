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
        player.StartAfter(animation, updates);
        return player;
    }

    /// <summary>
    /// Starts <paramref name="animation"/> again showing nothing until its <paramref name="updates"/>th update, as
    /// <see cref="StartingAfter"/> does (#2447): Samus's Speed Booster glow starts over on its first boosted update.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal void StartAfter(Animation<TSegment> animation, int updates)
    {
        if (updates < 1)
            throw new ArgumentOutOfRangeException(nameof(updates), updates, "An animation shows its first frame on its first update or later.");
        Start(animation);
        UpdatesLeft = updates;
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

    /// <summary>
    /// Starts <paramref name="animation"/> again from its start as <see cref="Start"/> does, but the frame shown, if any,
    /// stays on show until the animation's first update plays it to its own first frame (#2488): the intro's portrait keeps
    /// its eye open until the final page's closing shows.
    /// </summary>
    internal void StartKeepingShown(Animation<TSegment> animation)
    {
        TFrame? shown = Shown;
        Start(animation);
        Shown = shown;
    }

    /// <summary>Starts <paramref name="animation"/> again on its first frame, as <see cref="StartShowing"/> does.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal void Show(Animation<TSegment> animation, int firstFrameUpdates)
    {
        Start(animation);
        ShowFirstFrame(firstFrameUpdates);
    }

    /// <summary>
    /// Starts <paramref name="animation"/> on its first frame, shown at once for that frame's own updates (#2388), as the
    /// grapple's endpoint and rope start on a fresh beam and Samus's suit explosion on the flash's last update.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal static AnimationPlayer<TSegment, TFrame, TEvent> StartShowing(
        AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation)
    {
        var player = new AnimationPlayer<TSegment, TFrame, TEvent>(segments, animation);
        player.ShowFirstFrameForItsOwnUpdates();
        return player;
    }

    /// <summary>Starts <paramref name="animation"/> again on its first frame, for that frame's own updates (#2388).</summary>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal void Show(Animation<TSegment> animation)
    {
        Start(animation);
        ShowFirstFrameForItsOwnUpdates();
    }

    /// <summary>Plays the animation to its first frame, which shows for its own updates.</summary>
    private void ShowFirstFrameForItsOwnUpdates()
    {
        _ = PlayOn(raised => throw new InvalidOperationException($"The animation raised {raised} before its first frame."));
        if (Shown is null)
            throw new InvalidOperationException("The animation shows no frame to start on.");
    }

    /// <summary>
    /// Starts <paramref name="animation"/> again at <paramref name="place"/>, showing nothing until its next update plays on
    /// from there (#2488): a colour effect restarted partway through its animation, or at its cycle, plays on in the same
    /// turn.
    /// </summary>
    internal void StartAt(Animation<TSegment> animation, AnimationPlace place)
    {
        Start(animation);
        Playback = Playback with { Place = place };
    }

    /// <summary>
    /// Starts <paramref name="animation"/> again at <paramref name="place"/>, showing at once the frame it plays to from
    /// there for <paramref name="firstFrameUpdates"/> updates (#2388): a particle restored partway through its animation
    /// shows the frame it stood on for the updates it had left.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before the frame.</exception>
    internal void ShowAt(Animation<TSegment> animation, AnimationPlace place, int firstFrameUpdates)
    {
        Start(animation);
        Playback = Playback with { Place = place };
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
        StepToNewFrame(raised => throw new InvalidOperationException($"The animation raised {raised} with no owner to act on it."));

    /// <summary>
    /// Runs one update as <see cref="Step(Action{TEvent})"/> does, raising the events it passes in <paramref name="raise"/>,
    /// and returns the frame the update shows anew, as <see cref="StepToNewFrame()"/> does (#2404).
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal TFrame? StepToNewFrame(Action<TEvent> raise)
    {
        Ensure.NotNull(raise);
        return IsResting || --UpdatesLeft > 0 ? null : PlayOn(raise);
    }

    /// <summary>
    /// Runs one update as <see cref="StepToNewFrame(Action{TEvent})"/> does, but the owner's reaction to an event it passes
    /// may continue the play elsewhere (#2487): in the animation and at the place <paramref name="react"/> returns, at once,
    /// as a Tourian statue that finds its area's boss dead goes on into its release on the same update.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal TFrame? StepToNewFrame(Func<TEvent, AnimationContinuation<TSegment>?> react)
    {
        Ensure.NotNull(react);
        return IsResting || --UpdatesLeft > 0 ? null : PlayOnReacting(react);
    }

    /// <summary>
    /// Runs one update as <see cref="StepToNewFrame()"/> does, but once the shown frame's updates run out plays on in
    /// <paramref name="animation"/>, of the same segments, going on from where the play stands as
    /// <see cref="AnimationPlayback{TSegment}.GoingOnIn"/> hands it (#2447): an owner sharing its place with another
    /// animation, as Samus's Speed Booster glow shares hers with her Screw Attack glow, takes the place over only as it
    /// moves on, and counts its own frame's updates meanwhile.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal TFrame? StepToNewFrameGoingOnIn(Animation<TSegment> animation)
    {
        Ensure.NotNull(animation);
        if (IsResting || --UpdatesLeft > 0)
            return null;
        if (!ReferenceEquals(Playback.Animation, animation))
            Playback = Playback.GoingOnIn(animation);
        return PlayOn(raised => throw new InvalidOperationException($"The animation raised {raised} with no owner to act on it."));
    }

    /// <summary>
    /// Runs one update of an animation that raises no event, as <see cref="Step()"/> does, but once the shown frame's updates
    /// run out plays on from <paramref name="place"/> instead of where the play stands (#2488): the Fireflea flash, which the
    /// room's darkness holds, shows its brightest look each time its next one is due.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal void StepPlayingOnFrom(AnimationPlace place)
    {
        if (IsResting || --UpdatesLeft > 0)
            return;
        Playback = Playback with { Place = place };
        _ = PlayOn(raised => throw new InvalidOperationException($"The animation raised {raised} with no owner to act on it."));
    }

    /// <summary>
    /// Goes on from <paramref name="playback"/>, of the same segments, in place of where the play stands, the shown frame
    /// keeping its updates left and whether it rests (#2447, #2458): another animation sharing the place has moved it on,
    /// as Samus's Screw Attack glow moves on the place her Speed Booster glow counts its shades from, or started it again;
    /// or the frames after it come from another animation, as a Power Bomb's fuse moves its live animation onto the same
    /// frame of its fast animation.
    /// </summary>
    internal void GoOnFrom(AnimationPlayback<TSegment> playback)
    {
        Ensure.NotNull(playback.Animation);
        Playback = playback;
    }

    /// <summary>
    /// Goes on playing <paramref name="animation"/>, of the same segments, in place of the animation playing (#2404): from
    /// where the play stands, the shown frame keeping its updates left, so the frames after it take their timing from the
    /// new animation, as the Shitroid's membrane pulse changes its pace.
    /// </summary>
    internal void GoOnIn(Animation<TSegment> animation) => Rebind(segments, animation);

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
    private TFrame? PlayOn(Action<TEvent> raise) => PlayOnReacting(raised =>
    {
        raise(raised);
        return null;
    });

    /// <summary>
    /// Plays the animation on to its next frame, pause or rest, going on where <paramref name="react"/> continues it, and
    /// returns the frame it shows anew, if any.
    /// </summary>
    private TFrame? PlayOnReacting(Func<TEvent, AnimationContinuation<TSegment>?> react)
    {
        Animation<TSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        AnimationShown<TSegment, TFrame> next = segments.PlayToNextFrame(ref animation, ref place, react);
        Playback = new(animation, place);
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

    /// <summary>
    /// Starts <paramref name="animation"/> again at <paramref name="place"/>, showing at once the frame it plays to from
    /// there for that frame's own updates (#2396): the grapple beam's flare restarts at the main flare's loop.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before the frame.</exception>
    internal void ShowFrom(Animation<TSegment> animation, AnimationPlace place)
    {
        Start(animation);
        Playback = Playback with { Place = place };
        ShowFirstFrameForItsOwnUpdates();
    }

    /// <summary>
    /// Starts <paramref name="animation"/> again on its last frame, the last beat of its last phase, shown at once for
    /// <paramref name="updates"/> updates (#2396), to be played backward from there (<see cref="StepBackward"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation has no phase, or its last beat is no frame.</exception>
    internal void ShowLastFrame(Animation<TSegment> animation, int updates)
    {
        Ensure.NotNull(animation);
        if (animation.Phases.Count == 0)
            throw new InvalidOperationException("An animation of no phase has no last frame.");
        int last = animation.Phases.Count - 1;
        AnimationPhase<TSegment> lastPhase = animation.Phases[last];
        ShowAt(animation, new AnimationPlace(last, lastPhase.BeatCount - 1).WithPlaysLeft(lastPhase.IsRepeated ? 1 : 0), updates);
    }

    /// <summary>
    /// Runs one update of the animation played backward (#2396): counts the shown frame's updates down and, once they run
    /// out, steps back to the frame the animation shows before it, shown for <paramref name="frameUpdates"/> updates, or for
    /// <paramref name="firstFrameUpdates"/> when that is the animation's first frame. A backward play goes once through the
    /// phases, last to first, each from its last beat to its first and through each play of a repeated phase from its last
    /// play to its first (#2458), and follows no loop; stepping back past the first frame stops it, and it shows nothing
    /// more. A resting animation does nothing.
    /// </summary>
    /// <returns>Whether the update stepped back onto the animation's first frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either count of updates is below one.</exception>
    /// <exception cref="InvalidOperationException">
    /// The animation stands on no frame, or steps back onto a beat that shows none.
    /// </exception>
    internal bool StepBackward(int frameUpdates, int firstFrameUpdates)
    {
        if (frameUpdates < 1)
            throw new ArgumentOutOfRangeException(nameof(frameUpdates), frameUpdates, "A frame shows for at least one update.");
        if (firstFrameUpdates < 1)
            throw new ArgumentOutOfRangeException(nameof(firstFrameUpdates), firstFrameUpdates, "A frame shows for at least one update.");
        if (IsResting || --UpdatesLeft > 0)
            return false;
        AnimationPlace place = Playback.Place;
        IReadOnlyList<AnimationPhase<TSegment>> phases = Playback.Animation.Phases;
        if (Shown is null || place.Phase >= phases.Count || place.Beat < 1 || place.Beat > phases[place.Phase].BeatCount)
            throw new InvalidOperationException("The animation stands on no frame to step back from.");
        // The place stands just past the beat shown, so the beat before that one is two back; before a repeated phase's
        // first beat lies its play before, if it has played one.
        int phase = place.Phase, beat = place.Beat - 2, playsLeft = place.PlaysLeft;
        while (beat < 0)
        {
            AnimationPhase<TSegment> playing = phases[phase];
            if (playing.IsRepeated && playsLeft < playing.Plays)
            {
                playsLeft++;
                beat = playing.BeatCount - 1;
                continue;
            }
            if (--phase < 0)
            {
                Shown = null;
                IsResting = true;
                HasStopped = true;
                return false;
            }
            AnimationPhase<TSegment> before = phases[phase];
            playsLeft = before.IsRepeated ? 1 : 0;
            beat = before.BeatCount - 1;
        }
        AnimationPhase<TSegment> stepped = phases[phase];
        Shown = segments.Beats(stepped.Segment)[stepped.FirstBeat + beat].ShownFrame
            ?? throw new InvalidOperationException($"The animation steps back onto a beat of {stepped.Segment} that shows no frame.");
        Playback = Playback with { Place = new AnimationPlace(phase, beat + 1).WithPlaysLeft(playsLeft) };
        bool first = beat == 0 && (!stepped.IsRepeated || playsLeft == stepped.Plays) &&
            phases.Take(phase).All(earlier => earlier.BeatCount == 0);
        UpdatesLeft = first ? firstFrameUpdates : frameUpdates;
        return first;
    }

    /// <summary>
    /// Shows the shown frame for <paramref name="updates"/> more updates from now, in place of the updates it had left
    /// (#2396): an owner that hands a frame over to another way of counting gives it the wait that count leaves it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation rests or shows no frame.</exception>
    internal void HoldShownFrameFor(int updates)
    {
        if (updates < 1)
            throw new ArgumentOutOfRangeException(nameof(updates), updates, "A frame shows for at least one update.");
        if (IsResting || Shown is null)
            throw new InvalidOperationException("The animation holds no frame it is counting.");
        UpdatesLeft = updates;
    }
}
