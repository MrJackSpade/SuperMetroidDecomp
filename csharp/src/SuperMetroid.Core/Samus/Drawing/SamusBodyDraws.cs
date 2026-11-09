using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Publishes Samus's whole body pictures as sprite draws.</summary>
public static class SamusBodyDraws
{
    /// <summary>
    /// Draws Samus's whole body as one pre-coloured draw (#1392, #1513, #1585): the picture the body sheet holds for
    /// <paramref name="frame"/>, or nothing when the installed sheet holds that picture as an empty frame.
    /// </summary>
    /// <param name="sprites">The frame receiving the draw after everything already emitted.</param>
    /// <param name="frame">The whole body whose picture is drawn.</param>
    /// <param name="origin">Screen-space origin produced by Samus's draw.</param>
    /// <param name="sheets">The installed sheets.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    /// <exception cref="InvalidDataException">The body sheet holds no picture for the frame.</exception>
    public static void AddSamusBody(
        this SpriteFrameBuffer sprites,
        SamusBodyFrame frame,
        Position16<ScreenPixels> origin,
        InstalledSpriteSheets sheets,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(sheets);
        SpriteSheet body = sheets.Get(SpriteSheetId.SamusBody);
        string name = frame.Name;
        if (body.ShowsNothing(name))
            return;
        sprites.Add(new ImageSpriteDraw(source, body.Frame(name), origin, SpriteMirror.None));
    }
}
