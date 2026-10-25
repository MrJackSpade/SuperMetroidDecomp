using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomArtworkRenderParityVerification;
using static SuperMetroid.Verification.Rooms.RoomBackgroundLibraryParityVerification;
using static SuperMetroid.Verification.Rooms.RoomVisualLayoutsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayRevealVisualInstallationVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks stock and overridden room characters, palettes, blocks, backgrounds, and sky artwork.</summary>
internal static class RoomCharacterInstallationVerification
{
    /// <summary>Full stock import, live edit, repair and invalid-override contract for room art.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyRoomArtworkInstallation(string sourceRom)
    {
        using var rootScratch = new TestScratchDirectory("room-character-installation");
        string root = rootScratch.Root;
        try
        {
            GameInstallation installed = GameAssetInstaller.Install(sourceRom, root);
            VerifyRoomArtworkRenderParity(installed, sourceRom);
            VerifyLibraryBackgroundInstalledParity(sourceRom, installed);
            AssertTrue(File.Exists(Path.Combine(installed.RoomCharacterDirectory,
                    RoomCharacterArtworkFiles.ManifestFileName)),
                "room-character stock manifest is installed");
            AssertTrue(File.Exists(Path.Combine(installed.EnemyTileDirectory,
                    EnemyTileArtworkFormat.ManifestFileName)),
                "enemy-tile stock manifest is installed");
            AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount,
                Directory.GetFiles(installed.EnemyTileDirectory, "enemy-????-tiles.png").Length,
                "full installer publishes all retail enemy sheets");
            AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount,
                Directory.GetFiles(installed.EnemyTileDirectory, "enemy-????-colors.json").Length,
                "full installer publishes all retail enemy palettes");
            _ = installed.LoadEnemyTiles();
            RoomCharacterAtlasCatalog stock = installed.LoadRoomCharacters();
            CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            SelectedRoom landing = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
            VerifyRoomVisualLayouts(installed, bus, landing);
            VerifyXrayRevealVisualInstallation(installed);
            LoadedRoomAssets native = LoadFixtureRoomAssets(landing);
            LoadedRoomAssets installedRoom = LoadFixtureRoomAssets(landing, stock);
            AssertTrue(installedRoom.CreCharacters.SequenceEqual(native.CreCharacters) &&
                installedRoom.RoomCharacters.SequenceEqual(native.RoomCharacters),
                "installed stock room PNGs retain Landing Site character bytes");

            int source = native.Tileset.CharacterAddress;
            string name = RoomCharacterAtlasFormat.SourceFileName(source);
            int tileCount = native.RoomCharacters.Length / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            IndexedPngImage image;
            using (var stockFile = File.OpenRead(Path.Combine(installed.RoomCharacterDirectory, name)))
                image = IndexedPng.Read(stockFile, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] ^= 1;
            Directory.CreateDirectory(installed.RoomCharacterOverrideDirectory);
            string overridePath = Path.Combine(installed.RoomCharacterOverrideDirectory, name);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            RoomCharacterAtlasCatalog edited = installed.LoadRoomCharacters();
            LoadedRoomAssets editedRoom = LoadFixtureRoomAssets(landing, edited);
            AssertTrue(!editedRoom.RoomCharacters.SequenceEqual(native.RoomCharacters),
                "installed PNG override changes the real room-loader output");
            AssertTrue(stock.GetAt(source).Characters.Span.SequenceEqual(native.RoomCharacters),
                "room override does not mutate stock artwork already loaded");

