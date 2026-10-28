using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// What the shared menu screen's layers show (#1392, #1404): the world characters and the map characters it shows, as
/// their pre-coloured tiles under the palette the screen shows them with, at the tile numbers each layer reads.
/// </summary>
internal static class MenuScreenTiles
{
    /// <summary>The menu planes under the file-select palette: the world foreground and the map characters.</summary>
    internal static ScreenLayerPictures Menu(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenTileSet(TileInkDepth.SixteenColours)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.MenuForegroundTiles), MenuTileSetDefinitions.WorldForegroundFirstTile, 0,
                WorldMapArtworkFormat.ForegroundTileCount)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.MenuMapTiles), MenuTileSetDefinitions.MenuMapFirstTile, 0, MapTileAtlasFormat.CharacterCount)
            .Pictures();
    }

    /// <summary>The room map's planes under the file-select palette: the map characters.</summary>
    internal static ScreenLayerPictures RoomMap(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenTileSet(TileInkDepth.SixteenColours)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.MenuMapTiles), MenuTileSetDefinitions.RoomMapFirstTile, 0, MapTileAtlasFormat.CharacterCount)
            .Pictures();
    }

    /// <summary>The area map's layers under <paramref name="area"/>'s world-map palette: its foreground and its background.</summary>
    internal static AreaMapTiles AreaMap(InstalledSpriteSheets sheets, AreaId area)
    {
        Ensure.NotNull(sheets);
        return new AreaMapTiles(
            new ScreenTileSet(TileInkDepth.SixteenColours)
                .Place(ScreenTileSheets.Of(sheets, ScreenTileSheetDefinitions.AreaMapForegrounds[area]), MenuTileSetDefinitions.WorldForegroundFirstTile, 0,
                    WorldMapArtworkFormat.ForegroundTileCount)
                .Pictures(),
            new ScreenTileSet(TileInkDepth.FourColours)
                .Place(ScreenTileSheets.Of(sheets, ScreenTileSheetDefinitions.AreaMapBackgrounds[area]), MenuTileSetDefinitions.WorldBackgroundFirstTile, 0,
                    WorldMapArtworkFormat.BackgroundTileCount)
                .Pictures());
    }
}
