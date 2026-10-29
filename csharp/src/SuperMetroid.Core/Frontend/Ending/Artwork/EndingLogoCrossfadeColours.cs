using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The ending logo crossfade's supplied colours (#1464): at each of its steps, the logo background's colours and the logo
/// sprites' colours. The import pipeline decodes them from the installed crossfade palette, whose colour order is its
/// own knowledge; the logo reads them by step, layer and colour.
/// </summary>
internal sealed class EndingLogoCrossfadeColours
{
    /// <summary>The colours of one layer at one crossfade step.</summary>
    internal const int ColoursPerLayer = 16;

    /// <summary>The background's colours, step after step.</summary>
    private readonly RgbColor[] background = new RgbColor[EndingLogoDefinitions.PaletteSteps * ColoursPerLayer];

    /// <summary>The sprites' colours, step after step.</summary>
    private readonly RgbColor[] sprites = new RgbColor[EndingLogoDefinitions.PaletteSteps * ColoursPerLayer];

    /// <summary>Takes each step's colours of both layers from <paramref name="colour"/>.</summary>
    /// <param name="colour">The colour of a layer's colour at a step.</param>
    internal EndingLogoCrossfadeColours(Func<int, EndingLogoLayer, int, RgbColor> colour)
    {
        Ensure.NotNull(colour);
        for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
        for (int index = 0; index < ColoursPerLayer; index++)
        {
            background[Cell(step, index)] = colour(step, EndingLogoLayer.Background, index);
            sprites[Cell(step, index)] = colour(step, EndingLogoLayer.Sprites, index);
        }
    }

    /// <summary>Colour <paramref name="colour"/> of <paramref name="layer"/> at crossfade step <paramref name="step"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step, layer or colour is outside the crossfade.</exception>
    internal RgbColor Colour(int step, EndingLogoLayer layer, int colour) => layer switch
    {
        EndingLogoLayer.Background => background[Cell(step, colour)],
        EndingLogoLayer.Sprites => sprites[Cell(step, colour)],
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined logo layer."),
    };

    /// <summary>Where one layer's array holds colour <paramref name="colour"/> of step <paramref name="step"/>.</summary>
    private static int Cell(int step, int colour) =>
        Ensure.BetweenInclusive(0, EndingLogoDefinitions.PaletteSteps - 1, step) * ColoursPerLayer +
        Ensure.BetweenInclusive(0, ColoursPerLayer - 1, colour);
}
