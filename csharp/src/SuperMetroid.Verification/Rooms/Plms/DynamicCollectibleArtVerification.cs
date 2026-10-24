using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.LookupDynamicCollectibleStockVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks extracted collectible art, installed VRAM edits, stock integrity, and override failures.</summary>
internal static class DynamicCollectibleArtVerification
{
    /// <summary>Checks collectible art extraction, edits reaching production VRAM and block palettes, restoration, and malformed override failures.</summary>
    internal static void VerifyInstalledDynamicCollectibleArt(
        SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("dynamic-collectible");
        string testRoot = testRootScratch.Root;
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
            VerifyDynamicCollectibleStockMapping(rom, stock);

            Directory.CreateDirectory(overrideDirectory);
            string name = RoomPlmDynamicCollectibleArtFiles.FileName(
                InWorldCollectibleKind.Bombs);
            IndexedPngImage image = IndexedPng.Read(
                new MemoryStream(File.ReadAllBytes(Path.Combine(stockDirectory, name))),
                new(64, 8));
            image.Pixels[0] ^= 1;
            using (var editedPng = new FileStream(
                       Path.Combine(overrideDirectory, name), FileMode.CreateNew,
                       FileAccess.Write))
                IndexedPng.Write(editedPng, new(image.Size.X, image.Size.Y),
                    image.Pixels, image.Colours);

            JsonNode paletteDocument = JsonNode.Parse(File.ReadAllText(
                Path.Combine(stockDirectory,
                    RoomPlmDynamicCollectibleArtFiles.PaletteFileName)))
                ?? throw new InvalidDataException("Stock item palettes are empty.");
            JsonNode bombs = paletteDocument.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "Bombs");
            bombs.Required("offsets")[0] = 1;
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
                LoadRetailBombsWithArt(stock);
            (byte[] editedVram, ushort editedTileWord, CollectiblePlmSnapshot editedItem) =
                LoadRetailBombsWithArt(edited);
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

    /// <summary>Loads the retail Bombs PLM with selected art and captures its VRAM tile bytes, block word, and pickup state.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static LoadRetailBombsWithArtResult
        LoadRetailBombsWithArt(RoomPlmDynamicCollectibleArtCatalog art)
    {
        const int width = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 64), new RoomLevelPlanes(new ushort[width * 64], new byte[width * 64], new ushort[width * 64]), new byte[0x400 * 8]);
        var vram = new SnesVram();
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmDynamicCollectibleArt = art });
        PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(vram), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationDefinitions.Get(0x83fe), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        CollectiblePlmSnapshot item = plms.CollectiblePlms.Collectibles.Single();
        byte[] tiles = BgArtworkFixtures.Composed(vram).AsSpan(0x3e00 * 2, 0x100).ToArray();
        ushort firstWord = BinaryPrimitives.ReadUInt16LittleEndian(
            level.BlockDefinitions.Span.Slice(0x0470));
        return new LoadRetailBombsWithArtResult(tiles, firstWord, item);
    }
}
