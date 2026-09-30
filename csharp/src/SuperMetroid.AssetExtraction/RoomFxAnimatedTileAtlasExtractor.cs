using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports bank-$87 room-FX, Wrecked Ship treadmill, and Tourian statue characters.</summary>
public static class RoomFxAnimatedTileAtlasExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        using var planar = new MemoryStream();
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
            planar.Write(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                segment.SourceAddress, segment.ByteCount));
        if (planar.Length != RoomFxAnimatedTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Room-FX animation art has {planar.Length} bytes, expected {RoomFxAnimatedTileAtlasFormat.TotalByteCount}.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar.ToArray(),
            RoomFxAnimatedTileAtlasFormat.BitsPerPixel,
            RoomFxAnimatedTileAtlasFormat.TileCount, out int width, out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(RoomFxAnimatedTileAtlasFormat.ColorCount));
        return output.ToArray();
    }
}
