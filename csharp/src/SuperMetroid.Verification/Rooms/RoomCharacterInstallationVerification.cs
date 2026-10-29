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
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks stock and overridden room characters, palettes, blocks, backgrounds, and sky artwork.</summary>
internal static class RoomCharacterInstallationVerification
{
    /// <summary>Full stock import, live edit, repair and invalid-override contract for room art.</summary>
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
            RoomCharacterAtlases stock = installed.LoadRoomCharacters();
            CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            SelectedRoom landing = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0x91f8);
            VerifyRoomVisualLayouts(installed, bus, landing);
            VerifyXrayRevealVisualInstallation(installed);
            LoadedRoomAssets native = LoadFixtureRoomAssets(landing);
            LoadedRoomAssets installedRoom = LoadFixtureRoomAssets(landing);
            ReadOnlyMemory<byte> nativeCharacters = native.TilesetCharactersIn(FixtureRoomCharacters);
            AssertTrue(installedRoom.CreCharactersIn(stock).Span.SequenceEqual(native.CreCharactersIn(FixtureRoomCharacters).Span) &&
                installedRoom.TilesetCharactersIn(stock).Span.SequenceEqual(nativeCharacters.Span),
                "installed stock room PNGs retain Landing Site character bytes");

            int source = native.Tileset.CharacterAddress;
            string name = RoomCharacterAtlasFormat.SourceFileName(source);
            int tileCount = nativeCharacters.Length / RoomCharacterAtlasFormat.BytesPerTile;
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
            RoomCharacterAtlases edited = installed.LoadRoomCharacters();
            LoadedRoomAssets editedRoom = LoadFixtureRoomAssets(landing);
            AssertTrue(!editedRoom.TilesetCharactersIn(edited).Span.SequenceEqual(nativeCharacters.Span),
                "installed PNG override changes the real room-loader output");
            AssertTrue(stock.GetAt(source).Characters.Span.SequenceEqual(nativeCharacters.Span),
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
            // The ghost's characters reach sprite characters only the statue ghost would draw, and it draws nothing, so the
            // installed replay writes them nowhere (#1466); the check above compares the installed characters byte for byte. The rest
            // of the upload matches the native one with that range left as the fresh video memory holds it.
            nativeGhostVram.ExecuteQueuedAssetWrite(
                new byte[LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount],
                LibraryBackgroundOperands.DestinationWord(LibraryBackgroundDestination.StatueGhostSpriteCharacters));
            AssertTrue(nativeGhostVram.ImageEquals(installedGhostVram),
                "installed statue-ghost library upload matches the native one apart from the characters only the ghost would draw");
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
            RoomCharacterAtlases editedGhost = installed.LoadRoomCharacters();
            SnesVram editedGhostVram = ShownVram(LibraryBackgroundProgramDefinitions.Get(ghostTransfer.ListPointer), new LibraryBackgroundArtwork { TilemapArt = RepositoryInstallation.Installation.LoadRoomBackgroundTilemaps(), CharacterArt = editedGhost });
            // The edit reaches the installed ghost characters only: the replay writes them to no shown video memory (#1466).
            ReadOnlySpan<byte> editedGhostCharacters = editedGhost.GetAt(ghostSource).Characters.Span;
            AssertTrue(editedGhostCharacters[0] != nativeGhost[0] && editedGhostCharacters[1..].SequenceEqual(nativeGhost.AsSpan(1)),
                "edited statue-ghost pixel changes only its selected character byte");
            AssertTrue(editedGhostVram.ImageEquals(installedGhostVram),
                "an edited statue-ghost character leaves the shown library upload unchanged");

            AssertTrue(File.Exists(Path.Combine(installed.RoomPaletteDirectory,
                    RoomStaticPaletteArtworkFiles.ManifestFileName)),
                "room-palette stock manifest is installed");
            RoomStaticPalettes stockPalettes = installed.LoadRoomPalettes();
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
            RoomStaticPalettes editedPalettes = installed.LoadRoomPalettes();
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
            new CgramPalettes(compiledStockCgram).Background.LoadPortBytes(RoomStaticPaletteImage.PackedColours(stockPalettes.GetAt(paletteSource)));
            AssertTrue(new CgramPalettes(stockCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(compiledStockCgram)),
                "installed stock room palette matches native room CGRAM");

            AssertTrue(File.Exists(Path.Combine(installed.RoomMetatileDirectory,
                    RoomMetatileArtworkFiles.ManifestFileName)),
                "room-block stock manifest is installed");
            // The stock block compositions are extraction input (#1431): the block sheets are composed from them and each
            // tileset records the appearance inputs its blocks depend on beside its sheets (#1466). Rooms load no composition.
            RoomMetatileCatalog stockBlocks = RoomMetatileArtworkFiles.Load(installed.RoomMetatileDirectory);
            InstalledRoomArtwork installedSheets = installed.LoadRoomBlockSheets();
            AssertTrue(File.Exists(Path.Combine(installed.RoomBlockSheetDirectory, RoomBlockAppearanceFiles.FileNameOf(native.Tileset))),
                "the Landing Site tileset's block appearance record is installed beside its block sheets");
            AssertEqual((stockBlocks.Cre.BlockTiles.Length + stockBlocks.Get(native.Tileset.Blocks).BlockTiles.Length) / RoomMetatileFormat.TilesPerBlock,
                installedSheets.Get(native.Tileset).Inputs.Count,
                "the installed appearance record covers every block a Landing Site room loads");
            string blockName = RoomMetatileFormat.CreFileName;
            string blockStockPath = Path.Combine(installed.RoomMetatileDirectory, blockName);

            AssertTrue(File.Exists(Path.Combine(installed.RoomBackgroundTilemapDirectory,
                    RoomBackgroundTilemapArtworkFiles.ManifestFileName)),
                "room-background stock manifest is installed");
            RoomBackgroundTilemaps stockBackgrounds = installed.LoadRoomBackgroundTilemaps();
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
            string stockName = RoomBackgroundTilemapArtworkFiles.FileName(Enum.GetValues<RoomBackgroundTilemap>()[0]);
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
            string backgroundName = RoomBackgroundTilemapArtworkFiles.FileName(
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

            RoomSkyTilemaps stockSky = installed.LoadRoomSkyTilemaps();
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
            AssertTrue(BackgroundTileWordBytes.Of(stockSky.Page(skyPage, landingSky.ByteCount / sizeof(ushort)).Span).AsSpan().SequenceEqual(nativeSky),
                "installed scrolling-sky page retains native door-selected bytes");
            JsonNode skyDocument = JsonNode.Parse(File.ReadAllText(skyStockPath))
                ?? throw new InvalidDataException("Installed scrolling-sky JSON is empty.");
            JsonNode skyCell = skyDocument.Required("pages").Required(0).Required("cells").Required(0);
            skyCell["tileColumn"] = (skyCell.Required("tileColumn").GetValue<int>() + 1)
                % RoomBackgroundTilemapFormat.TileColumns;
            string skyOverridePath = Path.Combine(
                installed.RoomBackgroundTilemapOverrideDirectory, skyName);
            File.WriteAllText(skyOverridePath, skyDocument.ToJsonString());
            RoomSkyTilemaps editedSky = installed.LoadRoomSkyTilemaps();
            ReadOnlyMemory<byte> changedSky = BackgroundTileWordBytes.Of(editedSky.Page(skyPage, landingSky.ByteCount / sizeof(ushort)).Span);
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
            AssertTrue(File.Exists(Path.Combine(repaired.RoomMetatileDirectory, blockName)),
                "room-block stock is restored by repair");
            AssertTrue(File.Exists(backgroundOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomBackgroundTilemapDirectory, backgroundName)),
                "room-background stock is restored while its override survives repair");
            AssertTrue(File.Exists(skyOverridePath) &&
                File.Exists(Path.Combine(repaired.RoomBackgroundTilemapDirectory, skyName)),
                "scrolling-sky stock is restored while its override survives repair");
            RoomCharacterAtlases repairedCharacters = repaired.LoadRoomCharacters();
            LoadedRoomAssets repairedEdited = LoadFixtureRoomAssets(landing);
            AssertTrue(repairedEdited.TilesetCharactersIn(repairedCharacters).Span.SequenceEqual(editedRoom.TilesetCharactersIn(edited).Span),
                "repaired installation retains the selected user room artwork");
            LoadedRoomAssets repairedPaletteRoom = LoadFixtureRoomAssets(landing,
                paletteArt: repaired.LoadRoomPalettes());
            var repairedCgram = new GameplayColours(new SpriteColourSets());
            repairedPaletteRoom.LoadGraphics(BgArtworkFixtures.For(new SnesVram()), repairedCgram.BackgroundSets);
            AssertTrue(new CgramPalettes(repairedCgram.AsPaletteColors()).ImageEquals(new CgramPalettes(editedCgram.AsPaletteColors())),
                "repaired installation retains the selected user room palette");
            AssertEqual(installedSheets.ContentIdentity, repaired.LoadRoomBlockSheets().ContentIdentity,
                "repaired installation composes the same block pictures and character records from the restored stock blocks");
            var repairedBackgroundVram = new SnesVram();
            LibraryBackgroundReplay.Execute(repairedBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = repaired.LoadRoomBackgroundTilemaps() });
            AssertTrue(repairedBackgroundVram.ImageEquals(editedBackgroundVram),
                "repaired installation retains selected user background tilemap");
            AssertTrue(BackgroundTileWordBytes.Of(repaired.LoadRoomSkyTilemaps().Page(skyPage, landingSky.ByteCount / sizeof(ushort)).Span).AsSpan().SequenceEqual(changedSky.Span),
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
    /// Runs a library background program with installed character art and returns the VRAM it leaves: the statue ghost's
    /// characters are written nowhere, as only the statue ghost would draw them and it draws nothing.
    /// </summary>
    private static SnesVram ShownVram(LibraryBackgroundProgram program, LibraryBackgroundArtwork artwork)
    {
        var vram = new SnesVram();
        LibraryBackgroundReplay.Execute(vram, program, activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: artwork);
        return vram;
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
