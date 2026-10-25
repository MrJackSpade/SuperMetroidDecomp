using SuperMetroid.Core.Assets;
using InitialSlots = SuperMetroid.Core.Bosses.MotherBrain.Artwork.BabyMetroidInitialPaintDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed final-health paints used as the Baby's six displayed fade endpoints.
/// Only these endpoint choices and material shade dependencies are covered, not other health rows or timing.</summary>
internal static class BabyMetroidFinalHealthPaintDefinitions
{
    /// <summary>$AD:E870, final-health visible ink1: yellow-green dome highlight.</summary>
    private static readonly RgbColor DomeHighlight = RgbColor.FromWord(0x1b9a);
    /// <summary>$AD:E872, visible ink2: yellow-green dome surface.</summary>
    private static readonly RgbColor DomeSurface = RgbColor.FromWord(0x06d5);
    /// <summary>$AD:E874, visible ink3: dome shadow.</summary>
    private static readonly RgbColor DomeShadow = RgbColor.FromWord(0x05cc);
    /// <summary>$AD:E876, visible ink4: dark yellow dome rim, equal red/green.</summary>
    private static readonly RgbColor DomeRim = new(7, 7, 0);
    /// <summary>$AD:E8DA, visible ink6: purple organ surface light.</summary>
    private static readonly RgbColor InnardLight = RgbColor.FromWord(0x3870);
    /// <summary>$AD:E8E0, visible ink9: dark organ contours, also reused by outer fang contours.</summary>
    private static readonly RgbColor DarkContour = RgbColor.FromWord(0x0c44);
    /// <summary>$AD:E878, visible ink10: weakened fang light.</summary>
    private static readonly RgbColor FangLight = RgbColor.FromWord(0x3676);
    /// <summary>$AD:E87C, visible ink12: weakened fang dark.</summary>
    private static readonly RgbColor FangDark = RgbColor.FromWord(0x1d6d);
    /// <summary>$AD:E880, visible ink14: pale yellow fang glint with equal red/green.</summary>
    private static readonly RgbColor FangHighlight = new(30, 30, 21);
    /// <summary>$AD:E8DA-E8DE, blue14/13/12 surface progression; darkest contour uses its independent endpoint.</summary>
    private const int InnardBlueSurfaceStep = 1;
    /// <summary>$AD:E8D8, glint retains organ-light red and adds half the reviewed initial glint17,
    /// green rounded up and blue down. The asymmetric half-light policy is selected paint composition.</summary>
    private const int GlintDivisor = 2;
    /// <summary>$AD:E87E, fang contour is three quarters of initial$A9:94EC, nearest rounding with ties toward dark.</summary>
    private const int ContourNumerator = 3, ContourDenominator = 4;

    /// <summary>Selects a final-health fade endpoint for the fourteen transferred palette slots.</summary>
    internal static RgbColor Color(int color)
    {
        BabyMetroidInitialColorSlot slot = BabyMetroidInitialPalette.ColorOrder.At(color);
        return slot switch
        {
            BabyMetroidInitialColorSlot.DomeHighlight => DomeHighlight,
            BabyMetroidInitialColorSlot.DomeSurface => DomeSurface,
            BabyMetroidInitialColorSlot.DomeShadow => DomeShadow,
            BabyMetroidInitialColorSlot.DomeRim => DomeRim,
            BabyMetroidInitialColorSlot.InnardGlint => Glint(),
            BabyMetroidInitialColorSlot.InnardLight => InnardLight,
            BabyMetroidInitialColorSlot.InnardLightShade or BabyMetroidInitialColorSlot.InnardDarkShade =>
                InnardShade(color - BabyMetroidInitialPalette.ColorOrder.PositionOf(BabyMetroidInitialColorSlot.InnardLight)),
            BabyMetroidInitialColorSlot.InnardDark => DarkContour,
            BabyMetroidInitialColorSlot.FangLight => FangLight,
            BabyMetroidInitialColorSlot.FangMiddle => BabyMetroidInitialPalette.Midpoint(FangLight, FangDark),
            BabyMetroidInitialColorSlot.FangDark => FangDark,
            BabyMetroidInitialColorSlot.FangOutline => FangContour(),
            BabyMetroidInitialColorSlot.White => FangHighlight,
            // The fade transfers fourteen colors; the initial black slot fifteen is never rewritten.
            BabyMetroidInitialColorSlot.Black => throw new ArgumentOutOfRangeException(nameof(color)),
            _ => throw new InvalidOperationException($"Undefined BabyMetroidInitialColorSlot {slot}."),
        };
    }

    /// <summary>Adds half the initial glint increment to the weakened organ light, with channel-specific rounding.</summary>
    private static RgbColor Glint()
    {
        int green = Math.Min(31, InnardLight.Green + (InitialSlots.InnardGlintAddition + GlintDivisor - 1) / GlintDivisor);
        int blue = Math.Min(31, InnardLight.Blue + InitialSlots.InnardGlintAddition / GlintDivisor);
        return new(InnardLight.Red, green, blue);
    }

    /// <summary>Interpolates the organ's red by smoothstep, green linearly, and blue by one unit per shade.</summary>
    private static RgbColor InnardShade(int shade)
    {
        int intervals = BabyMetroidInitialColorSlot.InnardDark - BabyMetroidInitialColorSlot.InnardLight;
        int denominator = intervals * intervals * intervals;
        int weight = shade * shade * (3 * intervals - 2 * shade);
        // Standard smoothstep red and linear green both round up for this selected composition.
        int red = (InnardLight.Red * (denominator - weight) + DarkContour.Red * weight + denominator - 1) / denominator;
        int green = (InnardLight.Green * (intervals - shade) + DarkContour.Green * shade + intervals - 1) / intervals;
        int blue = InnardLight.Blue - shade * InnardBlueSurfaceStep;
        return new(red, green, blue);
    }

    /// <summary>Darkens the initial fang outline to three quarters with the selected rounding policy.</summary>
    private static RgbColor FangContour() => InitialSlots.FangOutline.Map((_, channel) =>
        (channel * ContourNumerator + (ContourDenominator - 1) / 2) / ContourDenominator);
}
