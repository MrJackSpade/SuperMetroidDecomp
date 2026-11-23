namespace SuperMetroid.Core.Animation;

/// <summary>
/// An animation that loops for good (#2553): a lead played once and then a cycle repeated, so an owner can restart it at
/// its cycle and no reader asks an animation that does not loop where its cycle starts.
/// </summary>
/// <typeparam name="TSegment">The segment identity the phases play.</typeparam>
internal sealed partial record LoopingAnimation<TSegment>
    where TSegment : struct
{
    /// <summary>Keeps <paramref name="animation"/>, whose cycle starts at <paramref name="cyclePhase"/>.</summary>
    private LoopingAnimation(Animation<TSegment> animation, int cyclePhase)
    {
        Animation = animation;
        CyclePhase = cyclePhase;
    }

    /// <summary>
    /// The animation that plays <paramref name="lead"/> once and then loops <paramref name="cycle"/> for good
    /// (<see cref="Animation{TSegment}.LeadingInto"/>): its cycle starts where the lead ends.
    /// </summary>
    /// <exception cref="ArgumentException">The cycle has no phase, or a phase never plays.</exception>
    internal static LoopingAnimation<TSegment> LeadingInto(IReadOnlyList<AnimationPhase<TSegment>> lead, IReadOnlyList<AnimationPhase<TSegment>> cycle) =>
        new(Animation<TSegment>.LeadingInto(lead, cycle), lead.Count);

    /// <summary>The animation that loops <paramref name="cycle"/> for good from its first phase.</summary>
    /// <exception cref="ArgumentException">The cycle has no phase, or a phase never plays.</exception>
    internal static LoopingAnimation<TSegment> Looping(IReadOnlyList<AnimationPhase<TSegment>> cycle) => LeadingInto([], cycle);

    /// <summary>The animation, its lead and then its cycle.</summary>
    internal Animation<TSegment> Animation { get; }

    /// <summary>The phase its cycle starts at: the phase it loops from.</summary>
    internal int CyclePhase { get; }
}
