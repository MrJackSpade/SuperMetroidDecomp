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

    /// <summary>The phases, in the order they play.</summary>
    internal IReadOnlyList<AnimationPhase<TSegment>> Phases { get; }

    /// <summary>How the animation ends once its last phase has played.</summary>
    internal AnimationEnd End { get; }

    /// <summary>The phase a looping animation's cycle starts at: the phase it loops from.</summary>
    /// <exception cref="InvalidOperationException">The animation does not loop, so it has no cycle.</exception>
    internal int CyclePhase => End is AnimationEnd.Loop loop
        ? loop.Phase
        : throw new InvalidOperationException($"An animation that ends with {End} has no cycle.");

}
