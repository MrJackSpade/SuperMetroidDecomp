using System.Collections.Immutable;
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

    /// <summary>The installed background colours, eight sets of sixteen in order (#1512).</summary>
    public ImmutableArray<RgbColor> BackgroundColours => Colours(backgroundSets, IntroCinematicPaint.Background);

    /// <summary>The installed sprite colours, eight sets of sixteen in order (#1512).</summary>
    public ImmutableArray<RgbColor> SpriteColours => Colours(spriteSets, IntroCinematicPaint.Sprite);

    /// <summary>The colour the opening shows where nothing is drawn: its first background colour (#1512).</summary>
    public RgbColor Backdrop => backgroundSets.Length == 0 ? IntroCinematicPaint.Background(0, 0) : backgroundSets[0].Resolve(0);

    /// <summary>Each colour of <paramref name="sets"/>, or of the compiled stock paint <paramref name="stock"/> when none were decoded.</summary>
    private static ImmutableArray<RgbColor> Colours(IntroColourSet[] sets, Func<int, int, RgbColor> stock)
    {
        var colours = ImmutableArray.CreateBuilder<RgbColor>(ImageColourGroup.Count * ImageColourGroup.Colours);
        for (int set = 0; set < ImageColourGroup.Count; set++)
        for (int color = 0; color < ImageColourGroup.Colours; color++)
            colours.Add(sets.Length == 0 ? stock(set, color) : sets[set].Resolve(color));
        return colours.MoveToImmutable();
    }
}
