using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The title cards (#1979, #2359): each card's definition and its typing, played by the shared animation player as the
/// ending's typed texts are: its blank opening when it has one, each letter typed for <see cref="TitleCardDefinitions.LetterFrames"/>, the last held for the
/// card's hold, then its ending cue and its removal.
/// </summary>
internal static class TitleCards
{
    /// <summary>The four cards, in the order the title shows them.</summary>
    internal static IReadOnlyList<TitleCardDefinition> All { get; } =
    [
        TitleCardDefinitions.Year, TitleCardDefinitions.Nintendo, TitleCardDefinitions.Presents, TitleCardDefinitions.MetroidThree,
    ];

    /// <summary>Each card's typing, a segment named for the phase that shows the card.</summary>
    internal static AnimationSegments<TitleSequencePhase, TitleCardTyping, TitleCardCue> Segments { get; } = new(
        All.ToDictionary(card => card.Card, Typed), All.Select(card => card.Card));

    /// <summary>The definition of the card <paramref name="phase"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static TitleCardDefinition Of(TitleSequencePhase phase) => All.SingleOrDefault(card => card.Card == phase)
        ?? throw new ArgumentOutOfRangeException(nameof(phase), phase, "Phase shows no title card.");

    /// <summary>The animation of the card <paramref name="phase"/> shows: its typing once, after which it is over.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static Animation<TitleSequencePhase> Typing(TitleSequencePhase phase) =>
        new([Segments.Whole(Of(phase).Card)], AnimationEnd.Stops);

    /// <summary>What the card <paramref name="phase"/> shows as it types, in typing order: its text typed as far as each letter.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static IReadOnlyList<TitleCardTyping> Typings(TitleSequencePhase phase) =>
        [.. Enumerable.Range(1, Of(phase).Text.Length).Select(letters => new TitleCardTyping(phase, letters))];

    /// <summary>
    /// The beats of <paramref name="card"/>: its blank opening when it has one, each letter typed for <see cref="TitleCardDefinitions.LetterFrames"/>, the
    /// last held for the card's hold, then its ending cue and its removal.
    /// </summary>
    private static AnimationBeat<TitleCardTyping, TitleCardCue>[] Typed(TitleCardDefinition card)
    {
        var beats = new List<AnimationBeat<TitleCardTyping, TitleCardCue>>();
        if (card.OpeningFrames > 0)
            beats.Add(new AnimationBeat<TitleCardTyping, TitleCardCue>.Pause(card.OpeningFrames));
        foreach (TitleCardTyping typing in Typings(card.Card))
        {
            int duration = typing.Letters < card.Text.Length ? TitleCardDefinitions.LetterFrames : card.HoldFrames;
            beats.Add(new AnimationBeat<TitleCardTyping, TitleCardCue>.Frame(typing, duration));
        }
        beats.Add(new AnimationBeat<TitleCardTyping, TitleCardCue>.Event(card.Ending));
        beats.Add(new AnimationBeat<TitleCardTyping, TitleCardCue>.Event(TitleCardCue.RemoveCard));
        return [.. beats];
    }
}
