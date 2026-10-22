using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Publishes Samus's installed spritemaps as whole-frame sprite draws.</summary>
public static class SamusSpritemapDraws
{
    /// <summary>
    /// Ports <c>AddSamusSpritemapToOAM</c> at <c>$81:89AE</c> as one whole-frame draw: the spritemap selected by a
    /// word index into the bank-$92 pointer table at <c>$92:808D</c>, at a screen origin, with the parts' authored
    /// palettes and priorities.
    /// </summary>
    /// <param name="sprites">The frame receiving the draw after everything already emitted.</param>
    /// <param name="spritemapIndex">Index into the installed Samus spritemap table.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="artwork">The spritemaps and the characters their parts name.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    public static void AddSamusSpritemap(
        this SpriteFrameBuffer sprites,
        int spritemapIndex,
        Position16<ScreenPixels> origin,
        SamusSpritemapArtwork artwork,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        if (artwork.Spritemaps.TryGetFrame(spritemapIndex, out SpriteFrame? frame))
        {
            // A spritemap that names a character beyond its own sheet reads the rest of the OBJ table there.
            SpriteArtwork characters = frame.LastCharacter < artwork.Characters.CharacterCount
                ? artwork.Characters : artwork.PageCharacters();
            sprites.Add(new CharacterSpriteDraw(source, frame, characters, origin, SnesTileFlipFlags.None, 0, 0));
            return;
        }
        // A zero entry in the installed pointer table dereferences $92:0000, direct-page scratch. Its parts would need
        // direct-page emulation the port does not model, so a zero entry is exempt and draws nothing (owner decision
        // on #1318).
    }

}
