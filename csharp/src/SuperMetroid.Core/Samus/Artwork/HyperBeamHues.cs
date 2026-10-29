using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The hue operations both Hyper Beam colour cycles share (#1464): Samus's full-body cycle, which the import pipeline
/// decodes, and the beam's own colour effect.
/// </summary>
public static class HyperBeamHues
{
    /// <summary>Interpolates RGB5 colors by half with the caller's channel rounding convention.</summary>
    /// <remarks>Even frames interpolate the adjacent odd hue endpoints, with
    /// frame0 wrapping between9/1. Frame6 uses the green-to-yellow red ramp.
    /// Only independently edited channels differing from the selected color
    /// construction are stored; original intermediate colors need no corrections.
    /// matching channels are always calculated, never stored as generated words.
    /// Upward rounding is the default for hue blends. The magenta-cyan
    /// middle shadow selects downward red and upward green/blue rounding.
    /// Each independent channel numerator is0..63; no saturation or overflow.</remarks>
    internal static RgbColor HueMidpoint(RgbColor first, RgbColor second, HueRounding direction = HueRounding.Up)
    {
        int rounding = direction == HueRounding.Up ? 1 : 0;
        int red = ((first.Red) + (second.Red) + rounding) / 2;
        int green = ((first.Green) + (second.Green) + rounding) / 2;
        int blue = ((first.Blue) + (second.Blue) + rounding) / 2;
        return new RgbColor(red, green, blue);
    }

    /// <summary>Turns the red endpoint into magenta by raising blue to red.</summary>
    /// <remarks>Original projectile frame8 ink1 ($8D:D9A8) derives from
    /// frame0 ink3 ($D90C). Body-cycle frame1 also has blue=red,including
    /// its independently edited shade-red channels. Red/green stay fixed; copying red into blue
    /// needs no rounding,saturation or overflow. Edits remain independent.</remarks>
    internal static RgbColor MagentaFromRed(RgbColor red) =>
        red.WithBlue(red.Red);

    /// <summary>Changes the green Hyper Beam hue into yellow by raising red to green.</summary>
    /// <remarks>Every opaque original frame7 word ($9B:A280) equals frame5
    /// ($9B:A2C0) with red replaced by green. Green/blue remain unchanged.
    /// RGB5 component copying needs no rounding or saturation. Transparent
    /// payloads are outside the hue transform; differing asset values override it.</remarks>
    internal static RgbColor YellowFromGreen(RgbColor green) =>
        green.WithRed(green.Green);
}
