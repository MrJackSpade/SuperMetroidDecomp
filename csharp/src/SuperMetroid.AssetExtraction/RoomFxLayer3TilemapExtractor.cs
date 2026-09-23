using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports all six contiguous bank-$8A room-FX BG3 pages as named tile cells.</summary>
public static class RoomFxLayer3TilemapExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var pages = new Dictionary<string, RoomBackgroundTilemapCell[]>();
        foreach (RoomFxType type in RoomFxLayer3TilemapFormat.Types)
        {
            int source = RoomFxLayer3TilemapFormat.SourceAddress(type);
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                RoomFxRomData.Tables.Layer3TilemapPointers + (int)type);
            if ((RoomFxRomData.Banks.Tilemaps | pointer) != source)
                throw new InvalidDataException(
                    $"Room-FX {type} tilemap pointer ${pointer:X4} does not select ${source:X6}.");
            byte[] native = RomDataReader.ReadFixedBank(bus, source,
                RoomFxLayer3TilemapFormat.PageByteCount);
            var cells = new RoomBackgroundTilemapCell[RoomFxLayer3TilemapFormat.CellsPerPage];
            for (int index = 0; index < cells.Length; index++)
            {
                var word = new SnesBgTilemapWord(
                    BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * sizeof(ushort))));
                cells[index] = new RoomBackgroundTilemapCell
                {
                    TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                    TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                    Palette = word.PaletteIndex,
                    Priority = word.HasPriority,
                    FlipX = word.FlipHorizontally,
                    FlipY = word.FlipVertically,
                };
            }
            pages.Add(type.ToString(), cells);
        }
        using var output = new MemoryStream();
        RoomFxLayer3TilemapCatalog.Write(output, new RoomFxLayer3TilemapDocument
        {
            Version = RoomFxLayer3TilemapFormat.Version,
            Pages = pages,
        });
        return output.ToArray();
    }
}
