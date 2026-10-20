using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Composes the intro's bound artwork, font and tilemaps into the memory image the backends draw (#1358): the scene
/// characters, the narration font with the Japanese glyphs blanked once the first illustrated page begins, the scene
/// background pages, the intro and cinematic OBJ sheets, and the portrait and narration tilemaps.
/// </summary>
[PpuLayoutBoundary("codec: the intro's characters, background pages and tilemaps at their native words")]
internal static class IntroScreenImage
{
    /// <summary>Rejects a font atlas that is not the native size.</summary>
    internal static void RequireFont(ReadOnlySpan<byte> font)
    {
        if (font.Length != IntroFontAtlasFormat.ByteCount)
            throw new InvalidDataException($"Opening font contains {font.Length} bytes; expected {IntroFontAtlasFormat.ByteCount}.");
    }

    /// <summary>The character image of <paramref name="artwork"/> and <paramref name="font"/>, as `$8B:A469-$A529` upload them.</summary>
    internal static byte[] Characters(IntroCinematicArtworkCatalog artwork, ReadOnlySpan<byte> font, bool japaneseGlyphsBlanked)
    {
        RequireFont(font);
        var vram = new SnesVram();
        var intro = new IntroVram(vram);
        intro.BackgroundCharacters.Load(artwork.BackgroundCharacters.Transfer.Span[..IntroCinematicRomData.Vram.BackgroundCharacterBytes]);
        intro.FontOne.Load(font);
        if (japaneseGlyphsBlanked)
        {
            // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times and DMA copies the $600 bytes
            // to VMADD $4180. BTS[$1E8E-$1E9D] aliases the resident font, so the repeated character is the font's own.
            ReadOnlySpan<byte> blank = font.Slice(IntroCinematicRomData.Text.JapaneseBlankSourceOffset,
                IntroCinematicRomData.Text.JapaneseBlankCharacterByteCount);
            var blanked = new byte[IntroCinematicRomData.Vram.JapaneseBlankCharactersByteCount];
            for (int offset = 0; offset < blanked.Length; offset += blank.Length)
                blank.CopyTo(blanked.AsSpan(offset));
            intro.JapaneseBlankCharacters.Load(blanked);
        }
        intro.BackgroundPages.Load(artwork.BackgroundPages.Span[..IntroCinematicRomData.Vram.BackgroundPageTilemapBytes]);
        intro.IntroObjectCharacters.Load(artwork.IntroObjectCharacters.Transfer.Span);
        intro.CinematicObjectCharacters.Load(artwork.CinematicObjectCharacters.Transfer.Span[..IntroCinematicRomData.Vram.ObjectCharacterBytes]);
        return vram.CaptureImage();
    }

    /// <summary>The memory image of <paramref name="characters"/> with <paramref name="tilemaps"/> over them.</summary>
    internal static byte[] Compose(ReadOnlySpan<byte> characters, IntroTilemaps tilemaps)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, characters);
        var intro = new IntroVram(vram);
        intro.PortraitTilemap.Write(tilemaps.Portrait);
        intro.NarrationTilemap.Write(tilemaps.Narration);
        return vram.CaptureImage();
    }
}
