using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The memory image the cartridge's opening cinematic holds (#1404): the scene characters, the narration font with the
/// Japanese glyphs blanked once the first illustrated page begins, the scene background pages, the intro and cinematic
/// OBJ sheets, and the portrait and narration tilemaps, at the words `$8B:A469-$A529` upload them to. The cinematic keeps
/// no such image; verification composes it to compare against native VRAM.
/// </summary>
[PpuLayoutBoundary("exact hardware test: the intro's characters, background pages and tilemaps at their native words")]
internal static class IntroScreenMemory
{
    /// <summary>The character image of <paramref name="artwork"/> and <paramref name="font"/>, as `$8B:A469-$A529` upload them.</summary>
    internal static byte[] Characters(IntroCinematicArtworkCatalog artwork, ReadOnlySpan<byte> font, bool japaneseGlyphsBlanked)
    {
        IntroCinematicState.RequireFont(font);
        var vram = new SnesVram();
        var intro = new IntroVram(vram);
        intro.BackgroundCharacters.Load(artwork.BackgroundCharacters.Characters.Span[..IntroCinematicRomData.Transfers.BackgroundCharacterBytes]);
        intro.FontOne.Load(font);
        if (japaneseGlyphsBlanked)
        {
            // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times and DMA copies the $600 bytes
            // to VMADD $4180. BTS[$1E8E-$1E9D] aliases the resident font, so the repeated character is the font's own.
            ReadOnlySpan<byte> blank = font.Slice(IntroCinematicRomData.Text.JapaneseBlankSourceOffset,
                IntroCinematicRomDataTextConstants.JapaneseBlankCharacterByteCount);
            var blanked = new byte[IntroCinematicRomData.Transfers.JapaneseBlankCharactersByteCount];
            for (int offset = 0; offset < blanked.Length; offset += blank.Length)
                blank.CopyTo(blanked.AsSpan(offset));
            intro.JapaneseBlankCharacters.Load(blanked);
        }
        intro.BackgroundPages.Load(artwork.BackgroundPages.Span[..IntroCinematicRomData.Transfers.BackgroundPageTilemapBytes]);
        intro.IntroObjectCharacters.Load(artwork.IntroObjectCharacters.Characters.Span);
        intro.CinematicObjectCharacters.Load(artwork.CinematicObjectCharacters.Characters.Span[..IntroCinematicRomData.Transfers.ObjectCharacterBytes]);
        return vram.CaptureImage();
    }

    /// <summary>The memory image <paramref name="intro"/> shows: its bound characters with its portrait and narration tilemaps over them.</summary>
    internal static byte[] Compose(IntroCinematicState intro)
    {
        IntroCinematicArtworkCatalog artwork = intro.CharacterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic rendering requires installed artwork.");
        IntroFontAtlas font = intro.IntroFont ?? throw new InvalidOperationException(
            "Opening cinematic rendering requires the installed font atlas.");
        return Compose(Characters(artwork, font.Characters.Span, intro.JapaneseGlyphsBlanked), intro.tilemaps);
    }

    /// <summary>The memory image of <paramref name="characters"/> with <paramref name="tilemaps"/> over them.</summary>
    internal static byte[] Compose(ReadOnlySpan<byte> characters, IntroTilemaps tilemaps)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, characters);
        var view = new IntroVram(vram);
        view.PortraitTilemap.Write(tilemaps.Portrait);
        view.NarrationTilemap.Write(tilemaps.Narration);
        return vram.CaptureImage();
    }
}
