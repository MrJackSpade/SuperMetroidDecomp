using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Backgrounds;
using SuperMetroid.Verification.Harness;

/// <summary>Checks extracted compressed room backgrounds and installed Ceres tilemap edits.</summary>
internal static class RoomBackgroundTilemapsVerification
{
    /// <summary>Compares all retail sources, Ceres VRAM parity, and the effect of a single JSON tile edit.</summary>
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
                new MemoryStream(files[RoomBackgroundTilemapFormat.FileName(tilemap)],
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
        LibraryBackgroundReplay.Execute(selectedVram, LibraryBackgroundProgramDefinitions.Get(ceresList), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = catalog });
        AssertTrue(nativeVram.ImageEquals(selectedVram),
            "Ceres library BG executes installed tilemap with exact VRAM parity and no source reread");

        byte[] original = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), ceresSource);
        JsonNode edited = JsonNode.Parse(files[RoomBackgroundTilemapFormat.FileName(ceresTilemap)])
            ?? throw new InvalidDataException("Ceres background JSON is empty.");
        JsonNode firstCell = edited.Required("pages").Required(0).Required("cells").Required(0);
        firstCell["tileColumn"] = (firstCell.Required("tileColumn").GetValue<int>() + 1)
            % RoomBackgroundTilemapFormat.TileColumns;
        RoomBackgroundTilemapAtlas modified = RoomBackgroundTilemapAtlas.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)),
            original.Length);
        AssertTrue(!modified.Bytes.Span[..2].SequenceEqual(original.AsSpan(0, 2)) &&
            modified.Bytes.Span[2..].SequenceEqual(original.AsSpan(2)),
            "editing one background tile reference changes only its native BG word");
        compiled[ceresTilemap] = modified;
        var editedVram = new SnesVram();
        LibraryBackgroundReplay.Execute(editedVram, LibraryBackgroundProgramDefinitions.Get(ceresList), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = new RoomBackgroundTilemaps(compiled) });
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

    /// <summary>Runs the room background extraction and edited-tile check.</summary>
    [VerificationCommand("--room-background-tilemaps")]
    internal static int RoomBackgroundTilemapsCommand()
    {
        Suite(nameof(VerifyRoomBackgroundTilemapExtraction), () => VerifyRoomBackgroundTilemapExtraction());
        return 0;
    }
}
