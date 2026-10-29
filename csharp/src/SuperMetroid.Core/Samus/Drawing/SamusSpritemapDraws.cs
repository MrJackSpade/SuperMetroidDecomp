using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Publishes Samus's installed body pictures as whole-frame sprite draws.</summary>
public static class SamusSpritemapDraws
{
    /// <summary>
    /// Ports <c>AddSamusSpritemap</c> at <c>$81:89AE</c> for Samus's body as one pre-coloured draw (#1392, #1463): the
    /// picture <paramref name="frame"/> shows, with the definition records it shows.
    /// </summary>
    /// <param name="sprites">The frame receiving the draw after everything already emitted.</param>
    /// <param name="frame">The pose half and animation frame whose picture is drawn.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="spritemaps">The compositions each pose half shows.</param>
    /// <param name="sheets">The installed sheets.</param>
    /// <param name="records">The definition records the body shows.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    public static void AddSamusBody(
        this SpriteFrameBuffer sprites,
        SamusBodyHalfFrame frame,
        Position16<ScreenPixels> origin,
        InstalledSamusSpritemaps spritemaps,
        InstalledSpriteSheets sheets,
        SamusBodyRecords records,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(spritemaps);
        Ensure.NotNull(sheets);
        // A frame with no composition draws nothing (owner decision on #1318).
        if (!spritemaps.TryGetFrameKey(frame, records, out SamusBodyFrameKey key))
            return;
        SpriteSheetFrame image = sheets.Get(SamusBodySheets.Sheet(key)).Frame(key.Name);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None));
    }
}
