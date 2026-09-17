using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the timer's contiguous twenty-five 4-bpp OBJ characters as indexed PNG.</summary>
public static class EscapeTimerTileAtlasExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(
                bus,
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
