using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Draws the map's markers, elevator and area labels, arrows and position indicator from the map sprite sheet (#1392),
/// whose frames colour the parts that take their draw's palette in <see cref="FrontendSheetPalette.FramePalette"/>.
/// </summary>
internal static class MapSpriteSheet
{
    /// <summary>
    /// Places <paramref name="id"/>'s frame at <paramref name="origin"/>, its palette-inheriting parts in the row
    /// <paramref name="paletteBits"/> select, as the spritemap draw replaces their palette and keeps the others'.
    /// </summary>
    /// <param name="sheet">The map sprite sheet.</param>
    /// <param name="id">The map sprite drawn.</param>
    /// <param name="sprites">The screen's sprite entries.</param>
    /// <param name="origin">Where the frame's origin lands.</param>
    /// <param name="paletteBits">OBJ palette bits the draw places its palette-inheriting parts in.</param>
    internal static void Draw(SpriteSheet sheet, MapSpriteId id, ScreenSprites sprites, Position16<SpritePixels> origin, int paletteBits)
    {
        Ensure.NotNull(sheet);
        // An undefined sprite fails before its palette is read.
        SpriteSheetFrame frame = sheet.Frame(MapSpriteDefinitions.Name(id));
        Ensure.NotNull(sprites).AddImage(frame, origin, FrontendSheetPalette.Override(checked((ushort)paletteBits)));
    }
}
