namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Where the shared menu screen's artwork sits among each of its layers' tile numbers (#1404).</summary>
internal static class MenuTileSetDefinitions
{
    /// <summary>The tile number of the world map's first foreground character on the menu planes and the area map's foreground.</summary>
    internal const int WorldForegroundFirstTile = 0;

    /// <summary>The tile number of the first map character on the menu planes, after the world foreground's characters.</summary>
    internal const int MenuMapFirstTile = 768;

    /// <summary>The tile number of the first map character on the room map's planes, which show only map characters.</summary>
    internal const int RoomMapFirstTile = 0;

    /// <summary>The tile number of the world map's first background character on the area map's background.</summary>
    internal const int WorldBackgroundFirstTile = 0;
}
