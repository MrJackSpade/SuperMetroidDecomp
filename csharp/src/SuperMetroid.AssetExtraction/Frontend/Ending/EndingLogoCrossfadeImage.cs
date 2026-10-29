using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Decodes the installed logo crossfade palette into its colours by step and layer (#1464): the one place the
/// crossfade resource's colour order is read. The resource holds its steps in order, each step's background colours
/// before its sprite colours, as the cartridge's sixteen paired transfers lay them out.
/// </summary>
internal static class EndingLogoCrossfadeImage
{
    /// <summary>The colours of one layer at one step.</summary>
    private const int ColoursPerLayer = EndingLogoCrossfadeColours.ColoursPerLayer;

    /// <summary>The layers each step holds.</summary>
    private const int LayersPerStep = 2;

    /// <summary>The colours of the whole crossfade resource.</summary>
    internal const int ColourCount = EndingLogoDefinitions.PaletteSteps * LayersPerStep * ColoursPerLayer;

    /// <summary>The crossfade's colours that <paramref name="image"/>, the resource's colours in its own order, holds.</summary>
    /// <exception cref="ArgumentException"><paramref name="image"/> does not hold the whole crossfade.</exception>
    internal static EndingLogoCrossfadeColours Decode(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, ColourCount);
        RgbColor[] colours = image.ToArray();
        return new EndingLogoCrossfadeColours((step, layer, colour) => colours[Position(step, layer, colour)]);
    }

    /// <summary>The crossfade's colours that <paramref name="words"/>, the resource as little-endian colour words, holds.</summary>
    /// <exception cref="ArgumentException"><paramref name="words"/> does not hold the whole crossfade.</exception>
    internal static EndingLogoCrossfadeColours DecodeWords(ReadOnlySpan<byte> words)
    {
        Ensure.LengthEqual(words, ColourCount * RgbColor.ByteCount);
        var colours = new RgbColor[ColourCount];
        for (int index = 0; index < colours.Length; index++)
            colours[index] = RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(words.Slice(index * RgbColor.ByteCount)));
        return Decode(colours);
    }

    /// <summary>
    /// The colour at resource position <paramref name="index"/>, read from <paramref name="colour"/> by the step, layer and
    /// colour that position holds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the resource.</exception>
    internal static RgbColor At(int index, Func<int, EndingLogoLayer, int, RgbColor> colour)
    {
        Ensure.NotNull(colour);
        if ((uint)index >= ColourCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int step = index / (LayersPerStep * ColoursPerLayer);
        EndingLogoLayer layer = index / ColoursPerLayer % LayersPerStep == 0 ? EndingLogoLayer.Background : EndingLogoLayer.Sprites;
        return colour(step, layer, index % ColoursPerLayer);
    }

    /// <summary>Where the resource holds colour <paramref name="colour"/> of <paramref name="layer"/> at step <paramref name="step"/>.</summary>
    private static int Position(int step, EndingLogoLayer layer, int colour)
    {
        int layerIndex = layer == EndingLogoLayer.Background ? 0 : 1;
        return (step * LayersPerStep + layerIndex) * ColoursPerLayer + colour;
    }
}
