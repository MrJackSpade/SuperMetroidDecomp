using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the English opening's 144 contiguous 2-bpp font tiles.</summary>
public static class IntroFontAtlasExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] decoded = RomDataReader.Decompress(
            bus,
            IntroCinematicRomData.Assets.FontOne,
            maximumOutputBytes: IntroCinematicRomData.Vram.FontOneBytes);
        if (decoded.Length < IntroFontAtlasFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Opening font expanded to {decoded.Length} bytes; expected at least " +
                $"{IntroFontAtlasFormat.ByteCount}.");
        }
        byte[] pixels = SnesGraphics.DecodePlanarTiles(
            decoded.AsSpan(0, IntroFontAtlasFormat.ByteCount),
            IntroFontAtlasFormat.BitsPerPixel,
            IntroFontAtlasFormat.TilesPerRow,
            out int width,
            out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(IntroFontAtlasFormat.ColorCount));
        return output.ToArray();
    }
}
