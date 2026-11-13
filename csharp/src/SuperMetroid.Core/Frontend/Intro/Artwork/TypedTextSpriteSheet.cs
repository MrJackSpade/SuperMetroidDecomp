using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Draws a cinematic family whose typed texts the sheet pictures one glyph at a time (#1513): a typed pose places each
/// glyph typed up to its step, each at the actor's origin, as the pose showed them; any other pose of the family is drawn
/// whole. A glyph that shows nothing, such as a space, places nothing.
/// </summary>
/// <param name="sheet">The screen's sprite sheet.</param>
/// <param name="family">The family whose frames are drawn.</param>
/// <param name="shading">The shading the screen shows a draw named in a colouring's drawn shade in.</param>
/// <param name="texts">The family's typed texts.</param>
internal sealed class TypedTextSpriteSheet(SpriteSheet sheet, CinematicSpriteFamily family, Func<PictureShade, PictureShading> shading,
    CinematicTypedTexts texts) : IIntroCinematicSpritePresentation
{
    /// <summary>The sheet's frame for glyph <paramref name="glyph"/> of the text <paramref name="text"/> types in <paramref name="family"/>.</summary>
    internal static string GlyphFrameName(CinematicSpriteFamily family, CinematicSpriteSegment text, int glyph) => $"{family}.{text}.Glyph{glyph}";

    /// <inheritdoc/>
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<SpritePixels> origin, PictureShade drawn)
    {
        Ensure.NotNull(objects);
        if (Ensure.NotNull(texts).Find(pose) is not { } step)
        {
            new CinematicSpriteSheet(sheet, family, shading).Draw(pose, objects, origin, drawn);
            return;
        }
        PictureShading shown = Ensure.NotNull(shading)(drawn);
        for (int glyph = 0; glyph <= step.Glyph; glyph++)
        {
            string name = GlyphFrameName(family, step.Text, glyph);
            if (!Ensure.NotNull(sheet).ShowsNothing(name))
                objects.AddImage(sheet.Frame(name), origin, shading: shown);
        }
    }
}
