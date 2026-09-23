using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports compressed library-background tilemaps as editable ordered tile references.</summary>
public static class RoomBackgroundTilemapExtractor
{
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        IReadOnlyList<int> addresses = RoomBackgroundTilemapSources.All;
        if (addresses.Count != RoomBackgroundTilemapFormat.RetailCompressedSourceCount)
            throw new InvalidDataException(
                $"Expected {RoomBackgroundTilemapFormat.RetailCompressedSourceCount} " +
                $"retail compressed background sources, found {addresses.Count}.");
        var files = new Dictionary<string, byte[]>();
        foreach (int source in addresses)
        {
            byte[] native = RomDataReader.Decompress(bus, source);
            files.Add(RoomBackgroundTilemapFormat.SourceFileName(source), Encode(native));
        }
        return files;
    }

    /// <summary>Converts one or two native 32x32 tilemap pages without interpreting their room geometry.</summary>
    public static byte[] Encode(ReadOnlySpan<byte> native)
    {
        int pageCount = RoomBackgroundTilemapFormat.ValidatePageCount(native.Length);
        var pages = new RoomBackgroundTilemapPage[pageCount];
        for (int page = 0; page < pageCount; page++)
        {
            var cells = new RoomBackgroundTilemapCell[RoomBackgroundTilemapFormat.CellsPerPage];
            for (int cell = 0; cell < cells.Length; cell++)
            {
                int offset = (page * cells.Length + cell) * sizeof(ushort);
                var word = new SnesBgTilemapWord(
                    BinaryPrimitives.ReadUInt16LittleEndian(native[offset..]));
                cells[cell] = new RoomBackgroundTilemapCell
                {
                    TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                    TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                    Palette = word.PaletteIndex,
                    Priority = word.HasPriority,
                    FlipX = word.FlipHorizontally,
                    FlipY = word.FlipVertically,
                };
            }
            pages[page] = new RoomBackgroundTilemapPage { Cells = cells };
        }
        using var json = new MemoryStream();
        RoomBackgroundTilemapAtlas.Write(json,
            new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = pages,
            }, native.Length);
        byte[] serialized = json.ToArray();
        RoomBackgroundTilemapAtlas roundtrip = RoomBackgroundTilemapAtlas.Load(
            new MemoryStream(serialized, writable: false), native.Length);
        if (!roundtrip.Transfer.Span.SequenceEqual(native))
            throw new InvalidDataException("Room background tilemap JSON changed native tile words.");
        return serialized;
    }
}