            int ghostSource = RoomBackgroundSourceDefinitions.TourianStatueGhostCharacters;
            string ghostName = RoomCharacterAtlasFormat.SourceFileName(ghostSource);
            string ghostStockPath = Path.Combine(installed.RoomCharacterDirectory, ghostName);
            byte[] nativeGhost = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                ghostSource, LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount);
            AssertTrue(stock.GetAt(ghostSource).Characters.Span.SequenceEqual(nativeGhost),
                "installed statue-ghost PNG preserves every native character byte");
            LibraryBackgroundNativeOperand ghostTransfer = LibraryBackgroundSourceInventory.Scan(bus)
                .Single(entry => entry.SourceAddress == ghostSource);
            AssertEqual(LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount,
                (ghostTransfer.TransferByteCount ?? throw new InvalidOperationException("Verification requires ghostTransfer.TransferByteCount.")), "statue-ghost library upload length");
            AssertEqual(LibraryBackgroundOperands.DestinationWord(LibraryBackgroundDestination.StatueGhostSpriteCharacters),
                (ghostTransfer.VramDestination ?? throw new InvalidOperationException("Verification requires ghostTransfer.VramDestination.")), "statue-ghost library VRAM destination");
            var nativeGhostVram = new SnesVram();
            SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeGhostVram,
                ghostTransfer.ListPointer, activeDoorPointer: 0);
            SnesVram installedGhostVram = ShownVram(LibraryBackgroundProgramDefinitions.Get(ghostTransfer.ListPointer), new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), CharacterArt = stock });
            AssertTrue(nativeGhostVram.ImageEquals(installedGhostVram),
                "installed statue-ghost PNG matches the complete native library upload");
            int ghostTileCount = nativeGhost.Length / RoomCharacterAtlasFormat.BytesPerTile;
            int ghostColumns = Math.Min(RoomCharacterAtlasFormat.TileColumns, ghostTileCount);
            int ghostRows = (ghostTileCount + ghostColumns - 1) / ghostColumns;
            IndexedPngImage ghostImage;
            using (var stockFile = File.OpenRead(ghostStockPath))
                ghostImage = IndexedPng.Read(stockFile, new(checked((ushort)(ghostColumns * 8)), checked((ushort)(ghostRows * 8))));
            ghostImage.Pixels[0] ^= 1;
            string ghostOverridePath = Path.Combine(installed.RoomCharacterOverrideDirectory, ghostName);
            using (var output = File.Create(ghostOverridePath))
                IndexedPng.Write(output, new(ghostImage.Size.X, ghostImage.Size.Y),
                    ghostImage.Pixels, ghostImage.Colours);
            RoomCharacterAtlasCatalog editedGhost = installed.LoadRoomCharacters();
            SnesVram editedGhostVram = ShownVram(LibraryBackgroundProgramDefinitions.Get(ghostTransfer.ListPointer), new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), CharacterArt = editedGhost });
            int ghostDestinationByte = ghostTransfer.VramDestination.Value * 2;
            AssertTrue(nativeGhostVram.ReadByte(ghostDestinationByte) !=
                    editedGhostVram.ReadByte(ghostDestinationByte) &&
                nativeGhostVram.Bytes.Slice(ghostDestinationByte + 1, nativeGhost.Length - 1)
                    .SequenceEqual(editedGhostVram.Bytes.Slice(
                        ghostDestinationByte + 1, nativeGhost.Length - 1)),
                "edited statue-ghost pixel changes only its selected live VRAM byte");

            AssertTrue(File.Exists(Path.Combine(installed.RoomPaletteDirectory,
                    RoomStaticPaletteArtworkFiles.ManifestFileName)),
                "room-palette stock manifest is installed");
            RoomStaticPaletteCatalog stockPalettes = installed.LoadRoomPalettes();
            int paletteSource = native.Tileset.PaletteAddress;
            string paletteName = RoomStaticPaletteFormat.SourceFileName(paletteSource);
            string paletteStockPath = Path.Combine(installed.RoomPaletteDirectory, paletteName);
            JsonNode paletteDocument = JsonNode.Parse(File.ReadAllText(paletteStockPath))
                ?? throw new InvalidDataException("Installed room palette JSON is empty.");
            JsonNode firstColor = paletteDocument.Required("colors").Required(0);
            int stockRed = firstColor.Required("red").GetValue<int>();
            firstColor["red"] = stockRed ^ 1;
            Directory.CreateDirectory(installed.RoomPaletteOverrideDirectory);
            string paletteOverridePath = Path.Combine(installed.RoomPaletteOverrideDirectory, paletteName);
            File.WriteAllText(paletteOverridePath, paletteDocument.ToJsonString());
            RoomStaticPaletteCatalog editedPalettes = installed.LoadRoomPalettes();
            LoadedRoomAssets editedPaletteRoom = LoadFixtureRoomAssets(landing,
                paletteArt: editedPalettes);
            var stockCgram = new GameplayColours(new SpriteColourSets());
            var editedCgram = new GameplayColours(new SpriteColourSets());
            native.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), stockCgram.BackgroundSets);
            editedPaletteRoom.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), editedCgram.BackgroundSets);
            AssertTrue(stockCgram.Colors[0] != editedCgram.Colors[0] &&
                stockCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]
                    .SequenceEqual(editedCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]),
                "installed RGB5 override changes only its room CGRAM color");
            var compiledStockCgram = new SnesCgram();
            stockPalettes.GetAt(paletteSource).LoadTo(compiledStockCgram);
            AssertTrue(new CgramPalettes(stockCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(compiledStockCgram)),
                "installed stock room palette matches native room CGRAM");

            AssertTrue(File.Exists(Path.Combine(installed.RoomMetatileDirectory,
                    RoomMetatileArtworkFiles.ManifestFileName)),
                "room-block stock manifest is installed");
            RoomMetatileCatalog stockBlocks = installed.LoadRoomMetatiles();
            LoadedRoomAssets installedBlocks = LoadFixtureRoomAssets(landing,
                metatileArt: stockBlocks);
            AssertTrue(native.LevelData.BlockDefinitions.Span.SequenceEqual(
                    installedBlocks.LevelData.BlockDefinitions.Span),
                "installed stock room-block JSON retains native visual tile words");
            string blockName = RoomMetatileFormat.CreFileName;
            string blockStockPath = Path.Combine(installed.RoomMetatileDirectory, blockName);
            JsonNode blockDocument = JsonNode.Parse(File.ReadAllText(blockStockPath))
                ?? throw new InvalidDataException("Installed room-block JSON is empty.");
            JsonNode firstTile = blockDocument.Required("blocks").Required(0).Required("topLeft");
            firstTile["tileColumn"] = (firstTile.Required("tileColumn").GetValue<int>() + 1)
                % RoomMetatileFormat.TileColumns;
            Directory.CreateDirectory(installed.RoomMetatileOverrideDirectory);
            string blockOverridePath = Path.Combine(installed.RoomMetatileOverrideDirectory, blockName);
            File.WriteAllText(blockOverridePath, blockDocument.ToJsonString());
            LoadedRoomAssets editedBlocks = LoadFixtureRoomAssets(landing,
                metatileArt: installed.LoadRoomMetatiles());
            AssertTrue(!native.LevelData.BlockDefinitions.Span.SequenceEqual(
                    editedBlocks.LevelData.BlockDefinitions.Span) &&
                native.LevelData.ForegroundEntries.Span.SequenceEqual(
                    editedBlocks.LevelData.ForegroundEntries.Span) &&
                native.LevelData.BehaviorBytes.AsSpan().SequenceEqual(
                    editedBlocks.LevelData.BehaviorBytes.AsSpan()),
                "installed visual-block override changes room art without changing collision/BTS");

            AssertTrue(File.Exists(Path.Combine(installed.RoomBackgroundTilemapDirectory,
                    RoomBackgroundTilemapArtworkFiles.ManifestFileName)),
                "room-background stock manifest is installed");
            RoomBackgroundTilemapCatalog stockBackgrounds = installed.LoadRoomBackgroundTilemaps();
            string backgroundManifestPath = Path.Combine(installed.RoomBackgroundTilemapDirectory,
                RoomBackgroundTilemapArtworkFiles.ManifestFileName);
            byte[] originalBackgroundManifest = File.ReadAllBytes(backgroundManifestPath);
            // A manifest entry that names no background tilemap (#1414) is rejected.
            const string substitutedTilemap = "NotABackgroundTilemap";
            AssertTrue(!Enum.TryParse(substitutedTilemap, out RoomBackgroundTilemap _),
                "manifest substitution fixture names no background tilemap");
            JsonNode alteredManifest = JsonNode.Parse(originalBackgroundManifest)
                ?? throw new InvalidDataException("Installed background manifest is empty.");
            JsonObject manifestEntries = alteredManifest.Required("entries").AsObject();
            string stockName = RoomBackgroundTilemapFormat.FileName(Enum.GetValues<RoomBackgroundTilemap>()[0]);
            string substitutedName = "room-background-not-a-background-tilemap.json";
            JsonNode alteredEntry = manifestEntries.Required(stockName).DeepClone();
            alteredEntry["tilemap"] = substitutedTilemap;
            manifestEntries.Remove(stockName);
            manifestEntries[substitutedName] = alteredEntry;
            try
            {
                File.WriteAllText(backgroundManifestPath, alteredManifest.ToJsonString());
                try
                {
                    _ = installed.LoadRoomBackgroundTilemaps();
                    throw new InvalidOperationException(
                        "Substituted background tilemap was accepted by the installation loader.");
                }
                catch (InvalidDataException error)
                {
                    AssertTrue(error.Message.Contains("invalid tilemap entry",
                            StringComparison.Ordinal),
                        "substituted background tilemap fails the named identity check");
                }
            }
            finally { File.WriteAllBytes(backgroundManifestPath, originalBackgroundManifest); }
            SelectedRoom ceres = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xdf8d);
            int backgroundSource = LibraryBackgroundSourceInventory.Scan(bus)
                .Single(source => source.ListPointer == ceres.State.BackgroundDataPointer &&
                    source.Command == LibraryBackgroundCommand.Decompress).SourceAddress;
            string backgroundName = RoomBackgroundTilemapFormat.FileName(
                LibraryBackgroundOperandDecoding.DecompressedSource(backgroundSource).Tilemap);
            var nativeBackgroundVram = new SnesVram();
            var stockBackgroundVram = new SnesVram();
            SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeBackgroundVram,
                ceres.State.BackgroundDataPointer, activeDoorPointer: 0);
            LibraryBackgroundReplay.Execute(stockBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = stockBackgrounds });
            AssertTrue(nativeBackgroundVram.ImageEquals(stockBackgroundVram),
                "installed Ceres background tilemap retains native VRAM words");
            string backgroundStockPath = Path.Combine(installed.RoomBackgroundTilemapDirectory,
                backgroundName);
            JsonNode backgroundDocument = JsonNode.Parse(File.ReadAllText(backgroundStockPath))
                ?? throw new InvalidDataException("Installed background JSON is empty.");
            JsonNode backgroundCell = backgroundDocument.Required("pages").Required(0).Required("cells").Required(0);
            backgroundCell["tileColumn"] = (backgroundCell.Required("tileColumn").GetValue<int>() + 1)
                % RoomBackgroundTilemapFormat.TileColumns;
            Directory.CreateDirectory(installed.RoomBackgroundTilemapOverrideDirectory);
            string backgroundOverridePath = Path.Combine(
                installed.RoomBackgroundTilemapOverrideDirectory, backgroundName);
            File.WriteAllText(backgroundOverridePath, backgroundDocument.ToJsonString());
            var editedBackgroundVram = new SnesVram();
            LibraryBackgroundReplay.Execute(editedBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = installed.LoadRoomBackgroundTilemaps() });
            AssertTrue(nativeBackgroundVram.ReadWord(0x4800) != editedBackgroundVram.ReadWord(0x4800) &&
                nativeBackgroundVram.ReadWord(0x4801) == editedBackgroundVram.ReadWord(0x4801),
                "installed background override changes only its selected Ceres BG tile");

            RoomSkyTilemapCatalog stockSky = installed.LoadRoomSkyTilemaps();
            // The landing-cutscene door's command-E record selects the uploaded sky page.
            LibraryBackgroundInstruction landingSky = LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                LandingSiteRomData.LandingCutsceneDoorPointer);
            int skyPage = landingSky.Source is LibraryBackgroundSource.SkyPage sky ? sky.Page
                : throw new InvalidOperationException($"The landing-cutscene door transfer reads {landingSky.Source}, not a sky page.");
            string skyName = RoomSkyTilemapFormat.FileName(skyPage);
            string skyStockPath = Path.Combine(installed.RoomBackgroundTilemapDirectory, skyName);
            AssertTrue(File.Exists(skyStockPath), "selected scrolling-sky page is installed");
            byte[] nativeSky = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                LibraryBackgroundOperands.SkyPageAddress(skyPage), landingSky.ByteCount);
            AssertTrue(stockSky.Page(skyPage, landingSky.ByteCount).Span.SequenceEqual(nativeSky),
                "installed scrolling-sky page retains native door-selected bytes");
            JsonNode skyDocument = JsonNode.Parse(File.ReadAllText(skyStockPath))
                ?? throw new InvalidDataException("Installed scrolling-sky JSON is empty.");
            JsonNode skyCell = skyDocument.Required("pages").Required(0).Required("cells").Required(0);
            skyCell["tileColumn"] = (skyCell.Required("tileColumn").GetValue<int>() + 1)
                % RoomBackgroundTilemapFormat.TileColumns;
            string skyOverridePath = Path.Combine(
                installed.RoomBackgroundTilemapOverrideDirectory, skyName);
            File.WriteAllText(skyOverridePath, skyDocument.ToJsonString());
            RoomSkyTilemapCatalog editedSky = installed.LoadRoomSkyTilemaps();
            ReadOnlyMemory<byte> changedSky = editedSky.Page(skyPage, landingSky.ByteCount);
            AssertTrue(!changedSky.Span[..2].SequenceEqual(nativeSky.AsSpan(0, 2)) &&
                changedSky.Span[2..].SequenceEqual(nativeSky.AsSpan(2)),
                "installed sky override changes exactly its selected door tile word");

            // A missing stock resource causes atomic re-extraction from the installed
            // cartridge; the edit remains outside the replaceable game directory.
            File.Delete(Path.Combine(installed.RoomCharacterDirectory, name));
            File.Delete(ghostStockPath);
            File.Delete(paletteStockPath);
            File.Delete(blockStockPath);
            File.Delete(backgroundStockPath);
            File.Delete(skyStockPath);
            GameInstallation repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed room-art repair lost its ROM.");
            AssertTrue(File.Exists(overridePath), "room-character override survives stock repair");
            AssertTrue(File.Exists(Path.Combine(repaired.RoomCharacterDirectory, name)),
                "room-character stock sheet is restored by repair");
            AssertTrue(File.Exists(ghostOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomCharacterDirectory, ghostName)) &&
                repaired.LoadRoomCharacters().GetAt(ghostSource).Characters.Span.SequenceEqual(
                    editedGhost.GetAt(ghostSource).Characters.Span),
                "statue-ghost stock sheet is repaired without losing the user edit");
            AssertTrue(File.Exists(paletteOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomPaletteDirectory, paletteName)),
                "room-palette stock is restored while its override survives repair");
            AssertTrue(File.Exists(blockOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomMetatileDirectory, blockName)),
                "room-block stock is restored while its override survives repair");
            AssertTrue(File.Exists(backgroundOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomBackgroundTilemapDirectory, backgroundName)),
                "room-background stock is restored while its override survives repair");
            AssertTrue(File.Exists(skyOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomBackgroundTilemapDirectory, skyName)),
                "scrolling-sky stock is restored while its override survives repair");
            LoadedRoomAssets repairedEdited = LoadFixtureRoomAssets(landing,
                repaired.LoadRoomCharacters());
            AssertTrue(repairedEdited.RoomCharacters.SequenceEqual(editedRoom.RoomCharacters),
                "repaired installation retains the selected user room artwork");
            LoadedRoomAssets repairedPaletteRoom = LoadFixtureRoomAssets(landing,
                paletteArt: repaired.LoadRoomPalettes());
            var repairedCgram = new GameplayColours(new SpriteColourSets());
            repairedPaletteRoom.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), repairedCgram.BackgroundSets);
            AssertTrue(new CgramPalettes(repairedCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(editedCgram.AsPaletteColors())),
                "repaired installation retains the selected user room palette");
            LoadedRoomAssets repairedBlocks = LoadFixtureRoomAssets(landing,
                metatileArt: repaired.LoadRoomMetatiles());
            AssertTrue(repairedBlocks.LevelData.BlockDefinitions.Span.SequenceEqual(
                    editedBlocks.LevelData.BlockDefinitions.Span),
                "repaired installation retains selected user visual blocks");
            var repairedBackgroundVram = new SnesVram();
            LibraryBackgroundReplay.Execute(repairedBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = repaired.LoadRoomBackgroundTilemaps() });
            AssertTrue(repairedBackgroundVram.ImageEquals(editedBackgroundVram),
                "repaired installation retains selected user background tilemap");
            AssertTrue(repaired.LoadRoomSkyTilemaps().Page(skyPage, landingSky.ByteCount).Span.SequenceEqual(changedSky.Span),
                "repaired installation retains selected user scrolling sky");

            File.WriteAllBytes(overridePath, "invalid indexed PNG"u8.ToArray());
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomCharacters(),
                "invalid room-art override fails loudly instead of reverting to stock");
            File.Delete(overridePath);
            File.WriteAllBytes(ghostOverridePath, "invalid indexed PNG"u8.ToArray());
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomCharacters(),
                "invalid statue-ghost override fails loudly");
            File.Delete(ghostOverridePath);
            File.WriteAllText(paletteOverridePath, "invalid RGB5 JSON");
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomPalettes(),
                "invalid room-palette override fails loudly instead of reverting to stock");
            File.WriteAllText(blockOverridePath, "invalid visual block JSON");
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomMetatiles(),
                "invalid room-block override fails loudly instead of reverting to stock");

            File.WriteAllText(backgroundOverridePath, "invalid background tilemap JSON");
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomBackgroundTilemaps(),
                "invalid room-background override fails loudly instead of reverting to stock");
            File.WriteAllText(skyOverridePath, "invalid scrolling-sky JSON");
            AssertThrows<InvalidDataException>(() => repaired.LoadRoomSkyTilemaps(),
                "invalid scrolling-sky override fails loudly instead of reverting to stock");

            Console.WriteLine("  Room artwork installation: PNG, statue ghost, RGB5, block, BG and sky stock import, live edits, " +
                "repair preservation and invalid-override rejection verified.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// Runs a library background program with installed character art and returns VRAM as a frame shows it: installed
    /// character transfers bind their artwork instead of writing VRAM (#1350), and the frame composes them over the image.
    /// </summary>
    private static SnesVram ShownVram(LibraryBackgroundProgram program, LibraryBackgroundArtwork artwork)
    {
        var vram = new SnesVram();
        var bindings = new BgArtworkBindings();
        LibraryBackgroundReplay.Execute(vram, program,
            activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: artwork with { Backgrounds = bindings });
        byte[] image = vram.Bytes.ToArray();
        bindings.ComposeOver(image);
        var shown = new SnesVram();
        shown.LoadBytes(0, image);
        return shown;
    }

    /// <summary>Runs the room-art installation contract through the character-focused command alias.</summary>
    [VerificationCommand("--room-character-installation")]
    internal static int RoomCharacterInstallationCommand()
    {
        string roomCharacterRom = RepositoryRomPath;
        Suite(nameof(VerifyRoomArtworkInstallation), () => VerifyRoomArtworkInstallation(roomCharacterRom));
        return 0;
    }

    /// <summary>Runs the room-art installation contract through the general artwork command alias.</summary>
    [VerificationCommand("--room-artwork-installation")]
    internal static int RoomArtworkInstallationCommand()
    {
        string roomArtworkRom = RepositoryRomPath;
        Suite(nameof(VerifyRoomArtworkInstallation), () => VerifyRoomArtworkInstallation(roomArtworkRom));
        return 0;
    }
}
