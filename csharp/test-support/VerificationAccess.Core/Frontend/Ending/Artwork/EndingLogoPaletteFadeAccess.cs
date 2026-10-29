using System.Buffers.Binary;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Verification access: the logo crossfade read at the installed resource's colour positions (#1464), the native
/// boundary at which checks compare it with the cartridge's crossfade words. The resource holds its steps in order, each
/// step's background colours before its sprite colours; production reads it by step and layer.
/// </summary>
internal static class EndingLogoPaletteFadeAccess
{
    /// <summary>The layers each crossfade step holds.</summary>
    private const int LayersPerStep = 2;

    /// <summary>The colours of the whole crossfade resource.</summary>
    private const int ColourCount = EndingLogoDefinitions.PaletteSteps * LayersPerStep * EndingLogoCrossfadeColours.ColoursPerLayer;

    /// <summary>A calculated fade's colours by resource position.</summary>
    extension(EndingLogoPaletteFade self)
    {
        /// <summary>The colour at resource position <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the crossfade.</exception>
        internal RgbColor Color(int index) => At(index, self.Color);
    }

    /// <summary>An installed crossfade palette's colours by resource position.</summary>
    extension(EndingPalette self)
    {
        /// <summary>The logo crossfade's colour at resource position <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the crossfade.</exception>
        internal RgbColor CrossfadeColorAt(int index) => At(index, self.LogoCrossfadeColour);
    }

    /// <summary>The crossfade's colours that <paramref name="words"/>, the resource as little-endian colour words, holds.</summary>
    internal static EndingLogoCrossfadeColours DecodeWords(ReadOnlySpan<byte> words)
    {
        Ensure.LengthEqual(words, ColourCount * RgbColor.ByteCount);
        var colours = new RgbColor[ColourCount];
        for (int index = 0; index < colours.Length; index++)
            colours[index] = RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(words.Slice(index * RgbColor.ByteCount)));
        return EndingLogoCrossfadeImage.Decode(colours);
    }

    /// <summary>The colour at resource position <paramref name="index"/>, read by the step, layer and colour it holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the crossfade.</exception>
    private static RgbColor At(int index, Func<int, EndingLogoLayer, int, RgbColor> colour)
    {
        if ((uint)index >= ColourCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int perStep = LayersPerStep * EndingLogoCrossfadeColours.ColoursPerLayer;
        EndingLogoLayer layer = index % perStep < EndingLogoCrossfadeColours.ColoursPerLayer
            ? EndingLogoLayer.Background : EndingLogoLayer.Sprites;
        return colour(index / perStep, layer, index % EndingLogoCrossfadeColours.ColoursPerLayer);
    }
}
