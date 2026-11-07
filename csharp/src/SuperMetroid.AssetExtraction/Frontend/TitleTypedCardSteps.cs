using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The title cards' reveals in typing order (#1513), by which the import pictures each card's glyphs once.</summary>
internal static class TitleTypedCardSteps
{
    /// <summary>The compositions <paramref name="card"/> reveals, in typing order: the reveal at each place shows every glyph up to it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase shows no card.</exception>
    internal static IReadOnlyList<TitleSprite> Of(TitleSequencePhase card) => TitleTypedCards.Reveals.TryGetValue(card, out TitleSprite[]? sprites)
        ? sprites
        : throw new ArgumentOutOfRangeException(nameof(card), card, "Phase shows no title card.");
}
