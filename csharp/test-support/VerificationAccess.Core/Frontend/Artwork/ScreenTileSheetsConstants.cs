using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>Cartridge values and helpers of <see cref="ScreenTileSheets"/> that only verification reads.</summary>
internal static class ScreenTileSheetsConstants
{
    /// <summary>Whether installed sheet <paramref name="sheet"/> holds a screen's background or Mode 7 characters rather than sprite frames.</summary>
    public static bool Holds(SpriteSheetId sheet) =>
        ScreenTileSheetDefinitions.Depths.ContainsKey(sheet) || ScreenTileSheetDefinitions.Mode7Sheets.Contains(sheet);
}
