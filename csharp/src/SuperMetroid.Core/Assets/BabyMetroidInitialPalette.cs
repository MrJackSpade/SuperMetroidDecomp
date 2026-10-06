using SuperMetroid.Core.Game;
using static SuperMetroid.Core.Assets.BabyMetroidInitialPaintDefinitions;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculates the fifteen initial Baby/Shitroid colors from reviewed paints,
/// standard innard easing, fang midpoint and neutrals. Each supplied instance stays independent;
/// separate background, pulse, health and fade colors are excluded.</summary>
internal sealed class BabyMetroidInitialPalette
{
    private readonly ushort[]? supplied;
    private const ushort White = (31 << 10) | (31 << 5) | 31;

    internal BabyMetroidInitialPalette(ushort[] colors)
    {
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) { supplied = colors.ToArray(); return; }
    }

    internal ushort Resolve(int color)
    {
        if ((uint)color >= BabyMetroidCutsceneColorRomData.InitialColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return supplied is null ? Calculate(color) : supplied[color];
    }

    private static ushort Calculate(int color) => color switch
    {
        DomeHighlightColor => DomeHighlight,
        DomeSurfaceColor => DomeSurface,
        DomeShadowColor => DomeShadow,
        DomeRimColor => DomeRim,
        InnardGlint => Glint(InnardLightPaint),
        InnardLight => InnardLightPaint,
        InnardLightShade or InnardDarkShade => InnardShade(InnardLightPaint, InnardDarkPaint, color - InnardLight),
        InnardDark => InnardDarkPaint,
        BabyMetroidCutsceneColorRomData.InitialFangLightColor => FangLight,
        BabyMetroidCutsceneColorRomData.InitialFangMiddleColor => Midpoint(FangLight, FangDark),
        BabyMetroidCutsceneColorRomData.InitialFangDarkColor => FangDark,
        FangOutlineColor => FangOutline,
        BabyMetroidCutsceneColorRomData.InitialWhiteColor => White,
        BabyMetroidCutsceneColorRomData.InitialBlackColor => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    private static ushort Glint(ushort light)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= Math.Min(31, (light >> shift & 31) + InnardGlintAddition) << shift;
        return (ushort)result;
    }

    private static ushort InnardShade(ushort light, ushort dark, int shade)
    {
        int lightRed = light & 31, darkRed = dark & 31;
        int intervals = lightRed - darkRed;
        int shadeIntervals = InnardDark - InnardLight;
        int denominator = shadeIntervals * shadeIntervals * shadeIntervals;
        int weight = shade * shade * (3 * shadeIntervals - 2 * shade);
        // Standard smoothstep, rounded to the nearest RGB5 red level; no historical-tool claim.
        int red = (lightRed * (denominator - weight) + darkRed * weight + denominator / 2) / denominator;
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((light >> shift & 31) * (red - darkRed) + (dark >> shift & 31) * (lightRed - red)) / intervals) << shift;
        return (ushort)result;
    }

    internal static ushort Midpoint(ushort light, ushort dark)
    {
        int result = 0;
        for (int channel = 0; channel < 3; channel++)
            result |= (((light >> (5 * channel) & 31) + (dark >> (5 * channel) & 31)) / 2) << (5 * channel);
        return (ushort)result;
    }

    internal void AppendIdentity(SelectedPresentationHash content)
    {
        Span<ushort> colors = stackalloc ushort[BabyMetroidCutsceneColorRomData.InitialColorCount];
        for (int color = 0; color < colors.Length; color++) colors[color] = Resolve(color);
        content.AppendWords("initial", colors);
    }
}
