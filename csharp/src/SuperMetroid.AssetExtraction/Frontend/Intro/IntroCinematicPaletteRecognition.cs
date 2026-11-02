using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Recognises the opening narration's installed colours (#1502): the compiled stock paint, or each colour set held as
/// exact colours or the neutral-cycle and shared cross-fade relationships that reproduce them.
/// </summary>
internal static class IntroCinematicPaletteRecognition
{
    /// <summary>
    /// The colours whose sprite set repeats the neutral ramp; its set may be held as that ramp.
    /// </summary>
    private static readonly ScreenColourGroup NeutralRamp = ScreenColourGroup.IntroNarrationNeutralRamp;

    /// <summary>
    /// The colours whose sprite set cross-fades: its first eight visible colours may repeat the sepia
    /// gradient of <see cref="CrossFadeGradientSource"/>.
    /// </summary>
    private static readonly ScreenColourGroup CrossFade = ScreenColourGroup.IntroCrossFadeObjects;

    /// <summary>The discovery scene's background colours, whose sepia gradient the cross-fade repeats.</summary>
    private static readonly ScreenColourGroup CrossFadeGradientSource = ScreenColourGroup.IntroDiscoveryBackground;

    /// <summary>Keeps the opening's selected colours, recognising stock, neutral-cycle and shared cross-fade sets.</summary>
    /// <param name="colours">The selected colours by owner, which the import pipeline decoded.</param>
    internal static IntroCinematicPalette Recognise(ScreenColours colours)
    {
        Ensure.NotNull(colours);
        if (MatchesStockPaint(colours))
            return new IntroCinematicPalette([], []);
        var backgroundSets = new IntroColourSet[ColourSets.SetCount];
        for (int set = 0; set < backgroundSets.Length; set++)
            backgroundSets[set] = new IntroColourSet(colours.BackgroundSet(set).ToArray(), IntroColourSetCycle.None, crossFadeSource: null);
        var spriteSets = new IntroColourSet[ColourSets.SetCount];
        for (int set = 0; set < spriteSets.Length; set++)
        {
            var cell = new ScreenColourCell(ScreenColourOwner.Sprites, set, 0);
            IntroColourSetCycle cycle = cell == FirstCell(NeutralRamp)
                ? IntroColourSetCycle.NeutralRamp : IntroColourSetCycle.None;
            IntroColourSet? crossFadeSource = cell == FirstCell(CrossFade)
                ? SetHolding(CrossFadeGradientSource, backgroundSets, spriteSets) : null;
            spriteSets[set] = new IntroColourSet(colours.SpriteSet(set).ToArray(), cycle, crossFadeSource);
        }
        return new IntroCinematicPalette(backgroundSets, spriteSets);
    }

    /// <summary>The decoded set that holds <paramref name="group"/>, a whole set.</summary>
    private static IntroColourSet SetHolding(ScreenColourGroup group, IntroColourSet[] backgroundSets, IntroColourSet[] spriteSets)
    {
        ScreenColourCell first = FirstCell(group);
        return first.Owner == ScreenColourOwner.Backgrounds ? backgroundSets[first.Set] : spriteSets[first.Set];
    }

    /// <summary>Checks every colour of both of the intro screen's owners against the calculated set and ink painting.</summary>
    private static bool MatchesStockPaint(ScreenColours colours)
    {
        for (int set = 0; set < ColourSets.SetCount; set++)
        for (int ink = 0; ink < ColourSets.SetColours; ink++)
        {
            if (colours.BackgroundSet(set)[ink] != IntroCinematicPaint.Background(set, ink) ||
                colours.SpriteSet(set)[ink] != IntroCinematicPaint.Sprite(set, ink))
                return false;
        }
        return true;
    }

    /// <summary>The screen colour cell that holds the first colour of <paramref name="group"/>.</summary>
    private static ScreenColourCell FirstCell(ScreenColourGroup group)
    {
        ScreenColourPlacement placement = ScreenColourGroups.Placement(group);
        return new ScreenColourCell(placement.Owner, placement.Set, placement.Colour);
    }
}
