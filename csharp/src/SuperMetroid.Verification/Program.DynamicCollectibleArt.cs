using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledDynamicCollectibleArt(
        SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "dynamic-collectible-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Dynamic collectible test root escaped test-temp.");
        string stockDirectory = Path.Combine(testRoot, "stock");
        string overrideDirectory = Path.Combine(testRoot, "overrides");
        try
        {
            RoomPlmDynamicCollectibleArtFiles.Extract(rom, stockDirectory,
                SupportedCartridge.Sha256);
            RoomPlmDynamicCollectibleArtFiles.ValidateStock(stockDirectory);
            RoomPlmDynamicCollectibleArtCatalog stock =
                RoomPlmDynamicCollectibleArtFiles.Load(stockDirectory, null);
            RoomPlmDynamicCollectibleGraphic nativeBombs =
                RoomPlmDynamicCollectibleGraphicsDefinitions.Get(
                    InWorldCollectibleKind.Bombs);
            AssertTrue(stock.Resolve(InWorldCollectibleKind.Bombs).Tiles.Span
                    .SequenceEqual(nativeBombs.Tiles.Span),
                "installed Bombs PNG compiles to exact stock tiles");

            Directory.CreateDirectory(overrideDirectory);
            string name = RoomPlmDynamicCollectibleArtFiles.FileName(
                InWorldCollectibleKind.Bombs);
            IndexedPngImage image = IndexedPng.Read(
                new MemoryStream(File.ReadAllBytes(Path.Combine(stockDirectory, name))),
                64, 8);
            image.Pixels[0] ^= 1;
            using (var editedPng = new FileStream(
                       Path.Combine(overrideDirectory, name), FileMode.CreateNew,
                       FileAccess.Write))
                IndexedPng.Write(editedPng, image.Width, image.Height,
                    image.Pixels, image.Palette);

            JsonNode paletteDocument = JsonNode.Parse(File.ReadAllText(
                Path.Combine(stockDirectory,
                    RoomPlmDynamicCollectibleArtFiles.PaletteFileName)))
                ?? throw new InvalidDataException("Stock item palettes are empty.");
            JsonNode bombs = paletteDocument["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "Bombs")!;
            bombs["offsets"]![0] = 1;
            string paletteOverride = Path.Combine(overrideDirectory,
                RoomPlmDynamicCollectibleArtFiles.PaletteFileName);
            File.WriteAllText(paletteOverride, paletteDocument.ToJsonString());
            RoomPlmDynamicCollectibleArtCatalog edited =
                RoomPlmDynamicCollectibleArtFiles.Load(stockDirectory,
                    overrideDirectory);
            AssertTrue(!edited.Resolve(InWorldCollectibleKind.Bombs).Tiles.Span
                    .SequenceEqual(nativeBombs.Tiles.Span),
                "edited item PNG changes the compiled tile upload");
            AssertEqual((byte)1,
                edited.Resolve(InWorldCollectibleKind.Bombs).PaletteOffsets.Span[0],
                "edited item palette offset reaches the loaded catalog");

            (byte[] stockVram, ushort stockTileWord, CollectiblePlmSnapshot stockItem) =
                LoadRetailBombsWithArt(rom, stock);
            (byte[] editedVram, ushort editedTileWord, CollectiblePlmSnapshot editedItem) =
                LoadRetailBombsWithArt(rom, edited);
            AssertTrue(!stockVram.SequenceEqual(editedVram),
                "edited permanent-item PNG reaches production VRAM");
            AssertEqual((ushort)(stockTileWord + 0x0400), editedTileWord,
                "edited palette offset reaches production block definitions");
            AssertEqual(stockItem, editedItem,
                "item artwork edit preserves the native PLM pickup state");

            File.Delete(Path.Combine(overrideDirectory, name));
            File.Delete(paletteOverride);
            RoomPlmDynamicCollectibleArtCatalog restored =
                RoomPlmDynamicCollectibleArtFiles.Load(stockDirectory,
                    overrideDirectory);
            AssertTrue(restored.Resolve(InWorldCollectibleKind.Bombs).Tiles.Span
                    .SequenceEqual(nativeBombs.Tiles.Span),
                "removing item PNG override restores cartridge pixels");
            AssertEqual(nativeBombs.PaletteOffsets.Span[0],
                restored.Resolve(InWorldCollectibleKind.Bombs)
                    .PaletteOffsets.Span[0],
                "removing item JSON override restores cartridge palette selection");

            File.WriteAllText(Path.Combine(overrideDirectory, name), "not a PNG");
            AssertThrows<InvalidDataException>(
                () => RoomPlmDynamicCollectibleArtFiles.Load(stockDirectory,
                    overrideDirectory),
                "malformed item PNG override fails loudly");
            File.Delete(Path.Combine(overrideDirectory, name));
            File.WriteAllText(paletteOverride, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmDynamicCollectibleArtFiles.Load(stockDirectory,
                    overrideDirectory),
                "incomplete item palette override fails loudly");
            File.Delete(paletteOverride);
            File.WriteAllText(Path.Combine(stockDirectory, name), "corrupted stock");
            AssertThrows<InvalidDataException>(
                () => RoomPlmDynamicCollectibleArtFiles.ValidateStock(stockDirectory),
                "corrupted stock item art fails its manifest hash");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private static (byte[] Vram, ushort TileWord, CollectiblePlmSnapshot Item)
        LoadRetailBombsWithArt(ISnesAddressSpace bus,
            RoomPlmDynamicCollectibleArtCatalog art)
    {
        const int width = 64;
        var level = new RoomLevelData(width, 64,
            new ushort[width * 64], new byte[width * 64],
            new ushort[width * 64], new byte[0x400 * 8]);
        var vram = new SnesVram();
        var plms = new RoomPlmSystem { DynamicCollectibleArt = art };
        plms.LoadRoomPopulation(bus, level, level.CreateBackgroundStreamer(),
            vram, 0x83fe, new Bank80SystemState(), AreaId.Crateria,
            () => new SamusState(), () => false,
            useCompiledRetailPopulation: true);
        CollectiblePlmSnapshot item = plms.Collectibles.Single();
        byte[] tiles = vram.Bytes.Slice(0x3e00 * 2, 0x100).ToArray();
        ushort firstWord = BinaryPrimitives.ReadUInt16LittleEndian(
            level.BlockDefinitions.Span.Slice(0x0470));
        return (tiles, firstWord, item);
    }
}
