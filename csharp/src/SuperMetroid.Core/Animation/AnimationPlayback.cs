namespace SuperMetroid.Core.Animation;

/// <summary>An owner's playing animation (#1600): the animation, and where it stands in it.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Animation">The animation playing.</param>
/// <param name="Place">Where it stands.</param>
internal readonly record struct AnimationPlayback<TSegment>(Animation<TSegment> Animation, AnimationPlace Place)
    where TSegment : struct
{
    /// <summary>The segment the playing phase plays; before the first play, the first phase's (#1886).</summary>
    /// <exception cref="InvalidOperationException">The animation has no phase.</exception>
    internal TSegment PlayingSegment => Animation.Phases.Count > Place.Phase
        ? Animation.Phases[Place.Phase].Segment
        : throw new InvalidOperationException("An animation of no phase plays no segment.");

    /// <summary>
    /// Whether the play has shown the playing phase's last beat and rests after it (#1886): on its last frame until it plays
    /// on, or where the animation holds for good.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation has no phase.</exception>
    internal bool HasShownPhaseEnd => Animation.Phases.Count > Place.Phase
        ? Place.Beat >= Animation.Phases[Place.Phase].BeatCount
        : throw new InvalidOperationException("An animation of no phase has no phase to finish.");

    /// <summary>
    /// The playback of <paramref name="other"/> going on from where this play stands (#2387), as two cycles sharing one
    /// place hand it to each other: its next beat is the one this play shows next, counted through its animation's phases
    /// from the first, or <paramref name="other"/>'s last beat when it has fewer. Samus's Screw Attack glow and Speed
    /// Booster glow share their place this way, so a Speed Booster glow following a Screw Attack spin starts where the spin
    /// stood and rests on its own brightest shade past its own end.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Either animation has no beat or repeats a phase, whose plays a beat's number cannot tell apart.
    /// </exception>
    internal AnimationPlayback<TSegment> GoingOnIn(Animation<TSegment> other)
    {
        Ensure.NotNull(other);
        int beats = BeatCount(other);
        int next = Math.Min(NextBeatNumber(), beats - 1);
        for (int phase = 0; ; phase++)
        {
            int count = other.Phases[phase].BeatCount;
            if (next < count)
                return new(other, new AnimationPlace(phase, next));
            next -= count;
        }
    }

    /// <summary>
    /// The beat this play shows next, counted through its animation's phases from the first: a phase it has finished goes on
    /// to the next phase or to the one its animation loops from, and a play resting at its end goes on resting on its last
    /// beat.
    /// </summary>
    private int NextBeatNumber()
    {
        _ = BeatCount(Animation);
        AnimationPlace next = NextBeat();
        return Animation.Phases.Take(next.Phase).Sum(earlier => earlier.BeatCount) + next.Beat;
    }

    /// <summary>
    /// The phase and beat this play shows next: a phase it has finished goes on to the next phase or to the one its
    /// animation loops from, and a play resting at its end goes on resting on its last beat.
    /// </summary>
    private AnimationPlace NextBeat()
    {
        int beats = Animation.Phases[Place.Phase].BeatCount;
        if (Place.Beat < beats)
            return AnimationPlace.StartOf(Place.Phase) with { Beat = Math.Max(Place.Beat, 0) };
        if (Place.Phase + 1 < Animation.Phases.Count)
            return AnimationPlace.StartOf(Place.Phase + 1);
        return Animation.End is AnimationEnd.Loop loop && Place.Beat == beats
            ? AnimationPlace.StartOf(loop.Phase)
            : AnimationPlace.StartOf(Place.Phase) with { Beat = beats - 1 };
    }

    /// <summary>The beats <paramref name="animation"/>'s phases play once through, each phase played once.</summary>
    /// <exception cref="InvalidOperationException">The animation has no beat or repeats a phase.</exception>
    private static int BeatCount(Animation<TSegment> animation)
    {
        if (animation.Phases.Any(phase => phase.IsRepeated))
            throw new InvalidOperationException("An animation that repeats a phase has no single count of its beats.");
        int beats = animation.Phases.Sum(phase => phase.BeatCount);
        return beats > 0 ? beats : throw new InvalidOperationException("An animation of no beat has no place to go on from.");
    }
}
