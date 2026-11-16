namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's lints' own state beside what every part holds: the phase it runs, its X offset from his body and how
/// far it has travelled out of his mouth.
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidLintState : KraidPartState
{
    /// <summary>Makes a lint of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the lint is part of.</param>
    internal KraidLintState(KraidEnemyState kraid)
        : base(kraid)
    {
    }

    /// <summary>The phase the lint runs, inactive from its setup until his growth produces it.</summary>
    public KraidLintPhase Phase { get; internal set; }

    /// <summary>A lint's X offset from the body in pixels: zero, or sixteen to the left.</summary>
    public int LintXOffset { get; internal set; }

    /// <summary>The pixels a lint has travelled out of Kraid's mouth while it is produced; never negative.</summary>
    public int LintTravel
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}
