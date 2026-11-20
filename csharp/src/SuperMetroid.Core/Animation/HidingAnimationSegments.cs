namespace SuperMetroid.Core.Animation;

/// <summary>
/// The segments of a family of animations that may pause and hide (#2553, #2558): a play of one comes to a frame, a held
/// frame, a pause, a stretch that shows nothing, a rest or the animation's end. Only such a family holds a hidden beat.
/// </summary>
/// <typeparam name="TSegment">
/// The segment identity: an enum, whose every member must be a segment, or a composite value naming the segments it requires.
/// </typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed class HidingAnimationSegments<TSegment, TFrame, TEvent>
    : AnimationSegmentPlay<TSegment, TFrame, TEvent, IHidingAnimationBeat<TFrame, TEvent>, IHidingAnimationShown<TSegment, TFrame>>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>
    /// Creates the segments; every one of <paramref name="required"/> must be present, and a held frame must end its
    /// segment, since nothing plays after it.
    /// </summary>
    /// <exception cref="ArgumentException">A required segment is missing, or a segment plays on past a held frame.</exception>
    internal HidingAnimationSegments(IReadOnlyDictionary<TSegment, IHidingAnimationBeat<TFrame, TEvent>[]> segments, IEnumerable<TSegment> required)
        : base(segments, required)
    {
    }

    /// <inheritdoc/>
    private protected override IHidingAnimationShown<TSegment, TFrame>? Pass(IHidingAnimationBeat<TFrame, TEvent> beat, ref Play play) =>
        Ensure.NotNull(beat).Match(BeatPlays, ref play);

    /// <inheritdoc/>
    private protected override IHidingAnimationShown<TSegment, TFrame> Shown(AnimationShown<TSegment, TFrame>.Unpaused outcome) => outcome;

    /// <summary>What a play shows on each beat it passes, built once: a hidden stretch hides the owner for its updates.</summary>
    private static HidingAnimationBeatCases<TFrame, TEvent, Play, IHidingAnimationShown<TSegment, TFrame>?> BeatPlays { get; } = new(
        Beat: static (beat, ref play) => beat.Match(PausingBeatPlays, ref play),
        Hidden: static (hidden, ref play) => new AnimationHiddenShown<TSegment, TFrame>(play.Phase.Segment, hidden.Duration));
}
