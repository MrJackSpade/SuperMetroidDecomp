using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The installed title presentation (#1409, #1463, #1467). The installed title sheets picture its sprite compositions and
/// its floor, which belong to the import pipeline that reads the installed artwork. Motion and animation page order
/// remain compiled behavior.
/// </summary>
public abstract class TitleGraphicsPresentation
{
    /// <summary>Only the import's reading of the installed artwork is a title presentation.</summary>
    internal TitleGraphicsPresentation()
    {
    }

    /// <summary>Places one title frame of the title sheet, its parts drawn in <paramref name="colouring"/> (#1512).</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<SpritePixels> origin,
        TitleSpriteColouring colouring) =>
        DrawSprite(sheet, sprite, sprites, origin, PictureShading.Shaded(TitleShades.Drawn(colouring)));

    /// <summary>
    /// Places one title composition of the title sheet in <paramref name="shading"/> (#1512): a card's reveal as each glyph
    /// its card has typed up to it, each at the card's origin (#1513), any other composition as its whole frame. A glyph
    /// that shows nothing, such as a space, places nothing.
    /// </summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Position16<SpritePixels> origin,
        PictureShading shading)
    {
        Ensure.NotNull(shading);
        Ensure.NotNull(sheet);
        Ensure.NotNull(sprites);
        if (TitleTypedCards.Find(sprite) is not { } step)
        {
            sprites.AddImage(sheet.Frame(TitleSpriteDefinitions.FrameName(sprite)), origin, shading: shading);
            return;
        }
        for (int glyph = 0; glyph <= step.Glyph; glyph++)
        {
            string name = TitleSpriteDefinitions.GlyphFrameName(step.Card, glyph);
            if (!sheet.ShowsNothing(name))
                sprites.AddImage(sheet.Frame(name), origin, shading: shading);
        }
    }
}
