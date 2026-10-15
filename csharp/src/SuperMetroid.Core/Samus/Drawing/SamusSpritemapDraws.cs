using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

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
    /// <param name="memory">Mutable memory for the native zero-pointer WRAM spritemap.</param>
    /// <param name="spritemapIndex">Word index into <c>$92:808D</c>.</param>
    /// <param name="origin">Screen-space origin produced by bank $90.</param>
    /// <param name="artwork">The spritemaps and the characters their parts name.</param>
    /// <param name="source">The Samus element the draw shows.</param>
    public static void AddSamusSpritemap(
        this SpriteFrameBuffer sprites,
        ISnesMutableMemory memory,
        ushort spritemapIndex,
        Position16<ScreenPixels> origin,
        SamusSpritemapArtwork artwork,
        SpriteSource source)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(memory);
        if (artwork.Spritemaps.TryGetFrame(spritemapIndex, out SpriteFrame? frame))
        {
            // A spritemap that names a character beyond its own sheet reads the rest of the OBJ table there.
            SpriteArtwork characters = frame.LastCharacter < artwork.Characters.CharacterCount
                ? artwork.Characters : artwork.PageCharacters();
            sprites.Add(new SpriteDraw(source, frame, characters, origin, SnesTileFlipFlags.None, 0, 0));
            return;
        }
        // A zero entry in the installed pointer table is not an empty picture: the native bank-local dereference
        // lands in the WRAM mirror at $92:0000. Read that mutable state explicitly (#1318 owns where it comes
        // from); its parts may name any OBJ character, so they bind the full page.
        SpriteFramePart[] parts = [.. SamusOverreadMemory.ZeroPointerSpritemap(memory).Select(entry =>
            ObjPartFrames.Part(entry.EncodedX, entry.EncodedY, new SnesObjAttributeWord(entry.Attributes), 0))];
        sprites.Add(new SpriteDraw(source, new SpriteFrame(WramSpritemapName, [.. parts]), artwork.PageCharacters(),
            origin, SnesTileFlipFlags.None, 0, 0));
    }

    /// <summary>Frame name of the zero-pointer spritemap read from WRAM.</summary>
    public const string WramSpritemapName = "Samus.WramSpritemap";
}
