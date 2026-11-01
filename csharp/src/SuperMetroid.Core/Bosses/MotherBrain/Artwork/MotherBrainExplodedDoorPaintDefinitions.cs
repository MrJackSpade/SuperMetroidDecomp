namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Exploded-door opaque inks1..14, copied when the escape opens the door.
/// Reviewed categorical hue/intensity and gamma composition; mechanics, timing and pixels are excluded.</summary>
internal static class MotherBrainExplodedDoorPaintDefinitions
{
    /// <summary>First ramp's green/blue hue at maximum red.</summary>
    internal const int VioletGreen = 8, VioletBlue = 19;
    /// <summary>Darkest first-ramp red; gamma2 interpolation derives both middle intensities.</summary>
    internal const int VioletDarkRed = 8;
    /// <summary>Yellow ramp's light blue and dark red/green; other endpoint channels are max/zero.</summary>
    internal const int YellowLightBlue = 17, YellowDarkRed = 18, YellowDarkGreen = 9;
    /// <summary>Each chromatic material has four shades including endpoints.</summary>
    internal const int ShadeIntervals = 3;
    /// <summary>Fourteen copied opaque colors, excluding transparent3800 and final black.</summary>
    internal const int ColorCount = 14;
}
