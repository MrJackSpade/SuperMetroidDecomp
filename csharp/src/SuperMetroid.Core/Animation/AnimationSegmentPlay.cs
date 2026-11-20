namespace SuperMetroid.Core.Animation;

/// <summary>
/// The segments a family of animations plays (#1600): each segment's beats, its frames shown for their durations and the
/// events its owner reacts to. An animation's phases name runs of these beats; playing one moves its place from frame
/// to frame and raises the events it passes. A family whose animations may pause plays
/// <see cref="AnimationSegments{TSegment, TFrame, TEvent}"/>, and one whose animations never pause plays
/// <see cref="UnpausedAnimationSegments{TSegment, TFrame, TEvent}"/>, whose outcomes hold no pause (#2553); a family whose
/// animations hide plays <see cref="HidingAnimationSegments{TSegment, TFrame, TEvent}"/> or
/// <see cref="HidingUnpausedAnimationSegments{TSegment, TFrame, TEvent}"/>, the only segments that hold a hidden beat (#2558).
/// </summary>
/// <typeparam name="TSegment">
/// The segment identity: an enum, whose every member must be a segment, or a composite value naming the segments it requires.
/// </typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TBeat">The beats the family's segments hold.</typeparam>
/// <typeparam name="TShown">What a play of the family's animations comes to.</typeparam>
internal abstract class AnimationSegmentPlay<TSegment, TFrame, TEvent, TBeat, TShown>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
    where TBeat : class, IAnimationBeat<TFrame, TEvent>
    where TShown : class
{
    /// <summary>Each segment's beats.</summary>
    private readonly Dictionary<TSegment, TBeat[]> beats;

    /// <summary>
    /// Keeps the segments; every one of <paramref name="required"/> must be present, and a held frame must end its
    /// segment, since nothing plays after it.
    /// </summary>
    /// <exception cref="ArgumentException">A required segment is missing, or a segment plays on past a held frame.</exception>
    private protected AnimationSegmentPlay(IReadOnlyDictionary<TSegment, TBeat[]> segments, IEnumerable<TSegment> required)
    {
        Ensure.NotNull(segments);
        foreach (TSegment segment in Ensure.NotNull(required))
        {
            if (!segments.ContainsKey(segment))
                throw new ArgumentException($"Animation segment {segment} has no beats.", nameof(segments));
        }
        foreach (KeyValuePair<TSegment, TBeat[]> segment in segments)
        {
            if (segment.Value.SkipLast(1).Any(beat => beat is AnimationBeat<TFrame, TEvent>.HeldFrame))
                throw new ArgumentException($"Animation segment {segment.Key} plays on past its held frame.", nameof(segments));
        }
        beats = segments.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /// <summary>Every member of the enum segment identity <typeparamref name="TSegment"/>, which the segments must all hold.</summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TSegment"/> is not an enum, so it names no members to require.
    /// </exception>
    private protected static IEnumerable<TSegment> EveryMember(string parameter) => typeof(TSegment).IsEnum
        ? Enum.GetValues(typeof(TSegment)).Cast<TSegment>().ToHashSet()
        : throw new ArgumentException($"{typeof(TSegment).Name} is not an enum; name the segments it requires.", parameter);

    /// <summary>The beats of <paramref name="segment"/>.</summary>
    internal IReadOnlyList<TBeat> Beats(TSegment segment) => beats[segment];

    /// <summary>The phase that plays every beat of <paramref name="segment"/> once (#2358), its length read from the segment.</summary>
    internal AnimationPhase<TSegment> Whole(TSegment segment) => Repeated(segment, 1);

    /// <summary>
    /// The phase that plays every beat of <paramref name="segment"/> <paramref name="plays"/> times (#2358), its length read
    /// from the segment.
    /// </summary>
    internal AnimationPhase<TSegment> Repeated(TSegment segment, int plays) => new(segment, 0, beats[segment].Length, plays);

    /// <summary>
    /// The beats one play of <paramref name="animation"/> passes from its start to the end of its last phase, each phase's
    /// run laid out as many times as it plays (#2458); a loop back is not followed.
    /// </summary>
    internal IReadOnlyList<TBeat> PlayedBeats(Animation<TSegment> animation) =>
        [.. Ensure.NotNull(animation).Phases
            .SelectMany(phase => Enumerable.Repeat(phase, phase.Plays))
            .SelectMany(phase => beats[phase.Segment].Skip(phase.FirstBeat).Take(phase.BeatCount))];

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

    /// <summary>Every frame a segment shows, each once: a set, since no order of the segments is stated (#2567).</summary>
    internal IReadOnlySet<TFrame> Frames =>
        beats.Values.SelectMany(segment => segment).Select(beat => beat.ShownFrame).OfType<TFrame>().ToHashSet();

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
    internal TShown PlayToNextFrame(Animation<TSegment> animation, ref AnimationPlace place, Action<TEvent> raise)
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
    internal TShown PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationContinuation<TSegment>?> react)
    {
        Ensure.NotNull(react);
        return PlayToNextFrame(ref animation, ref place, (raised, _) => react(raised));
    }

    /// <summary>
    /// Plays the animation as the other overloads do, also telling <paramref name="react"/> where the play stands just
    /// past each event it raises: the place an owner keeps to come back to later, as an animation that plays another comes
    /// back to where it left off.
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
    internal TShown PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
    {
        Ensure.NotNull(animation);
        Ensure.NotNull(react);
        int limit = beats.Values.Sum(segment => segment.Length + 1) + 1;
        for (int visited = 0; visited < limit; visited++)
        {
            // An animation of no phase is over at once, or rests where it is when it holds.
            if (animation.Phases.Count == 0)
            {
                return Shown(animation.End is AnimationEnd.Hold
                    ? AnimationShown<TSegment, TFrame>.Held.Instance
                    : AnimationShown<TSegment, TFrame>.Ended.Instance);
            }
            if (place.HasNotEntered)
                place = Enter(animation, place.Phase);
            AnimationPhase<TSegment> phase = animation.Phases[place.Phase];
            // A held animation stays held: its last play is done and counts nothing again.
            if (place.IsHolding)
                return Shown(AnimationShown<TSegment, TFrame>.Held.Instance);
            if (place.Beat < phase.BeatCount)
            {
                int beat = phase.FirstBeat + place.Beat;
                place = new AnimationPlace(place.Phase, place.Beat + 1).WithPlaysLeft(place.PlaysLeft);
                var passing = new Play(animation, place, phase, react);
                TShown? shown = Pass(beats[phase.Segment][beat], ref passing);
                animation = passing.Animation;
                place = passing.Place;
                // A beat that raises an event or marks a moment shows nothing: the play goes on where the owner's reaction
                // leaves it.
                if (shown is not null)
                    return shown;
                continue;
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
            var ending = new Play(animation, place, phase, react);
            AnimationShown<TSegment, TFrame>.Unpaused? ended = animation.End.Match(EndPlays, ref ending);
            place = ending.Place;
            // A loop plays on from its cycle's first phase.
            if (ended is not null)
                return Shown(ended);
        }
        throw new InvalidOperationException("The animation passed every beat without showing a frame.");
    }

    /// <summary>What a play shows as it passes <paramref name="beat"/>: an outcome, or nothing as it plays on past it.</summary>
    private protected abstract TShown? Pass(TBeat beat, ref Play play);

    /// <summary><paramref name="outcome"/>, an outcome every family's play can come to, as this family's.</summary>
    private protected abstract TShown Shown(AnimationShown<TSegment, TFrame>.Unpaused outcome);

    /// <summary>
    /// What a play shows on each beat no pause is (#2553), built once: a frame, a frame held for good, or nothing as it raises
    /// an event or passes a marker.
    /// </summary>
    private protected static UnpausedAnimationBeatCases<TFrame, TEvent, Play, AnimationShown<TSegment, TFrame>.Unpaused?> UnpausedBeatPlays { get; } = new(
        Frame: static (frame, ref play) => new AnimationShown<TSegment, TFrame>.Frame(play.Phase.Segment, frame.Shown, frame.Duration),
        HeldFrame: static (held, ref play) =>
        {
            // The animation rests on a held frame for good: its play is done where it stands.
            play.Place = AnimationPlace.HoldingIn(play.Place.Phase, play.Phase.BeatCount);
            return new AnimationShown<TSegment, TFrame>.HeldFrame(held.Shown);
        },
        Event: static (raised, ref play) =>
        {
            play.Raise(raised.Raised);
            return null;
        },
        Marker: static (marked, ref play) =>
        {
            // A marker is raised as an event is (#2301): it marks a moment for its owner and plays nothing.
            play.Raise(marked.Raised);
            return null;
        });

    /// <summary>What a play shows on each beat of a family that may pause (#2553), built once: a pause pauses the animation for its updates.</summary>
    private protected static AnimationBeatCases<TFrame, TEvent, Play, AnimationShown<TSegment, TFrame>?> PausingBeatPlays { get; } = new(
        Unpaused: static (beat, ref play) => beat.Match(UnpausedBeatPlays, ref play),
        Pause: static (pause, ref play) => new AnimationShown<TSegment, TFrame>.Paused(pause.Duration));

    /// <summary>What a play comes to at each end of its animation (#2553), built once.</summary>
    private static AnimationEndCases<Play, AnimationShown<TSegment, TFrame>.Unpaused?> EndPlays { get; } = new(
        Stop: static (_, ref play) => AnimationShown<TSegment, TFrame>.Ended.Instance,
        Loop: static (loop, ref play) =>
        {
            play.Place = Enter(play.Animation, loop.Phase);
            return null;
        },
        Hold: static (_, ref play) =>
        {
            play.Place = AnimationPlace.HoldingIn(play.Place.Phase, play.Phase.BeatCount);
            return AnimationShown<TSegment, TFrame>.Held.Instance;
        });

    /// <summary>
    /// A play of the segments as it passes a beat or its animation's end (#2553): the animation playing and where it
    /// stands, the phase it is in and the owner's reaction to the events it raises.
    /// </summary>
    /// <param name="animation">The animation playing.</param>
    /// <param name="place">Where the play stands.</param>
    /// <param name="phase">The phase it is in.</param>
    /// <param name="react">The owner's reaction to an event, and where the play goes on, if anywhere.</param>
    private protected struct Play(
        Animation<TSegment> animation, AnimationPlace place, AnimationPhase<TSegment> phase,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
    {
        /// <summary>The animation playing, which the owner's reaction may replace.</summary>
        internal Animation<TSegment> Animation = animation;

        /// <summary>Where the play stands.</summary>
        internal AnimationPlace Place = place;

        /// <summary>The phase the play is in.</summary>
        internal readonly AnimationPhase<TSegment> Phase => phase;

        /// <summary>
        /// Raises <paramref name="raised"/> to the owner where the play stands, which goes on where its reaction says, if
        /// it says.
        /// </summary>
        internal void Raise(TEvent raised) => React(raised, ref Animation, ref Place, react);
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
        TBeat[] shown = beats[segment];
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
