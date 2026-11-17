namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's foot's own state beside what every part holds: the phase it runs and its second-phase think timer.</summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidFootState : KraidAnimatedPartState
{
    /// <summary>Makes the foot of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the foot is part of.</param>
    /// <param name="reference">Kraid's reference to the foot, which it joins as it sets up.</param>
    internal KraidFootState(KraidEnemyState kraid, KraidPartReference reference)
        : base(kraid, reference, KraidPartSegment.FootInitial)
    {
    }

    /// <summary>The phase the foot runs; it rests until it is given a phase.</summary>
    public KraidFootPhase Phase { get; internal set; }

    /// <summary>Gets the second-phase foot's think timer, which expires when its decrement reaches zero.</summary>
    public int ThinkTimer { get; internal set; }

    /// <summary>The way the second-phase foot walks the body toward its target (#2165).</summary>
    public HorizontalDirection WalkDirection { get; internal set; }
}
