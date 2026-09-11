namespace SuperMetroid.Core.Game;

/// <summary>Compiled interpretation of stock map cells; presentation replacements cannot change these rules.</summary>
public static class AreaMapExplorationRules
{
    public static bool IsDiscoverable(MapTileWord stockTile) => !stockTile.IsBlank;

    /// <summary>Retail slope identity reveals the adjacent corner independently of replacement artwork.</summary>
    public static bool RevealsCellAbove(MapTileWord stockTile) =>
        (stockTile.Raw & MapTileWords.SlopedHallwayIdentityMask) == MapTileWords.SlopedHallwayCharacter;
}
