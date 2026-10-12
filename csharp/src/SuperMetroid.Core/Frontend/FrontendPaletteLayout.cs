using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Single CGRAM colours and palette rows the frontend screens write directly (#630).</summary>
[PpuLayoutBoundary("catalog: frontend screen palette writes")]
public static class FrontendPaletteLayout
{
    /// <summary>CGADD $00: the backdrop, which the ending's explosion flash turns white and its planet escape clears.</summary>
    public static readonly PpuLayoutCell EndingBackdrop = PpuMemoryLayout.Colors(nameof(EndingBackdrop), 0x00, 1, "Bgr555 backdrop");

    /// <summary>CGADD $10-$1F: BG palette 1, the ending logo's first crossfade palette and the explosion flash's white row.</summary>
    public static readonly PpuLayoutCell EndingLogoBackground = PpuMemoryLayout.Colors(nameof(EndingLogoBackground), 0x10, 16,
        "Bgr555 BG palette 1");

    /// <summary>CGADD $80: OBJ palette 0's transparent slot, written white by the explosion flash and black by the planet escape.</summary>
    public static readonly PpuLayoutCell EndingObjectTransparent = PpuMemoryLayout.Colors(nameof(EndingObjectTransparent), 0x80, 1,
        "Bgr555 OBJ colour 0");

    /// <summary>CGADD $F0-$FF: OBJ palette 7, the ending logo's second crossfade palette.</summary>
    public static readonly PpuLayoutCell EndingLogoObjects = PpuMemoryLayout.Colors(nameof(EndingLogoObjects), 0xf0, 16,
        "Bgr555 OBJ palette 7");

    /// <summary>CGADD $FE: the dark-red glow colour instruction <c>FadeExplosionPalette</c> installs.</summary>
    public static readonly PpuLayoutCell EndingExplosionGlow = PpuMemoryLayout.Colors(nameof(EndingExplosionGlow), 0xfe, 1,
        "Bgr555 OBJ palette 7 colour 14");

    /// <summary>CGADD $0E: BG palette 0 colour 14, held black through the file-select map entry fade.</summary>
    public static readonly PpuLayoutCell FileSelectEntryBlackFirst = PpuMemoryLayout.Colors(nameof(FileSelectEntryBlackFirst), 0x0e, 1,
        "Bgr555 BG palette 0 colour 14");

    /// <summary>CGADD $1E: BG palette 1 colour 14, held black through the file-select map entry fade.</summary>
    public static readonly PpuLayoutCell FileSelectEntryBlackSecond = PpuMemoryLayout.Colors(nameof(FileSelectEntryBlackSecond), 0x1e, 1,
        "Bgr555 BG palette 1 colour 14");

    /// <summary>CGADD $91-$97: the seven copy-arrow colours file select rotates every four frames.</summary>
    public static readonly PpuLayoutCell FileCopyArrow = PpuMemoryLayout.Colors(nameof(FileCopyArrow), FileCopyArrowDefinitions.FirstColor,
        FileCopyArrowDefinitions.ColorCount, "Bgr555 rotating arrow colours");

    /// <summary>
    /// CGADD $C9-$CA: OBJ palette 4 colours 9 and 10, the white and red glyph colours of the Nintendo
    /// copyright spritemap that skip transition <c>$8B:9A9C</c> overwrites.
    /// </summary>
    public static readonly PpuLayoutCell TitleCopyright = PpuMemoryLayout.Colors(nameof(TitleCopyright),
        TitleSequenceRomData.Palette.CopyrightWhiteIndex,
        TitleSequenceRomData.Palette.CopyrightRedIndex - TitleSequenceRomData.Palette.CopyrightWhiteIndex + 1, "Bgr555 white, red");

    /// <summary>CGADD $F0-$FF: OBJ palette 7, the intro Mother Brain's colours that her hurt flash whitens.</summary>
    public static readonly PpuLayoutCell IntroMotherBrain = PpuMemoryLayout.Colors(nameof(IntroMotherBrain), 0xf0, 16,
        "Bgr555 OBJ palette 7");
}

/// <summary>Typed view of the frontend screens' direct palette writes (#630).</summary>
[PpuLayoutBoundary("rendering view: frontend screen palette writes")]
public readonly struct FrontendPalettes(SnesCgram cgram)
{
    /// <summary>The backdrop colour.</summary>
    public CgramPalette EndingBackdrop => new(cgram, FrontendPaletteLayout.EndingBackdrop);

    /// <summary>BG palette 1 during the ending.</summary>
    public CgramPalette EndingLogoBackground => new(cgram, FrontendPaletteLayout.EndingLogoBackground);

    /// <summary>OBJ colour 0 during the ending.</summary>
    public CgramPalette EndingObjectTransparent => new(cgram, FrontendPaletteLayout.EndingObjectTransparent);

    /// <summary>OBJ palette 7 during the ending logo.</summary>
    public CgramPalette EndingLogoObjects => new(cgram, FrontendPaletteLayout.EndingLogoObjects);

    /// <summary>The explosion's glow colour.</summary>
    public CgramPalette EndingExplosionGlow => new(cgram, FrontendPaletteLayout.EndingExplosionGlow);

    /// <summary>The first colour file select's map entry holds black.</summary>
    public CgramPalette FileSelectEntryBlackFirst => new(cgram, FrontendPaletteLayout.FileSelectEntryBlackFirst);

    /// <summary>The second colour file select's map entry holds black.</summary>
    public CgramPalette FileSelectEntryBlackSecond => new(cgram, FrontendPaletteLayout.FileSelectEntryBlackSecond);

    /// <summary>Writes the title copyright glyph's white and red colours.</summary>
    public void WriteTitleCopyright(Bgr555 white, Bgr555 red)
    {
        var palette = new CgramPalette(cgram, FrontendPaletteLayout.TitleCopyright);
        palette.Write(0, white);
        palette.Write(palette.Count - 1, red);
    }

    /// <summary>The rotating copy-arrow colours.</summary>
    public CgramPalette FileCopyArrow => new(cgram, FrontendPaletteLayout.FileCopyArrow);

    /// <summary>Writes the intro Mother Brain's colours: white, or <paramref name="introPalette"/>'s (a whole CGRAM image) when not flashing.</summary>
    public void WriteIntroMotherBrain(ReadOnlySpan<Bgr555> introPalette, bool flashWhite)
    {
        var palette = new CgramPalette(cgram, FrontendPaletteLayout.IntroMotherBrain);
        if (flashWhite)
            palette.Fill(Bgr555.White);
        else
            palette.CopyFrom(introPalette.Slice(FrontendPaletteLayout.IntroMotherBrain.Offset, palette.Count));
    }
}
