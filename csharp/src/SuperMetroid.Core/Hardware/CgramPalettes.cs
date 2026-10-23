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

    /// <summary>The 128 OBJ colours.</summary>
    public CgramPalette Objects => new(cgram, PpuMemoryLayout.ObjectPalettes);

    /// <summary>The backdrop (background palette 0, colour 0) as opaque RGBA.</summary>
    public Rgba32 BackdropRgba => All.Rgba(0);

    /// <summary>The complete 256-colour image, in CGRAM order.</summary>
    public ReadOnlySpan<Bgr555> Image => All.Colors;

    /// <summary>Replaces the complete image with <paramref name="colors"/> (exactly 256 colours).</summary>
    public void LoadImage(ReadOnlySpan<Bgr555> colors)
    {
        Ensure.LengthEqual(colors, ColorCount);
        All.CopyFrom(colors);
    }

    /// <summary>Background palette row <paramref name="row"/> (0-7).</summary>
    public CgramPalette BackgroundRow(int row) => Background.Slice(RowStart(row), PpuMemoryLayout.ColorsPerRow);

    /// <summary>OBJ palette row <paramref name="row"/> (0-7).</summary>
    public CgramPalette ObjectRow(int row) => Objects.Slice(RowStart(row), PpuMemoryLayout.ColorsPerRow);

    /// <summary>Palette row <paramref name="row"/> of the whole CGRAM (0-15): background rows 0-7, then OBJ rows 0-7.</summary>
    public CgramPalette Row(int row) => row < PpuMemoryLayout.PaletteRowCount
        ? BackgroundRow(row)
        : ObjectRow(row - PpuMemoryLayout.PaletteRowCount);

    /// <summary>The number of 16-colour rows in the whole CGRAM.</summary>
    public const int RowCount = 2 * PpuMemoryLayout.PaletteRowCount;

    /// <summary>Converts a bank-local row from zero through seven into its first color index.</summary>
    private static int RowStart(int row) => (uint)row < PpuMemoryLayout.PaletteRowCount
        ? row * PpuMemoryLayout.ColorsPerRow
        : throw new ArgumentOutOfRangeException(nameof(row), row, "A palette bank has eight rows.");
}
