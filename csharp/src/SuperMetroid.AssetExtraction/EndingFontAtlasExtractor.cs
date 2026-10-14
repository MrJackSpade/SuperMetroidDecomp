using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the ending's 160 contiguous 4-bpp font and subtitle tiles as indexed PNG.</summary>
[CpuBusBoundary("codec: ROM ending 4-bpp font and subtitle tiles → indexed font atlas PNG")]
public static class EndingFontAtlasExtractor
{
    /// <summary>Decompresses the ending font source and converts its first 160 four-bit planar characters into an indexed tile atlas.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the compressed ending font and subtitle characters.</param>
    /// <returns>New indexed PNG bytes preserving character order and pixel indices 0..15, with a diagnostic palette rather than runtime colors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">The compressed data is invalid or expands to fewer than the required character bytes.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] decoded = RomDataReader.Decompress(
            CartridgeImportSource.Require(bus),
            EndingCreditsRomData.Assets.EndingFontCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        if (decoded.Length < EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font expanded to {decoded.Length} bytes; expected at least {EndingFontAtlasFormat.ByteCount}.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(
            decoded.AsSpan(0, EndingFontAtlasFormat.ByteCount),
            EndingFontAtlasFormat.BitsPerPixel,
            EndingFontAtlasFormat.TilesPerRow,
            out Extent16<ImagePixels> size);
        using var output = new MemoryStream();
        IndexedPng.Write(output, size, pixels,
            SnesGraphics.DiagnosticPalette(EndingFontAtlasFormat.ColorCount));
        return output.ToArray();
    }
}
