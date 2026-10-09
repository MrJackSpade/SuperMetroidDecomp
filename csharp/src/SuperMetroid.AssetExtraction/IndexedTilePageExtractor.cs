using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Exports one tile-aligned four-bit cartridge source to indexed PNG and proves
/// that an unedited import restores every source byte. Callers own its identity.
/// </summary>
internal static class IndexedTilePageExtractor
{
    /// <summary>Exports a tile-aligned four-bit native character span as indexed PNG and verifies an exact planar-byte round trip.</summary>
    /// <param name="bus">Import address space containing the native character span.</param>
    /// <param name="sourceAddress">SNES CPU address of the first planar character byte.</param>
    /// <param name="byteCount">Number of tile-aligned native bytes to export.</param>
    /// <param name="identity">Human-readable resource identity included in round-trip failures.</param>
    /// <returns>The encoded indexed PNG bytes.</returns>
    internal static byte[] Extract(ISnesAddressSpace bus, int sourceAddress,
        int byteCount, string identity)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        _ = RoomCharacterAtlasFormat.ValidateTileCount(byteCount);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), sourceAddress, byteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(16));
        byte[] png = output.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(png, writable: false), byteCount);
        if (!roundtrip.Transfer.Span.SequenceEqual(native))
            throw new InvalidDataException(
                $"{identity} changed native tile bytes during PNG extraction.");
        return png;
    }
}
