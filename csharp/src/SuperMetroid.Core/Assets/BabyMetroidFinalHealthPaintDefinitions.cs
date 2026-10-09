using SuperMetroid.Core.Game;
using InitialSlots = SuperMetroid.Core.Assets.BabyMetroidInitialPaintDefinitions;

namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed final-health paints used as the Baby's six displayed fade endpoints.
/// Only these endpoint choices and material shade dependencies are covered, not other health rows or timing.</summary>
internal static class BabyMetroidFinalHealthPaintDefinitions
{
    /// <summary>$AD:E870, final-health visible ink1: yellow-green dome highlight.</summary>
    private const ushort DomeHighlight = 0x1b9a;
    /// <summary>$AD:E872, visible ink2: yellow-green dome surface.</summary>
    private const ushort DomeSurface = 0x06d5;
    /// <summary>$AD:E874, visible ink3: dome shadow.</summary>
    private const ushort DomeShadow = 0x05cc;
    /// <summary>$AD:E876, visible ink4: dark yellow dome rim, equal red/green.</summary>
    private const ushort DomeRim = 7 | (7 << 5);
    /// <summary>$AD:E8DA, visible ink6: purple organ surface light.</summary>
    private const ushort InnardLight = 0x3870;
    /// <summary>$AD:E8E0, visible ink9: dark organ contours, also reused by outer fang contours.</summary>
    private const ushort DarkContour = 0x0c44;
    /// <summary>$AD:E878, visible ink10: weakened fang light.</summary>
    private const ushort FangLight = 0x3676;
    /// <summary>$AD:E87C, visible ink12: weakened fang dark.</summary>
    private const ushort FangDark = 0x1d6d;
    /// <summary>$AD:E880, visible ink14: pale yellow fang glint with equal red/green.</summary>
    private const ushort FangHighlight = 30 | (30 << 5) | (21 << 10);
    /// <summary>$AD:E8DA-E8DE, blue14/13/12 surface progression; darkest contour uses its independent endpoint.</summary>
    private const int InnardBlueSurfaceStep = 1;
    /// <summary>$AD:E8D8, glint retains organ-light red and adds half the reviewed initial glint17,
    /// green rounded up and blue down. The asymmetric half-light policy is selected paint composition.</summary>
    private const int GlintDivisor = 2;
    /// <summary>$AD:E87E, fang contour is three quarters of initial$A9:94EC, nearest rounding with ties toward dark.</summary>
    private const int ContourNumerator = 3, ContourDenominator = 4;

    /// <summary>Resolves a supported Baby cutscene palette identity to its final-health BGR15 endpoint.</summary>
    /// <param name="color">The palette identity used by the cutscene color catalog, including material shade slots.</param>
    /// <returns>The packed 15-bit color selected or composed for the final-health fade endpoint.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The identity is not one of the supported final-health paint slots.</exception>
    internal static ushort Color(int color) => color switch
    {
        InitialSlots.DomeHighlightColor => DomeHighlight,
        InitialSlots.DomeSurfaceColor => DomeSurface,
        InitialSlots.DomeShadowColor => DomeShadow,
        InitialSlots.DomeRimColor => DomeRim,
        InitialSlots.InnardGlint => Glint(),
        InitialSlots.InnardLight => InnardLight,
        InitialSlots.InnardLightShade or InitialSlots.InnardDarkShade => InnardShade(color - InitialSlots.InnardLight),
        InitialSlots.InnardDark => DarkContour,
        BabyMetroidCutsceneColorRomData.InitialFangLightColor => FangLight,
        BabyMetroidCutsceneColorRomData.InitialFangMiddleColor => BabyMetroidInitialPalette.Midpoint(FangLight, FangDark),
        BabyMetroidCutsceneColorRomData.InitialFangDarkColor => FangDark,
        InitialSlots.FangOutlineColor => FangContour(),
        BabyMetroidCutsceneColorRomData.InitialWhiteColor => FangHighlight,
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>Builds the organ glint from its light base, preserving red and adding the initial glint contribution to green and blue.</summary>
    /// <remarks>The green contribution rounds up, blue rounds down, and both channels are capped at the BGR15 maximum.</remarks>
    private static ushort Glint()
    {
        int red = InnardLight & 31;
        int green = Math.Min(31, (InnardLight >> 5 & 31) + (InitialSlots.InnardGlintAddition + GlintDivisor - 1) / GlintDivisor);
        int blue = Math.Min(31, (InnardLight >> 10 & 31) + InitialSlots.InnardGlintAddition / GlintDivisor);
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Interpolates an organ surface shade between the light paint and the dark contour.</summary>
    /// <param name="shade">The ordinal distance from the light slot toward the dark slot.</param>
    /// <returns>A packed BGR15 shade using smoothstep red, linear green, and the configured per-slot blue step.</returns>
    private static ushort InnardShade(int shade)
    {
        int intervals = InitialSlots.InnardDark - InitialSlots.InnardLight;
        int denominator = intervals * intervals * intervals;
        int weight = shade * shade * (3 * intervals - 2 * shade);
        // Standard smoothstep red and linear green both round up for this selected composition.
        int red = ((InnardLight & 31) * (denominator - weight) + (DarkContour & 31) * weight + denominator - 1) / denominator;
        int green = ((InnardLight >> 5 & 31) * (intervals - shade) + (DarkContour >> 5 & 31) * shade + intervals - 1) / intervals;
        int blue = (InnardLight >> 10 & 31) - shade * InnardBlueSurfaceStep;
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Derives the outer fang contour by scaling each five-bit channel of the initial outline.</summary>
    /// <remarks>Each channel is scaled to three quarters with nearest rounding; exact ties round toward the darker value.</remarks>
    private static ushort FangContour()
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((InitialSlots.FangOutline >> shift & 31) * ContourNumerator + (ContourDenominator - 1) / 2) / ContourDenominator) << shift;
        return (ushort)result;
    }
}
