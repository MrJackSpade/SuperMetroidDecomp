using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Cartridge values and helpers of <see cref="TitleSpriteDrawing"/> that only verification reads.</summary>
internal static class TitleSpriteDrawingConstants
{
    /// <summary>Places one title frame of the title sheet, its parts drawn in <paramref name="colouring"/> (#1512).</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Offset16<ScreenPixels> origin,
        TitleSpriteColouring colouring) =>
        TitleSpriteDrawing.DrawSprite(sheet, sprite, sprites, origin, PictureShading.Shaded(TitleShades.Drawn(colouring)));
    /// <summary>Places a title card's text typed so far, its letters drawn in <paramref name="colouring"/> (#1512, #1979).</summary>
    public static void DrawCard(SpriteSheet sheet, TitleCardTyping typed, ScreenSprites sprites, Offset16<ScreenPixels> origin,
        TitleSpriteColouring colouring) =>
        TitleSpriteDrawing.DrawCard(sheet, typed, sprites, origin, PictureShading.Shaded(TitleShades.Drawn(colouring)));
}
