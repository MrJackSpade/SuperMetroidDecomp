namespace SuperMetroid.Core.Programs;

/// <summary>
/// The segments a family of animations plays (#1600): each segment's beats, its frames shown for their durations and the
/// events its owner reacts to. An animation's phases name runs of these beats; playing one moves its place from frame
/// to frame and raises the events it passes.
/// </summary>
/// <typeparam name="TSegment">The segment identity: an enum, whose every member must be a segment.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed class AnimationSegments<TSegment, TFrame, TEvent>
    where TSegment : struct, Enum
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Each segment's beats.</summary>
    private readonly Dictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> beats;

    /// <summary>Creates the segments; every member of <typeparamref name="TSegment"/> must be present.</summary>
    /// <exception cref="ArgumentException">A segment is missing.</exception>
    internal AnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> segments)
    {
        Ensure.NotNull(segments);
        foreach (TSegment segment in Enum.GetValues<TSegment>())
        {
            if (!segments.ContainsKey(segment))
                throw new ArgumentException($"Animation segment {segment} has no beats.", nameof(segments));
        }
        beats = segments.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /// <summary>The beats of <paramref name="segment"/>.</summary>
    internal IReadOnlyList<AnimationBeat<TFrame, TEvent>> Beats(TSegment segment) => beats[segment];

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
    /// The repeat counter is the owner's, as a loop timer the owner keeps: a phase played more than once loads it with its
    /// plays as the phase starts, each play's end counts it down and replays the run while it is not zero, and nothing else
    /// changes it, so after the phase it stays at zero for whatever else reads it.
    /// </remarks>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, ref WrappingCountdown repeats, Func<TEvent, AnimationContinuation<TSegment>?> react)
    {
        Ensure.NotNull(animation);
        Ensure.NotNull(react);
        int limit = beats.Values.Sum(segment => segment.Length + 1) + 1;
        for (int visited = 0; visited < limit; visited++)
        {
            if (animation.Phases.Count == 0)
                return AnimationShown<TSegment, TFrame>.Ended.Instance;
            if (place.Beat == AnimationPlace.NotEntered)
                place = Enter(animation, place.Phase, ref repeats);
            AnimationPhase<TSegment> phase = animation.Phases[place.Phase];
            if (place.Beat < phase.BeatCount)
            {
                int beat = phase.FirstBeat + place.Beat;
                place = place with { Beat = place.Beat + 1 };
                switch (beats[phase.Segment][beat])
                {
                    case AnimationBeat<TFrame, TEvent>.Frame frame:
                        return new AnimationShown<TSegment, TFrame>.Frame(phase.Segment, frame.Shown, frame.Duration);
                    case AnimationBeat<TFrame, TEvent>.Pause pause:
                        return new AnimationShown<TSegment, TFrame>.Paused(pause.Duration);
                    case AnimationBeat<TFrame, TEvent>.Event raised:
                        if (react(raised.Raised) is { } continuation)
                        {
                            animation = continuation.Animation;
                            place = continuation.Place;
                        }
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined beat in segment {phase.Segment}.");
                }
            }
            if (phase.Plays > 1)
            {
                repeats = repeats.Decrement().Remaining;
                if (!repeats.IsZero)
                {
                    place = AnimationPlace.StartOf(place.Phase);
                    continue;
                }
            }
            if (place.Phase + 1 < animation.Phases.Count)
            {
                place = Enter(animation, place.Phase + 1, ref repeats);
                continue;
            }
            switch (animation.End)
            {
                case AnimationEnd.Stop:
                    return AnimationShown<TSegment, TFrame>.Ended.Instance;
                case AnimationEnd.Hold:
                    return AnimationShown<TSegment, TFrame>.Held.Instance;
                case AnimationEnd.Loop loop:
                    place = Enter(animation, loop.Phase, ref repeats);
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined animation end {animation.End}.");
            }
        }
        throw new InvalidOperationException("The animation passed every beat without showing a frame.");
    }

    /// <summary>The start of phase <paramref name="phase"/> of <paramref name="animation"/>, loading a counted phase's plays.</summary>
    private static AnimationPlace Enter(Animation<TSegment> animation, int phase, ref WrappingCountdown repeats)
    {
        int plays = animation.Phases[phase].Plays;
        if (plays > 1)
            repeats = WrappingCountdown.FromRaw(plays);
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
