using Layout = SuperMetroid.Core.Assets.MotherBrainRainbowPaletteFormat.RedOriginLayout;

namespace SuperMetroid.Core.Assets;

/// <summary>Mutually exclusive material-channel profiles in the native body rainbow phases.</summary>
internal enum MotherBrainRainbowShadeProfile
{
    /// <summary>$AD:E486: phase1 green, with ceiling plate and nearest-up tissue shades.</summary>
    FirstGreenRise,
    /// <summary>$AD:E4C2: phase2 green, with ceiling plate and nearest-even tissue shades.</summary>
    SecondGreenRise,
    /// <summary>$AD:E576: phase5 blue, with a flat head, nearest plate and floor tissue shades.</summary>
    MixedBlue,
    /// <summary>$AD:E5B2: phase6 blue, with paired head, ceiling plate and nearest-up tissue shades.</summary>
    MaximumBlue,
    /// <summary>$AD:E5EE: phase7 green, with a head ramp, shared plate/tail and three tissue greens, retaining two independent tissue samples.</summary>
    RecoveringGreen,
}

/// <summary>Chosen shade increments for the reviewed Mother Brain material-paint animation.</summary>
internal static class MotherBrainRainbowShadeDefinitions
{
    /// <summary>$AD:E486/E4C2/E5EE first four green channels descend by1 per head/outline ink; chosen head shading for this color performance.</summary>
    internal const int GreenHeadStep = 1;
    /// <summary>$AD:E5B2 first four blue channels descend by1 after each pair; chosen head shading/pairing for this color performance.</summary>
    internal const int BlueHeadStep = 1;
}

/// <summary>
/// Exact supplied-match material ramps. Retained endpoints, independent samples and
/// selected profile/rounding policies specify the reviewed temporal material paint.
/// $A9:BCFD-BD44 only advances palette selection and copies colors to CGRAM; these
/// choices paint the cortex, plate, tissue, glint and exposed-head outline masks.
/// A different generated hue performance would invent different content. Shared
/// channels and interpolated shades still calculate; arbitrary supplied edits stay exact.
/// </summary>
internal sealed class MotherBrainRainbowShadeChannel
{
    /// <summary>Native phase's selected head, plate, tissue, and tail shade rule.</summary>
    private readonly MotherBrainRainbowShadeProfile profile;
    /// <summary>Existing shared green channel used by the recovering phase.</summary>
    private readonly Func<int, int> sourceGreen;
    /// <summary>Independent shade samples, or the complete supplied row when it differs from the reviewed rule.</summary>
    private readonly byte[] inputs;
    /// <summary>Whether every supplied shade matches the reviewed interpolation profile.</summary>
    private readonly bool calculated;

    /// <summary>Retains independent samples only when the supplied row follows the native shade profile.</summary>
    internal MotherBrainRainbowShadeChannel(byte[] samples, MotherBrainRainbowShadeProfile profile, Func<int, int> sourceGreen)
    {
        this.profile = profile;
        this.sourceGreen = sourceGreen;
        inputs = profile == MotherBrainRainbowShadeProfile.RecoveringGreen
            ? [samples[0], samples[Layout.TissueStart + 2], samples[Layout.TissueStart + 3]]
            : [samples[0], samples[Layout.PlateStart], samples[Layout.PlateEnd],
                samples[Layout.TissueStart], samples[Layout.TissueEnd], samples[Layout.TailStart], samples[Layout.TailStart + 1]];
        if (SharesDarkHeadTail(profile)) inputs = inputs[..^1];
        for (int color = 0; color < samples.Length; color++)
        {
            if (Calculate(color) == samples[color]) continue;
            inputs = samples;
            return;
        }
        calculated = true;
    }

    /// <summary>Uses the calculated shade for a matching row, or the exact supplied edit otherwise.</summary>
    internal byte this[int color] => calculated ? (byte)Calculate(color) : inputs[color];

