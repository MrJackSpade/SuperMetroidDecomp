using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// Builds the ending's colour resources from the colours the import decoded (#1502): a whole screen's by owner, a run of
/// colours, or the logo crossfade's, recognising the calculated crossfade when every supplied colour follows it.
/// </summary>
public static class EndingPaletteRecognition
{
    /// <summary>The step whose sprite colours are the sprites' endpoint: the first, not yet darkened.</summary>
    private const int SpriteEndpointStep = 0;

    /// <summary>Keeps a whole ending screen's selected colours, which the import pipeline decoded by owner.</summary>
    /// <param name="colours">The screen's colours by owner.</param>
    public static EndingPalette FromScreen(ScreenColours colours) => new(Ensure.NotNull(colours));

    /// <summary>Keeps one ending colour run's selected colours.</summary>
    /// <param name="colours">The selected colours.</param>
    public static EndingPalette FromRun(ReadOnlySpan<RgbColor> colours) => new(EndingPaletteWords.Of(colours));

    /// <summary>
    /// Keeps the logo crossfade's selected colours, which the import pipeline decoded by step and layer, recognising the
    /// calculated fade when its colours follow it.
    /// </summary>
    /// <param name="colours">The crossfade's colours.</param>
    internal static EndingPalette FromCrossfade(EndingLogoCrossfadeColours colours) =>
        LogoFade(colours) is { } fade ? new EndingPalette(fade) : new EndingPalette(colours);

    /// <summary>Recognize the complete supplied progression before replacing its samples.
    /// An independently edited sequence that does not follow this rule remains ordinary
    /// caller-owned palette content. Neither path substitutes stock colors for edits.</summary>
    /// <param name="supplied">The crossfade's supplied colours by step and layer.</param>
    /// <returns>The calculated fade that reproduces every supplied colour, or null.</returns>
    internal static EndingLogoPaletteFade? LogoFade(EndingLogoCrossfadeColours supplied)
    {
        Ensure.NotNull(supplied);
        var backgroundEndpoint = new RgbColor[EndingLogoPaletteFade.ColoursPerLayer];
        var spriteEndpoint = new RgbColor[EndingLogoPaletteFade.ColoursPerLayer];
        for (int color = 0; color < EndingLogoPaletteFade.ColoursPerLayer; color++)
        {
            backgroundEndpoint[color] = supplied.Colour(EndingLogoPaletteFade.BackgroundEndpointStep, EndingLogoLayer.Background, color);
            spriteEndpoint[color] = supplied.Colour(SpriteEndpointStep, EndingLogoLayer.Sprites, color);
        }
        var fade = new EndingLogoPaletteFade(backgroundEndpoint, spriteEndpoint);
        for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
        for (int color = 0; color < EndingLogoPaletteFade.ColoursPerLayer; color++)
        {
            if (fade.Color(step, EndingLogoLayer.Background, color) != supplied.Colour(step, EndingLogoLayer.Background, color) ||
                fade.Color(step, EndingLogoLayer.Sprites, color) != supplied.Colour(step, EndingLogoLayer.Sprites, color))
                return null;
        }
        return fade;
    }
}
