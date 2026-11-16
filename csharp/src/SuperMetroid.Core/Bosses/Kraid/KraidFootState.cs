namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's foot's own state beside what every part holds: the phase it runs and its second-phase think timer.</summary>
public sealed class KraidFootState : KraidPartState
{
    /// <summary>Makes the foot of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the foot is part of.</param>
    internal KraidFootState(KraidEnemyState kraid)
        : base(kraid)
    {
    }

    /// <summary>The phase the foot runs; it rests until it is given a phase.</summary>
    public KraidFootPhase Phase { get; internal set; }

    /// <summary>Gets the second-phase foot's think timer, which expires when its decrement reaches zero.</summary>
    public int ThinkTimer { get; internal set; }

    /// <summary>The way the second-phase foot walks the body toward its target (#2165).</summary>
    public HorizontalDirection WalkDirection { get; internal set; }
}
