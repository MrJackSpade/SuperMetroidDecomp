using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The SNES PPU's 256-entry Color Generator RAM (CGRAM), retained as native <see cref="Bgr555"/> colors.
/// </summary>
/// <remarks>
/// Keeping the hardware words instead of prematurely converting them is useful while
/// debugging palette fades: the game mutates 15-bit target/current palette buffers and
/// NMI later uploads their exact 512-byte image to CGRAM.
/// </remarks>
public sealed class SnesCgram
{
    /// <summary>The number of native palette words held by CGRAM.</summary>
    public const int ColorCount = SnesPpuLayout.CgramColorCount;
    /// <summary>The size in bytes of CGRAM's complete two-byte color image.</summary>
    public const int ByteCount = SnesPpuLayout.CgramByteCount;

    private readonly Bgr555[] _colors = new Bgr555[ColorCount];

    /// <summary>Read-only palette colors for watches and verification.</summary>
    public ReadOnlySpan<Bgr555> Colors => _colors;

    /// <summary>Loads consecutive little-endian palette bytes already decoded in host memory.</summary>
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
    {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException("CGRAM data must contain complete two-byte colors.", nameof(bytes));
        int colorCount = bytes.Length / 2;
        if (destinationIndex < 0 || destinationIndex + colorCount > ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex), "CGRAM load must remain within 256 colors.");

        for (int color = 0; color < colorCount; color++)
        {
            ushort word = (ushort)(bytes[color * 2] | (bytes[color * 2 + 1] << 8));
            _colors[destinationIndex + color] = Bgr555.FromCgramPortWord(word);
        }
    }

    /// <summary>Writes one color.</summary>
    public void SetColor(int index, Bgr555 color)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        _colors[index] = color;
    }

    /// <summary>Returns one palette word converted to ordinary 8-bit RGBA.</summary>
    public Rgba32 GetRgba(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _colors[index].ToRgba32();
    }
}
