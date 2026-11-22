namespace SuperMetroid.Core.Animation;

/// <summary>
/// The segments of a family of animations that may pause (#1600, #2553): a play of one comes to a frame, a held frame, a
/// pause, a rest or the animation's end.
/// </summary>
/// <typeparam name="TSegment">
/// The segment identity: an enum, whose every member must be a segment, or a composite value naming the segments it requires.
/// </typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed partial class AnimationSegments<TSegment, TFrame, TEvent>
    : AnimationSegmentPlay<TSegment, TFrame, TEvent, AnimationBeat<TFrame, TEvent>, AnimationShown<TSegment, TFrame>>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Creates the segments of an enum identity; every member of <typeparamref name="TSegment"/> must be present.</summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TSegment"/> is not an enum, so it names no members to require, or a segment is missing.
    /// </exception>
    internal AnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> segments)
        : this(segments, EveryMember(nameof(segments)))
    {
    }

    /// <summary>
    /// Creates the segments; every one of <paramref name="required"/> must be present, and a held frame must end its
    /// segment, since nothing plays after it.
    /// </summary>
    /// <exception cref="ArgumentException">A required segment is missing, or a segment plays on past a held frame.</exception>
    internal AnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>[]> segments, IEnumerable<TSegment> required)
        : base(segments, required)
    {
    }

    /// <inheritdoc/>
    private protected override AnimationShown<TSegment, TFrame>? Pass(AnimationBeat<TFrame, TEvent> beat, ref Play play) =>
        Ensure.NotNull(beat).Match(PausingBeatPlays, ref play);

    /// <inheritdoc/>
    private protected override AnimationShown<TSegment, TFrame> Shown(AnimationShown<TSegment, TFrame>.Unpaused outcome) => outcome;

    /// <summary>The frames <paramref name="segment"/>'s beats show, in order, for a segment every beat of which shows one.</summary>
    /// <exception cref="InvalidDataException">A beat of the segment pauses, showing no frame.</exception>
    internal IReadOnlyList<TFrame> FramesShownBy(TSegment segment) =>
        [.. Enumerable.Range(0, Beats(segment).Count).Select(beat => FrameShownAt(segment, beat))];

    /// <summary>The frame beat <paramref name="beat"/> of <paramref name="segment"/> shows, a beat that shows one.</summary>
    /// <exception cref="InvalidDataException">The beat pauses, showing no frame.</exception>
    internal TFrame FrameShownAt(TSegment segment, int beat) => Beats(segment)[beat] switch
    {
        AnimationBeat<TFrame, TEvent>.Frame frame => frame.Shown,
        AnimationBeat<TFrame, TEvent>.HeldFrame held => held.Shown,
        var other => throw new InvalidDataException($"Beat {beat} of animation segment {segment} shows no frame: {other}."),
    };

    /// <summary>The first frame one play of <paramref name="animation"/> shows from its start, after any events it opens with.</summary>
    /// <exception cref="InvalidDataException">The animation shows no frame in its play.</exception>
    internal TFrame FirstFrameShown(Animation<TSegment> animation)
    {
        foreach (AnimationBeat<TFrame, TEvent> beat in PlayedBeats(animation))
        {
            if (beat is AnimationBeat<TFrame, TEvent>.Frame frame)
                return frame.Shown;
            if (beat is AnimationBeat<TFrame, TEvent>.HeldFrame held)
                return held.Shown;
        }
        throw new InvalidDataException($"Animation {animation} shows no frame in its play.");
    }
}
