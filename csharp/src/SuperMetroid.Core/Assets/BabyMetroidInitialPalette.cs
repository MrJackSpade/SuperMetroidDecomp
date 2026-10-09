using SuperMetroid.Core.Game;
using static SuperMetroid.Core.Assets.BabyMetroidInitialPaintDefinitions;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculates the fifteen initial Baby/Shitroid colors from reviewed paints,
/// standard innard easing, fang midpoint and neutrals. Each supplied instance stays independent;
/// separate background, pulse, health and fade colors are excluded.</summary>
internal sealed class BabyMetroidInitialPalette
{
    /// <summary>Optional copy of the supplied palette, retained only when it differs from calculated colors.</summary>
    private readonly ushort[]? supplied;

    /// <summary>RGB5 white used for the cutscene's initial white-color entry.</summary>
    private const ushort White = (31 << 10) | (31 << 5) | 31;

    /// <summary>Stores a copy only when the supplied entries are not all equal to the calculated palette.</summary>
    /// <param name="colors">Initial cutscene colors in palette-index order.</param>
    internal BabyMetroidInitialPalette(ushort[] colors)
    {
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) { supplied = colors.ToArray(); return; }
    }

    /// <summary>Returns the supplied or calculated RGB5 value for one initial palette index.</summary>
    /// <param name="color">Index in the initial Baby cutscene palette.</param>
    /// <returns>The color at that index, preserving supplied values when they differ from calculated defaults.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the initial palette.</exception>
    internal ushort Resolve(int color)
    {
        if ((uint)color >= BabyMetroidCutsceneColorRomData.InitialColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return supplied is null ? Calculate(color) : supplied[color];
    }

    /// <summary>Builds a palette entry from the authored paints and the cutscene's color rules.</summary>
    /// <param name="color">Initial palette index whose value is calculated.</param>
    /// <returns>The corresponding RGB5 color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is not one of the defined initial colors.</exception>
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

    /// <summary>Brightens each RGB5 channel of the innard light paint, clamping channels at 31.</summary>
    /// <param name="light">RGB5 light paint to brighten.</param>
    /// <returns>The channel-wise brightened color.</returns>
    private static ushort Glint(ushort light)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= Math.Min(31, (light >> shift & 31) + InnardGlintAddition) << shift;
        return (ushort)result;
    }

    /// <summary>Interpolates between the innard paints using smoothstep progress across the shade indices.</summary>
    /// <param name="light">RGB5 color at the light end of the shade range.</param>
    /// <param name="dark">RGB5 color at the dark end of the shade range.</param>
    /// <param name="shade">Current shade index measured from <c>InnardLight</c>.</param>
    /// <returns>The RGB5 shade calculated with a smoothstep-weighted red-channel progression.</returns>
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

    /// <summary>Calculates the floor-rounded midpoint of each RGB5 channel independently.</summary>
    /// <param name="light">First endpoint color.</param>
    /// <param name="dark">Second endpoint color.</param>
    /// <returns>The channel-wise midpoint encoded as RGB5.</returns>
    internal static ushort Midpoint(ushort light, ushort dark)
    {
        int result = 0;
        for (int channel = 0; channel < 3; channel++)
            result |= (((light >> (5 * channel) & 31) + (dark >> (5 * channel) & 31)) / 2) << (5 * channel);
        return (ushort)result;
    }

    /// <summary>Adds the resolved initial palette words to a presentation identity in index order.</summary>
    /// <param name="content">Hash accumulator receiving the palette under the <c>initial</c> identity section.</param>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        Span<ushort> colors = stackalloc ushort[BabyMetroidCutsceneColorRomData.InitialColorCount];
        for (int color = 0; color < colors.Length; color++) colors[color] = Resolve(color);
        content.AppendWords("initial", colors);
    }
}
