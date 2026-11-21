namespace SuperMetroid.Core.Animation;

/// <summary>
/// The segments of a family of animations that never pause (#2553): its segments hold no pause, so a play of one comes
/// only to a frame, a held frame, a rest or the animation's end, and its owner has no pause to answer.
/// </summary>
/// <typeparam name="TSegment">
/// The segment identity: an enum, whose every member must be a segment, or a composite value naming the segments it requires.
/// </typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed class UnpausedAnimationSegments<TSegment, TFrame, TEvent>
    : AnimationSegmentPlay<TSegment, TFrame, TEvent, AnimationBeat<TFrame, TEvent>.Unpaused, AnimationShown<TSegment, TFrame>.Unpaused>,
        IUnpausedAnimationPlay<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Creates the segments of an enum identity; every member of <typeparamref name="TSegment"/> must be present.</summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TSegment"/> is not an enum, so it names no members to require, or a segment is missing.
    /// </exception>
    internal UnpausedAnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>.Unpaused[]> segments)
        : this(segments, EveryMember(nameof(segments)))
    {
    }

    /// <summary>
    /// Creates the segments; every one of <paramref name="required"/> must be present, and a held frame must end its
    /// segment, since nothing plays after it.
    /// </summary>
    /// <exception cref="ArgumentException">A required segment is missing, or a segment plays on past a held frame.</exception>
    internal UnpausedAnimationSegments(IReadOnlyDictionary<TSegment, AnimationBeat<TFrame, TEvent>.Unpaused[]> segments, IEnumerable<TSegment> required)
        : base(segments, required)
    {
    }

    /// <inheritdoc/>
    IUnpausedHidingAnimationShown<TSegment, TFrame> IUnpausedAnimationPlay<TSegment, TFrame, TEvent>.PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react) =>
        PlayToNextFrame(ref animation, ref place, react);

    /// <inheritdoc/>
    private protected override AnimationShown<TSegment, TFrame>.Unpaused? Pass(AnimationBeat<TFrame, TEvent>.Unpaused beat, ref Play play) =>
        Ensure.NotNull(beat).Match(UnpausedBeatPlays, ref play);

    /// <inheritdoc/>
    private protected override AnimationShown<TSegment, TFrame>.Unpaused Shown(AnimationShown<TSegment, TFrame>.Unpaused outcome) => outcome;
}
