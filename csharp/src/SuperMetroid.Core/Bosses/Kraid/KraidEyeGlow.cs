using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eyes as they glow and fade after a hit (#1512), a compatibility service: the game reads its eye colours back to
/// decide when the glow has peaked and when the eyes are back to his health band's, which times his roar, so this owns
/// the three eye colours and evolves them exactly as the game does, counting them in the levels the game authored them
/// in (<see cref="FadeLevelColour"/>). A glow raises each eye's red and green one level an update, a glow that starts
/// mid-fade starting from the colours the fade left, and the fade lowers each channel a level toward the band's,
/// wrapping from none to full, so when the eyes are back to the band's depends on those levels. A health colour update
/// shows the band's eyes at once. Evidence: the original glow and fade routines, which step the eye colours' channels
/// one level an update and test them for the peak and for the band's colours; the Kraid replays confirm the roar's
/// timing.
/// </summary>
internal sealed class KraidEyeGlow
{
    /// <summary>The eyes Kraid has.</summary>
    internal const int Eyes = 3;

    /// <summary>The channels of the three eyes that reach their peak, red and green each, when the glow has peaked.</summary>
    private const int PeakChannels = 2 * Eyes;

    /// <summary>The level from which one glow step reaches the peak.</summary>
    private const int NearPeak = 30;

    /// <summary>The eye colours shown now.</summary>
    internal readonly RgbColor[] shown = new RgbColor[Eyes];


    /// <summary>Shows <paramref name="bandEyes"/>, his health band's eye colours, at once, as a health colour update writes them.</summary>
    internal void ShowBand(ReadOnlySpan<RgbColor> bandEyes)
    {
        Ensure.LengthEqual(bandEyes, Eyes);
        bandEyes.CopyTo(shown);
    }

    /// <summary>Raises each eye's red and green one level, from whatever they show now.</summary>
    /// <returns>Whether every red and green channel has reached its peak.</returns>
    internal bool Glow()
    {
        int peaked = 0;
        for (int eye = 0; eye < Eyes; eye++)
        {
            FadeLevelColour levels = shown[eye].FadeLevels;
            int red = levels.Red >= NearPeak ? FadeLevelColour.FullLevel : levels.Red + 1;
            int green = levels.Green >= NearPeak ? FadeLevelColour.FullLevel : levels.Green + 1;
            peaked += (levels.Red >= NearPeak ? 1 : 0) + (levels.Green >= NearPeak ? 1 : 0);
            shown[eye] = levels.WithRed(red).WithGreen(green).ToColour();
        }
        return peaked >= PeakChannels;
    }

    /// <summary>
    /// Lowers each eye's red and green one level toward <paramref name="targets"/>, his health band's eye colours, each
    /// channel that differs wrapping from none to full.
    /// </summary>
    /// <returns>Whether no channel differed, so the eyes show the band's.</returns>
    internal bool Unglow(ReadOnlySpan<RgbColor> targets)
    {
        Ensure.LengthEqual(targets, Eyes);
        int changed = 0;
        for (int eye = 0; eye < Eyes; eye++)
        {
            FadeLevelColour current = shown[eye].FadeLevels;
            FadeLevelColour target = targets[eye].FadeLevels;
            int red = current.Red, green = current.Green;
            if (red != target.Red)
            {
                red = LevelBelow(red);
                changed++;
            }
            if (green != target.Green)
            {
                green = LevelBelow(green);
                changed++;
            }
            shown[eye] = current.WithRed(red).WithGreen(green).ToColour();
        }
        return changed == 0;
    }

    /// <summary>The level one below <paramref name="level"/>, none wrapping to full.</summary>
    private static int LevelBelow(int level) => level == 0 ? FadeLevelColour.FullLevel : level - 1;

    /// <summary>
    /// The effect showing the eyes' colours over his body's picture, whose shade shows them in <paramref name="shaded"/>:
    /// each shaded eye colour shows the eye's colour now, or null while they are the shaded colours.
    /// </summary>
    internal ColourSubstitution? Effect(ReadOnlySpan<RgbColor> shaded)
    {
        Ensure.LengthEqual(shaded, Eyes);
        return shaded.SequenceEqual(shown) ? null : new ColourSubstitution([.. shaded], [.. shown]);
    }
}
