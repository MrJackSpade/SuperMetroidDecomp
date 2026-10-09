using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports compressed library-background tilemaps as editable ordered tile references.</summary>
public static class RoomBackgroundTilemapExtractor
{
    /// <summary>Decompresses every compiled retail library-background source and exports its ordered BG tilemap pages.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed background tilemaps.</param>
    /// <returns>A new filename-keyed collection of UTF-8 JSON byte arrays, each preserving one or two 32x32 pages of tile references and BG attributes.</returns>
    /// <remarks>Verifies the retail source count and exact native-word roundtrips. Page order is retained without inferring room geometry, and files are not written.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Source coverage, decompression, page size, or tilemap roundtrip validation fails.</exception>
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
            byte[] native = RomDataReader.Decompress(CartridgeImportSource.Require(bus), source);
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
