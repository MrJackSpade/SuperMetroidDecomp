using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>The bank-$8C spritemap of each whole title composition and of each card typing step (#1979).</summary>
/// <param name="Sprites">The logo's and the copyright's spritemaps.</param>
/// <param name="Cards">Each card typing step's spritemap.</param>
internal sealed partial record TitleSpritemaps(IReadOnlyDictionary<TitleSprite, int> Sprites, IReadOnlyDictionary<TitleCardTyping, int> Cards)
{
    /// <summary>Whether every composition and card typing has a spritemap of its own.</summary>
    internal bool AllDistinct => Spritemaps.Count == Sprites.Count + Cards.Count;

    /// <summary>Every spritemap the compositions and card typings name, each once.</summary>
    internal HashSet<int> Spritemaps => Sprites.Values.Concat(Cards.Values).ToHashSet();
}
