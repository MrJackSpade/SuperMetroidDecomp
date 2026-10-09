using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the timer's contiguous twenty-five 4-bpp OBJ characters as indexed PNG.</summary>
public static class EscapeTimerTileAtlasExtractor
{
    /// <summary>Converts the escape timer's 25 contiguous four-bit planar OBJ characters into an indexed atlas.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the native timer character span.</param>
    /// <returns>New indexed PNG bytes with the characters in one row and pixel indices 0..15; its diagnostic palette does not replace runtime CGRAM colors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(
                CartridgeImportSource.Require(bus),
                EscapeTimerTileRomData.FirstSourceAddress,
                EscapeTimerTileAtlasFormat.TotalByteCount),
            EscapeTimerTileAtlasFormat.BitsPerPixel,
            EscapeTimerTileAtlasFormat.TileCount,
            out int width,
            out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(EscapeTimerTileAtlasFormat.ColorCount));
        return output.ToArray();
    }
}
