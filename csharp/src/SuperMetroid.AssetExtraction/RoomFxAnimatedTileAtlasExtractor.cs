using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports bank-$87 room-FX, Wrecked Ship treadmill, and Tourian statue characters.</summary>
public static class RoomFxAnimatedTileAtlasExtractor
{
    /// <summary>Concatenates the defined room-FX, treadmill, statue, spore, and spike character segments into their stable atlas order.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the native bank-$87 two-bit planar artwork spans.</param>
    /// <returns>New indexed PNG bytes containing one horizontal character row with pixel indices 0..3 and a diagnostic palette, not runtime colors.</returns>
    /// <remarks>The shared statue strip is stored once; overlapping transfer windows and all animation timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">The imported segments do not total the required planar byte count.</exception>
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
