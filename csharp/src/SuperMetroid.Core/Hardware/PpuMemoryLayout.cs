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

    /// <summary>
    /// The eight background palette rows at CGADD $00-$7F, indexed by palette: row N is colours N*16 through N*16+15, and
    /// colour 0 of row 0 is the backdrop.
    /// </summary>
    public static readonly PpuLayoutCell[] BackgroundPaletteRows =
        [.. Enumerable.Range(0, PaletteRowCount).Select(row => Row($"{nameof(BackgroundPaletteRows)}[{row}]", row, BackgroundPalettes))];

    /// <summary>The eight OBJ palette rows at CGADD $80-$FF, indexed by palette: row N follows the background rows.</summary>
    public static readonly PpuLayoutCell[] ObjectPaletteRows =
        [.. Enumerable.Range(0, PaletteRowCount).Select(row => Row($"{nameof(ObjectPaletteRows)}[{row}]", PaletteRowCount + row, ObjectPalettes))];

    /// <summary>
    /// Declares a domain CGRAM range starting at colour <paramref name="first"/>. Its alias is the
    /// smallest hardware container that covers it: a palette row, the background or OBJ bank, or
    /// all of CGRAM.
    /// </summary>
    public static PpuLayoutCell Colors(string name, int first, int count, string encoding)
    {
        PpuLayoutCell container = BackgroundPaletteRows.Concat(ObjectPaletteRows).FirstOrDefault(row => Covers(row, first, count))
            ?? (Covers(BackgroundPalettes, first, count) ? BackgroundPalettes
                : Covers(ObjectPalettes, first, count) ? ObjectPalettes
                : Cgram);
        return new PpuLayoutCell(name, PpuLayoutRegion.Cgram, first, count, encoding, container);
    }

    /// <summary>Creates a sixteen-color CGRAM row nested under its background or OBJ bank.</summary>
    private static PpuLayoutCell Row(string name, int row, PpuLayoutCell palettes) =>
        new(name, PpuLayoutRegion.Cgram, row * ColorsPerRow, ColorsPerRow, "Bgr555 4-bpp palette row", palettes);

    /// <summary>Checks whether the requested color interval fits entirely inside a hardware container.</summary>
    private static bool Covers(PpuLayoutCell outer, int first, int count) =>
        outer.Offset <= first && first + count <= outer.Offset + outer.Count;
}
