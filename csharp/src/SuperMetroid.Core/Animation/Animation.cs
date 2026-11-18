namespace SuperMetroid.Core.Animation;

/// <summary>An actor's animation (#1600): its phases in the order they play, and how it ends.</summary>
/// <typeparam name="TSegment">The segment identity the phases play.</typeparam>
internal sealed record Animation<TSegment>
    where TSegment : struct
{
    /// <summary>
    /// Creates an animation; every phase must play at least once, a loop must name a phase, and only an animation that
    /// stops may have no phase: it is over as soon as it starts.
    /// </summary>
    /// <exception cref="ArgumentException">A phase never plays, or the loop names no phase.</exception>
    internal Animation(IReadOnlyList<AnimationPhase<TSegment>> phases, AnimationEnd end)
    {
        Phases = Ensure.NotNull(phases);
        End = Ensure.NotNull(end);
        if (phases.Any(phase => phase.Plays < 1 || phase.BeatCount < 0 || phase.FirstBeat < 0))
            throw new ArgumentException("Every phase plays a run of beats at least once.", nameof(phases));
        if (end is AnimationEnd.Loop loop && loop.Phase >= phases.Count)
            throw new ArgumentException($"An animation of {phases.Count} phases cannot loop from phase {loop.Phase}.", nameof(end));
    }

    /// <summary>
    /// An animation that plays <paramref name="lead"/> once and then loops <paramref name="cycle"/> for good (#2331): the loop
    /// starts where the lead ends, so no phase is named by its place.
    /// </summary>
    /// <exception cref="ArgumentException">The cycle has no phase, or a phase never plays.</exception>
    internal static Animation<TSegment> LeadingInto(IReadOnlyList<AnimationPhase<TSegment>> lead, IReadOnlyList<AnimationPhase<TSegment>> cycle) =>
        new([.. Ensure.NotNull(lead), .. Ensure.NotNull(cycle)], new AnimationEnd.Loop(lead.Count));

    /// <summary>An animation that loops <paramref name="cycle"/> for good from its first phase (#2331).</summary>
    /// <exception cref="ArgumentException">The cycle has no phase, or a phase never plays.</exception>
    internal static Animation<TSegment> Looping(IReadOnlyList<AnimationPhase<TSegment>> cycle) => LeadingInto([], cycle);

    /// <summary>The phases, in the order they play.</summary>
    internal IReadOnlyList<AnimationPhase<TSegment>> Phases { get; }

    /// <summary>How the animation ends once its last phase has played.</summary>
    internal AnimationEnd End { get; }

    /// <summary>
    /// The end of a play of the one phase that plays <paramref name="segment"/> (#2275): its last beat played, where an
    /// owner continuing the animation there counts the phase's next play.
    /// </summary>
    /// <exception cref="InvalidOperationException">No phase, or more than one, plays <paramref name="segment"/>.</exception>
    internal AnimationPlace EndOfPlayOf(TSegment segment)
    {
        int[] playing = [.. Enumerable.Range(0, Phases.Count).Where(index => EqualityComparer<TSegment>.Default.Equals(Phases[index].Segment, segment))];
        if (playing.Length != 1)
            throw new InvalidOperationException($"{playing.Length} phases play {segment}, not one.");
        return new(playing[0], Phases[playing[0]].BeatCount);
    }

    /// <summary>The phase a looping animation's cycle starts at: the phase it loops from.</summary>
    /// <exception cref="InvalidOperationException">The animation does not loop, so it has no cycle.</exception>
    internal int CyclePhase => End is AnimationEnd.Loop loop
        ? loop.Phase
        : throw new InvalidOperationException($"An animation that ends with {End} has no cycle.");

}
