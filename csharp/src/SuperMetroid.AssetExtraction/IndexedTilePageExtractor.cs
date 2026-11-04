using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Exports one tile-aligned four-bit cartridge source to indexed PNG. Callers own its identity.
/// </summary>
internal static class IndexedTilePageExtractor
{
    /// <summary>Reads a bounded four-bit tile page and emits indexed PNG.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus, int sourceAddress,
        int byteCount, string identity)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        _ = RoomCharacterAtlasFormat.ValidateTileCount(byteCount);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), sourceAddress, byteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var output = new MemoryStream();
        IndexedPng.Write(output, size, pixels,
            SnesGraphics.DiagnosticPalette(16));
        return output.ToArray();
    }
}
