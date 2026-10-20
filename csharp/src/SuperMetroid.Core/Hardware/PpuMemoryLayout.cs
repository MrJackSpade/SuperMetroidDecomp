namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The PPU's physical stores and CGRAM's hardware palette rows (#630). Every domain layout cell
/// lies inside one of these and declares, through <see cref="PpuLayoutCell.AliasOf"/>, the
/// hardware container whose storage it shares: a screen's VRAM allocation aliases
/// <see cref="Vram"/>, and a palette range aliases the row, bank or whole CGRAM that covers it.
/// </summary>
[PpuLayoutBoundary("catalog: physical PPU stores and CGRAM palette rows")]
public static class PpuMemoryLayout
{
    /// <summary>All 32,768 VRAM words (VMADD $0000-$7FFF); every screen mode reuses this store.</summary>
    public static readonly PpuLayoutCell Vram = new(nameof(Vram), PpuLayoutRegion.Vram, 0, SnesPpuLayout.VramWordCount,
        "VRAM words (u16 little-endian)");

    /// <summary>All 256 CGRAM colours (CGADD $00-$FF).</summary>
    public static readonly PpuLayoutCell Cgram = new(nameof(Cgram), PpuLayoutRegion.Cgram, 0, SnesPpuLayout.CgramColorCount,
        "Bgr555 colours");

    /// <summary>CGADD $00-$7F: the eight 16-colour background palettes (BG3 uses the first 32 as eight 4-colour palettes).</summary>
    public static readonly PpuLayoutCell BackgroundPalettes = new(nameof(BackgroundPalettes), PpuLayoutRegion.Cgram,
        0, PaletteRowCount * ColorsPerRow, "Bgr555 background palettes", Cgram);

    /// <summary>CGADD $80-$FF: the eight 16-colour OBJ palettes.</summary>
    public static readonly PpuLayoutCell ObjectPalettes = new(nameof(ObjectPalettes), PpuLayoutRegion.Cgram,
        PaletteRowCount * ColorsPerRow, PaletteRowCount * ColorsPerRow, "Bgr555 OBJ palettes", Cgram);

    /// <summary>Colours in one hardware 4-bpp palette row.</summary>
    public const int ColorsPerRow = 16;

    /// <summary>Hardware palette rows in each of the background and OBJ banks.</summary>
    public const int PaletteRowCount = 8;

    /// <summary>CGADD $00-$0F: background palette 0; colour 0 is the backdrop.</summary>
    public static readonly PpuLayoutCell BgPalette0 = Row(nameof(BgPalette0), 0, BackgroundPalettes);
    /// <summary>CGADD $10-$1F: background palette 1.</summary>
    public static readonly PpuLayoutCell BgPalette1 = Row(nameof(BgPalette1), 1, BackgroundPalettes);
    /// <summary>CGADD $20-$2F: background palette 2.</summary>
    public static readonly PpuLayoutCell BgPalette2 = Row(nameof(BgPalette2), 2, BackgroundPalettes);
    /// <summary>CGADD $30-$3F: background palette 3.</summary>
    public static readonly PpuLayoutCell BgPalette3 = Row(nameof(BgPalette3), 3, BackgroundPalettes);
    /// <summary>CGADD $40-$4F: background palette 4.</summary>
    public static readonly PpuLayoutCell BgPalette4 = Row(nameof(BgPalette4), 4, BackgroundPalettes);
    /// <summary>CGADD $50-$5F: background palette 5.</summary>
    public static readonly PpuLayoutCell BgPalette5 = Row(nameof(BgPalette5), 5, BackgroundPalettes);
    /// <summary>CGADD $60-$6F: background palette 6.</summary>
    public static readonly PpuLayoutCell BgPalette6 = Row(nameof(BgPalette6), 6, BackgroundPalettes);
    /// <summary>CGADD $70-$7F: background palette 7.</summary>
    public static readonly PpuLayoutCell BgPalette7 = Row(nameof(BgPalette7), 7, BackgroundPalettes);
    /// <summary>CGADD $80-$8F: OBJ palette 0.</summary>
    public static readonly PpuLayoutCell ObjPalette0 = Row(nameof(ObjPalette0), 8, ObjectPalettes);
    /// <summary>CGADD $90-$9F: OBJ palette 1.</summary>
    public static readonly PpuLayoutCell ObjPalette1 = Row(nameof(ObjPalette1), 9, ObjectPalettes);
    /// <summary>CGADD $A0-$AF: OBJ palette 2.</summary>
    public static readonly PpuLayoutCell ObjPalette2 = Row(nameof(ObjPalette2), 10, ObjectPalettes);
    /// <summary>CGADD $B0-$BF: OBJ palette 3.</summary>
    public static readonly PpuLayoutCell ObjPalette3 = Row(nameof(ObjPalette3), 11, ObjectPalettes);
    /// <summary>CGADD $C0-$CF: OBJ palette 4.</summary>
    public static readonly PpuLayoutCell ObjPalette4 = Row(nameof(ObjPalette4), 12, ObjectPalettes);
    /// <summary>CGADD $D0-$DF: OBJ palette 5.</summary>
    public static readonly PpuLayoutCell ObjPalette5 = Row(nameof(ObjPalette5), 13, ObjectPalettes);
    /// <summary>CGADD $E0-$EF: OBJ palette 6.</summary>
    public static readonly PpuLayoutCell ObjPalette6 = Row(nameof(ObjPalette6), 14, ObjectPalettes);
    /// <summary>CGADD $F0-$FF: OBJ palette 7.</summary>
    public static readonly PpuLayoutCell ObjPalette7 = Row(nameof(ObjPalette7), 15, ObjectPalettes);

    /// <summary>Palette rows in CGRAM order for choosing the narrowest alias container.</summary>
    private static readonly PpuLayoutCell[] Rows =
    [
        BgPalette0, BgPalette1, BgPalette2, BgPalette3, BgPalette4, BgPalette5, BgPalette6, BgPalette7,
        ObjPalette0, ObjPalette1, ObjPalette2, ObjPalette3, ObjPalette4, ObjPalette5, ObjPalette6, ObjPalette7,
    ];

    /// <summary>
    /// Declares a domain CGRAM range starting at colour <paramref name="first"/>. Its alias is the
    /// smallest hardware container that covers it: a palette row, the background or OBJ bank, or
    /// all of CGRAM.
    /// </summary>
    public static PpuLayoutCell Colors(string name, int first, int count, string encoding)
    {
        PpuLayoutCell container = Rows.FirstOrDefault(row => Covers(row, first, count))
            ?? (Covers(BackgroundPalettes, first, count) ? BackgroundPalettes
                : Covers(ObjectPalettes, first, count) ? ObjectPalettes
                : Cgram);
        return new PpuLayoutCell(name, PpuLayoutRegion.Cgram, first, count, encoding, container);
    }

    /// <summary>Creates a sixteen-color CGRAM row nested under its background or OBJ bank.</summary>
    private static PpuLayoutCell Row(string name, int row, PpuLayoutCell bank) =>
        new(name, PpuLayoutRegion.Cgram, row * ColorsPerRow, ColorsPerRow, "Bgr555 4-bpp palette row", bank);

    /// <summary>Checks whether the requested color interval fits entirely inside a hardware container.</summary>
    private static bool Covers(PpuLayoutCell outer, int first, int count) =>
        outer.Offset <= first && first + count <= outer.Offset + outer.Count;
}
