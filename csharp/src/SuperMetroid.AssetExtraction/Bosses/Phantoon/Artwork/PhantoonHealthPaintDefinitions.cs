namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's selected olive hide, iris and red eye-surround paints,
/// and their red health tint across the eight health bands. The material endpoints and selected
/// shade/tint policy describe the illustration; inventing different hues
/// would redraw it. All shades and all 128 outputs calculate, without residual samples.
/// </summary>
/// <remarks>
/// The health-palette update selects and copies a palette after independently choosing
/// a health band. Source body-layer masks identify hide/mouth/tentacle inks1-8, pupil glint9,
/// iris10-12 and eye-surround13-15. Ink0 starts black and follows the same tint rule.
/// Only this color composition is retained; health thresholds, collision, timings,
/// pixels and Wrecked Ship power-on colors are independent obligations.
/// </remarks>
internal static class PhantoonHealthPaintDefinitions
{
    /// <summary>Olive hide's brightest red/green level and blue channel.</summary>
    internal const int HideGlint = 27, HideGlintBlue = 17;
    /// <summary>Mid-light hide is the selected two-thirds shade of the glint, with floor rounding.</summary>
    internal const int HideMidNumerator = 2, HideMidDenominator = 3;
    /// <summary>Deepest yellow hide contour, followed by its half-intensity outline.</summary>
    internal const int HideDeep = 6;
    /// <summary>Olive body surface red/green ramp, clipped at its selected shadow floor.</summary>
    internal const int OlivePeak = 21, OliveStep = 5, OliveFloor = 8;
    /// <summary>Body surface blue endpoints are the corresponding olive endpoints minus this yellow tint; intermediate blue uses nearest gamma-two interpolation.</summary>
    internal const int YellowSeparation = 7;
    /// <summary>Iris shares the selected olive peak but has a distinct shade step, floor and blue endpoints.</summary>
    internal const int IrisStep = 8, IrisFloor = 9, IrisBlueLight = 13, IrisBlueDark = 2;
    /// <summary>Saturated-red eye-surround shade endpoints and clipped blue falloff.</summary>
    internal const int SurroundLight = 29, SurroundDark = 10, SurroundBlue = 14, SurroundBlueStep = 8;
    /// <summary>Retained healthy-color weight starts at8/15 and increases by one fifteenth per band.</summary>
    internal const int MinimumWeight = 8, WeightDenominator = 15;
    /// <summary>Global green/blue quantization bias, applied before division to every ink/band. It accounts for all nine products whose remainder is14; there is no exceptional channel list.</summary>
    internal const int CoolChannelNumeratorBias = 1;
}
