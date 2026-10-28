using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Phantoon.Artwork.PhantoonHealthPaintDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Paints Phantoon's material inks and their red health tint from the catalog's selected endpoints and shade policy.</summary>
internal static class PhantoonHealthPaint
{
    /// <summary>Blends one material ink toward red using the selected health band and cool-channel quantization bias.</summary>
    internal static RgbColor Color(int band, int ink)
    {
        if ((uint)band >= 8) throw new ArgumentOutOfRangeException(nameof(band));
        RgbColor healthy = Healthy(ink);
        int weight = MinimumWeight + band;
        int red = (31 * WeightDenominator + ((healthy.Red) - 31) * weight) / WeightDenominator;
        int green = ((healthy.Green) * weight + CoolChannelNumeratorBias) / WeightDenominator;
        int blue = ((healthy.Blue) * weight + CoolChannelNumeratorBias) / WeightDenominator;
        return Rgb(red, green, blue);
    }

    /// <summary>Chooses the untinted hide, pupil, iris, or eye-surround material shade.</summary>
    private static RgbColor Healthy(int ink) => ink switch
    {
        0 => RgbColor.Black,
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

    /// <summary>Calculates the olive body ramp with a clipped red-green floor and gamma-spaced blue.</summary>
    private static RgbColor BodyShade(int shade) => Olive(
        Math.Max(OliveFloor, OlivePeak - OliveStep * shade),
        GammaShade(OlivePeak - YellowSeparation, OliveFloor - YellowSeparation, shade, 3));
    /// <summary>Calculates iris shades with their own channel step and blue endpoints.</summary>
    private static RgbColor IrisShade(int shade) => Olive(
        Math.Max(IrisFloor, OlivePeak - IrisStep * shade),
        GammaShade(IrisBlueLight, IrisBlueDark, shade, 2));
    /// <summary>Calculates the saturated red eye surround and its clipped blue falloff.</summary>
    private static RgbColor SurroundShade(int shade) => Rgb(
        (SurroundLight * (2 - shade) + SurroundDark * shade + 1) / 2,
        0, Math.Max(0, SurroundBlue - SurroundBlueStep * shade));
    /// <summary>Interpolates channel intensity in gamma-two space and rounds the interior shade.</summary>
    private static int GammaShade(int from, int to, int shade, int intervals)
    {
        if (shade == 0) return from;
        if (shade == intervals) return to;
        double intensity = (Math.Sqrt(from) * (intervals - shade) + Math.Sqrt(to) * shade) / intervals;
        return (int)Math.Floor(intensity * intensity + 0.5);
    }
    /// <summary>Uses equal red and green channels for Phantoon's olive materials.</summary>
    private static RgbColor Olive(int intensity, int blue) => Rgb(intensity, intensity, blue);
    /// <summary>Forms one five-bit material color from its calculated channels.</summary>
    private static RgbColor Rgb(int red, int green, int blue) => new RgbColor(red, green, blue);
}
