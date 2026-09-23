using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports all five simple bank-$87 room-FX animation strips in native frame order.</summary>
public static class RoomFxAnimatedTileAtlasExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        using var planar = new MemoryStream();
        foreach (RoomFxAnimatedTileObjectDefinition definition in
                 RoomFxAnimatedTileMechanicsDefinitions.All)
        foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
        {
            int source = RoomFxAnimatedTileArtworkDefinitions.SourceAddress(
                definition, frame.InstructionPointer);
            planar.Write(RomDataReader.ReadFixedBank(bus, source, definition.TransferByteCount));
        }
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
