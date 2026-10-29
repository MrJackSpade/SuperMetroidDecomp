using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Sixteen-step logo RGB5 interpolation, stored as two caller-supplied endpoints.
/// Original $8C:EFE9..F3E8 channels use biased truncation: divide the endpoint times
/// remaining steps by15, rounding up only a remainder of14. Every remainder0..14
/// occurs in the original data and has this same result across both palette families.
/// Thus (channel*remaining+1)/15 reproduces all1536 channels without correction entries.
/// This identifies the exact quantizer, not the historical tool that generated it.
/// Colours are addressed by crossfade step, layer and colour (#1464).</summary>
internal sealed class EndingLogoPaletteFade
{
    /// <summary>The colours of one layer at one crossfade step.</summary>
    internal const int ColoursPerLayer = EndingLogoCrossfadeColours.ColoursPerLayer;
    /// <summary>The layers each crossfade step holds: the background's colours and the sprites'.</summary>
    private const int LayersPerStep = 2;
    /// <summary>Every step's colours of both layers.</summary>
    internal const int ColorCount = EndingLogoDefinitions.PaletteSteps * LayersPerStep * ColoursPerLayer;
    /// <summary>The step whose background colours are the background's endpoint: the last, fully brightened.</summary>
    private const int BackgroundEndpointStep = EndingLogoDefinitions.PaletteSteps - 1;
    /// <summary>The step whose sprite colours are the sprites' endpoint: the first, not yet darkened.</summary>
    private const int SpriteEndpointStep = 0;
    /// <summary>Caller-supplied drawing colors. Stock $8C:EFE9 uses independent yellow
    /// face, blue/cyan edging, orange-sector shades and a dark outline, as confirmed by
    /// the original $99:E089 tiles and final OAM compositions. These selected hues are
    /// retained artwork; numerical cases would merely recite the drawing's palette.
    /// Temporal samples are calculated separately, not covered by this disposition.</summary>
    private readonly RgbColor[] spriteEndpoint;
    /// <summary>Supplied BG colors when they do not match the calculated background gradient.</summary>
    private readonly RgbColor[]? backgroundEndpoint;
    /// <summary>Calculated BG endpoint when the supplied colors follow the stock gradient.</summary>
    private readonly EndingLogoBackgroundPalette? backgroundGradient;

    /// <summary>Separates the supplied sprite endpoint from the background gradient or independent background endpoint.</summary>
    private EndingLogoPaletteFade(RgbColor[] backgroundEndpoint, RgbColor[] spriteEndpoint)
    {
        backgroundGradient = EndingLogoBackgroundPalette.TryCreate(backgroundEndpoint);
        if (backgroundGradient is null) this.backgroundEndpoint = backgroundEndpoint;
        this.spriteEndpoint = spriteEndpoint;
    }

    /// <summary>Recognize the complete supplied progression before replacing its samples.
    /// An independently edited sequence that does not follow this rule remains ordinary
    /// caller-owned palette content. Neither path substitutes stock colors for edits.</summary>
    internal static EndingLogoPaletteFade? TryCreate(EndingLogoCrossfadeColours supplied)
    {
        Ensure.NotNull(supplied);
        var backgroundEndpoint = new RgbColor[ColoursPerLayer];
        var spriteEndpoint = new RgbColor[ColoursPerLayer];
        for (int color = 0; color < ColoursPerLayer; color++)
        {
            backgroundEndpoint[color] = supplied.Colour(BackgroundEndpointStep, EndingLogoLayer.Background, color);
            spriteEndpoint[color] = supplied.Colour(SpriteEndpointStep, EndingLogoLayer.Sprites, color);
        }
        var fade = new EndingLogoPaletteFade(backgroundEndpoint, spriteEndpoint);
        for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
        for (int color = 0; color < ColoursPerLayer; color++)
        {
            if (fade.Color(step, EndingLogoLayer.Background, color) != supplied.Colour(step, EndingLogoLayer.Background, color) ||
                fade.Color(step, EndingLogoLayer.Sprites, color) != supplied.Colour(step, EndingLogoLayer.Sprites, color))
                return null;
        }
        return fade;
    }

    /// <summary>Colour <paramref name="color"/> of <paramref name="layer"/> at crossfade step <paramref name="step"/>: the
    /// background gains brightness while the sprites lose it. Each channel is evaluated directly; no sample cache exists.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step, layer or colour is outside the crossfade.</exception>
    internal RgbColor Color(int step, EndingLogoLayer layer, int color)
    {
        Ensure.BetweenInclusive(0, EndingLogoDefinitions.PaletteSteps - 1, step);
        Ensure.BetweenInclusive(0, ColoursPerLayer - 1, color);
        RgbColor endpoint = layer switch
        {
            EndingLogoLayer.Background => BackgroundEndpoint(color),
            EndingLogoLayer.Sprites => spriteEndpoint[color],
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined logo layer."),
        };
        // The background brightens over the steps; the sprites darken over them.
        int remaining = layer == EndingLogoLayer.Background ? step : BackgroundEndpointStep - step;
        int red = ((endpoint.Red) * remaining + 1) / 15;
        int green = ((endpoint.Green) * remaining + 1) / 15;
        int blue = ((endpoint.Blue) * remaining + 1) / 15;
        return new RgbColor(red, green, blue);
    }

    /// <summary>The background's endpoint colour <paramref name="color"/>: the calculated gradient's, or the supplied one.</summary>
    private RgbColor BackgroundEndpoint(int color) =>
        backgroundGradient is not null ? backgroundGradient.Color(color)
        : backgroundEndpoint is not null ? backgroundEndpoint[color]
        : throw new InvalidOperationException("The ending logo fade has neither a background gradient nor endpoint colors.");
}
