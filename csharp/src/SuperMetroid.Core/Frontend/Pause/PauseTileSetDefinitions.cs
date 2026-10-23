using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Where the pause screen's artwork sits among each of its layers' tile numbers (#1404).</summary>
internal static class PauseTileSetDefinitions
{
    /// <summary>The tile number of the first map character on the map, equipment and frame planes.</summary>
    internal const int MapFirstTile = 0;

    /// <summary>The tile number of the first pause interface character on those planes, after the map characters.</summary>
    internal const int InterfaceFirstTile = MapTileAtlasFormat.CharacterCount;

    /// <summary>The tile number of the first HUD character on the HUD plane.</summary>
    internal const int HudFirstTile = 0;
}
