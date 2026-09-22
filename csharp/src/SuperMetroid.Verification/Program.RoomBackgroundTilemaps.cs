using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRoomBackgroundTilemapExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        int[] sources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
            .Select(source => source.SourceAddress).Distinct().Order().ToArray();
        AssertEqual(RoomBackgroundTilemapFormat.RetailCompressedSourceCount, files.Count,
            "one tilemap resource per distinct retail compressed background source");
        foreach (int source in sources)
        {
            byte[] native = RomDataReader.Decompress(bus, source);
            RoomBackgroundTilemapAtlas atlas = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(files[RoomBackgroundTilemapFormat.SourceFileName(source)],
                    writable: false), native.Length);
            AssertTrue(atlas.Transfer.Span.SequenceEqual(native),
                $"background tilemap ${source:X6} preserves all native tile words");
        }

        int ceresSource = sources[^1];
        byte[] original = RomDataReader.Decompress(bus, ceresSource);
        JsonNode edited = JsonNode.Parse(files[RoomBackgroundTilemapFormat.SourceFileName(ceresSource)])
            ?? throw new InvalidDataException("Ceres background JSON is empty.");
        JsonNode firstCell = edited["pages"]![0]!["cells"]![0]!;
        firstCell["tileColumn"] = (firstCell["tileColumn"]!.GetValue<int>() + 1)
            % RoomBackgroundTilemapFormat.TileColumns;
        RoomBackgroundTilemapAtlas modified = RoomBackgroundTilemapAtlas.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)),
            original.Length);
        AssertTrue(!modified.Transfer.Span[..2].SequenceEqual(original.AsSpan(0, 2)) &&
            modified.Transfer.Span[2..].SequenceEqual(original.AsSpan(2)),
            "editing one background tile reference changes only its native BG word");
        firstCell["palette"] = RoomBackgroundTilemapFormat.PaletteCount;
        AssertThrows<InvalidDataException>(() => RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)),
                original.Length),
            "background tilemap rejects an invalid palette selector");
        Console.WriteLine($"  Room background tilemaps: {files.Count} compressed retail sources " +
            "roundtrip byte-exactly; a tile-reference edit changes only its BG word.");
    }
}
