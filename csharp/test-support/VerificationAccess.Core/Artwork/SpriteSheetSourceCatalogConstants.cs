using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SpriteSheetSourceCatalog"/> that only verification reads.</summary>
internal static class SpriteSheetSourceCatalogConstants
{
    /// <summary>
    /// Whether <paramref name="sheet"/> composes from sprite parts (<see cref="SpriteSheetSourceCatalog.For"/>), rather than from the cartridge (the
    /// escape timer) or from the pictures, pages and floors <see cref="SpriteSheetSourceCatalog.Compose"/> paints directly.
    /// </summary>
    internal static bool ComposesFromSpriteParts(SpriteSheetId sheet) =>
        sheet is not (SpriteSheetId.EscapeTimer or SpriteSheetId.PauseHud or SpriteSheetId.MenuPictures or SpriteSheetId.RoomMapPictures
            or SpriteSheetId.PausePictures or SpriteSheetId.CeresCinematicPictures or SpriteSheetId.IntroScenePictures
            or SpriteSheetId.IntroNarrationPictures)
        && !ScreenTileSheetDefinitions.AreaMapForegrounds.Values.Contains(sheet)
        && !ScreenTileSheetDefinitions.AreaMapBackgrounds.Values.Contains(sheet)
        && !ScreenTileSheetDefinitions.FloorSheets.Contains(sheet)
        && !EndingPictureSheetComposer.Sheets.Contains(sheet);
}
