using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>Helpers of <see cref="ScreenTileSheetDefinitions"/> that only verification reads.</summary>
internal static class ScreenTileSheetsConstants
{
    /// <summary>Whether installed sheet <paramref name="sheet"/> holds a screen's Mode 7 characters rather than sprite frames.</summary>
    public static bool Holds(SpriteSheetId sheet) => ScreenTileSheetDefinitions.FloorSheets.Contains(sheet);
}
