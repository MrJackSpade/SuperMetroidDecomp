using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// What the shared menu screen's background characters show (#1392): the world characters and the map characters it
/// loads, as their pre-coloured tiles under the palette the screen shows them with.
/// </summary>
[PpuLayoutBoundary("codec: the shared menu screen's background characters at their native words")]
internal static class MenuScreenTiles
{
    /// <summary>The menus' characters under the file-select palette: the world foreground and the map characters.</summary>
    internal static ScreenCharacters FileSelect(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenCharacters()
            .Show(ScreenTileSheets.Of(sheets, SpriteSheetId.MenuForegroundTiles), WorldMapVramLayout.Foreground.Offset, 0, WorldMapArtworkFormat.ForegroundTileCount)
            .Show(ScreenTileSheets.Of(sheets, SpriteSheetId.MenuMapTiles), FileSelectMapRomData.RoomCharacters, 0, MapTileAtlasFormat.CharacterCount);
    }

    /// <summary>The area map's characters under <paramref name="area"/>'s world-map palette: the world foreground and background.</summary>
    internal static ScreenCharacters AreaMap(InstalledSpriteSheets sheets, AreaId area)
    {
        Ensure.NotNull(sheets);
        return new ScreenCharacters()
            .Show(ScreenTileSheets.Of(sheets, ScreenTileSheetDefinitions.AreaMapForegrounds[area]), WorldMapVramLayout.Foreground.Offset, 0,
                WorldMapArtworkFormat.ForegroundTileCount)
            .Show(ScreenTileSheets.Of(sheets, ScreenTileSheetDefinitions.AreaMapBackgrounds[area]), WorldMapVramLayout.Background.Offset, 0,
                WorldMapArtworkFormat.BackgroundTileCount);
    }
}
