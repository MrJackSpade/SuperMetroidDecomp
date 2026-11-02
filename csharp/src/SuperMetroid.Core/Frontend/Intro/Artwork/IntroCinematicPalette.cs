using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The opening narration's full, editable screen colours (#1464): its background sets and sprite sets, each set held as
/// exact colours or the authored relationship that reproduces them.
/// </summary>
public sealed class IntroCinematicPalette
{
    /// <summary>Decoded background sets; empty when the installed colours match the compiled stock paint.</summary>
    internal readonly IntroColourSet[] backgroundSets;

    /// <summary>Decoded sprite sets; empty when the installed colours match the compiled stock paint.</summary>
    internal readonly IntroColourSet[] spriteSets;

    /// <summary>Retains the colour sets the import recognised in the opening's selected colours.</summary>
    /// <param name="backgroundSets">The decoded background sets, or none when the colours match the compiled stock paint.</param>
    /// <param name="spriteSets">The decoded sprite sets, or none when the colours match the compiled stock paint.</param>
    internal IntroCinematicPalette(IntroColourSet[] backgroundSets, IntroColourSet[] spriteSets)
    {
        this.backgroundSets = backgroundSets;
        this.spriteSets = spriteSets;
    }

    /// <summary>The installed colours by owner.</summary>
    public ScreenColours Colours
    {
        get
        {
            var screen = new ScreenPalette();
            for (int set = 0; set < ColourSets.SetCount; set++)
            for (int color = 0; color < ColourSets.SetColours; color++)
            {
                screen.BackgroundSets.Set(set).Write(color,
                    backgroundSets.Length == 0 ? IntroCinematicPaint.Background(set, color) : backgroundSets[set].Resolve(color));
                screen.SpriteSets.Set(set).Write(color,
                    spriteSets.Length == 0 ? IntroCinematicPaint.Sprite(set, color) : spriteSets[set].Resolve(color));
            }
            return ScreenColours.Of(screen);
        }
    }

    /// <summary>Replaces every colour of the screen with the installed opening palette; does not advance or reset cinematic fade timers.</summary>
    /// <param name="colours">The screen's colours, every one of which the palette replaces.</param>
    public void LoadTo(ScreenPalette colours) => Colours.LoadTo(colours);
}
