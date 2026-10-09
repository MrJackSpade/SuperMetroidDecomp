using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the English opening's 144 contiguous 2-bpp font tiles.</summary>
public static class IntroFontAtlasExtractor
{
    /// <summary>Decompresses opening font resource $95:D089 and exports its first 144 contiguous English narration glyph tiles.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed FontOne resource.</param>
    /// <returns>A new 128-by-72 indexed PNG buffer with sixteen 8-by-8 two-bit glyphs per row and four diagnostic palette entries.</returns>
    /// <remarks>Only the required 2304 planar bytes are decoded; additional decompressed bytes are not part of this atlas. The diagnostic colors do not replace the cinematic's runtime palette.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">The compressed resource is invalid or expands to fewer than the required glyph bytes.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] decoded = RomDataReader.Decompress(
            CartridgeImportSource.Require(bus),
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
