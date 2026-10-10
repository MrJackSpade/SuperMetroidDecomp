using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using static SuperMetroid.Core.Assets.BabyMetroidInitialPaintDefinitions;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculates the fifteen initial Baby/Shitroid colors from reviewed paints,
/// standard innard easing, fang midpoint and neutrals. Each supplied instance stays independent;
/// separate background, pulse, health and fade colors are excluded.</summary>
internal sealed class BabyMetroidInitialPalette
{
    private readonly Bgr555[]? supplied;

    internal BabyMetroidInitialPalette(Bgr555[] colors)
    {
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) { supplied = colors.ToArray(); return; }
    }

    internal Bgr555 Resolve(int color)
    {
        if ((uint)color >= BabyMetroidCutsceneColorRomData.InitialColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return supplied is null ? Calculate(color) : supplied[color];
    }

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

    private static Bgr555 Glint(Bgr555 light) =>
        light.Map((_, channel) => Math.Min(Bgr555.MaxChannel, channel + InnardGlintAddition));

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

    internal static Bgr555 Midpoint(Bgr555 light, Bgr555 dark) =>
        light.Zip(dark, (_, lightChannel, darkChannel) => (lightChannel + darkChannel) / 2);

    internal void AppendIdentity(SelectedPresentationHash content)
    {
        Span<Bgr555> colors = stackalloc Bgr555[BabyMetroidCutsceneColorRomData.InitialColorCount];
        for (int color = 0; color < colors.Length; color++) colors[color] = Resolve(color);
        content.AppendColors("initial", colors);
    }
}
