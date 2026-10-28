using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Exports compressed library-background tilemaps as editable ordered tile references.</summary>
public static class RoomBackgroundTilemapExtractor
{
    /// <summary>Decompresses every background tilemap and exports its ordered BG tilemap pages.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed background tilemaps.</param>
    /// <returns>A new filename-keyed collection of UTF-8 JSON byte arrays, each preserving one or two 32x32 pages of tile references and BG attributes.</returns>
    /// <remarks>Verifies exact native-word roundtrips. Page order is retained without inferring room geometry, and files are not written.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Decompression, page size, or tilemap roundtrip validation fails.</exception>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var files = new Dictionary<string, byte[]>();
        foreach ((RoomBackgroundTilemap tilemap, int source) in RoomBackgroundSourceDefinitions.Tilemaps.OrderBy(pair => pair.Key))
        {
            byte[] native = RomDataReader.Decompress(CartridgeImportSource.Require(bus), source);
            files.Add(RoomBackgroundTilemapFormat.FileName(tilemap), Encode(native));
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
                cells[cell] = BackgroundTileCells.Of(BinaryPrimitives.ReadUInt16LittleEndian(native[offset..]));
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
        if (!roundtrip.Tiles.Span.SequenceEqual(BackgroundTileWords.ToTiles(native)))
            throw new InvalidDataException("Room background tilemap JSON changed native tiles.");
        return serialized;
    }
}
