using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.BabyMetroidInitialPaintDefinitions;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Calculates the fifteen initial Baby/Shitroid colors from reviewed paints,
/// standard innard easing, fang midpoint and neutrals. Each supplied instance stays independent;
/// separate background, pulse, health and fade colors are excluded.</summary>
internal sealed class BabyMetroidInitialPalette
{
    /// <summary>Complete imported palette retained only when it differs from the reviewed paint calculation.</summary>
    internal readonly Bgr555[]? supplied;

    /// <summary>Uses calculated Baby colors when all fifteen supplied entries agree, otherwise keeps this palette's edits.</summary>
    internal BabyMetroidInitialPalette(Bgr555[] colors)
    {
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) { supplied = colors.ToArray(); return; }
    }

    /// <summary>Selects an independently supplied color or the reviewed dome, innard, and fang paint.</summary>
    internal Bgr555 Resolve(int color)
    {
        if ((uint)color >= BabyMetroidCutsceneColorRomData.InitialColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return supplied is null ? Calculate(color) : supplied[color];
    }

    /// <summary>Composes selected material paints, smooth innard shades, and the fang midpoint by palette slot.</summary>
    private static Bgr555 Calculate(int color)
    {
        var slot = (BabyMetroidInitialColorSlot)color;
        if (!Enum.IsDefined(slot))
            throw new ArgumentOutOfRangeException(nameof(color));
        return slot switch
        {
            BabyMetroidInitialColorSlot.DomeHighlight => DomeHighlight,
            BabyMetroidInitialColorSlot.DomeSurface => DomeSurface,
            BabyMetroidInitialColorSlot.DomeShadow => DomeShadow,
            BabyMetroidInitialColorSlot.DomeRim => DomeRim,
            BabyMetroidInitialColorSlot.InnardGlint => Glint(InnardLightPaint),
            BabyMetroidInitialColorSlot.InnardLight => InnardLightPaint,
            BabyMetroidInitialColorSlot.InnardLightShade or BabyMetroidInitialColorSlot.InnardDarkShade =>
                InnardShade(InnardLightPaint, InnardDarkPaint, slot - BabyMetroidInitialColorSlot.InnardLight),
            BabyMetroidInitialColorSlot.InnardDark => InnardDarkPaint,
            BabyMetroidInitialColorSlot.FangLight => FangLight,
            BabyMetroidInitialColorSlot.FangMiddle => Midpoint(FangLight, FangDark),
            BabyMetroidInitialColorSlot.FangDark => FangDark,
            BabyMetroidInitialColorSlot.FangOutline => FangOutline,
            BabyMetroidInitialColorSlot.White => Bgr555.White,
            BabyMetroidInitialColorSlot.Black => Bgr555.Black,
            _ => throw new InvalidOperationException($"Undefined BabyMetroidInitialColorSlot {slot}."),
        };
    }

    /// <summary>Raises each innard-light channel to form the clipped glint.</summary>
    private static Bgr555 Glint(Bgr555 light) =>
        light.Map((_, channel) => Math.Min(Bgr555.MaxChannel, channel + InnardGlintAddition));

    /// <summary>Eases the innard red channel with smoothstep, then interpolates other channels by that red level.</summary>
    private static Bgr555 InnardShade(Bgr555 light, Bgr555 dark, int shade)
    {
        int lightRed = light.Red, darkRed = dark.Red;
        int intervals = lightRed - darkRed;
        int shadeIntervals = BabyMetroidInitialColorSlot.InnardDark - BabyMetroidInitialColorSlot.InnardLight;
        int denominator = shadeIntervals * shadeIntervals * shadeIntervals;
        int weight = shade * shade * (3 * shadeIntervals - 2 * shade);
        // Standard smoothstep, rounded to the nearest RGB5 red level; no historical-tool claim.
        int red = (lightRed * (denominator - weight) + darkRed * weight + denominator / 2) / denominator;
        return light.Zip(dark, (_, lightChannel, darkChannel) =>
            (lightChannel * (red - darkRed) + darkChannel * (lightRed - red)) / intervals);
    }

    /// <summary>Averages each RGB5 channel down to the intermediate fang paint.</summary>
    internal static Bgr555 Midpoint(Bgr555 light, Bgr555 dark) =>
        light.Zip(dark, (_, lightChannel, darkChannel) => (lightChannel + darkChannel) / 2);

    /// <summary>Hashes all resolved initial colors so supplied edits affect presentation identity.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        Span<Bgr555> colors = stackalloc Bgr555[BabyMetroidCutsceneColorRomData.InitialColorCount];
        for (int color = 0; color < colors.Length; color++) colors[color] = Resolve(color);
        content.AppendColors("initial", colors);
    }
}
