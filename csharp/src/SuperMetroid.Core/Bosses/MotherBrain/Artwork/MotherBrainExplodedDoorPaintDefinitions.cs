namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>$A9:9534-954F exploded-door opaque inks1..14, copied by B275-B27E.
/// Reviewed categorical hue/intensity and gamma composition; mechanics, timing and pixels are excluded.</summary>
internal static class MotherBrainExplodedDoorPaintDefinitions
{
    /// <summary>$A9:9534: first ramp's green/blue hue at maximum red.</summary>
    internal const int VioletGreen = 8, VioletBlue = 19;
    /// <summary>$A9:953A: darkest first-ramp red; gamma2 interpolation derives both middle intensities.</summary>
    internal const int VioletDarkRed = 8;
    /// <summary>$A9:953C/9542: yellow ramp's light blue and dark red/green; other endpoint channels are max/zero.</summary>
    internal const int YellowLightBlue = 17, YellowDarkRed = 18, YellowDarkGreen = 9;
    /// <summary>$A9:9534-9542: each chromatic material has four shades including endpoints.</summary>
    internal const int ShadeIntervals = 3;
    /// <summary>$A9:B27B: fourteen copied opaque colors, excluding transparent3800 and final black.</summary>
    internal const int ColorCount = 14;
}
