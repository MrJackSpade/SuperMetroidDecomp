using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Publishes Samus's installed body pictures as whole-frame sprite draws.</summary>
public static class SamusSpritemapDraws
{
    /// <summary>
    /// Ports <c>AddSamusSpritemap</c> for Samus's whole body as one pre-coloured draw (#1392, #1463, #1513): the
    /// full-body picture <paramref name="frame"/> shows, with the half pictures it shows.
    /// </summary>
    /// <param name="sprites">The frame receiving the draw after everything already emitted.</param>
    /// <param name="frame">The full body whose picture is drawn.</param>
    /// <param name="origin">Screen-space origin produced by Samus's draw.</param>
    /// <param name="spritemaps">The compositions each pose half shows.</param>
    /// <param name="sheets">The installed sheets.</param>
    /// <param name="pictures">The half pictures the body shows.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    public static void AddSamusBody(
        this SpriteFrameBuffer sprites,
        SamusBodyFrame frame,
        Position16<ScreenPixels> origin,
        InstalledSamusSpritemaps spritemaps,
        InstalledSpriteSheets sheets,
        SamusBodyPictures pictures,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(spritemaps);
        Ensure.NotNull(sheets);
        // A body with no composition draws nothing (owner decision on #1318).
        if (!spritemaps.TryGetFrameKey(frame.Top, frame.Bottom, frame.ChestCover, pictures, out SamusBodyFrameKey key))
            return;
        // The body sheet holds the picture of every body the game can show that shows anything (#1513). A body whose
        // selected characters are blank wherever its compositions read them shows nothing, is on no sheet and draws nothing.
        if (!sheets.Get(SpriteSheetId.SamusBody).frames.TryGetValue(key.Name, out SpriteSheetFrame? image))
            return;
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None));
    }
}
