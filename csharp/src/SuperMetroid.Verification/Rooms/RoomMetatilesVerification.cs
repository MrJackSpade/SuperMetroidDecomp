using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Checks the extracted block compositions the block sheets are composed from (#1431) against the compressed cartridge
/// sources and the installed stock compositions.
/// </summary>
internal static class RoomMetatilesVerification
{
    /// <summary>
    /// Roundtrips distinct CRE and area metatiles, checks the installed compositions are the cartridge's, and checks an edited
    /// composition document changes exactly its edited tile.
    /// </summary>
    internal static void VerifyRoomMetatileExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room metatiles: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomMetatileExtractor.Extract(bus);
        var sources = new Dictionary<string, int>
        {
            [RoomMetatileFormat.CreFileName] = (int)RoomTilesetSourceDefinitions.CreBlocks,
        };
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesets.Get(graphicsSet).BlockDefinitionsAddress;
            sources[RoomMetatileFormat.SourceFileName(address)] = address;
        }
        AssertEqual(sources.Count, files.Count,
            "one visual metatile JSON resource per distinct retail source");
        foreach ((string name, int source) in sources)
        {
            byte[] native = RomDataReader.Decompress(bus, source);
            RoomMetatileAtlas atlas = RoomMetatileAtlas.Load(
                new MemoryStream(files[name], writable: false), native.Length);
            AssertTrue(BackgroundTileWordBytes.Of(atlas.BlockTiles.Span).AsSpan().SequenceEqual(native),
                $"metatile source ${source:X6} roundtrips every tile reference, palette, priority and flip");
        }
        var bySource = new Dictionary<int, RoomMetatileAtlas>();
        foreach ((string name, int source) in sources)
        {
            if (name == RoomMetatileFormat.CreFileName) continue;
            int nativeLength = RomDataReader.Decompress(bus, source).Length;
            bySource.Add(source, RoomMetatileAtlas.Load(
                new MemoryStream(files[name], writable: false), nativeLength));
        }
        RoomMetatileAtlas stockCre = RoomMetatileAtlas.Load(
            new MemoryStream(files[RoomMetatileFormat.CreFileName], writable: false),
            RomDataReader.Decompress(bus,
                (int)RoomTilesetSourceDefinitions.CreBlocks).Length);
        var stockCatalog = new RoomMetatileCatalog(stockCre, bySource.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.BlockSetAt(pair.Key), pair => pair.Value));
        // The tiles of every block a room loads, which extraction pictures, are the cartridge's: the installed compositions
        // a Zebes and a Ceres room's block sheets were composed from equal those freshly extracted.
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            SelectedRoom header = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            TilesetDefinition tileset = RoomTilesets.Get(header.State.GraphicsSet);
            bool ceres = header.AreaIndex == AreaId.Ceres;
            AssertTrue(RoomBlockComposition.StockTiles(stockCatalog, tileset.Blocks, ceres).AsSpan().SequenceEqual(
                    InstalledFixtures.StockBlockTiles(tileset, ceres)),
                $"room $8F:{roomPointer:X4}'s blocks are composed from the cartridge's exact visual blocks");
        }

        byte[] cre = RomDataReader.Decompress(bus,
            (int)RoomTilesetSourceDefinitions.CreBlocks);
        JsonNode edited = JsonNode.Parse(files[RoomMetatileFormat.CreFileName])
            ?? throw new InvalidDataException("CRE metatile JSON is empty.");
        JsonNode topLeft = edited.Required("blocks").Required(0).Required("topLeft");
        int originalColumn = topLeft.Required("tileColumn").GetValue<int>();
        topLeft["tileColumn"] = (originalColumn + 1) % RoomMetatileFormat.TileColumns;
        RoomMetatileAtlas modified = RoomMetatileAtlas.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)), cre.Length);
        ExpandedBlockTiles before = LevelBlockTilemapExpander.Expand(RoomBlockAppearance.Of(0), stockCre.BlockTiles.Span);
        ExpandedBlockTiles after = LevelBlockTilemapExpander.Expand(RoomBlockAppearance.Of(0), modified.BlockTiles.Span);
        AssertTrue(before.TopLeft != after.TopLeft && before.TopRight == after.TopRight &&
            before.BottomLeft == after.BottomLeft && before.BottomRight == after.BottomRight &&
            stockCre.BlockTiles.Span[4..].SequenceEqual(modified.BlockTiles.Span[4..]),
            "an edited composition document changes only its edited 8x8 tile");

        topLeft["palette"] = RoomMetatileFormat.PaletteCount;
        byte[] invalid = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited);
        AssertThrows<InvalidDataException>(() => RoomMetatileAtlas.Load(
                new MemoryStream(invalid, writable: false), cre.Length),
            "metatile JSON rejects an invalid palette selector");
        Console.WriteLine($"  Room metatiles: {files.Count} distinct CRE/area resources roundtrip " +
            "byte-exactly and are the installed compositions the block sheets are composed from.");
    }

    /// <summary>Runs the focused metatile extraction and room-loader comparison.</summary>
    [VerificationCommand("--room-metatiles")]
    internal static int RoomMetatilesCommand()
    {
        Suite(nameof(VerifyRoomMetatileExtraction), () => VerifyRoomMetatileExtraction());
        return 0;
    }
}
