using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>The bank-$8C spritemap of each whole title composition and of each card typing step (#1979).</summary>
/// <param name="Sprites">The logo's and the copyright's spritemaps.</param>
/// <param name="Cards">Each card typing step's spritemap.</param>
internal sealed record TitleSpritemaps(IReadOnlyDictionary<TitleSprite, int> Sprites, IReadOnlyDictionary<TitleCardTyping, int> Cards)
{
    /// <summary>Every spritemap, the compositions' then the cards'.</summary>
    internal IEnumerable<int> All => Sprites.Values.Concat(Cards.Values);
}
