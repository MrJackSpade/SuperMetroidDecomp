namespace SuperMetroid.Core.Assets;

/// <summary>$A9:9534-954F exploded-door opaque inks1..14, copied by B275-B27E.
/// Reviewed categorical hue/intensity and gamma composition; mechanics, timing and pixels are excluded.</summary>
internal static class MotherBrainExplodedDoorPaintDefinitions
{
    /// <summary>$A9:9534: first ramp's green/blue hue at maximum red.</summary>
    private const int VioletGreen = 8, VioletBlue = 19;
    /// <summary>$A9:953A: darkest first-ramp red; gamma2 interpolation derives both middle intensities.</summary>
    private const int VioletDarkRed = 8;
    /// <summary>$A9:953C/9542: yellow ramp's light blue and dark red/green; other endpoint channels are max/zero.</summary>
    private const int YellowLightBlue = 17, YellowDarkRed = 18, YellowDarkGreen = 9;
    /// <summary>$A9:9534-9542: each chromatic material has four shades including endpoints.</summary>
    private const int ShadeIntervals = 3;
    /// <summary>$A9:B27B: fourteen copied opaque colors, excluding transparent3800 and final black.</summary>
    internal const int ColorCount = 14;

    /// <summary>Determines whether the supplied opaque inks match the computed stock exploded-door ramp.</summary>
    /// <param name="colors">Palette entries whose first <see cref="ColorCount"/> values are compared in order.</param>
    /// <returns><see langword="true"/> if all fourteen opaque inks match; otherwise, <see langword="false"/>.</returns>
    internal static bool Matches(ReadOnlySpan<ushort> colors)
    {
        for (int color = 0; color < ColorCount; color++)
            if (colors[color] != Color(color)) return false;
        return true;
    }

    /// <summary>Computes one of the fourteen opaque BGR555 inks used by the exploded Mother Brain door.</summary>
    /// <param name="color">Zero-based opaque-ink index, from the violet and yellow ramps through metal shades and white.</param>
    /// <returns>The packed SNES color word for the selected ink.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the opaque-ink range.</exception>
    internal static ushort Color(int color)
    {
        if ((uint)color >= ColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        if (color < 4)
        {
            double intensity = (Math.Sqrt(31) * (ShadeIntervals - color) + Math.Sqrt(VioletDarkRed) * color) / ShadeIntervals;
            int red = (int)Math.Floor(intensity * intensity + 0.5);
            return Pack(red, VioletGreen * red / 31, (VioletBlue * red + 30) / 31);
        }
        if (color < 8)
        {
            int shade = color - 4;
            return Pack(Interpolate(31, YellowDarkRed, shade), Interpolate(31, YellowDarkGreen, shade), Interpolate(YellowLightBlue, 0, shade));
        }
        if (color == ColorCount - 1) return Pack(31, 31, 31);
        int light = MotherBrainFinalRoomPaintDefinitions.MetalLight & 31;
        int dark = (MotherBrainFinalRoomPaintDefinitions.MetalLow & 31) / MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor;
        int neutral = (light * (12 - color) + dark * (color - 8)) / 4;
        return Pack(neutral, neutral, neutral);
    }

    /// <summary>Interpolates one integer channel value across the ramp's three shade intervals.</summary>
    /// <param name="first">Channel value at the first endpoint.</param>
    /// <param name="last">Channel value at the last endpoint.</param>
    /// <param name="shade">Zero-based shade position from the first endpoint through the fourth shade.</param>
    /// <returns>The interpolated channel value using integer division.</returns>
    private static int Interpolate(int first, int last, int shade) => (first * (ShadeIntervals - shade) + last * shade) / ShadeIntervals;

    /// <summary>Packs three five-bit color channels into a SNES BGR555 word.</summary>
    /// <param name="red">Five-bit red intensity.</param>
    /// <param name="green">Five-bit green intensity.</param>
    /// <param name="blue">Five-bit blue intensity.</param>
    /// <returns>The packed BGR555 color word.</returns>
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
