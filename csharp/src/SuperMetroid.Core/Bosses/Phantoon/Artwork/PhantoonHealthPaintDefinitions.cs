namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's selected olive hide, iris and red eye-surround paints at $A7:CC21-CC40,
/// and their red health tint across $CB41-CC40. The material endpoints and selected
/// shade/tint policy describe the native illustration; inventing different hues
/// would redraw it. All shades and all 128 outputs calculate, without residual samples.
/// </summary>
/// <remarks>
/// Native $A7:DC0F-DC49 selects and copies a palette word after independently choosing
/// a health band. Source BG2 masks identify hide/mouth/tentacle inks1-8, pupil glint9,
/// iris10-12 and eye-surround13-15. Ink0 starts black and follows the same tint rule.
/// Only this color composition is retained; health thresholds, collision, timings,
/// pixels and Wrecked Ship power-on colors are independent obligations.
/// </remarks>
internal static class PhantoonHealthPaintDefinitions
{
    /// <summary>$A7:CC23: olive hide's brightest red/green level and blue channel.</summary>
    internal const int HideGlint = 27, HideGlintBlue = 17;
    /// <summary>$A7:CC25: mid-light hide is the selected two-thirds shade of the glint, with floor rounding.</summary>
    internal const int HideMidNumerator = 2, HideMidDenominator = 3;
    /// <summary>$A7:CC27-CC29: deepest yellow hide contour, followed by its half-intensity outline.</summary>
    internal const int HideDeep = 6;
    /// <summary>$A7:CC2B-CC31: olive body surface red/green ramp, clipped at its selected shadow floor.</summary>
    internal const int OlivePeak = 21, OliveStep = 5, OliveFloor = 8;
    /// <summary>$A7:CC2B/CC31: body surface blue endpoints are the corresponding olive endpoints minus this yellow tint; intermediate blue uses nearest gamma-two interpolation.</summary>
    internal const int YellowSeparation = 7;
    /// <summary>$A7:CC35-CC39: iris shares the selected olive peak but has a distinct shade step, floor and blue endpoints.</summary>
    internal const int IrisStep = 8, IrisFloor = 9, IrisBlueLight = 13, IrisBlueDark = 2;
    /// <summary>$A7:CC3B-CC3F: saturated-red eye-surround shade endpoints and clipped blue falloff.</summary>
    internal const int SurroundLight = 29, SurroundDark = 10, SurroundBlue = 14, SurroundBlueStep = 8;
    /// <summary>$A7:CB41-CC40: retained healthy-color weight starts at8/15 and increases by one fifteenth per band.</summary>
    internal const int MinimumWeight = 8, WeightDenominator = 15;
    /// <summary>$A7:CB41-CC40: global green/blue quantization bias, applied before division to every ink/band. It accounts for all nine products whose remainder is14; there is no exceptional channel list.</summary>
    internal const int CoolChannelNumeratorBias = 1;
}
