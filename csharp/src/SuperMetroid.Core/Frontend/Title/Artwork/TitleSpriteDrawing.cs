using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// How the title places its compositions (#1409, #1463, #1467, #1583): each is a frame of the installed title sheet, which
/// the import pictures from the installed title artwork; the game reads none of that artwork. Motion and the Baby's
/// animation are the game's own.
/// </summary>
public static class TitleSpriteDrawing
{

    /// <summary>Places one whole title composition of the title sheet in <paramref name="shading"/> (#1512).</summary>
    public static void DrawSprite(SpriteSheet sheet, TitleSprite sprite, ScreenSprites sprites, Offset16<ScreenPixels> origin,
        PictureShading shading)
    {
        Ensure.NotNull(shading);
        Ensure.NotNull(sheet);
        Ensure.NotNull(sprites);
        sprites.AddImage(sheet.Frame(TitleSpriteDefinitions.FrameName(sprite)), origin, shading: shading);
    }

    /// <summary>
    /// Places a title card's text typed so far in <paramref name="shading"/> (#1513, #1979): each letter it has typed, each at
    /// the card's origin moved by the card's scroll once for every letter typed after it. A letter that shows nothing, such
    /// as a space, places nothing.
    /// </summary>
    public static void DrawCard(SpriteSheet sheet, TitleCardTyping typed, ScreenSprites sprites, Offset16<ScreenPixels> origin,
        PictureShading shading)
    {
        Ensure.NotNull(shading);
        Ensure.NotNull(sheet);
        Ensure.NotNull(sprites);
        // No two of the card's letters overlap, so they are placed in the order they are typed (#2573).
        for (int letter = 1; letter <= typed.Letters; letter++)
        {
            string name = TitleSpriteDefinitions.LetterFrameName(typed with { Letters = letter });
            if (sheet.ShowsNothing(name))
                continue;
            // Each letter typed after this one has scrolled the card's text once more.
            int scrolls = typed.Letters - letter;
            Offset16<ScreenPixels> scrolled = Offset16<ScreenPixels>.Of(
                TitleCardDefinitions.TypedScroll.X * scrolls, TitleCardDefinitions.TypedScroll.Y * scrolls);
            sprites.AddImage(sheet.Frame(name), origin + scrolled, shading: shading);
        }
    }
}
