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
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.World;

/// <summary>Checks stock and overridden room characters, palettes, blocks, backgrounds, and sky artwork.</summary>
internal static class RoomCharacterInstallationVerification
{
    /// <summary>Full stock import, live edit, repair and invalid-override contract for room art.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian statue ghost and scrolling-sky source bytes")]
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
            CartridgeRoomHeader landing = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
            VerifyRoomVisualLayouts(installed, bus, landing);
            VerifyXrayRevealVisualInstallation(installed);
            CartridgeRoomAssets native = LoadFixtureRoomAssets(landing);
            CartridgeRoomAssets installedRoom = LoadFixtureRoomAssets(landing, stock);
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
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            RoomCharacterAtlasCatalog edited = installed.LoadRoomCharacters();
            CartridgeRoomAssets editedRoom = LoadFixtureRoomAssets(landing, edited);
            AssertTrue(!editedRoom.RoomCharacters.SequenceEqual(native.RoomCharacters),
                "installed PNG override changes the real room-loader output");
            AssertTrue(stock.Get(source).Transfer.Span.SequenceEqual(native.RoomCharacters),
                "room override does not mutate stock artwork already loaded");

            int ghostSource = RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress;
            string ghostName = RoomCharacterAtlasFormat.SourceFileName(ghostSource);
            string ghostStockPath = Path.Combine(installed.RoomCharacterDirectory, ghostName);
            byte[] nativeGhost = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                ghostSource, RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount);
            AssertTrue(stock.Get(ghostSource).Transfer.Span.SequenceEqual(nativeGhost),
                "installed statue-ghost PNG preserves every native character byte");
            LibraryBackgroundSource ghostTransfer = LibraryBackgroundSourceInventory.Scan(bus)
                .Single(entry => entry.SourceAddress == ghostSource);
            AssertEqual(RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount,
                (ghostTransfer.TransferByteCount ?? throw new InvalidOperationException("Verification requires ghostTransfer.TransferByteCount.")), "statue-ghost library upload length");
            AssertEqual(RoomAssetRomData.LibraryBackground.TourianStatueGhost.VramDestinationWord,
                (ghostTransfer.VramDestination ?? throw new InvalidOperationException("Verification requires ghostTransfer.VramDestination.")), "statue-ghost library VRAM destination");
            var nativeGhostVram = new SnesVram();
            SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeGhostVram,
                ghostTransfer.ListPointer, activeDoorPointer: 0);
            var installedGhostVram = new SnesVram();
            LibraryBackgroundLoader.ExecuteProgram(LibraryBackgroundStagingFixtures.Fresh(), installedGhostVram, LibraryBackgroundProgramDefinitions.Get(ghostTransfer.ListPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), CharacterArt = stock });
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
                    ghostImage.Pixels, ghostImage.Palette);
            RoomCharacterAtlasCatalog editedGhost = installed.LoadRoomCharacters();
            var editedGhostVram = new SnesVram();
            LibraryBackgroundLoader.ExecuteProgram(LibraryBackgroundStagingFixtures.Fresh(), editedGhostVram, LibraryBackgroundProgramDefinitions.Get(ghostTransfer.ListPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), CharacterArt = editedGhost });
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
            CartridgeRoomAssets editedPaletteRoom = LoadFixtureRoomAssets(landing,
                paletteArt: editedPalettes);
            var stockCgram = new SnesCgram();
            var editedCgram = new SnesCgram();
            native.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), stockCgram);
            editedPaletteRoom.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), editedCgram);
            AssertTrue(stockCgram.Colors[0] != editedCgram.Colors[0] &&
                stockCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]
                    .SequenceEqual(editedCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]),
                "installed RGB5 override changes only its room CGRAM color");
            var compiledStockCgram = new SnesCgram();
            stockPalettes.Get(paletteSource).LoadTo(compiledStockCgram);
            AssertTrue(new CgramPalettes(stockCgram).ImageEquals(new CgramPalettes(compiledStockCgram)),
                "installed stock room palette matches native room CGRAM");

            AssertTrue(File.Exists(Path.Combine(installed.RoomMetatileDirectory,
                    RoomMetatileArtworkFiles.ManifestFileName)),
                "room-block stock manifest is installed");
            RoomMetatileCatalog stockBlocks = installed.LoadRoomMetatiles();
            CartridgeRoomAssets installedBlocks = LoadFixtureRoomAssets(landing,
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
            CartridgeRoomAssets editedBlocks = LoadFixtureRoomAssets(landing,
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
            int stockSource = RoomBackgroundTilemapSources.All[0];
            int substitutedSource = stockSource + 1;
            AssertTrue(!RoomBackgroundTilemapSources.Contains(substitutedSource),
                "manifest substitution fixture uses an uncatalogued source");
            JsonNode alteredManifest = JsonNode.Parse(originalBackgroundManifest)
                ?? throw new InvalidDataException("Installed background manifest is empty.");
            JsonObject manifestEntries = alteredManifest.Required("entries").AsObject();
            string stockName = RoomBackgroundTilemapFormat.SourceFileName(stockSource);
            string substitutedName = RoomBackgroundTilemapFormat.SourceFileName(substitutedSource);
            JsonNode alteredEntry = manifestEntries.Required(stockName).DeepClone();
            alteredEntry["sourceAddress"] = substitutedSource;
            manifestEntries.Remove(stockName);
            manifestEntries[substitutedName] = alteredEntry;
            try
            {
                File.WriteAllText(backgroundManifestPath, alteredManifest.ToJsonString());
                try
                {
                    _ = installed.LoadRoomBackgroundTilemaps();
                    throw new InvalidOperationException(
                        "Substituted background source was accepted by the installation loader.");
                }
                catch (InvalidDataException error)
                {
                    AssertTrue(error.Message.Contains("invalid source entry",
                            StringComparison.Ordinal),
                        "substituted background source fails the compiled identity check");
                }
            }
            finally { File.WriteAllBytes(backgroundManifestPath, originalBackgroundManifest); }
            CartridgeRoomHeader ceres = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xdf8d);
            int backgroundSource = LibraryBackgroundSourceInventory.Scan(bus)
                .Single(source => source.ListPointer == ceres.State.BackgroundDataPointer &&
                    source.Command == LibraryBackgroundCommand.DecompressToWorkRam).SourceAddress;
            string backgroundName = RoomBackgroundTilemapFormat.SourceFileName(backgroundSource);
            var nativeBackgroundVram = new SnesVram();
            var stockBackgroundVram = new SnesVram();
            SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeBackgroundVram,
                ceres.State.BackgroundDataPointer, activeDoorPointer: 0);
            LibraryBackgroundLoader.ExecuteProgram(LibraryBackgroundStagingFixtures.Fresh(), stockBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = stockBackgrounds });
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
            LibraryBackgroundLoader.ExecuteProgram(LibraryBackgroundStagingFixtures.Fresh(), editedBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = installed.LoadRoomBackgroundTilemaps() });
            AssertTrue(nativeBackgroundVram.ReadWord(0x4800) != editedBackgroundVram.ReadWord(0x4800) &&
                nativeBackgroundVram.ReadWord(0x4801) == editedBackgroundVram.ReadWord(0x4801),
                "installed background override changes only its selected Ceres BG tile");

            RoomSkyTilemapCatalog stockSky = installed.LoadRoomSkyTilemaps();
            // The landing-cutscene door's command-E record selects the uploaded sky page.
            LibraryBackgroundInstruction landingSky = LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                LandingSiteRomData.LandingCutsceneDoorPointer);
            int skyPage = (landingSky.SourceAddress -
                RoomSkyTilemapFormat.FirstSourceAddress) / RoomSkyTilemapFormat.PageByteCount;
            string skyName = RoomSkyTilemapFormat.FileName(skyPage);
            string skyStockPath = Path.Combine(installed.RoomBackgroundTilemapDirectory, skyName);
            AssertTrue(File.Exists(skyStockPath), "selected scrolling-sky page is installed");
            byte[] nativeSky = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                landingSky.SourceAddress, landingSky.ByteCount);
            AssertTrue(stockSky.TryResolve(landingSky.SourceAddress,
                    landingSky.ByteCount, out ReadOnlyMemory<byte> installedSky) &&
                installedSky.Span.SequenceEqual(nativeSky),
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
            AssertTrue(editedSky.TryResolve(landingSky.SourceAddress,
                    landingSky.ByteCount, out ReadOnlyMemory<byte> changedSky) &&
                !changedSky.Span[..2].SequenceEqual(nativeSky.AsSpan(0, 2)) &&
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
                repaired.LoadRoomCharacters().Get(ghostSource).Transfer.Span.SequenceEqual(
                    editedGhost.Get(ghostSource).Transfer.Span),
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
            CartridgeRoomAssets repairedEdited = LoadFixtureRoomAssets(landing,
                repaired.LoadRoomCharacters());
            AssertTrue(repairedEdited.RoomCharacters.SequenceEqual(editedRoom.RoomCharacters),
                "repaired installation retains the selected user room artwork");
            CartridgeRoomAssets repairedPaletteRoom = LoadFixtureRoomAssets(landing,
                paletteArt: repaired.LoadRoomPalettes());
            var repairedCgram = new SnesCgram();
            repairedPaletteRoom.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), repairedCgram);
            AssertTrue(new CgramPalettes(repairedCgram).ImageEquals(new CgramPalettes(editedCgram)),
                "repaired installation retains the selected user room palette");
            CartridgeRoomAssets repairedBlocks = LoadFixtureRoomAssets(landing,
                metatileArt: repaired.LoadRoomMetatiles());
            AssertTrue(repairedBlocks.LevelData.BlockDefinitions.Span.SequenceEqual(
                    editedBlocks.LevelData.BlockDefinitions.Span),
                "repaired installation retains selected user visual blocks");
            var repairedBackgroundVram = new SnesVram();
            LibraryBackgroundLoader.ExecuteProgram(LibraryBackgroundStagingFixtures.Fresh(), repairedBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = repaired.LoadRoomBackgroundTilemaps() });
            AssertTrue(repairedBackgroundVram.ImageEquals(editedBackgroundVram),
                "repaired installation retains selected user background tilemap");
            AssertTrue(repaired.LoadRoomSkyTilemaps().TryResolve(landingSky.SourceAddress,
                    landingSky.ByteCount, out ReadOnlyMemory<byte> repairedSky) &&
                repairedSky.Span.SequenceEqual(changedSky.Span),
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
