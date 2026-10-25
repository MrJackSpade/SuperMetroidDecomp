namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="PpuMemoryLayout"/> that only verification reads.</summary>
internal static class PpuMemoryLayoutConstants
{
    /// <summary>All 128 OAM low-table records ($2104 OAMDATA, OAMADD $000-$0FF words).</summary>
    public static readonly PpuLayoutCell OamLow = new(nameof(OamLow), PpuLayoutRegion.OamLow, 0, SnesPpuLayout.OamSpriteCount,
        "OBJ records (X low, Y, SnesObjAttributeWord)");
    /// <summary>All 128 OAM high-table pairs (OAMADD $100-$10F words): X bit 8 and the large-size bit.</summary>
    public static readonly PpuLayoutCell OamHigh = new(nameof(OamHigh), PpuLayoutRegion.OamHigh, 0, SnesPpuLayout.OamSpriteCount,
        "SnesOamHighTablePair (2 bits per OBJ)");
    /// <summary>CGADD $80-$FF: the eight 16-colour OBJ palettes.</summary>
    public static readonly PpuLayoutCell ObjectPalettes = new(nameof(ObjectPalettes), PpuLayoutRegion.Cgram,
        PpuMemoryLayout.PaletteRowCount * PpuMemoryLayout.ColorsPerRow, PpuMemoryLayout.PaletteRowCount * PpuMemoryLayout.ColorsPerRow, "RgbColor OBJ palettes", PpuMemoryLayout.Cgram);
    /// <summary>
    /// The eight background palette rows at CGADD $00-$7F, indexed by palette: row N is colours N*16 through N*16+15, and
    /// colour 0 of row 0 is the backdrop.
    /// </summary>
    public static readonly PpuLayoutCell[] BackgroundPaletteRows =
        [.. Enumerable.Range(0, PpuMemoryLayout.PaletteRowCount).Select(row => Row($"{nameof(BackgroundPaletteRows)}[{row}]", row, PpuMemoryLayout.BackgroundPalettes))];
    /// <summary>The eight OBJ palette rows at CGADD $80-$FF, indexed by palette: row N follows the background rows.</summary>
    public static readonly PpuLayoutCell[] ObjectPaletteRows =
        [.. Enumerable.Range(0, PpuMemoryLayout.PaletteRowCount).Select(row => Row($"{nameof(ObjectPaletteRows)}[{row}]", PpuMemoryLayout.PaletteRowCount + row, ObjectPalettes))];
    /// <summary>
    /// Declares a domain CGRAM range starting at colour <paramref name="first"/>. Its alias is the
    /// smallest hardware container that covers it: a palette row, the background or OBJ bank, or
    /// all of CGRAM.
    /// </summary>
    public static PpuLayoutCell Colors(string name, int first, int count, string encoding)
    {
        PpuLayoutCell container = BackgroundPaletteRows.Concat(ObjectPaletteRows).FirstOrDefault(row => Covers(row, first, count))
            ?? (Covers(PpuMemoryLayout.BackgroundPalettes, first, count) ? PpuMemoryLayout.BackgroundPalettes
                : Covers(ObjectPalettes, first, count) ? ObjectPalettes
                : PpuMemoryLayout.Cgram);
        return new PpuLayoutCell(name, PpuLayoutRegion.Cgram, first, count, encoding, container);
    }
    /// <summary>Creates a sixteen-color CGRAM row nested under its background or OBJ bank.</summary>
    internal static PpuLayoutCell Row(string name, int row, PpuLayoutCell palettes) =>
        new(name, PpuLayoutRegion.Cgram, row * PpuMemoryLayout.ColorsPerRow, PpuMemoryLayout.ColorsPerRow, "RgbColor 4-bpp palette row", palettes);
    /// <summary>Checks whether the requested color interval fits entirely inside a hardware container.</summary>
    internal static bool Covers(PpuLayoutCell outer, int first, int count) =>
        outer.Offset <= first && first + count <= outer.Offset + outer.Count;
}
