using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The colours of Mother Brain's rainbow beam backdrop, which the game reads while the beam fires: its first colour and
/// its editable hue cycle. Her rainbow, drain, revival and restoration colours are the import's, which pre-colours her
/// pictures in them (#1512, #1527).
/// </summary>
public class MotherBrainRainbowPalettePresentation
{
    /// <summary>The backdrop's hue cycle.</summary>
    internal readonly BeamColors beamCycle;

    /// <summary>Keeps the beam backdrop's colours.</summary>
    /// <param name="beamInitial">The beam backdrop's first colour.</param>
    /// <param name="beamCycle">The beam backdrop's hue cycle.</param>
    internal MotherBrainRainbowPalettePresentation(RgbColor beamInitial, BeamColors beamCycle)
    {
        BeamInitialColor = beamInitial;
        this.beamCycle = beamCycle;
    }

    /// <summary>
    /// The rainbow beam's backdrop glow colours:
    /// five linear hue legs, of which the cycle shows every other hue step. The blue-to-magenta leg
    /// delays red by one step and adds a one-step green tint in its upper half.
    /// Falling green/blue changes its rounding bias after local step eight.
    /// These phase boundaries preserve the source's asymmetric wheel exactly.
    /// </summary>
    internal sealed class BeamColors
    {
        /// <summary>Retained only when an edited sample differs from the five-leg wheel.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>The colours in the cycle.</summary>
        public int Length { get; }
        /// <summary>Keeps the cycle's length and, when one sample differs from the wheel, every supplied sample.</summary>
        /// <param name="length">The number of samples.</param>
        /// <param name="supplied">Every supplied sample when one differs from the calculated wheel, otherwise null.</param>
        internal BeamColors(int length, RgbColor[]? supplied)
        {
            Length = length;
            this.supplied = supplied;
        }
        /// <summary>Reads an authored override or reconstructs the matching wheel sample.</summary>
        public RgbColor this[int index] => supplied is null ? Calculate(index) : supplied[index];

        /// <summary>Reconstructs the five hue legs using the game's asymmetric blue-to-magenta shift.</summary>
        internal static RgbColor Calculate(int index)
        {
            int step = index * MotherBrainBeamDefinitions.HueStepsPerColor;
            int red, green, blue;
            if (step <= 15)
            {
                red = 31; green = Rising(step); blue = 0;
            }
            else if (step <= 30)
            {
                red = Rising(30 - step); green = 31; blue = 0;
            }
            else if (step <= 45)
            {
                red = 0; green = Falling(step - 30); blue = Rising(step - 30);
            }
            else if (step < 60)
            {
                int phase = step - 45;
                red = 2 * phase - (phase >= 7 ? 1 : 0);
                green = phase >= 8 ? 1 : 0;
                blue = 31;
            }
            else
            {
                red = 31; green = 0; blue = Falling(step - 60);
            }
            return new FadeLevelColour(red, green, blue).ToColour();
        }
        /// <summary>Rounds an increasing channel to its level.</summary>
        private static int Rising(int step) => (31 * step + 7) / 15;
        /// <summary>Applies the descending channel's changed bias after step eight.</summary>
        private static int Falling(int step) => 31 - 2 * step - (step >= 9 ? 1 : 0);
    }
    /// <summary>Fixed-color backdrop used on the beam effect's first active frame.</summary>
    public RgbColor BeamInitialColor { get; }

    /// <summary>The colours of the beam backdrop's hue cycle; after its last, the beam starts the cycle again.</summary>
    public int BeamCycleLength => beamCycle.Length;

    /// <summary>Beam cycle colour <paramref name="index"/>, zero through one before <see cref="BeamCycleLength"/>.</summary>
    /// <exception cref="InvalidDataException">The index is outside the cycle.</exception>
    public RgbColor BeamColor(int index) =>
        index >= 0 && index < beamCycle.Length
            ? beamCycle[index]
            : throw new InvalidDataException($"Mother Brain beam color {index} is outside its authored cycle.");
}
