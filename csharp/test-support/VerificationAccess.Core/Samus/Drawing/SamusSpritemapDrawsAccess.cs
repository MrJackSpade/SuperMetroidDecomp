using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Samus spritemaps with the characters their parts name.</summary>
/// <param name="Spritemaps">The installed bank-$92 spritemaps.</param>
/// <param name="Characters">Characters the installed spritemaps' tile numbers index.</param>
/// <param name="PageCharacters">
/// The standard OBJ page with <paramref name="Characters"/> over it, for spritemaps that name characters beyond them
/// (the zero-pointer WRAM spritemap among them); built only when such a spritemap is drawn.
/// </param>
internal readonly record struct SamusSpritemapArtwork(
    SamusSpritemapSources Spritemaps,
    SpriteArtwork Characters,
    Func<SpriteArtwork> PageCharacters);

/// <summary>
/// Verification access (#1392): Samus's spritemaps as character draws, which tests compare against native OAM. The
/// game draws them from pre-coloured sheets.
/// </summary>
internal static class SamusSpritemapDrawsAccess
{
    /// <summary>
    /// Ports <c>AddSamusSpritemapToOAM</c> at <c>$81:89AE</c> as one whole-frame draw: a decoded composition, at a screen
    /// origin, with the parts' authored palettes and priorities.
    /// </summary>
    /// <param name="frame">The decoded composition, or none for one that draws nothing.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="artwork">The spritemaps and the characters their parts name.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    internal static OamBuffer SamusSpritemapOam(
        SpriteFrame? frame,
        Position16<ScreenPixels> origin,
        SamusSpritemapArtwork artwork,
        SpriteSource source)
    {
        if (frame is not null)
        {
            // A spritemap that names a character beyond its own sheet reads the rest of the OBJ table there.
            SpriteArtwork characters = frame.LastCharacter < artwork.Characters.CharacterCount
                ? artwork.Characters : artwork.PageCharacters();
            return SpriteFrameBufferAccess.Assemble([new CharacterSpriteDraw(source, frame, characters, origin, PictureFlip.None, 0, 0)],
                parkUnused: false);
        }
        // A zero entry in the cartridge's pointer table dereferences $92:0000, direct-page scratch. Its parts would need
        // direct-page emulation the port does not model, so the import decodes no composition and it draws nothing (owner
        // decision on #1318).
        return SpriteFrameBufferAccess.Assemble([], parkUnused: false);
    }
}
