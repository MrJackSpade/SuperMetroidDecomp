namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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

    /// <summary>Admits a supplied fourteen-color opaque door ramp only when every word matches the reviewed paint calculation.</summary>
    internal static bool Matches(ReadOnlySpan<RgbColor> colors)
    {
        for (int color = 0; color < ColorCount; color++)
            if (colors[color] != Color(color)) return false;
        return true;
    }

    /// <summary>Builds the violet gamma ramp, yellow ramp, neutral metal shades, and terminal white door ink by native color index.</summary>
    internal static RgbColor Color(int color)
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
        int light = MotherBrainFinalRoomPaintDefinitions.MetalLight.Red;
        int dark = (MotherBrainFinalRoomPaintDefinitions.MetalLow.Red) / MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor;
        int neutral = (light * (12 - color) + dark * (color - 8)) / 4;
        return Pack(neutral, neutral, neutral);
    }

    /// <summary>Interpolates a channel across the four authored shade positions using integer truncation.</summary>
    private static int Interpolate(int first, int last, int shade) => (first * (ShadeIntervals - shade) + last * shade) / ShadeIntervals;
    /// <summary>Combines calculated RGB5 channels into the door's opaque CGRAM color word.</summary>
    private static RgbColor Pack(int red, int green, int blue) => new RgbColor(red, green, blue);
}
