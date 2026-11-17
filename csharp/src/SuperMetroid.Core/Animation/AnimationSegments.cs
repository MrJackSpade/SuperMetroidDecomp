namespace SuperMetroid.Core.Animation;

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

    /// <summary>
    /// Whether <paramref name="animation"/> opens with a frame (#2317): played from its start, it shows a frame or a held
    /// frame before it raises any event.
    /// </summary>
    internal bool OpensWithFrame(Animation<TSegment> animation)
    {
        Ensure.NotNull(animation);
        if (animation.Phases.Count == 0 || animation.Phases[0].BeatCount == 0)
            return false;
        AnimationPhase<TSegment> opening = animation.Phases[0];
        return beats[opening.Segment][opening.FirstBeat] is AnimationBeat<TFrame, TEvent>.Frame or AnimationBeat<TFrame, TEvent>.HeldFrame;
    }

    /// <summary>Every frame a segment shows, each once, in the order the segments first show them.</summary>
    internal IEnumerable<TFrame> Frames =>
        beats.Values.SelectMany(segment => segment).Select(beat => beat.ShownFrame).OfType<TFrame>().Distinct();

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame, raising each event it passes
    /// in <paramref name="raise"/>: a phase plays its run as many times as it says, counting its plays in the place, the
    /// next phase follows, and past the last the animation stops, loops or holds as it ends. On return
    /// <paramref name="place"/> is where the next play resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation passes every beat without showing a frame, or ends a play of a repeated phase it stands in with no
    /// plays left.
    /// </exception>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(Animation<TSegment> animation, ref AnimationPlace place, Action<TEvent> raise)
    {
        Ensure.NotNull(raise);
        Animation<TSegment> playing = Ensure.NotNull(animation);
        return PlayToNextFrame(ref playing, ref place, raised =>
        {
            raise(raised);
            return null;
        });
    }

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> as
    /// <see cref="PlayToNextFrame(Animation{TSegment}, ref AnimationPlace, Action{TEvent})"/> does, but the owner's reaction to
    /// an event may continue the play elsewhere: in the animation and at the place <paramref name="react"/> returns, at
    /// once, as the owner's state changes on that event. On return <paramref name="animation"/> and
    /// <paramref name="place"/> are where the next play resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation passes every beat without showing a frame, or ends a play of a repeated phase it stands in with no
    /// plays left.
    /// </exception>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationContinuation<TSegment>?> react)
    {
        Ensure.NotNull(react);
        return PlayToNextFrame(ref animation, ref place, (raised, _) => react(raised));
    }

    /// <summary>
    /// Plays the animation as the other overloads do, also telling <paramref name="react"/> where the play stands just
    /// past each event it raises: the place an owner's call saves, to return to later, as an animation's call saves where it
    /// resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The animation passes every beat without showing a frame, or ends a play of a repeated phase it stands in with no
    /// plays left.
    /// </exception>
    /// <remarks>
    /// The plays of a repeated phase are the playback's own (#2006): entering the phase sets the place's plays left to the
    /// phase's plays, the end of each play counts one off and replays the run while any are left, and a continuation goes on
    /// with its own place's, or, when it counts on, with the plays left where the event raised it.
    /// </remarks>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
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
                place = Enter(animation, place.Phase);
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
                        React(raised.Raised, ref animation, ref place, react);
                        continue;
                    case AnimationBeat<TFrame, TEvent>.Marker marked:
                        // A marker is raised as an event is (#2301): it marks a moment for its owner and plays nothing.
                        React(marked.Raised, ref animation, ref place, react);
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined beat in segment {phase.Segment}.");
                }
            }
            if (phase.IsRepeated)
            {
                if (place.PlaysLeft == 0)
                    throw new InvalidOperationException($"A play of repeated phase {place.Phase} ({phase.Segment}) ended with none of its {phase.Plays} plays left.");
                if (place.PlaysLeft > 1)
                {
                    place = AnimationPlace.StartOf(place.Phase).WithPlaysLeft(place.PlaysLeft - 1);
                    continue;
                }
            }
            if (place.Phase + 1 < animation.Phases.Count)
            {
                place = Enter(animation, place.Phase + 1);
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
                    place = Enter(animation, loop.Phase);
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined animation end {animation.End}.");
            }
        }
        throw new InvalidOperationException("The animation passed every beat without showing a frame.");
    }

    /// <summary>
    /// Raises <paramref name="raised"/> to <paramref name="react"/> where the play stands just past it, and continues the play
    /// where the owner's reaction says, if it says.
    /// </summary>
    private static void React(
        TEvent raised, ref Animation<TSegment> animation, ref AnimationPlace place,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
    {
        if (react(raised, new AnimationPlayback<TSegment>(animation, place)) is not { } continuation)
            return;
        animation = continuation.Animation;
        place = continuation.CountsOn ? continuation.Place.WithPlaysLeft(place.PlaysLeft) : continuation.Place;
    }

    /// <summary>The start of phase <paramref name="phase"/> of <paramref name="animation"/>, with every play of a repeated phase left.</summary>
    private static AnimationPlace Enter(Animation<TSegment> animation, int phase) =>
        AnimationPlace.StartOf(phase).WithPlaysLeft(animation.Phases[phase].IsRepeated ? animation.Phases[phase].Plays : 0);

    /// <summary>
    /// The place of <paramref name="animation"/> whose next beat shows <paramref name="frame"/> in <paramref name="segment"/>,
    /// in the first phase playing that beat.
    /// </summary>
    /// <exception cref="ArgumentException">The segment shows no such frame, or no phase of the animation plays it.</exception>
    internal AnimationPlace PlaceOfFrame(Animation<TSegment> animation, TSegment segment, TFrame frame)
    {
        AnimationBeat<TFrame, TEvent>[] shown = beats[segment];
        for (int beat = 0; beat < shown.Length; beat++)
        {
            if (shown[beat].ShownFrame is { } candidate && EqualityComparer<TFrame>.Default.Equals(candidate, frame))
                return PlaceOf(animation, segment, beat);
        }
        throw new ArgumentException($"Segment {segment} shows no frame {frame}.", nameof(frame));
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
