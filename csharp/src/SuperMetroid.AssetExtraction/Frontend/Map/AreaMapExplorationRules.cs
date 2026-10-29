using SuperMetroid.Core.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The cartridge's exploration rules read from stock map cells (#1466): which cells can be explored and which reveal the
/// cell above. The import decides them once from the stock words, so replacement artwork cannot change them and the game
/// never asks what a cell's shape is.
/// </summary>
internal static class AreaMapExplorationRules
{
    /// <summary>Determines whether a stock map cell participates in exploration, independently of replacement presentation artwork.</summary>
    /// <param name="stockCell">The stock cell.</param>
    /// <returns>True for every nonblank stock cell; false for the canonical blank map cell.</returns>
    internal static bool IsDiscoverable(MapCell stockCell) => !MapTileWord.Of(stockCell).IsBlank;

    /// <summary>Retail slope identity reveals the adjacent corner independently of replacement artwork.</summary>
    internal static bool RevealsCellAbove(MapCell stockCell) => MapTileWord.Of(stockCell).IsSlopedHallway;
}
