using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// What the pause screen's layers show (#1392, #1404): the map, pause interface and HUD characters, as their pre-coloured
/// tiles under the pause palette, at the tile numbers each layer reads.
/// </summary>
internal static class PauseScreenTiles
{
    /// <summary>The map, equipment and frame planes: the map characters, then the pause interface characters.</summary>
    internal static ScreenLayerPictures Planes(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenTileSet(TileInkDepth.SixteenColours)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseMapTiles), PauseTileSetDefinitions.MapFirstTile, 0, MapTileAtlasFormat.CharacterCount)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseInterfaceTiles), PauseTileSetDefinitions.InterfaceFirstTile, 0,
                PauseTileAtlasFormat.ByteCount / (ScreenTileSheet.CharacterWordsOf(TileInkDepth.SixteenColours) * sizeof(ushort)))
            .Pictures();
    }

    /// <summary>The HUD plane: the HUD characters.</summary>
    internal static ScreenLayerPictures Hud(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenTileSet(TileInkDepth.FourColours)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseHudTiles), PauseTileSetDefinitions.HudFirstTile, 0,
                HudTileAtlasFormat.CharacterByteCount / (ScreenTileSheet.CharacterWordsOf(TileInkDepth.FourColours) * sizeof(ushort)))
            .Pictures();
    }
}