    /// <summary>Applies the phase's head ramp, plate and tissue rounding, and shared tail rule.</summary>
    private int Calculate(int color)
    {
        if (color < Layout.PlateStart)
            return profile switch
            {
                MotherBrainRainbowShadeProfile.MixedBlue => inputs[0],
                MotherBrainRainbowShadeProfile.MaximumBlue => inputs[0] - color / Layout.DarkHead * MotherBrainRainbowShadeDefinitions.BlueHeadStep,
                MotherBrainRainbowShadeProfile.FirstGreenRise or MotherBrainRainbowShadeProfile.SecondGreenRise or
                    MotherBrainRainbowShadeProfile.RecoveringGreen => inputs[0] - color * MotherBrainRainbowShadeDefinitions.GreenHeadStep,
                _ => throw new InvalidOperationException($"Undefined rainbow shade profile {profile}."),
            };
        if (profile == MotherBrainRainbowShadeProfile.RecoveringGreen)
            return color is Layout.TissueStart + 2 or Layout.TissueStart + 3
                ? inputs[color - Layout.TissueStart - 1] : sourceGreen(color);
        if (color < Layout.TissueStart)
        {
            Rounding rounding = profile == MotherBrainRainbowShadeProfile.MixedBlue ? Rounding.NearestUp : Rounding.Ceiling;
            return Interpolate(inputs[1], inputs[2], color - Layout.PlateStart, Layout.PlateIntervals, rounding);
        }
        if (color < Layout.TailStart)
        {
            Rounding rounding = profile switch
            {
                MotherBrainRainbowShadeProfile.SecondGreenRise => Rounding.NearestEven,
                MotherBrainRainbowShadeProfile.MixedBlue => Rounding.Floor,
                MotherBrainRainbowShadeProfile.FirstGreenRise or MotherBrainRainbowShadeProfile.MaximumBlue => Rounding.NearestUp,
                MotherBrainRainbowShadeProfile.RecoveringGreen =>
                    throw new InvalidOperationException("Recovering green tissue samples are selected above."),
                _ => throw new InvalidOperationException($"Undefined rainbow shade profile {profile}."),
            };
            return Interpolate(inputs[3], inputs[4], color - Layout.TissueStart, Layout.TissueIntervals, rounding);
        }
        if (color == Layout.TailStart + 1 && SharesDarkHeadTail(profile))
            return Calculate(Layout.PlateStart - 1);
        return inputs[5 + color - Layout.TailStart];
    }

    /// <summary>Identifies phases where the exposed-head outline shares the darkest cortex channel.</summary>
    private static bool SharesDarkHeadTail(MotherBrainRainbowShadeProfile profile) =>
        profile is MotherBrainRainbowShadeProfile.FirstGreenRise or MotherBrainRainbowShadeProfile.MixedBlue or MotherBrainRainbowShadeProfile.MaximumBlue;

    /// <summary>Selected integer tie rule for a phase's material-shade interpolation.</summary>
    private enum Rounding
    {
        /// <summary>Discards a positive fractional shade remainder.</summary>
        Floor,
        /// <summary>Raises a positive shade when interpolation leaves any remainder.</summary>
        Ceiling,
        /// <summary>Rounds halfway shades upward to the next channel value.</summary>
        NearestUp,
        /// <summary>Rounds a halfway shade to the even channel value.</summary>
        NearestEven,
    }
    /// <summary>Interpolates between two supplied channel endpoints with the profile's integer tie rule.</summary>
    private static int Interpolate(int first, int last, int shade, int intervals, Rounding rounding)
    {
        int numerator = first * (intervals - shade) + last * shade;
        if (rounding == Rounding.Floor) return numerator / intervals;
        if (rounding == Rounding.Ceiling) return (numerator + intervals - 1) / intervals;
        if (rounding == Rounding.NearestUp) return (numerator + intervals / 2) / intervals;
        int quotient = numerator / intervals, remainder = numerator % intervals;
        return quotient + (2 * remainder > intervals || 2 * remainder == intervals && (quotient & 1) != 0 ? 1 : 0);
    }
}
