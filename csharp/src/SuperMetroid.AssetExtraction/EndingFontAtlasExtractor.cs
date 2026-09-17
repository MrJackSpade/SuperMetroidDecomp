using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the ending's 160 contiguous 4-bpp font and subtitle tiles as indexed PNG.</summary>
public static class EndingFontAtlasExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] decoded = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.EndingFontCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        if (decoded.Length < EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font expanded to {decoded.Length} bytes; expected at least {EndingFontAtlasFormat.ByteCount}.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(
            decoded.AsSpan(0, EndingFontAtlasFormat.ByteCount),
            EndingFontAtlasFormat.BitsPerPixel,
            EndingFontAtlasFormat.TilesPerRow,
            out int width,
            out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(EndingFontAtlasFormat.ColorCount));
        return output.ToArray();
    }
}
