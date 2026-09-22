using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRoomMetatileExtraction()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Room metatiles: private ROM absent; retail audit skipped.");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        IReadOnlyDictionary<string, byte[]> files = RoomMetatileExtractor.Extract(bus);
        var sources = new Dictionary<string, int>
        {
            [RoomMetatileFormat.CreFileName] = RoomAssetRomData.Tilesets.CreBlockDefinitionsAddress,
        };
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).BlockDefinitionsAddress;
            sources[RoomMetatileFormat.SourceFileName(address)] = address;
        }
        AssertEqual(sources.Count, files.Count,
            "one visual metatile JSON resource per distinct retail source");
        foreach ((string name, int source) in sources)
        {
            byte[] native = RomDataReader.Decompress(bus, source);
            RoomMetatileAtlas atlas = RoomMetatileAtlas.Load(
                new MemoryStream(files[name], writable: false), native.Length);
            AssertTrue(atlas.Transfer.Span.SequenceEqual(native),
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
                RoomAssetRomData.Tilesets.CreBlockDefinitionsAddress).Length);
        var stockCatalog = new RoomMetatileCatalog(stockCre, bySource);
        foreach (ushort roomPointer in new ushort[] { 0x91f8, 0xdf8d })
        {
            CartridgeRoomHeader header = CartridgeRoomHeader.Load(bus, roomPointer);
            int areaSource = RoomTilesetDefinitions.Get(header.State.GraphicsSet).BlockDefinitionsAddress;
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, header);
            CartridgeRoomAssets installed = CartridgeRoomAssets.Load(
                new RoomMetatileSourceReadGuard(bus, areaSource), header,
                metatileArt: stockCatalog);
            AssertTrue(native.LevelData.BlockDefinitions.Span.SequenceEqual(
                    installed.LevelData.BlockDefinitions.Span),
                $"room $8F:{roomPointer:X4} installs exact visual blocks without ROM source reads");
        }

        byte[] cre = RomDataReader.Decompress(bus,
            RoomAssetRomData.Tilesets.CreBlockDefinitionsAddress);
        JsonNode edited = JsonNode.Parse(files[RoomMetatileFormat.CreFileName])
            ?? throw new InvalidDataException("CRE metatile JSON is empty.");
        JsonNode topLeft = edited["blocks"]![0]!["topLeft"]!;
        int originalColumn = topLeft["tileColumn"]!.GetValue<int>();
        topLeft["tileColumn"] = (originalColumn + 1) % RoomMetatileFormat.TileColumns;
        RoomMetatileAtlas modified = RoomMetatileAtlas.Load(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited)), cre.Length);
        var levelWord = RoomLevelWord.Create(0, LevelBlockFlipFlags.None,
            RoomCollisionType.SolidBlock);
        byte[] bts = [0x46];
        var originalLevel = new RoomLevelData(1, 1, [levelWord.Raw], bts,
            [0], cre);
        var modifiedLevel = new RoomLevelData(1, 1, [levelWord.Raw], bts,
            [0], modified.Transfer.Span);
        AssertTrue(originalLevel.ForegroundEntries.Span.SequenceEqual(modifiedLevel.ForegroundEntries.Span) &&
            originalLevel.BehaviorBytes.Span.SequenceEqual(modifiedLevel.BehaviorBytes.Span),
            "visual metatile edit leaves room collision nibble and BTS unchanged");
        ExpandedBlockTiles before = LevelBlockTilemapExpander.Expand(
            levelWord, originalLevel.BlockDefinitions.Span);
        ExpandedBlockTiles after = LevelBlockTilemapExpander.Expand(
            levelWord, modifiedLevel.BlockDefinitions.Span);
        AssertTrue(before.TopLeft != after.TopLeft && before.TopRight == after.TopRight &&
            before.BottomLeft == after.BottomLeft && before.BottomRight == after.BottomRight,
            "edited JSON changes only the intended 8x8 visual child of a solid block");
        RoomMetatileCatalog editedCatalog = new(modified, bySource);
        CartridgeRoomHeader landing = CartridgeRoomHeader.Load(bus, 0x91f8);
        CartridgeRoomAssets nativeLanding = CartridgeRoomAssets.Load(bus, landing);
        CartridgeRoomAssets editedLanding = CartridgeRoomAssets.Load(
            new RoomMetatileSourceReadGuard(bus,
                RoomTilesetDefinitions.Get(landing.State.GraphicsSet).BlockDefinitionsAddress),
            landing, metatileArt: editedCatalog);
        AssertTrue(!nativeLanding.LevelData.BlockDefinitions.Span.SequenceEqual(
                editedLanding.LevelData.BlockDefinitions.Span) &&
            nativeLanding.LevelData.ForegroundEntries.Span.SequenceEqual(
                editedLanding.LevelData.ForegroundEntries.Span) &&
            nativeLanding.LevelData.BehaviorBytes.Span.SequenceEqual(
                editedLanding.LevelData.BehaviorBytes.Span),
            "real room loader applies the visual CRE edit without changing level placement or BTS");

        topLeft["palette"] = RoomMetatileFormat.PaletteCount;
        byte[] invalid = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(edited);
        AssertThrows<InvalidDataException>(() => RoomMetatileAtlas.Load(
                new MemoryStream(invalid, writable: false), cre.Length),
            "metatile JSON rejects an invalid palette selector");
        Console.WriteLine($"  Room metatiles: {files.Count} distinct CRE/area resources roundtrip " +
            "byte-exactly; a visual edit changes one child without changing collision/BTS.");
    }

    private sealed class RoomMetatileSourceReadGuard(ISnesAddressSpace source, int areaSource)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address == areaSource ||
                address == RoomAssetRomData.Tilesets.CreBlockDefinitionsAddress)
                throw new InvalidOperationException(
                    $"Installed room loader reread metatile source ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
