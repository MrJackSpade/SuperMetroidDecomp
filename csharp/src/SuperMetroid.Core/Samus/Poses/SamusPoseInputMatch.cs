namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A pose input list's decision (#2560), one closed choice: a condition matched with its transition, the conditions were
/// exhausted, or the list has none.
/// </summary>
internal abstract record SamusPoseInputMatch
{
    /// <summary>Only the three outcomes derive from it (#2560).</summary>
    private SamusPoseInputMatch()
    {
    }

    /// <summary>The list has conditions and none of them matched.</summary>
    internal static SamusPoseInputMatch Exhausted { get; } = new ConditionsExhausted();

    /// <summary>The list has no transitions at all.</summary>
    internal static SamusPoseInputMatch NoTransitions { get; } = new EmptyList();

    /// <summary>The transition the first matching condition chose, or null when none did.</summary>
    internal SamusPoseTransition? Transition => this is Matched matched ? matched.Choice : null;

    /// <summary>Whether the list has conditions: it matched one, or exhausted them.</summary>
    internal bool HasConditions => this is not EmptyList;

    /// <summary>The same match for the mirrored pose list: a matched transition leads to the mirrored pose.</summary>
    internal SamusPoseInputMatch Mirrored() =>
        this is Matched matched ? new Matched(matched.Choice with { ProspectivePose = SamusPoseMirror.Of(matched.Choice.ProspectivePose) }) : this;

    /// <summary>A condition matched.</summary>
    /// <param name="Choice">The transition it chose.</param>
    internal sealed record Matched(SamusPoseTransition Choice) : SamusPoseInputMatch;

    /// <summary>The list's conditions were all tried and none matched.</summary>
    private sealed record ConditionsExhausted : SamusPoseInputMatch;

    /// <summary>The list has no transitions.</summary>
    private sealed record EmptyList : SamusPoseInputMatch;
}
