using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Publishes Samus's installed body pictures as whole-frame sprite draws.</summary>
public static class SamusSpritemapDraws
{
    /// <summary>
    /// Ports <c>AddSamusSpritemap</c> at <c>$81:89AE</c> for Samus's body as one pre-coloured draw (#1392, #1463): the
    /// picture the entry at <paramref name="spritemapIndex"/> selects, with the definition records it shows.
    /// </summary>
    /// <param name="sprites">The frame receiving the draw after everything already emitted.</param>
    /// <param name="spritemapIndex">Index into the installed Samus pose-half table.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="spritemaps">The installed pose-half table.</param>
    /// <param name="sheets">The installed sheets.</param>
    /// <param name="records">The definition records the body shows.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    public static void AddSamusBody(
        this SpriteFrameBuffer sprites,
        int spritemapIndex,
        Position16<ScreenPixels> origin,
        InstalledSamusSpritemaps spritemaps,
        InstalledSpriteSheets sheets,
        SamusBodyRecords records,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(spritemaps);
        Ensure.NotNull(sheets);
        // A zero entry in the installed pointer table dereferences $92:0000, direct-page scratch the port does not model, so it draws nothing (owner decision on #1318).
        if (!spritemaps.TryGetFrameKey(spritemapIndex, records, out SamusBodyFrameKey key))
            return;
        SpriteSheetFrame image = sheets.Get(SamusBodySheets.Sheet(key)).Frame(key.Name);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None));
    }
}
