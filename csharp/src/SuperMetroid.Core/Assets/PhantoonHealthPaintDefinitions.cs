namespace SuperMetroid.Core.Assets;

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
    private const int HideGlint = 27, HideGlintBlue = 17;
    /// <summary>$A7:CC25: mid-light hide is the selected two-thirds shade of the glint, with floor rounding.</summary>
    private const int HideMidNumerator = 2, HideMidDenominator = 3;
    /// <summary>$A7:CC27-CC29: deepest yellow hide contour, followed by its half-intensity outline.</summary>
    private const int HideDeep = 6;
    /// <summary>$A7:CC2B-CC31: olive body surface red/green ramp, clipped at its selected shadow floor.</summary>
    private const int OlivePeak = 21, OliveStep = 5, OliveFloor = 8;
    /// <summary>$A7:CC2B/CC31: body surface blue endpoints are the corresponding olive endpoints minus this yellow tint; intermediate blue uses nearest gamma-two interpolation.</summary>
    private const int YellowSeparation = 7;
    /// <summary>$A7:CC35-CC39: iris shares the selected olive peak but has a distinct shade step, floor and blue endpoints.</summary>
    private const int IrisStep = 8, IrisFloor = 9, IrisBlueLight = 13, IrisBlueDark = 2;
    /// <summary>$A7:CC3B-CC3F: saturated-red eye-surround shade endpoints and clipped blue falloff.</summary>
    private const int SurroundLight = 29, SurroundDark = 10, SurroundBlue = 14, SurroundBlueStep = 8;
    /// <summary>$A7:CB41-CC40: retained healthy-color weight starts at8/15 and increases by one fifteenth per band.</summary>
    private const int MinimumWeight = 8, WeightDenominator = 15;
    /// <summary>$A7:CB41-CC40: global green/blue quantization bias, applied before division to every ink/band. It accounts for all nine products whose remainder is14; there is no exceptional channel list.</summary>
    private const int CoolChannelNumeratorBias = 1;

    /// <summary>Calculates Phantoon's native red health tint for one ink and health band.</summary>
    /// <param name="band">Health band from 0 (strongest red tint) through 7 (healthiest retained color).</param>
    /// <param name="ink">Palette ink index from 0 through 15.</param>
    /// <returns>The packed 15-bit color for the selected ink and band.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="band"/> is outside the eight supported bands or <paramref name="ink"/> is not defined.</exception>
    internal static ushort Color(int band, int ink)
    {
        if ((uint)band >= 8) throw new ArgumentOutOfRangeException(nameof(band));
        ushort healthy = Healthy(ink);
        int weight = MinimumWeight + band;
        int red = (31 * WeightDenominator + ((healthy & 31) - 31) * weight) / WeightDenominator;
        int green = (((healthy >> 5) & 31) * weight + CoolChannelNumeratorBias) / WeightDenominator;
        int blue = ((healthy >> 10) * weight + CoolChannelNumeratorBias) / WeightDenominator;
        return Rgb(red, green, blue);
    }

    /// <summary>Resolves an ink to its healthy Phantoon palette color before health tinting.</summary>
    /// <param name="ink">Palette index selecting a hide, body, glint, iris, or eye-surround color.</param>
    /// <returns>The selected healthy color in packed SNES 15-bit format.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ink"/> is outside the defined 0-through-15 palette range.</exception>
    private static ushort Healthy(int ink) => ink switch
    {
        0 => 0,
        1 => Olive(HideGlint, HideGlintBlue),
        2 => Olive(HideGlint * HideMidNumerator / HideMidDenominator,
            HideGlintBlue * HideMidNumerator / HideMidDenominator),
        3 => Olive(HideDeep, 0),
        4 => Olive(HideDeep / 2, 0),
        >= 5 and <= 8 => BodyShade(ink - 5),
        9 => Rgb(31, 31, 31),
        >= 10 and <= 12 => IrisShade(ink - 10),
        >= 13 and <= 15 => SurroundShade(ink - 13),
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };

    /// <summary>Calculates a hide/body-surface shade using its olive-channel ramp and gamma-interpolated blue value.</summary>
    /// <param name="shade">Position in the three-step body-surface shade ramp.</param>
    /// <returns>The packed olive body color for that shade.</returns>
    private static ushort BodyShade(int shade) => Olive(
        Math.Max(OliveFloor, OlivePeak - OliveStep * shade),
        GammaShade(OlivePeak - YellowSeparation, OliveFloor - YellowSeparation, shade, 3));

    /// <summary>Calculates an iris shade using its red/green floor and separate blue endpoints.</summary>
    /// <param name="shade">Position in the two-step iris shade ramp.</param>
    /// <returns>The packed olive iris color for that shade.</returns>
    private static ushort IrisShade(int shade) => Olive(
        Math.Max(IrisFloor, OlivePeak - IrisStep * shade),
        GammaShade(IrisBlueLight, IrisBlueDark, shade, 2));

    /// <summary>Interpolates the eye-surround's red channel while reducing blue to its selected shadow floor.</summary>
    /// <param name="shade">Position in the two-step eye-surround shade ramp.</param>
    /// <returns>The packed red eye-surround color for that shade.</returns>
    private static ushort SurroundShade(int shade) => Rgb(
        (SurroundLight * (2 - shade) + SurroundDark * shade + 1) / 2,
        0, Math.Max(0, SurroundBlue - SurroundBlueStep * shade));

    /// <summary>Interpolates between two channel intensities in gamma-two space, rounding to the nearest integer.</summary>
    /// <param name="from">Channel intensity at the first endpoint.</param>
    /// <param name="to">Channel intensity at the last endpoint.</param>
    /// <param name="shade">Zero-based interpolation step between the endpoints.</param>
    /// <param name="intervals">Number of steps separating the endpoints.</param>
    /// <returns>The rounded channel intensity for the requested step.</returns>
    private static int GammaShade(int from, int to, int shade, int intervals)
    {
        if (shade == 0) return from;
        if (shade == intervals) return to;
        double intensity = (Math.Sqrt(from) * (intervals - shade) + Math.Sqrt(to) * shade) / intervals;
        return (int)Math.Floor(intensity * intensity + 0.5);
    }
    /// <summary>Packs an olive color whose red and green channels share one intensity.</summary>
    /// <param name="intensity">Five-bit value used for both red and green.</param>
    /// <param name="blue">Five-bit blue-channel value.</param>
    /// <returns>The packed 15-bit color.</returns>
    private static ushort Olive(int intensity, int blue) => Rgb(intensity, intensity, blue);

    /// <summary>Packs three five-bit color channels into the SNES 15-bit color word.</summary>
    /// <param name="red">Red channel in bits 0 through 4.</param>
    /// <param name="green">Green channel in bits 5 through 9.</param>
    /// <param name="blue">Blue channel in bits 10 through 14.</param>
    /// <returns>The packed color word.</returns>
    private static ushort Rgb(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
