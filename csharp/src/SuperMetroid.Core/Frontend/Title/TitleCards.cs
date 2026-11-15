namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The title cards (#1979): each card's definition and the typed steps it runs.</summary>
internal static class TitleCards
{
    /// <summary>The four cards, in the order the title shows them.</summary>
    internal static IReadOnlyList<TitleCardDefinition> All { get; } =
    [
        TitleCardDefinitions.Year, TitleCardDefinitions.Nintendo, TitleCardDefinitions.Presents, TitleCardDefinitions.MetroidThree,
    ];

    /// <summary>Each card's steps.</summary>
    private static readonly Dictionary<TitleSequencePhase, TitleCardStep[]> StepsOf = All.ToDictionary(card => card.Card, Typed);

    /// <summary>The definition of the card <paramref name="phase"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static TitleCardDefinition Of(TitleSequencePhase phase) => All.SingleOrDefault(card => card.Card == phase)
        ?? throw new ArgumentOutOfRangeException(nameof(phase), phase, "Phase shows no title card.");

    /// <summary>The steps of the card <paramref name="phase"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static IReadOnlyList<TitleCardStep> Steps(TitleSequencePhase phase) => StepsOf.TryGetValue(phase, out TitleCardStep[]? steps)
        ? steps
        : throw new ArgumentOutOfRangeException(nameof(phase), phase, "Phase shows no title card.");

    /// <summary>What the card <paramref name="phase"/> shows as it types, in typing order: its text typed as far as each letter.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static IReadOnlyList<TitleCardTyping> Typings(TitleSequencePhase phase) =>
        [.. Enumerable.Range(1, Of(phase).Text.Length).Select(letters => new TitleCardTyping(phase, letters))];

    /// <summary>
    /// The steps of <paramref name="card"/>: its blank opening when it has one, each letter typed for eight updates, the
    /// last held for the card's hold, then its ending cue and its removal.
    /// </summary>
    private static TitleCardStep[] Typed(TitleCardDefinition card)
    {
        var steps = new List<TitleCardStep>();
        if (card.OpeningFrames > 0)
            steps.Add(TitleCardStep.Show(card.OpeningFrames, null));
        for (int letters = 1; letters <= card.Text.Length; letters++)
        {
            steps.Add(TitleCardStep.Show(letters < card.Text.Length ? TitleCardDefinitions.LetterFrames : card.HoldFrames,
                new TitleCardTyping(card.Card, letters)));
        }
        steps.Add(TitleCardStep.Run(card.Ending));
        steps.Add(TitleCardStep.Run(TitleCardCue.RemoveCard));
        return [.. steps];
    }
}
