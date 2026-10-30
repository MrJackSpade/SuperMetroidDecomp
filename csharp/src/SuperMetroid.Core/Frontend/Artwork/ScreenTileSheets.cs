using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>Reads the installed screen tile sheets (#1392).</summary>
public static class ScreenTileSheets
{

    /// <summary>The pre-coloured characters installed sheet <paramref name="sheet"/> holds.</summary>
    /// <exception cref="KeyNotFoundException">The sheet holds no screen tiles.</exception>
    /// <exception cref="InvalidDataException">The sheet is not whole blocks of tiles coloured for its depth.</exception>
    public static ScreenTileSheet Of(InstalledSpriteSheets sheets, SpriteSheetId sheet) =>
        new(sheet, Ensure.NotNull(sheets).Get(sheet), ScreenTileSheetDefinitions.Depths[sheet]);
}
