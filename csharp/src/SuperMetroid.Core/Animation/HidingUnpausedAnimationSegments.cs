namespace SuperMetroid.Core.Animation;

/// <summary>
/// The segments of a family of animations that never pause but may hide (#2553, #2558): a play of one comes to a frame, a
/// held frame, a stretch that shows nothing, a rest or the animation's end. Only such a family, or one that may also pause,
/// holds a hidden beat.
/// </summary>
/// <typeparam name="TSegment">
/// The segment identity: an enum, whose every member must be a segment, or a composite value naming the segments it requires.
/// </typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed partial class HidingUnpausedAnimationSegments<TSegment, TFrame, TEvent>
    : AnimationSegmentPlay<TSegment, TFrame, TEvent, IUnpausedHidingAnimationBeat<TFrame, TEvent>, IUnpausedHidingAnimationShown<TSegment, TFrame>>,
        IUnpausedAnimationPlay<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Creates the segments of an enum identity; every member of <typeparamref name="TSegment"/> must be present.</summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="TSegment"/> is not an enum, so it names no members to require, or a segment is missing.
    /// </exception>
    internal HidingUnpausedAnimationSegments(IReadOnlyDictionary<TSegment, IUnpausedHidingAnimationBeat<TFrame, TEvent>[]> segments)
        : this(segments, EveryMember(nameof(segments)))
    {
    }

    /// <summary>
    /// Creates the segments; every one of <paramref name="required"/> must be present, and a held frame must end its
    /// segment, since nothing plays after it.
    /// </summary>
    /// <exception cref="ArgumentException">A required segment is missing, or a segment plays on past a held frame.</exception>
    internal HidingUnpausedAnimationSegments(IReadOnlyDictionary<TSegment, IUnpausedHidingAnimationBeat<TFrame, TEvent>[]> segments, IEnumerable<TSegment> required)
        : base(segments, required)
    {
    }

    /// <inheritdoc/>
    IUnpausedHidingAnimationShown<TSegment, TFrame> IUnpausedAnimationPlay<TSegment, TFrame, TEvent>.PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react) =>
        PlayToNextFrame(ref animation, ref place, react);

    /// <inheritdoc/>
    private protected override IUnpausedHidingAnimationShown<TSegment, TFrame>? Pass(IUnpausedHidingAnimationBeat<TFrame, TEvent> beat, ref Play play) =>
        Ensure.NotNull(beat).Match(BeatPlays, ref play);

    /// <inheritdoc/>
    private protected override IUnpausedHidingAnimationShown<TSegment, TFrame> Shown(AnimationShown<TSegment, TFrame>.Unpaused outcome) => outcome;

    /// <summary>What a play shows on each beat it passes, built once: a hidden stretch hides the owner for its updates.</summary>
    private static UnpausedHidingAnimationBeatCases<TFrame, TEvent, Play, IUnpausedHidingAnimationShown<TSegment, TFrame>?> BeatPlays { get; } = new(
        Unpaused: static (beat, ref play) => beat.Match(UnpausedBeatPlays, ref play),
        Hidden: static (hidden, ref play) => new AnimationHiddenShown<TSegment, TFrame>(play.Phase.Segment, hidden.Duration));
}

/// <summary>
/// A family of animations that never pause (#2553), whether or not it may hide: what a projectile's timeline plays, each
/// play coming to an outcome that is no pause, or to a hidden stretch in a family that hides.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal interface IUnpausedAnimationPlay<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>
    /// Plays <paramref name="animation"/> on from <paramref name="place"/> to its next frame, as
    /// <see cref="AnimationSegmentPlay{TSegment, TFrame, TEvent, TBeat, TShown}"/> plays its segments, each event going to
    /// <paramref name="react"/>, which may continue the play elsewhere.
    /// </summary>
    IUnpausedHidingAnimationShown<TSegment, TFrame> PlayToNextFrame(
        ref Animation<TSegment> animation, ref AnimationPlace place, Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react);
}
