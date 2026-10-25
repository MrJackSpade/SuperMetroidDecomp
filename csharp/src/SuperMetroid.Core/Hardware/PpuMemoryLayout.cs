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
        "RgbColor colours");

    /// <summary>CGADD $00-$7F: the eight 16-colour background palettes (BG3 uses the first 32 as eight 4-colour palettes).</summary>
    public static readonly PpuLayoutCell BackgroundPalettes = new(nameof(BackgroundPalettes), PpuLayoutRegion.Cgram,
        0, PaletteRowCount * ColorsPerRow, "RgbColor background palettes", Cgram);

    /// <summary>Colours in one hardware 4-bpp palette row.</summary>
    public const int ColorsPerRow = 16;

    /// <summary>Hardware palette rows in each of the background and OBJ banks.</summary>
    public const int PaletteRowCount = 8;
}
