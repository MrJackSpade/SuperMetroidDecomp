using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Hardware-level palette views of a whole CGRAM (#630): the complete colour table, the
/// background and OBJ banks and their 16-colour rows. Domain code reaches its own named ranges
/// through its catalog's view instead.
/// </summary>
[PpuLayoutBoundary("rendering view: whole-CGRAM banks and rows")]
public readonly struct CgramPalettes(IPaletteColors cgram)
{
    /// <summary>Colours in a complete CGRAM image.</summary>
    public const int ColorCount = SnesPpuLayout.CgramColorCount;

    /// <summary>Bytes in a complete CGRAM image of little-endian colour words.</summary>
    [PpuLayoutAccess]
    public const int ByteCount = SnesPpuLayout.CgramByteCount;

    /// <summary>All 256 colours.</summary>
    [PpuLayoutAccess]
    public CgramPalette All => new(cgram, PpuMemoryLayout.Cgram);

    /// <summary>The 128 background colours.</summary>
    public CgramPalette Background => new(cgram, PpuMemoryLayout.BackgroundPalettes);

    /// <summary>The complete 256-colour image, in CGRAM order.</summary>
    public ReadOnlySpan<RgbColor> Image => All.Colors;

    /// <summary>Replaces the complete image with <paramref name="colors"/> (exactly 256 colours).</summary>
    public void LoadImage(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, ColorCount);
        All.CopyFrom(colors);
    }

    /// <summary>Background palette row <paramref name="row"/> (0-7).</summary>
    public CgramPalette BackgroundRow(int row) => Background.Slice(RowStart(row), PpuMemoryLayout.ColorsPerRow);

    /// <summary>Converts a bank-local row from zero through seven into its first color index.</summary>
    internal static int RowStart(int row) => (uint)row < PpuMemoryLayout.PaletteRowCount
        ? row * PpuMemoryLayout.ColorsPerRow
        : throw new ArgumentOutOfRangeException(nameof(row), row, "A palette bank has eight rows.");
}
