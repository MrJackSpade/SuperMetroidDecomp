using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eyes as they glow and fade after a hit (#1512), a compatibility service: the game reads its eye colours back to
/// decide when the glow has peaked and when the eyes are back to his health band's, which times his roar, so this owns
/// the three eye colours and evolves them exactly as the game does. A glow raises each eye's red and green one step
/// an update, a glow that starts mid-fade starting from the colours the fade left, and the fade lowers each channel
/// toward the band's, wrapping within its five bits. A health colour update shows the band's eyes at once.
/// </summary>
internal sealed class KraidEyeGlow
{
    /// <summary>The eyes Kraid has.</summary>
    internal const int Eyes = 3;

    /// <summary>The channels of the three eyes that reach their peak, red and green each, when the glow has peaked.</summary>
    private const int PeakChannels = 2 * Eyes;

    /// <summary>The channel value from which one glow step reaches the peak.</summary>
    private const int NearPeak = 30;

    /// <summary>The eye colours shown now.</summary>
    internal readonly RgbColor[] shown = new RgbColor[Eyes];


    /// <summary>Shows <paramref name="bandEyes"/>, his health band's eye colours, at once, as a health colour update writes them.</summary>
    internal void ShowBand(ReadOnlySpan<RgbColor> bandEyes)
    {
        Ensure.LengthEqual(bandEyes, Eyes);
        bandEyes.CopyTo(shown);
    }

    /// <summary>Raises each eye's red and green one step, from whatever they show now.</summary>
    /// <returns>Whether every red and green channel has reached its peak.</returns>
    internal bool Glow()
    {
        int peaked = 0;
        for (int eye = 0; eye < Eyes; eye++)
        {
            RgbColor colour = shown[eye];
            int red = colour.Red >= NearPeak ? RgbColor.MaxChannel : colour.Red + 1;
            int green = colour.Green >= NearPeak ? RgbColor.MaxChannel : colour.Green + 1;
            peaked += (colour.Red >= NearPeak ? 1 : 0) + (colour.Green >= NearPeak ? 1 : 0);
            shown[eye] = new RgbColor(red, green, colour.Blue);
        }
        return peaked >= PeakChannels;
    }

    /// <summary>
    /// Lowers each eye's red and green one step toward <paramref name="targets"/>, his health band's eye colours, each
    /// channel that differs wrapping within its five bits.
    /// </summary>
    /// <returns>Whether no channel differed, so the eyes show the band's.</returns>
    internal bool Unglow(ReadOnlySpan<RgbColor> targets)
    {
        Ensure.LengthEqual(targets, Eyes);
        int changed = 0;
        for (int eye = 0; eye < Eyes; eye++)
        {
            RgbColor current = shown[eye];
            int red = current.Red, green = current.Green;
            if (red != targets[eye].Red)
            {
                red = (red - 1) & RgbColor.MaxChannel;
                changed++;
            }
            if (green != targets[eye].Green)
            {
                green = (green - 1) & RgbColor.MaxChannel;
                changed++;
            }
            shown[eye] = new RgbColor(red, green, current.Blue);
        }
        return changed == 0;
    }

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
