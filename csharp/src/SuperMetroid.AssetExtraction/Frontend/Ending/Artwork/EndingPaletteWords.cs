using System.Buffers.Binary;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The ending colour resources as the installed palette's little-endian colour words, which only the import reads (#1502).</summary>
public static class EndingPaletteWords
{
    /// <summary>Byte/export view, for the content identity: a whole screen's background sets' colour words then its sprite
    /// sets', a run's words in order, or the crossfade's steps in order, each step's background colours before its sprite
    /// colours.</summary>
    /// <param name="palette">The ending colour resource.</param>
    public static ReadOnlyMemory<byte> Of(EndingPalette palette)
    {
        Ensure.NotNull(palette);
        return Of(palette.Colours.AsSpan());
    }

    /// <summary><paramref name="colours"/> as little-endian colour words.</summary>
    /// <param name="colours">The colours in resource order.</param>
    internal static byte[] Of(ReadOnlySpan<RgbColor> colours)
    {
        var words = new byte[colours.Length * ColourWordCodec.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(index * ColourWordCodec.ByteCount), colours[index].ToWord());
        return words;
    }
}
