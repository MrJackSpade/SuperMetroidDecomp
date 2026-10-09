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
    /// <summary>Material profile selecting how head, plate, tissue, and tail channels are reconstructed.</summary>
    private readonly MotherBrainRainbowShadeProfile profile;

    /// <summary>Reads source green-channel samples used by the recovering-green tissue ramp.</summary>
    private readonly Func<int, int> sourceGreen;

    /// <summary>Supplied channel samples retained when a calculated profile does not exactly match the source artwork.</summary>
    private readonly byte[] inputs;

    /// <summary>Whether every channel entry matched the selected formula and can be calculated on demand.</summary>
    private readonly bool calculated;

    /// <summary>Builds the compact channel inputs for a profile, retaining the complete source when formulas differ.</summary>
    /// <param name="samples">Channel values in palette color order for one rainbow phase.</param>
    /// <param name="profile">Native phase profile that defines the shade relationships.</param>
    /// <param name="sourceGreen">Accessor for source artwork's green channel when the recovering ramp reuses it.</param>
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

    /// <summary>Gets a channel value from its exact supplied input or the matching calculated shade profile.</summary>
    /// <param name="color">Palette color index within the phase.</param>
    internal byte this[int color] => calculated ? (byte)Calculate(color) : inputs[color];

    /// <summary>Calculates one head, plate, tissue, or tail shade from the selected material profile.</summary>
    /// <param name="color">Palette color index whose channel value is required.</param>
    /// <returns>The reconstructed channel component.</returns>
    private int Calculate(int color)
    {
        if (color < Layout.PlateStart)
            return profile switch
            {
                MotherBrainRainbowShadeProfile.MixedBlue => inputs[0],
                MotherBrainRainbowShadeProfile.MaximumBlue => inputs[0] - color / Layout.DarkHead * MotherBrainRainbowShadeDefinitions.BlueHeadStep,
                _ => inputs[0] - color * MotherBrainRainbowShadeDefinitions.GreenHeadStep,
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
                _ => Rounding.NearestUp,
            };
            return Interpolate(inputs[3], inputs[4], color - Layout.TissueStart, Layout.TissueIntervals, rounding);
        }
        if (color == Layout.TailStart + 1 && SharesDarkHeadTail(profile))
            return Calculate(Layout.PlateStart - 1);
        return inputs[5 + color - Layout.TailStart];
    }

    // Exposed-head outline shares the darkest cortex channel in these native phases.
    /// <summary>Reports whether a profile derives the exposed-head outline from the darkest cortex channel.</summary>
    /// <param name="profile">Material profile being checked.</param>
    /// <returns><see langword="true"/> for phases with the shared dark head and tail value.</returns>
    private static bool SharesDarkHeadTail(MotherBrainRainbowShadeProfile profile) =>
        profile is MotherBrainRainbowShadeProfile.FirstGreenRise or MotherBrainRainbowShadeProfile.MixedBlue or MotherBrainRainbowShadeProfile.MaximumBlue;

    /// <summary>Integer rounding rules used when interpolating channel values between supplied shade anchors.</summary>
    private enum Rounding
    {
        /// <summary>Discards the fractional part toward the lower integer.</summary>
        Floor,

        /// <summary>Rounds any nonintegral value to the next higher integer.</summary>
        Ceiling,

        /// <summary>Rounds to the nearest integer, resolving exact halves upward.</summary>
        NearestUp,

        /// <summary>Rounds to the nearest integer, resolving exact halves toward an even result.</summary>
        NearestEven
    }

    /// <summary>Interpolates between two sampled channel endpoints using the selected integer rounding policy.</summary>
    /// <param name="first">Channel value at the beginning of the ramp.</param>
    /// <param name="last">Channel value at the end of the ramp.</param>
    /// <param name="shade">Zero-based position within the ramp.</param>
    /// <param name="intervals">Number of steps between the endpoint samples.</param>
    /// <param name="rounding">Rule used to convert the interpolated rational value to an integer.</param>
    /// <returns>The interpolated channel component.</returns>
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
