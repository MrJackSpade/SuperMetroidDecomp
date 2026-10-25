using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// A frontend screen's own colours addressed by palette position (#1422), so the import pipeline's palette views and
/// its decoders of stored colour words can fill a screen the way they fill an import scratch CGRAM.
/// </summary>
/// <param name="screen">The screen's colours.</param>
internal sealed class ScreenPaletteColors(ScreenPalette screen) : IPaletteColors
{
    /// <inheritdoc />
    public ReadOnlySpan<RgbColor> Colors => screen.Colors;

    /// <inheritdoc />
    public void SetColor(int index, RgbColor color) => screen.All.Write(index, color);

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
    {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
        int count = bytes.Length / RgbColor.ByteCount;
        Ensure.BetweenInclusive(0, ScreenPalette.ColorCount - count, destinationIndex);
        for (int color = 0; color < count; color++)
            screen.All.Write(destinationIndex + color,
                RgbColor.FromWordIgnoringTopBit(SnesLittleEndian.Word(bytes[color * 2], bytes[color * 2 + 1])));
    }
}
