namespace SuperMetroid.Core.Programs;

/// <summary>
/// The segments a family of animations plays (#1600): each segment's beats, its frames shown for their durations and the
/// events its owner reacts to. An animation's phases name runs of these beats; playing one moves its place from frame
/// to frame and raises the events it passes.
/// </summary>
/// <typeparam name="TSegment">
/// The segment identity: an enum, whose every member must be a segment, or a composite value naming the segments it requires.
/// </typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed class AnimationSegments<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Each segment's beats.</summary>
    private readonly Dictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> beats;

    /// <summary>Creates the segments of an enum identity; every member of <typeparamref name="TSegment"/> must be present.</summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TSegment"/> is not an enum, so it names no members to require, or a segment is missing.
    /// </exception>
    internal AnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> segments)
        : this(segments, typeof(TSegment).IsEnum
            ? Enum.GetValues(typeof(TSegment)).Cast<TSegment>()
            : throw new ArgumentException($"{typeof(TSegment).Name} is not an enum; name the segments it requires.", nameof(segments)))
    {
    }

    /// <summary>
    /// Creates the segments; every one of <paramref name="required"/> must be present, and a held frame must end its
    /// segment, since nothing plays after it.
    /// </summary>
    /// <exception cref="ArgumentException">A required segment is missing, or a segment plays on past a held frame.</exception>
    internal AnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> segments, IEnumerable<TSegment> required)
    {
        Ensure.NotNull(segments);
        foreach (TSegment segment in Ensure.NotNull(required))
        {
            if (!segments.ContainsKey(segment))
                throw new ArgumentException($"Animation segment {segment} has no beats.", nameof(segments));
        }
        foreach (KeyValuePair<TSegment, AnimationBeat<TFrame, TEvent>[]> segment in segments)
        {
            if (segment.Value.SkipLast(1).Any(beat => beat is AnimationBeat<TFrame, TEvent>.HeldFrame))
                throw new ArgumentException($"Animation segment {segment.Key} plays on past its held frame.", nameof(segments));
        }
        beats = segments.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /// <summary>The beats of <paramref name="segment"/>.</summary>
    internal IReadOnlyList<AnimationBeat<TFrame, TEvent>> Beats(TSegment segment) => beats[segment];

    /// <summary>Every frame a segment shows, each once, in the order the segments first show them.</summary>
    internal IEnumerable<TFrame> Frames =>
        beats.Values.SelectMany(segment => segment).Select(beat => beat.ShownFrame).OfType<TFrame>().Distinct();

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame, raising each event it passes
    /// in <paramref name="raise"/>: a phase plays its run as many times as it says, counting its plays down
    /// <paramref name="repeats"/>, the next phase follows, and past the last the animation stops, loops or holds as it ends. On
    /// return <paramref name="place"/> is where the next play resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(
        Animation<TSegment> animation, ref AnimationPlace place, ref WrappingCountdown repeats, Action<TEvent> raise)
    {
        Ensure.NotNull(raise);
        Animation<TSegment> playing = Ensure.NotNull(animation);
        return PlayToNextFrame(ref playing, ref place, ref repeats, raised =>
        {
            raise(raised);
            return null;
        });
    }

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> as
    /// <see cref="PlayToNextFrame(Animation{TSegment}, ref AnimationPlace, ref WrappingCountdown, Action{TEvent})"/> does, but the owner's reaction
    /// to an event may continue the play elsewhere: in the animation and at the place <paramref name="react"/> returns, at
    /// once, as the owner's state changes on that event. On return <paramref name="animation"/> and
    /// <paramref name="place"/> are where the next play resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    /// <remarks>
    /// The repeat counter is the owner's, as a loop timer the owner keeps: a counted phase loads it with its
    /// plays as the phase starts, each play's end counts it down and replays the run while it is not zero, and nothing else
    /// changes it, so after the phase it stays at zero for whatever else reads it.
    /// </remarks>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, ref WrappingCountdown repeats, Func<TEvent, AnimationContinuation<TSegment>?> react)
    {
        var held = new HeldRepeats(repeats);
        AnimationShown<TSegment, TFrame> shown = PlayToNextFrame(ref animation, ref place, ref held, react);
        repeats = held.Repeats;
        return shown;
    }

    /// <summary>
    /// Plays the animation as the other overloads do, reading and writing the owner's repeat counter through
    /// <paramref name="counter"/> at the moment a counted phase loads or counts it, so a write an event's reaction makes to
    /// the same counter in between stands as it would on a loop timer the owner shares.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame<TCounter>(
        ref Animation<TSegment> animation, ref AnimationPlace place, ref TCounter counter, Func<TEvent, AnimationContinuation<TSegment>?> react)
        where TCounter : IAnimationRepeatCounter
    {
        Ensure.NotNull(react);
        return PlayToNextFrame(ref animation, ref place, ref counter, (raised, _) => react(raised));
    }

    /// <summary>
    /// Plays the animation as the other overloads do, also telling <paramref name="react"/> where the play stands just
    /// past each event it raises: the place an owner's call saves, to return to later, as a script's call saves where it
    /// resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame<TCounter>(
        ref Animation<TSegment> animation, ref AnimationPlace place, ref TCounter counter,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
        where TCounter : IAnimationRepeatCounter
    {
        Ensure.NotNull(animation);
        Ensure.NotNull(react);
        int limit = beats.Values.Sum(segment => segment.Length + 1) + 1;
        for (int visited = 0; visited < limit; visited++)
        {
            // An animation of no phase is over at once, or rests where it is when it holds.
            if (animation.Phases.Count == 0)
                return animation.End is AnimationEnd.Hold ? AnimationShown<TSegment, TFrame>.Held.Instance : AnimationShown<TSegment, TFrame>.Ended.Instance;
            if (place.Beat == AnimationPlace.NotEntered)
                place = Enter(animation, place.Phase, ref counter);
            AnimationPhase<TSegment> phase = animation.Phases[place.Phase];
            // A held animation stays held: its last play is done and counts nothing again.
            if (place.Beat > phase.BeatCount)
                return AnimationShown<TSegment, TFrame>.Held.Instance;
            if (place.Beat < phase.BeatCount)
            {
                int beat = phase.FirstBeat + place.Beat;
                place = place with { Beat = place.Beat + 1 };
                switch (beats[phase.Segment][beat])
                {
                    case AnimationBeat<TFrame, TEvent>.Frame frame:
                        return new AnimationShown<TSegment, TFrame>.Frame(phase.Segment, frame.Shown, frame.Duration);
                    case AnimationBeat<TFrame, TEvent>.HeldFrame held:
                        // The animation rests on a held frame for good: its play is done where it stands.
                        place = AnimationPlace.HoldingIn(place.Phase, phase.BeatCount);
                        return new AnimationShown<TSegment, TFrame>.HeldFrame(held.Shown);
                    case AnimationBeat<TFrame, TEvent>.Pause pause:
                        return new AnimationShown<TSegment, TFrame>.Paused(pause.Duration);
                    case AnimationBeat<TFrame, TEvent>.Event raised:
                        if (react(raised.Raised, new AnimationPlayback<TSegment>(animation, place)) is { } continuation)
                        {
                            animation = continuation.Animation;
                            place = continuation.Place;
                        }
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined beat in segment {phase.Segment}.");
                }
            }
            if (phase.CountsRepeats)
            {
                counter.Repeats = counter.Repeats.Decrement().Remaining;
                if (!counter.Repeats.IsZero)
                {
                    place = AnimationPlace.StartOf(place.Phase);
                    continue;
                }
            }
            if (place.Phase + 1 < animation.Phases.Count)
            {
                place = Enter(animation, place.Phase + 1, ref counter);
                continue;
            }
            switch (animation.End)
            {
                case AnimationEnd.Stop:
                    return AnimationShown<TSegment, TFrame>.Ended.Instance;
                case AnimationEnd.Hold:
                    place = AnimationPlace.HoldingIn(place.Phase, phase.BeatCount);
                    return AnimationShown<TSegment, TFrame>.Held.Instance;
                case AnimationEnd.Loop loop:
                    place = Enter(animation, loop.Phase, ref counter);
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined animation end {animation.End}.");
            }
        }
        throw new InvalidOperationException("The animation passed every beat without showing a frame.");
    }

    /// <summary>The start of phase <paramref name="phase"/> of <paramref name="animation"/>, loading a counted phase's plays.</summary>
    private static AnimationPlace Enter<TCounter>(Animation<TSegment> animation, int phase, ref TCounter counter)
        where TCounter : IAnimationRepeatCounter
    {
        if (animation.Phases[phase].CountsRepeats)
            counter.Repeats = WrappingCountdown.Start(animation.Phases[phase].Plays);
        return AnimationPlace.StartOf(phase);
    }

    /// <summary>
    /// The place of <paramref name="animation"/> whose next beat is beat <paramref name="beat"/> of <paramref name="segment"/>,
    /// in the first phase playing it.
    /// </summary>
    /// <exception cref="ArgumentException">No phase of the animation plays that beat.</exception>
    internal static AnimationPlace PlaceOf(Animation<TSegment> animation, TSegment segment, int beat)
    {
        Ensure.NotNull(animation);
        for (int index = 0; index < animation.Phases.Count; index++)
        {
            AnimationPhase<TSegment> phase = animation.Phases[index];
            if (EqualityComparer<TSegment>.Default.Equals(phase.Segment, segment) && beat >= phase.FirstBeat && beat < phase.FirstBeat + phase.BeatCount)
                return new AnimationPlace(index, beat - phase.FirstBeat);
        }
        throw new ArgumentException($"No phase of the animation plays beat {beat} of segment {segment}.", nameof(beat));
    }
}
