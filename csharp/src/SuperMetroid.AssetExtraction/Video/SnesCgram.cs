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
public sealed class SnesCgram : IPaletteColors
{
    /// <summary>The number of native palette words held by CGRAM.</summary>
    public const int ColorCount = 256;

    /// <summary>Native 15-bit palette words retained until rendering or inspection converts them.</summary>
    private readonly RgbColor[] _colors = new RgbColor[ColorCount];

    /// <summary>Read-only palette colors for watches and verification.</summary>
    public ReadOnlySpan<RgbColor> Colors => _colors;

    /// <summary>Writes one color.</summary>
    public void SetColor(int index, RgbColor color)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        _colors[index] = color;
    }
}
