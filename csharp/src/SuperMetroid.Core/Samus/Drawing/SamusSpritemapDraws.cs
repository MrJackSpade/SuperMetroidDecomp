using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Publishes Samus's installed spritemaps as whole-frame sprite draws.</summary>
public static class SamusSpritemapDraws
{
    /// <summary>
    /// Ports <c>AddSamusSpritemap</c> at <c>$81:89AE</c> for Samus's body as one pre-coloured draw (#1392): the frame of
    /// the spritemap at <paramref name="spritemapIndex"/> with the definition records whose characters it shows.
    /// </summary>
    /// <param name="sprites">The frame receiving the draw after everything already emitted.</param>
    /// <param name="spritemapIndex">Index into the installed Samus spritemap table.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="spritemaps">The installed spritemap table.</param>
    /// <param name="sheets">The installed sheets.</param>
    /// <param name="records">The definition records the body shows.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    public static void AddSamusBody(
        this SpriteFrameBuffer sprites,
        int spritemapIndex,
        Position16<ScreenPixels> origin,
        SamusSpritemapArtworkCatalog spritemaps,
        InstalledSpriteSheets sheets,
        SamusBodyRecords records,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(spritemaps);
        Ensure.NotNull(sheets);
        // A zero entry in the installed pointer table dereferences $92:0000 and draws nothing, as above.
        if (!spritemaps.TryGetComposition(spritemapIndex, out SamusSpritemapId composition) ||
            !spritemaps.TryGetFrame(spritemapIndex, out SpriteFrame? frame))
            return;
        SamusBodyFrameKey key = SamusBodySheetDefinitions.Key(composition, frame, records.Top, records.Bottom);
        SpriteSheetFrame image = sheets.Get(SamusBodySheetDefinitions.Sheet(key)).Frame(key.Name);
        sprites.Add(new ImageSpriteDraw(source, image, origin, SpriteMirror.None));
    }
}
