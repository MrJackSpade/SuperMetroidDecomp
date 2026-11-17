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

    /// <summary>
    /// Whether the foot shows its walk's last frame (#1886, #2301), as the walk marks it: a forward walk or lunge has
    /// finished and rests on it, and the backward walk has reached the frame from which it loops, until its next lap
    /// begins. Every animation the foot starts begins without it.
    /// </summary>
    internal bool ShowsWalkEnd { get; private set; }

    /// <summary>The foot's walk shows its last frame, as its animation's cue marks.</summary>
    internal void ReachWalkEnd() => ShowsWalkEnd = true;

    /// <summary>The foot's backward walk begins a lap, as its animation's cue marks: it no longer shows its last frame.</summary>
    internal void PlayWalkOn() => ShowsWalkEnd = false;

    /// <inheritdoc/>
    private protected override void Started(KraidPartSegment segment) => ShowsWalkEnd = false;
}
