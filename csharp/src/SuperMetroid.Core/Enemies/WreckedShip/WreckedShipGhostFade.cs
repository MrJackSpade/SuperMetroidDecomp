using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's fading colours (#1512): a compatibility value the ghost owns, reproducing the colours the
/// game fades the ghost's palette through, because when each fade ends decides when the ghost's AI moves on.
/// </summary>
/// <remarks>
/// <para>
/// Gameplay behaviour reproduced: the ghost darkens to black when it initializes; each brightening step raises every
/// colour whose red is below full by one level in each channel, carrying a channel that passes full into the next
/// (red into green, green into blue, blue lost), as the game's addition of one level to all three channels of a packed
/// colour does, and the brightening ends on the first step that changes nothing; each fade step moves every channel of
/// every colour one level toward the fade's target and ends on the first step that changes no channel. The AI consults
/// the step results every update of its brightening, materializing and vanishing phases. Evidence: the ghost's
/// brightening and fade routines (bank $A8), whose end conditions the Wrecked Ship ghost replays confirm.
/// </para>
/// <para>
/// The colours themselves are shown as shades the import pre-coloured by running this value along the ghost's one
/// path, so each step's colours are named by the fade and the steps taken in it (<see cref="Shade"/>).
/// </para>
/// </remarks>
public sealed class WreckedShipGhostFade
{
    /// <summary>The colours the ghost's palette holds.</summary>
    public const int ColourCount = 16;

    /// <summary>The highest level of a colour channel.</summary>
    private const int FullLevel = 31;

    /// <summary>The channels a fade step moves, in the order the game steps them.</summary>
    private static readonly ColorChannel[] Channels = [ColorChannel.Red, ColorChannel.Green, ColorChannel.Blue];

    /// <summary>The colours the ghost shows.</summary>
    private readonly RgbColor[] shown = new RgbColor[ColourCount];

    /// <summary>The colours the current fade moves toward.</summary>
    internal readonly RgbColor[] target = new RgbColor[ColourCount];

    /// <summary>The effect the colours were last changed by.</summary>
    private string effect = WreckedShipGhostShades.DarkEffect;

    /// <summary>The steps of <see cref="effect"/> that changed the colours.</summary>
    private int steps;

    /// <summary>The fade the next step toward the target belongs to.</summary>
    private string fade = WreckedShipGhostShades.DarkEffect;

    /// <summary>The colours the ghost shows, colour zero first.</summary>
    public ReadOnlySpan<RgbColor> Shown => shown;

    /// <summary>
    /// The shade naming the colours the ghost shows: the effect that last changed them and how many of its steps did.
    /// A step that changes nothing leaves the name, as it leaves the colours.
    /// </summary>
    public PictureShade Shade => new(effect, steps);

    /// <summary>Darkens every colour, shown and targeted, to black, as the ghost's initialization leaves its palette.</summary>
    public void Darken()
    {
        Array.Fill(shown, RgbColor.Black);
        Array.Fill(target, RgbColor.Black);
        fade = WreckedShipGhostShades.DarkEffect;
        effect = WreckedShipGhostShades.DarkEffect;
        steps = 0;
    }

    /// <summary>
    /// Raises every colour whose red is below full by one level per channel, with the carry between channels, and gives
    /// how many colours changed; none ends the brightening.
    /// </summary>
    public int Brighten()
    {
        int changed = 0;
        for (int colour = 0; colour < ColourCount; colour++)
        {
            RgbColor current = shown[colour];
            if (current.Red >= FullLevel)
                continue;
            shown[colour] = RaisedOneLevel(current);
            changed++;
        }
        if (changed != 0)
            Changed(WreckedShipGhostShades.BrighteningEffect);
        return changed;
    }

    /// <summary>Starts the fade toward the ghost's own colours, which materializes it.</summary>
    public void BeginMaterializing()
    {
        for (int colour = 0; colour < ColourCount; colour++)
            target[colour] = WreckedShipGhostAppearance.PaletteColor(colour);
        fade = WreckedShipGhostShades.MaterializingEffect;
    }

    /// <summary>Starts the fade toward white, which vanishes it.</summary>
    public void BeginVanishing()
    {
        Array.Fill(target, RgbColor.White);
        fade = WreckedShipGhostShades.VanishingEffect;
    }

    /// <summary>Moves every channel of every colour one level toward the target and gives how many channels moved.</summary>
    public int StepTowardTarget()
    {
        int moved = 0;
        for (int colour = 0; colour < ColourCount; colour++)
        {
            RgbColor current = shown[colour];
            RgbColor next = current;
            foreach (ColorChannel channel in Channels)
            {
                int level = next[channel];
                int toward = target[colour][channel];
                if (level == toward)
                    continue;
                next = next.With(channel, level + (level > toward ? -1 : 1));
                moved++;
            }
            shown[colour] = next;
        }
        if (moved != 0)
            Changed(fade);
        return moved;
    }

    /// <summary>Records one more step of <paramref name="name"/> that changed the colours, its first if another effect changed them last.</summary>
    private void Changed(string name)
    {
        if (effect != name)
        {
            effect = name;
            steps = 0;
        }
        steps++;
    }

    /// <summary>
    /// <paramref name="colour"/> one level brighter in each channel, a channel passing full wrapping to zero and carrying
    /// one level into the next, the last channel's carry lost.
    /// </summary>
    private static RgbColor RaisedOneLevel(RgbColor colour)
    {
        int red = colour.Red + 1;
        int green = colour.Green + 1 + red / (FullLevel + 1);
        int blue = colour.Blue + 1 + green / (FullLevel + 1);
        return new RgbColor(red % (FullLevel + 1), green % (FullLevel + 1), blue % (FullLevel + 1));
    }
}
