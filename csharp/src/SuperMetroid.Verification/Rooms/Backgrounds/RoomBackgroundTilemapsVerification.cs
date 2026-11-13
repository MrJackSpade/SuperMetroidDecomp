using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Backgrounds;
using SuperMetroid.Verification.Harness;

/// <summary>Checks extracted compressed room backgrounds and the installed Ceres tilemap.</summary>
internal static class RoomBackgroundTilemapsVerification
{
    /// <summary>Compares all retail sources, Ceres VRAM parity, and rejection of an invalid palette selector.</summary>
    internal static void VerifyRoomBackgroundTilemapExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        int[] sources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.Command == LibraryBackgroundCommand.Decompress)
            .Select(source => source.SourceAddress).Distinct().Order().ToArray();
        AssertEqual(sources.Length, files.Count,
            "one tilemap resource per distinct retail compressed background source");
        AssertTrue(sources.SequenceEqual(RoomBackgroundSourceDefinitions.Tilemaps.Values.Order()),
            "every retail compressed background source is a named background tilemap");
        // Each named tilemap's installed file (#1414) holds the words its cartridge source decompresses to.
        var compiled = new Dictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas>();
        foreach ((RoomBackgroundTilemap tilemap, int source) in RoomBackgroundSourceDefinitions.Tilemaps)
        {
            byte[] native = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), source);
            RoomBackgroundTilemapAtlas atlas = RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(files[RoomBackgroundTilemapArtworkFiles.FileName(tilemap)],
                    writable: false), native.Length);
            AssertTrue(atlas.Bytes.Span.SequenceEqual(native),
                $"background tilemap {tilemap} preserves all native tile words");
            compiled.Add(tilemap, atlas);
        }
        var catalog = new RoomBackgroundTilemaps(compiled);
        const ushort ceresList = 0xe4a5;
        int ceresSource = LibraryBackgroundSourceInventory.Scan(bus)
            .Single(source => source.ListPointer == ceresList &&
                source.Command == LibraryBackgroundCommand.Decompress).SourceAddress;
        RoomBackgroundTilemap ceresTilemap = LibraryBackgroundOperandDecoding.DecompressedSource(ceresSource).Tilemap;
        var nativeVram = new SnesVram();
        var selectedVram = new SnesVram();
        SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeVram, ceresList, activeDoorPointer: 0);
        LibraryBackgroundReplay.Execute(selectedVram, LibraryBackgroundProgramDefinitions.Get(ceresList), activeDoor: FixtureRooms.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = catalog });
        AssertTrue(nativeVram.ImageEquals(selectedVram),
            "Ceres library BG executes installed tilemap with exact VRAM parity and no source reread");

        JsonNode malformed = JsonNode.Parse(files[RoomBackgroundTilemapArtworkFiles.FileName(ceresTilemap)])
            ?? throw new InvalidDataException("Ceres background JSON is empty.");
        malformed.Required("pages").Required(0).Required("cells").Required(0)["palette"] = RoomBackgroundTilemapFormat.PaletteCount;
        AssertThrows<InvalidDataException>(() => RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(malformed)),
                compiled[ceresTilemap].Bytes.Length),
            "background tilemap rejects an invalid palette selector");
        Console.WriteLine($"  Room background tilemaps: {files.Count} compressed retail sources " +
            "roundtrip byte-exactly.");
    }

    /// <summary>Runs the room background extraction check.</summary>
    [VerificationCommand("--room-background-tilemaps")]
    internal static int RoomBackgroundTilemapsCommand()
    {
        Suite(nameof(VerifyRoomBackgroundTilemapExtraction), () => VerifyRoomBackgroundTilemapExtraction());
        return 0;
    }
}
