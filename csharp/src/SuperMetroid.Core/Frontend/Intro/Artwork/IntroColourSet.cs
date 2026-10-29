using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// One of the opening narration's colour sets (#1464), held as exact colours or the authored relationship that
/// reproduces them: a flat fill, the neutral ramp, or the cross-fade that repeats another set's sepia gradient.
/// </summary>
internal sealed class IntroColourSet
{
    /// <summary>Background and common foreground inks for flat or derived neutral-cycle sets.</summary>
    private readonly RgbColor background, foreground;
    /// <summary>Original set colors when they cannot be represented by a stock shade relationship.</summary>
    private readonly RgbColor[]? supplied;
    /// <summary>Whether visible inks follow the four-shade neutral material cycle.</summary>
    private readonly bool cycle;
    /// <summary>The earlier colour set reused by the cross-fade set's first eight visible inks.</summary>
    private readonly IntroColourSet? sharedGradient;
    /// <summary>Independent cross-fade endpoints and trailing inks retained alongside the shared gradient.</summary>
    private readonly RgbColor[]? crossFadeInputs;
    /// <summary>Recognizes exact cross-fade or neutral-cycle relationships before retaining unmatched colors verbatim.</summary>
    internal IntroColourSet(RgbColor[] colors, IntroColourSetCycle allowedCycle, IntroColourSet? crossFadeSource)
    {
        if (crossFadeSource is not null && MatchesCrossFade(colors, crossFadeSource))
        {
            sharedGradient = crossFadeSource;
            crossFadeInputs = [colors[0], colors[9], colors[13], colors[14], colors[15]];
            return;
        }
        background = colors[0];
        foreground = colors[1];
        bool flat = colors.AsSpan(1).IndexOfAnyExcept(foreground) < 0;
        if (flat) return;
        cycle = allowedCycle == IntroColourSetCycle.NeutralRamp;
        for (int color = 1; color < colors.Length; color++)
        {
            if (cycle && colors[color] == CycleColor(color)) continue;
            supplied = colors;
            background = foreground = RgbColor.Black;
            break;
        }
    }
    /// <summary>Checks that the shared gradient and four descending blue inks reproduce the supplied set.</summary>
    private static bool MatchesCrossFade(RgbColor[] colors, IntroColourSet source)
    {
        for (int color = 1; color <= 8; color++)
            if (colors[color] != source.Resolve(color)) return false;
        int blue = colors[9].Blue;
        if (colors[9].Red != 0 || colors[9].Green != 0 || blue < 3 * IntroCinematicPaletteFormat.CrossFadeBlueStep) return false;
        for (int shade = 1; shade < 4; shade++)
            if (colors[9 + shade] != new RgbColor(0, 0, blue - shade * IntroCinematicPaletteFormat.CrossFadeBlueStep)) return false;
        return true;
    }
    /// <summary>Reconstructs a color from retained source inks or its proven cycle/cross-fade relationship.</summary>
    internal RgbColor Resolve(int color)
    {
        if (crossFadeInputs is not null)
        {
            if (color == 0) return crossFadeInputs[0];
            if (color <= 8)
            {
                return (sharedGradient ?? throw new InvalidOperationException(
                    "A cross-faded intro palette requires its shared gradient.")).Resolve(color);
            }
            if (color <= 12) return new RgbColor(0, 0, crossFadeInputs[1].Blue -
                (color - 9) * IntroCinematicPaletteFormat.CrossFadeBlueStep);
            return crossFadeInputs[color - 11];
        }
        return supplied is not null ? supplied[color] :
            color == 0 ? background : cycle ? CycleColor(color) : foreground;
    }
    /// <summary>Decreases each foreground RGB5 channel for its position in the repeating neutral ramp.</summary>
    private RgbColor CycleColor(int color)
    {
        int decrease = (color - 1) % IntroCinematicPaletteFormat.NeutralCycleLength *
            IntroCinematicPaletteFormat.NeutralCycleStep;
        return foreground.Map((_, a) => Math.Max(0, a - decrease));
    }
}
