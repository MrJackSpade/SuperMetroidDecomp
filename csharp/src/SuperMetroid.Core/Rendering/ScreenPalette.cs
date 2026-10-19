namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's own 256 colours (#1358): what its backgrounds and sprites show, written through the same named
/// palette views gameplay uses and published with the screen's frame instead of living in PPU memory.
/// </summary>
public sealed class ScreenPalette : IPaletteColors
{
    /// <summary>The number of colours a screen palette holds.</summary>
    public const int ColorCount = 256;

    /// <summary>The colours, in palette order.</summary>
    private readonly Bgr555[] colors = new Bgr555[ColorCount];

    /// <inheritdoc />
    public ReadOnlySpan<Bgr555> Colors => colors;

    /// <inheritdoc />
    public void SetColor(int index, Bgr555 color)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        colors[index] = color;
    }

    /// <inheritdoc />
    public Rgba32 GetRgba(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index].ToRgba32();
    }

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
    {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
        int count = bytes.Length / 2;
        if (destinationIndex < 0 || destinationIndex + count > ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex), "A palette load must remain within 256 colours.");
        for (int color = 0; color < count; color++)
            colors[destinationIndex + color] = Bgr555.FromCgramPortWord(SnesLittleEndian.Word(bytes[color * 2], bytes[color * 2 + 1]));
    }

}
