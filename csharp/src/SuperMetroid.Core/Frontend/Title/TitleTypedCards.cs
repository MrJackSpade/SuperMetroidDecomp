namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The title cards as typed text (#1513): each card reveals its compositions in order, each the one before it and one
/// more glyph. The title sheet pictures each card's glyphs once, and a reveal is drawn as the glyphs typed so far.
/// </summary>
internal static class TitleTypedCards
{
    /// <summary>The four cards, by the phases that show them.</summary>
    internal static IReadOnlyList<TitleSequencePhase> Cards { get; } =
    [
        TitleSequenceDefinitions.TextSequences.Year.Phase, TitleSequenceDefinitions.TextSequences.Nintendo.Phase,
        TitleSequenceDefinitions.TextSequences.Presents.Phase, TitleSequenceDefinitions.TextSequences.MetroidThree.Phase,
    ];

    /// <summary>Each card's reveals in typing order: the reveal at each place shows every glyph up to it.</summary>
    internal static readonly IReadOnlyDictionary<TitleSequencePhase, TitleSprite[]> Reveals = Typed();

    /// <summary>Each reveal's step.</summary>
    private static readonly Dictionary<TitleSprite, TitleTypedStep> StepOf = Reveals
        .SelectMany(card => card.Value.Select((sprite, glyph) => new KeyValuePair<TitleSprite, TitleTypedStep>(sprite, new(card.Key, glyph))))
        .ToDictionary(entry => entry.Key, entry => entry.Value);

    /// <summary>The reveal <paramref name="sprite"/> shows, or none for a composition no card reveals.</summary>
    internal static TitleTypedStep? Find(TitleSprite sprite) => StepOf.TryGetValue(sprite, out TitleTypedStep step) ? step : null;

    /// <summary>Each card's reveals: the compositions its steps show, each once, in order.</summary>
    private static Dictionary<TitleSequencePhase, TitleSprite[]> Typed()
    {
        var reveals = new Dictionary<TitleSequencePhase, TitleSprite[]>();
        foreach (TitleSequencePhase card in Cards)
        {
            var sprites = new List<TitleSprite>();
            foreach (TitleCardStep step in TitleCards.Steps(card))
            {
                if (step.Sprite is { } sprite && !sprites.Contains(sprite))
                    sprites.Add(sprite);
            }
            reveals.Add(card, [.. sprites]);
        }
        return reveals;
    }
}
