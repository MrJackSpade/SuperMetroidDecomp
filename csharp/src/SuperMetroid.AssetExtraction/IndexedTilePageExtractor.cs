using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Exports one tile-aligned four-bit cartridge source to indexed PNG and proves
/// that an unedited import restores every source byte. Callers own its identity.
/// </summary>
[CpuBusAccess]
internal static class IndexedTilePageExtractor
{
    /// <summary>Reads a bounded four-bit tile page, emits indexed PNG, and rejects any round trip that changes source bytes.</summary>
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
        RoomCharacterAtlas roundtrip = RoomCharacterAtlasPng.Read(
            new MemoryStream(png, writable: false), byteCount);
        if (!roundtrip.Characters.Span.SequenceEqual(native))
            throw new InvalidDataException(
                $"{identity} changed native tile bytes during PNG extraction.");
        return png;
    }
}
