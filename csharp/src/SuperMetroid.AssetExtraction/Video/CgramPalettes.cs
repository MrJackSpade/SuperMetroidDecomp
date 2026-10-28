using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Hardware-level palette views of a whole CGRAM (#630): the complete colour table, the
/// background and OBJ banks and their 16-colour rows. Domain code reaches its own named ranges
/// through its catalog's view instead.
/// </summary>
public readonly struct CgramPalettes(IPaletteColors cgram)
{
    /// <summary>Views a frontend screen's own colours, which it addresses by palette position.</summary>
    /// <param name="screen">The screen's colours.</param>
    public CgramPalettes(ScreenPalette screen)
        : this(new ScreenPaletteColors(Ensure.NotNull(screen)))
    {
    }

    /// <summary>Colours in a complete CGRAM image.</summary>
    public const int ColorCount = SnesCgram.ColorCount;

    /// <summary>Bytes in a complete CGRAM image of little-endian colour words.</summary>
    public const int ByteCount = ColorCount * RgbColor.ByteCount;

    /// <summary>All 256 colours.</summary>
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
