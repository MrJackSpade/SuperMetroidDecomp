using RedOrigin = SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork.MotherBrainRainbowPaletteGeometry.RedOrigin;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Exact supplied-match material ramps. Retained endpoints, independent samples and
/// selected profile/rounding policies specify the reviewed temporal material paint.
/// The rainbow palette step only advances palette selection and copies colors; these
/// choices paint the cortex, plate, tissue, glint and exposed-head outline masks.
/// A different generated hue performance would invent different content. Shared
/// channels and interpolated shades still calculate; arbitrary supplied edits stay exact.
/// </summary>
internal sealed class MotherBrainRainbowShadeChannel
{
    /// <summary>Phase's selected head, plate, tissue, and tail shade rule.</summary>
    private readonly MotherBrainRainbowShadeProfile profile;
    /// <summary>Existing shared green channel used by the recovering phase.</summary>
    private readonly Func<int, int> sourceGreen;
    /// <summary>Independent shade samples, or every supplied sample when they differ from the reviewed rule.</summary>
    internal readonly int[] inputs;
    /// <summary>Whether every supplied shade matches the reviewed interpolation profile.</summary>
    private readonly bool calculated;

    /// <summary>Keeps the phase's profile and either its independent samples or every supplied sample.</summary>
    /// <param name="profile">The phase's shade rule.</param>
    /// <param name="sourceGreen">The shared green channel the recovering phase reads.</param>
    /// <param name="inputs">The independent samples when <paramref name="calculated"/>, otherwise every supplied sample.</param>
    /// <param name="calculated">Whether every supplied shade follows the profile.</param>
    internal MotherBrainRainbowShadeChannel(MotherBrainRainbowShadeProfile profile, Func<int, int> sourceGreen, int[] inputs,
        bool calculated)
    {
        this.profile = profile;
        this.sourceGreen = sourceGreen;
        this.inputs = inputs;
        this.calculated = calculated;
    }

    /// <summary>Uses the calculated shade for a matching phase, or the exact supplied edit otherwise.</summary>
    internal int this[int color] => calculated ? Calculate(color) : inputs[color];

    /// <summary>Applies the phase's head ramp, plate and tissue rounding, and shared tail rule.</summary>
    internal int Calculate(int color)
    {
        if (color < RedOrigin.PlateStart)
            return profile switch
            {
                MotherBrainRainbowShadeProfile.MixedBlue => inputs[0],
                MotherBrainRainbowShadeProfile.MaximumBlue => inputs[0] - color / RedOrigin.DarkHead * MotherBrainRainbowShadeDefinitions.BlueHeadStep,
                MotherBrainRainbowShadeProfile.FirstGreenRise or MotherBrainRainbowShadeProfile.SecondGreenRise or
                    MotherBrainRainbowShadeProfile.RecoveringGreen => inputs[0] - color * MotherBrainRainbowShadeDefinitions.GreenHeadStep,
                _ => throw new InvalidOperationException($"Undefined rainbow shade profile {profile}."),
            };
        if (profile == MotherBrainRainbowShadeProfile.RecoveringGreen)
            return color is RedOrigin.TissueStart + 2 or RedOrigin.TissueStart + 3
                ? inputs[color - RedOrigin.TissueStart - 1] : sourceGreen(color);
        if (color < RedOrigin.TissueStart)
        {
            Rounding rounding = profile == MotherBrainRainbowShadeProfile.MixedBlue ? Rounding.NearestUp : Rounding.Ceiling;
            return Interpolate(inputs[1], inputs[2], color - RedOrigin.PlateStart, RedOrigin.PlateIntervals, rounding);
        }
        if (color < RedOrigin.TailStart)
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
            return Interpolate(inputs[3], inputs[4], color - RedOrigin.TissueStart, RedOrigin.TissueIntervals, rounding);
        }
        if (color == RedOrigin.TailStart + 1 && SharesDarkHeadTail(profile))
            return Calculate(RedOrigin.PlateStart - 1);
        return inputs[5 + color - RedOrigin.TailStart];
    }

    /// <summary>Identifies phases where the exposed-head outline shares the darkest cortex channel.</summary>
    internal static bool SharesDarkHeadTail(MotherBrainRainbowShadeProfile profile) =>
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
