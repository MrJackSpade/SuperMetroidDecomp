using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Exports one tile-aligned four-bit cartridge source to indexed PNG and proves
/// that an unedited import restores every source byte. Callers own its identity.
/// </summary>
[CpuBusAccess]
internal static class IndexedTilePageExtractor
{
    /// <summary>extract.</summary>
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
        byte[] png = output.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(png, writable: false), byteCount);
        if (!roundtrip.Transfer.Span.SequenceEqual(native))
            throw new InvalidDataException(
                $"{identity} changed native tile bytes during PNG extraction.");
        return png;
    }
}
