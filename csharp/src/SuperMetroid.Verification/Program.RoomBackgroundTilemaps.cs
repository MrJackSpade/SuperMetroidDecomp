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

        var compiled = new Dictionary<int, RoomBackgroundTilemapAtlas>();
        foreach (int source in sources)
        {
            byte[] native = RomDataReader.Decompress(bus, source);
            compiled.Add(source, RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(files[RoomBackgroundTilemapFormat.SourceFileName(source)],
                    writable: false), native.Length));
        }
        var catalog = new RoomBackgroundTilemapCatalog(compiled);
        const ushort ceresList = 0xe4a5;
        int ceresSource = LibraryBackgroundSourceInventory.Scan(bus)
            .Single(source => source.ListPointer == ceresList &&
                source.Command == LibraryBackgroundCommand.DecompressToWorkRam).SourceAddress;
        var nativeVram = new SnesVram();
        var selectedVram = new SnesVram();
        LibraryBackgroundLoader.Execute(bus, nativeVram, ceresList, activeDoorPointer: 0);
        LibraryBackgroundLoader.Execute(new BackgroundTilemapReadGuard(bus, ceresSource),
            selectedVram, ceresList, activeDoorPointer: 0, tilemapArt: catalog);
        AssertTrue(nativeVram.Bytes.SequenceEqual(selectedVram.Bytes),
            "Ceres library BG executes installed tilemap with exact VRAM parity and no source reread");

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
        compiled[ceresSource] = modified;
        var editedVram = new SnesVram();
        LibraryBackgroundLoader.Execute(new BackgroundTilemapReadGuard(bus, ceresSource),
            editedVram, ceresList, activeDoorPointer: 0,
            tilemapArt: new RoomBackgroundTilemapCatalog(compiled));
        AssertTrue(nativeVram.ReadWord(0x4800) != editedVram.ReadWord(0x4800) &&
            nativeVram.ReadWord(0x4801) == editedVram.ReadWord(0x4801),
            "edited JSON changes the first displayed Ceres BG tile and not its neighbor");
        firstCell["palette"] = RoomBackgroundTilemapFormat.PaletteCount;
        AssertThrows<InvalidDataException>(() => RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)),
                original.Length),
            "background tilemap rejects an invalid palette selector");
        Console.WriteLine($"  Room background tilemaps: {files.Count} compressed retail sources " +
            "roundtrip byte-exactly; a tile-reference edit changes only its BG word.");
    }

    private sealed class BackgroundTilemapReadGuard(ISnesAddressSpace source, int blockedSource)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address == blockedSource
            ? throw new InvalidOperationException(
                $"Installed room background reread source ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
