using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.Core.Rendering;

/// <summary>Native fixed-color arithmetic for the title's BG1/backdrop and winning OBJ.</summary>
public static class TitleGradientColorMath
{
    /// <param name="pixel">Winning RGB8 pixel before final display brightness; channels are quantized to RGB5 for arithmetic, alpha is preserved, and fully transparent pixels are returned unchanged.</param>
    /// <param name="line">Current scanline's five-bit fixed-color components and raw CGADSUB control selecting layer participation and saturated addition/subtraction.</param>
    /// <param name="objectPalette">Null for BG1/backdrop; otherwise the winning OAM palette 0..7.</param>
    public static Rgba32 Apply(Rgba32 pixel, TitleGradientLine line, byte? objectPalette = null)
    {
        if (objectPalette is > 7) throw new ArgumentOutOfRangeException(nameof(objectPalette));
        bool enabled = objectPalette.HasValue
            ? objectPalette >= 4 && ((ColorMathControlFlags)line.Control).HasFlag(ColorMathControlFlags.Obj)
            : ((ColorMathControlFlags)line.Control).HasFlag(ColorMathControlFlags.Bg1);
        if (!enabled || pixel.A == 0) return pixel;
        bool subtract = ((ColorMathControlFlags)line.Control).HasFlag(ColorMathControlFlags.Subtract);
        byte Component(byte source, byte color)
        {
            int value = subtract ? Math.Max(0, (source >> 3) - color) : Math.Min(31, (source >> 3) + color);
            return (byte)((value << 3) | (value >> 2));
        }
        return new(Component(pixel.R, line.Red), Component(pixel.G, line.Green), Component(pixel.B, line.Blue), pixel.A);
    }
}
