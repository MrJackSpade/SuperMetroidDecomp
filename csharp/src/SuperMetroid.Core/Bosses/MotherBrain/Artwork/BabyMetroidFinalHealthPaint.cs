using SuperMetroid.Core.Assets;
using InitialSlots = SuperMetroid.Core.Bosses.MotherBrain.Artwork.BabyMetroidInitialPaintDefinitions;
using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.BabyMetroidFinalHealthPaintDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Paints the Baby's final-health fade endpoints from the reviewed paints and shade dependencies in the catalog.</summary>
internal static class BabyMetroidFinalHealthPaint
{
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
