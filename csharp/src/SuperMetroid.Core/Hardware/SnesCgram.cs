using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The SNES PPU's 256-entry Color Generator RAM (CGRAM), retained as native BGR555 words.
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

    /// <summary>Stored native BGR555 words, with the unused top bit cleared.</summary>
    /// <summary>Stored native BGR555 words, with the unused top bit cleared.</summary>
    private readonly ushort[] _colors = new ushort[ColorCount];

    /// <summary>Read-only native palette words for watches and verification.</summary>
    public ReadOnlySpan<ushort> Colors => _colors;

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
            ushort value = (ushort)(bytes[color * 2] | (bytes[color * 2 + 1] << 8));
            _colors[destinationIndex + color] = (ushort)(value & 0x7fff);
        }
    }

    /// <summary>Writes one native word; primarily useful for isolated PPU tests.</summary>
    public void SetColor(int index, ushort bgr555)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        // The PPU ignores bit 15. Masking it makes the stored model match CGRAM rather
        // than preserving a value that real hardware could never display.
        _colors[index] = (ushort)(bgr555 & 0x7fff);
    }

    /// <summary>Returns one palette word converted to ordinary 8-bit RGBA.</summary>
    public Rgba32 GetRgba(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        return SnesGraphics.DecodeBgr555Color(_colors[index]);
    }
}
