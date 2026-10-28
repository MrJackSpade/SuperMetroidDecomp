using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The SNES PPU's 256-entry Color Generator RAM (CGRAM), retained as native <see cref="RgbColor"/> colors.
/// </summary>
/// <remarks>
/// Keeping the hardware words instead of prematurely converting them is useful while
/// debugging palette fades: the game mutates 15-bit target/current palette buffers and
/// NMI later uploads their exact 512-byte image to CGRAM.
/// </remarks>
[PpuLayoutAccess]
public sealed class SnesCgram : IPaletteColors
{
    /// <summary>The number of native palette words held by CGRAM.</summary>
    public const int ColorCount = 256;

    /// <summary>Native 15-bit palette words retained until rendering or inspection converts them.</summary>
    private readonly RgbColor[] _colors = new RgbColor[ColorCount];

    /// <summary>Read-only palette colors for watches and verification.</summary>
    public ReadOnlySpan<RgbColor> Colors => _colors;

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
            _colors[destinationIndex + color] = RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1]));
        }
    }

    /// <summary>Writes one color.</summary>
    public void SetColor(int index, RgbColor color)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        _colors[index] = color;
    }
}
