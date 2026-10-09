namespace SuperMetroid.Core.Game;

/// <summary>Compiled interpretation of stock map cells; presentation replacements cannot change these rules.</summary>
public static class AreaMapExplorationRules
{
    /// <summary>Determines whether a stock map cell participates in exploration, independently of replacement presentation artwork.</summary>
    /// <param name="stockTile">Compiled retail tile identity and attributes.</param>
    /// <returns>True for every nonblank stock tile; false for the canonical blank map cell.</returns>
    public static bool IsDiscoverable(MapTileWord stockTile) => !stockTile.IsBlank;

    /// <summary>Retail slope identity reveals the adjacent corner independently of replacement artwork.</summary>
    public static bool RevealsCellAbove(MapTileWord stockTile) =>
        (stockTile.Raw & MapTileWords.SlopedHallwayIdentityMask) == MapTileWords.SlopedHallwayCharacter;
}
