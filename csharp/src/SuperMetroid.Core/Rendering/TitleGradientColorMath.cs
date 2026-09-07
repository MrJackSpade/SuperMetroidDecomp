using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Rendering;

/// <summary>Native fixed-color arithmetic for the title's BG1/backdrop and winning OBJ.</summary>
public static class TitleGradientColorMath
{
    /// <param name="objectPalette">Null for BG1/backdrop; otherwise the winning OAM palette 0..7.</param>
    public static Rgba32 Apply(Rgba32 pixel, TitleGradientLine line, byte? objectPalette = null)
    {
        if (objectPalette is > 7) throw new ArgumentOutOfRangeException(nameof(objectPalette));
        bool enabled = objectPalette.HasValue
            ? objectPalette >= 4 && (line.Control & 0x10) != 0
            : (line.Control & 1) != 0;
        if (!enabled || pixel.A == 0) return pixel;
        bool subtract = (line.Control & 0x80) != 0;
        byte Component(byte source, byte color)
        {
            int value = subtract ? Math.Max(0, (source >> 3) - color) : Math.Min(31, (source >> 3) + color);
            return (byte)((value << 3) | (value >> 2));
        }
        return new(Component(pixel.R, line.Red), Component(pixel.G, line.Green), Component(pixel.B, line.Blue), pixel.A);
    }
}
