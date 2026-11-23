using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Decodes the installed logo crossfade palette into its colours by step and layer (#1464): the one place the
/// crossfade resource's colour order is read. The resource holds its steps in order, each step's background colours
/// before its sprite colours, as the cartridge's sixteen paired transfers lay them out.
/// </summary>
internal static partial class EndingLogoCrossfadeImage
{
    /// <summary>The colours of one layer at one step.</summary>
    private const int ColoursPerLayer = EndingLogoCrossfadeColours.ColoursPerLayer;

    /// <summary>The layers each step holds.</summary>
    private static int LayersPerStep => Layers.Length;

    /// <summary>The colours of the whole crossfade resource.</summary>
    internal static int ColourCount => EndingLogoDefinitions.CrossfadeSteps * LayersPerStep * ColoursPerLayer;

    /// <summary>The crossfade's colours that <paramref name="image"/>, the resource's colours in its own order, holds.</summary>
    /// <exception cref="ArgumentException"><paramref name="image"/> does not hold the whole crossfade.</exception>
    internal static EndingLogoCrossfadeColours Decode(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, ColourCount);
        RgbColor[] colours = image.ToArray();
        return new EndingLogoCrossfadeColours((step, layer, colour) => colours[Position(step, layer, colour)]);
    }

    /// <summary>
    /// The resource's colours in its own order (#2563): the inverse of <see cref="Decode"/>, so the order is stated here alone.
    /// </summary>
    /// <param name="colours">The crossfade's colours by step and layer.</param>
    internal static RgbColor[] Encode(EndingLogoCrossfadeColours colours)
    {
        Ensure.NotNull(colours);
        var image = new RgbColor[ColourCount];
        for (int step = 0; step < EndingLogoDefinitions.CrossfadeSteps; step++)
            foreach (EndingLogoLayer layer in Layers)
                for (int colour = 0; colour < ColoursPerLayer; colour++)
                    image[Position(step, layer, colour)] = colours.Colour(step, layer, colour);
        return image;
    }

    /// <summary>The layers each step holds, in the resource's order.</summary>
    private static readonly EndingLogoLayer[] Layers = [EndingLogoLayer.Background, EndingLogoLayer.Sprites];

    /// <summary>Where the resource holds colour <paramref name="colour"/> of <paramref name="layer"/> at step <paramref name="step"/>.</summary>
    internal static int Position(int step, EndingLogoLayer layer, int colour)
    {
        return (step * LayersPerStep + Array.IndexOf(Layers, layer)) * ColoursPerLayer + colour;
    }
}
