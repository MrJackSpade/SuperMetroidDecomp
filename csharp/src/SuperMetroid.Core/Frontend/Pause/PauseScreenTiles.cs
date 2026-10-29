using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// What the pause screen's map, equipment and frame planes show (#1392, #1404): the map and pause interface characters,
/// as their pre-coloured tiles under the pause palette, at the tile numbers each plane reads. The HUD plane draws the
/// HUD's images (#1466).
/// </summary>
internal static class PauseScreenTiles
{
    /// <summary>The map, equipment and frame planes: the map characters, then the pause interface characters.</summary>
    internal static ScreenLayerPictures Planes(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenTileSet(TileInkDepth.SixteenColours)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseMapTiles), PauseTileSetDefinitions.MapFirstTile, 0, MapScreenDefinitions.MapTileCount)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseInterfaceTiles), PauseTileSetDefinitions.InterfaceFirstTile, 0,
                MapScreenDefinitions.MapTileCount)
            .Pictures();
    }
}
