using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static class RoomBackgroundTilemapsVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for compressed room background tilemaps")]
    internal static void VerifyRoomBackgroundTilemapExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        int[] sources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
            .Select(source => source.SourceAddress).Distinct().Order().ToArray();
        AssertEqual(RoomBackgroundTilemapFormat.RetailCompressedSourceCount, files.Count,
            "one tilemap resource per distinct retail compressed background source");
        foreach (int source in sources)
        {
            byte[] native = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), source);
            RoomBackgroundTilemapAtlas atlas = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(files[RoomBackgroundTilemapFormat.SourceFileName(source)],
                    writable: false), native.Length);
            AssertTrue(atlas.Transfer.Span.SequenceEqual(native),
                $"background tilemap ${source:X6} preserves all native tile words");
        }

        var compiled = new Dictionary<int, RoomBackgroundTilemapAtlas>();
        foreach (int source in sources)
        {
            byte[] native = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), source);
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
        SuperMetroid.AssetExtraction.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeVram, ceresList, activeDoorPointer: 0);
        LibraryBackgroundLoader.Execute(NewBackgroundTilemapReadGuard(bus, ceresSource), selectedVram, ceresList, activeDoorPointer: 0, artwork: new LibraryBackgroundArtwork { TilemapArt = catalog });
        AssertTrue(nativeVram.ImageEquals(selectedVram),
            "Ceres library BG executes installed tilemap with exact VRAM parity and no source reread");

        byte[] original = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), ceresSource);
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
        LibraryBackgroundLoader.Execute(NewBackgroundTilemapReadGuard(bus, ceresSource), editedVram, ceresList, activeDoorPointer: 0, artwork: new LibraryBackgroundArtwork { TilemapArt = new RoomBackgroundTilemapCatalog(compiled) });
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

    private static MutableCartridgeReadGuard NewBackgroundTilemapReadGuard(ISnesAddressSpace source, int blockedSource)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address == blockedSource,
            address => $"Installed room background reread source ${address:X6}.");
        return guard;
    }

    [VerificationCommand("--room-background-tilemaps")]
    internal static int RoomBackgroundTilemapsCommand()
    {
        Suite(nameof(VerifyRoomBackgroundTilemapExtraction), () => VerifyRoomBackgroundTilemapExtraction());
        return 0;
    }
}
