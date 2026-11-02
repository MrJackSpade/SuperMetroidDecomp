using System.Buffers.Binary;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The ending colour resources as the installed palette's little-endian colour words, which only the import reads (#1502).</summary>
public static class EndingPaletteWords
{
    /// <summary>Byte/export view, for the content identity: a whole screen's background sets' colour words then its sprite
    /// sets', a run's words in order, or the crossfade's steps in order, each step's background colours before its sprite
    /// colours. A computed fade is materialized only for this request, never stored as a generated lookup cache.</summary>
    /// <param name="palette">The ending colour resource.</param>
    public static ReadOnlyMemory<byte> Of(EndingPalette palette)
    {
        Ensure.NotNull(palette);
        if (palette.screen is not null) return Of([.. palette.screen.Backgrounds, .. palette.screen.Sprites]);
        if (palette.colorBytes is not null) return palette.colorBytes;
        var colours = new List<RgbColor>(EndingLogoPaletteFade.ColorCount);
        for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
        foreach (EndingLogoLayer layer in Enum.GetValues<EndingLogoLayer>())
        for (int colour = 0; colour < EndingLogoPaletteFade.ColoursPerLayer; colour++)
            colours.Add(palette.LogoCrossfadeColour(step, layer, colour));
        return Of([.. colours]);
    }

    /// <summary><paramref name="colours"/> as little-endian colour words.</summary>
    /// <param name="colours">The colours in resource order.</param>
    internal static byte[] Of(ReadOnlySpan<RgbColor> colours)
    {
        var words = new byte[colours.Length * RgbColor.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(index * RgbColor.ByteCount), colours[index].ToWord());
        return words;
    }
}
