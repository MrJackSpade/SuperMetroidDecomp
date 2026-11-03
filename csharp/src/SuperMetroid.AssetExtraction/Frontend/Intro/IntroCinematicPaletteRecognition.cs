using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Recognises the opening narration's installed colours (#1502): the compiled stock paint, or each colour set held as
/// exact colours or the neutral-cycle and shared cross-fade relationships that reproduce them.
/// </summary>
internal static class IntroCinematicPaletteRecognition
{
    /// <summary>The sprite set that repeats the neutral ramp; it may be held as that ramp.</summary>
    private const int NeutralRampSpriteSet = 0;

    /// <summary>
    /// The sprite set that cross-fades: its first eight visible colours may repeat the sepia gradient of
    /// <see cref="CrossFadeGradientBackgroundSet"/>.
    /// </summary>
    private const int CrossFadeSpriteSet = 6;

    /// <summary>The discovery scene's background set, whose sepia gradient the cross-fade repeats.</summary>
    private const int CrossFadeGradientBackgroundSet = 2;

    /// <summary>Keeps the opening's selected colours, recognising stock, neutral-cycle and shared cross-fade sets.</summary>
    /// <param name="image">The selected colours as the palette image holds them: the background sets, then the sprite sets.</param>
    /// <exception cref="InvalidDataException">The image does not hold every background and sprite colour.</exception>
    internal static IntroCinematicPalette Recognise(ReadOnlySpan<RgbColor> image)
    {
        const int ownerColours = ImageColourGroup.Count * ImageColourGroup.Colours;
        if (image.Length != 2 * ownerColours)
            throw new InvalidDataException($"The opening palette holds {image.Length} colours; expected {2 * ownerColours}.");
        RgbColor[] backgrounds = image[..ownerColours].ToArray();
        RgbColor[] sprites = image[ownerColours..].ToArray();
        if (MatchesStockPaint(backgrounds, sprites))
            return new IntroCinematicPalette([], []);
        var backgroundSets = new IntroColourSet[ImageColourGroup.Count];
        for (int set = 0; set < backgroundSets.Length; set++)
            backgroundSets[set] = new IntroColourSet(Set(backgrounds, set), IntroColourSetCycle.None, crossFadeSource: null);
        var spriteSets = new IntroColourSet[ImageColourGroup.Count];
        for (int set = 0; set < spriteSets.Length; set++)
        {
            IntroColourSetCycle cycle = set == NeutralRampSpriteSet ? IntroColourSetCycle.NeutralRamp : IntroColourSetCycle.None;
            IntroColourSet? crossFadeSource = set == CrossFadeSpriteSet ? backgroundSets[CrossFadeGradientBackgroundSet] : null;
            spriteSets[set] = new IntroColourSet(Set(sprites, set), cycle, crossFadeSource);
        }
        return new IntroCinematicPalette(backgroundSets, spriteSets);
    }

    /// <summary>The colours of set <paramref name="set"/> of <paramref name="colours"/>.</summary>
    private static RgbColor[] Set(RgbColor[] colours, int set) =>
        colours.AsSpan(set * ImageColourGroup.Colours, ImageColourGroup.Colours).ToArray();

    /// <summary>Checks every colour of both of the intro screen's owners against the calculated set and ink painting.</summary>
    private static bool MatchesStockPaint(RgbColor[] backgrounds, RgbColor[] sprites)
    {
        for (int set = 0; set < ImageColourGroup.Count; set++)
        for (int ink = 0; ink < ImageColourGroup.Colours; ink++)
        {
            if (backgrounds[set * ImageColourGroup.Colours + ink] != IntroCinematicPaint.Background(set, ink) ||
                sprites[set * ImageColourGroup.Colours + ink] != IntroCinematicPaint.Sprite(set, ink))
                return false;
        }
        return true;
    }
}
