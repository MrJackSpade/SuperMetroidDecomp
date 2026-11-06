using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Draws the map's markers, elevator and area labels, arrows and position indicator from the map sprite sheet (#1392),
/// each in the shade the import pre-coloured its palette-inheriting parts in for the colouring it draws in (#1512).
/// </summary>
internal static class MapSpriteSheet
{
    /// <summary>
    /// Places <paramref name="id"/>'s frame at <paramref name="origin"/>, its palette-inheriting parts in
    /// <paramref name="colouring"/> as <paramref name="screen"/> shows them.
    /// </summary>
    /// <param name="sheet">The map sprite sheet.</param>
    /// <param name="id">The map sprite drawn.</param>
    /// <param name="sprites">The screen's sprite entries.</param>
    /// <param name="origin">Where the frame's origin lands.</param>
    /// <param name="screen">The map screen the sprite is drawn on.</param>
    /// <param name="colouring">What the map shows the sprite's palette-inheriting parts as.</param>
    /// <exception cref="InvalidOperationException">The sprite is not one <see cref="MapSpriteColourings"/> draws in <paramref name="colouring"/>, so the sheet does not picture it in that colouring.</exception>
    internal static void Draw(SpriteSheet sheet, MapSpriteId id, ScreenSprites sprites, Position16<SpritePixels> origin,
        MapSpriteScreen screen, MapSpriteColouring colouring)
    {
        if (!Ensure.Listed(MapSpriteColourings.Drawn, id).Contains(colouring))
            throw new InvalidOperationException($"Map sprite {id} is not drawn in {colouring}.");
        Draw(sheet, id, sprites, origin, PictureShading.Shaded(MapSpriteShades.Drawn(screen, colouring)));
    }

    /// <summary>Places <paramref name="id"/>'s frame at <paramref name="origin"/> in <paramref name="shading"/>.</summary>
    /// <param name="sheet">The map sprite sheet.</param>
    /// <param name="id">The map sprite drawn.</param>
    /// <param name="sprites">The screen's sprite entries.</param>
    /// <param name="origin">Where the frame's origin lands.</param>
    /// <param name="shading">The shades the sprite shows, such as the highlight cycle's current frame.</param>
    internal static void Draw(SpriteSheet sheet, MapSpriteId id, ScreenSprites sprites, Position16<SpritePixels> origin,
        PictureShading shading)
    {
        Ensure.NotNull(sheet);
        Ensure.NotNull(shading);
        SpriteSheetFrame frame = sheet.Frame(MapSpriteNames.Name(id));
        Ensure.NotNull(sprites).AddImage(frame, origin, shading: shading);
    }
}
