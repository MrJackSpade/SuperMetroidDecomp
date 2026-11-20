using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// How the ending places its typed texts (#1513, #2195), as the title places its cards (<see cref="Title.Artwork.TitleSpriteDrawing"/>):
/// the sheet pictures each text's letters once, and a typed text places each letter it has typed at the actor's origin, as
/// its typing step showed them. A letter that shows nothing places nothing.
/// </summary>
public static class EndingTypedTextDrawing
{
    /// <summary>The sheet's frame for the letter <paramref name="typed"/> types: the last of its text's letters it shows.</summary>
    [InstalledKey]
    public static string LetterFrameName(EndingTextTyping typed) =>
        $"{CinematicSpriteFamilyKeys.Installed.Of(EndingSpriteSheets.Cinematic(EndingTextAnimations.Family))}.{EndingTypedTextKeys.Installed.Of(typed.Text)}.Letter{typed.Letters}";

    /// <summary>Places <paramref name="typed"/>'s letters at <paramref name="origin"/>, first letter first, in <paramref name="shading"/>.</summary>
    public static void DrawText(SpriteSheet sheet, EndingTextTyping typed, ScreenSprites sprites, Offset16<ScreenPixels> origin,
        PictureShading shading)
    {
        Ensure.NotNull(shading);
        Ensure.NotNull(sheet);
        Ensure.NotNull(sprites);
        for (int letter = 1; letter <= typed.Letters; letter++)
        {
            string name = LetterFrameName(typed with { Letters = letter });
            if (!sheet.ShowsNothing(name))
                sprites.AddImage(sheet.Frame(name), origin, shading: shading);
        }
    }
}
