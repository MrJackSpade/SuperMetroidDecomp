using System.Collections.Concurrent;

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
        this is Matched matched ? Accepting(matched.Choice with { ProspectivePose = SamusPoseMirror.Of(matched.Choice.ProspectivePose) }) : this;

    /// <summary>
    /// The match choosing <paramref name="choice"/>: one match for each choice, made the first time a lookup chooses it, so
    /// the per-update pose lookup allocates nothing once each choice it reaches has been made.
    /// </summary>
    internal static SamusPoseInputMatch Accepting(SamusPoseTransition choice) => MatchedChoices.GetOrAdd(choice, static made => new Matched(made));

    /// <summary>The match made for each choice a lookup has chosen.</summary>
    private static readonly ConcurrentDictionary<SamusPoseTransition, SamusPoseInputMatch> MatchedChoices = new();

    /// <summary>A condition matched.</summary>
    /// <param name="Choice">The transition it chose.</param>
    internal sealed record Matched(SamusPoseTransition Choice) : SamusPoseInputMatch;

    /// <summary>The list's conditions were all tried and none matched.</summary>
    private sealed record ConditionsExhausted : SamusPoseInputMatch;

    /// <summary>The list has no transitions.</summary>
    private sealed record EmptyList : SamusPoseInputMatch;
}
