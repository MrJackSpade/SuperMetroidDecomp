namespace SuperMetroid.Core.Frontend;

/// <summary>Typed view of the frontend screens' direct palette writes (#630).</summary>
[PpuLayoutBoundary("rendering view: frontend screen palette writes")]
public readonly struct FrontendPalettes(IPaletteColors colours)
{
    /// <summary>The backdrop colour.</summary>
    public CgramPalette EndingBackdrop => new(colours, FrontendPaletteLayout.EndingBackdrop);

    /// <summary>BG palette 1 during the ending.</summary>
    public CgramPalette EndingLogoBackground => new(colours, FrontendPaletteLayout.EndingLogoBackground);

    /// <summary>OBJ colour 0 during the ending.</summary>
    public CgramPalette EndingObjectTransparent => new(colours, FrontendPaletteLayout.EndingObjectTransparent);

    /// <summary>OBJ palette 7 during the ending logo.</summary>
    public CgramPalette EndingLogoObjects => new(colours, FrontendPaletteLayout.EndingLogoObjects);

    /// <summary>The explosion's glow colour.</summary>
    public CgramPalette EndingExplosionGlow => new(colours, FrontendPaletteLayout.EndingExplosionGlow);

    /// <summary>The first colour file select's map entry holds black.</summary>
    public CgramPalette FileSelectEntryBlackFirst => new(colours, FrontendPaletteLayout.FileSelectEntryBlackFirst);

    /// <summary>The second colour file select's map entry holds black.</summary>
    public CgramPalette FileSelectEntryBlackSecond => new(colours, FrontendPaletteLayout.FileSelectEntryBlackSecond);

    /// <summary>Writes the title copyright glyph's white and red colours.</summary>
    public void WriteTitleCopyright(Bgr555 white, Bgr555 red)
    {
        var palette = new CgramPalette(colours, FrontendPaletteLayout.TitleCopyright);
        palette.Write(0, white);
        palette.Write(palette.Count - 1, red);
    }

    /// <summary>The rotating copy-arrow colours.</summary>
    public CgramPalette FileCopyArrow => new(colours, FrontendPaletteLayout.FileCopyArrow);

    /// <summary>Writes the intro Mother Brain's colours: white, or <paramref name="introPalette"/>'s (a whole CGRAM image) when not flashing.</summary>
    public void WriteIntroMotherBrain(ReadOnlySpan<Bgr555> introPalette, bool flashWhite)
    {
        var palette = new CgramPalette(colours, FrontendPaletteLayout.IntroMotherBrain);
        if (flashWhite)
            palette.Fill(Bgr555.White);
        else
            palette.CopyFrom(introPalette.Slice(FrontendPaletteLayout.IntroMotherBrain.Offset, palette.Count));
    }
}
