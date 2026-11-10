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

    /// <summary>Each segment's beats' places among its frames: a frame beat's frame number, or the next frame's for an event.</summary>
    private readonly Dictionary<TSegment, int[]> frameNumbers;

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
        frameNumbers = beats.ToDictionary(pair => pair.Key, pair =>
        {
            int frames = 0;
            return pair.Value.Select(beat => beat is AnimationBeat<TFrame, TEvent>.Frame ? frames++ : frames).ToArray();
        });
    }

    /// <summary>The beats of <paramref name="segment"/>.</summary>
    internal IReadOnlyList<AnimationBeat<TFrame, TEvent>> Beats(TSegment segment) => beats[segment];

    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame, raising each event it passes
    /// in <paramref name="raise"/>: a phase plays its run as many times as it says, the next phase follows, and past the last
    /// the animation stops or loops as it ends. On return <paramref name="place"/> is where the next play resumes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal AnimationShown<TSegment, TFrame> PlayToNextFrame(Animation<TSegment> animation, ref AnimationPlace place, Action<TEvent> raise)
    {
        Ensure.NotNull(animation);
        Ensure.NotNull(raise);
        if (animation.Phases.Count == 0)
            return AnimationShown<TSegment, TFrame>.Ended.Instance;
        int limit = animation.Phases.Sum(phase => phase.BeatCount + 1) + 1;
        for (int visited = 0; visited < limit; visited++)
        {
            AnimationPhase<TSegment> phase = animation.Phases[place.Phase];
            if (place.Beat < phase.BeatCount)
            {
                int beat = phase.FirstBeat + place.Beat;
                place = place with { Beat = place.Beat + 1 };
                switch (beats[phase.Segment][beat])
                {
                    case AnimationBeat<TFrame, TEvent>.Frame frame:
                        return new AnimationShown<TSegment, TFrame>.Frame(phase.Segment, frameNumbers[phase.Segment][beat], frame.Shown, frame.Duration);
                    case AnimationBeat<TFrame, TEvent>.Event raised:
                        raise(raised.Raised);
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined beat in segment {phase.Segment}.");
                }
            }
            if (place.PlaysLeft > 1)
            {
                place = new AnimationPlace(place.Phase, 0, place.PlaysLeft - 1);
                continue;
            }
            if (place.Phase + 1 < animation.Phases.Count)
            {
                place = AnimationPlace.StartOf(place.Phase + 1, animation.Phases[place.Phase + 1].Plays);
                continue;
            }
            switch (animation.End)
            {
                case AnimationEnd.Stop:
                    return AnimationShown<TSegment, TFrame>.Ended.Instance;
                case AnimationEnd.Loop loop:
                    place = AnimationPlace.StartOf(loop.Phase, animation.Phases[loop.Phase].Plays);
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined animation end {animation.End}.");
            }
        }
        throw new InvalidOperationException("The animation passed every beat without showing a frame.");
    }

    /// <summary>
    /// The place of <paramref name="animation"/> whose next beat is beat <paramref name="beat"/> of <paramref name="segment"/>,
    /// in the first phase playing it, on that phase's first play.
    /// </summary>
    /// <exception cref="ArgumentException">No phase of the animation plays that beat.</exception>
    internal static AnimationPlace PlaceOf(Animation<TSegment> animation, TSegment segment, int beat)
    {
        Ensure.NotNull(animation);
        for (int index = 0; index < animation.Phases.Count; index++)
        {
            AnimationPhase<TSegment> phase = animation.Phases[index];
            if (EqualityComparer<TSegment>.Default.Equals(phase.Segment, segment) && beat >= phase.FirstBeat && beat < phase.FirstBeat + phase.BeatCount)
                return new AnimationPlace(index, beat - phase.FirstBeat, phase.Plays);
        }
        throw new ArgumentException($"No phase of the animation plays beat {beat} of segment {segment}.", nameof(beat));
    }
}
