using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Full stock import, live edit, repair and invalid-override contract for room art.</summary>
    private static void VerifyRoomArtworkInstallation(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "room-character-installation-" + Guid.NewGuid().ToString("N")));
        try
        {
            GameInstallation installed = GameAssetInstaller.Install(sourceRom, root);
            RoomArtIndex artIndex = RoomArtIndexFiles.Load(installed.ContentDirectory);
            AssertEqual(RoomHeaderDefinitions.RetailRoomCount, artIndex.Rooms.Length,
                "room-art guide covers every retail room");
            AssertEqual(RoomStateDefinitions.RetailStateCount,
                artIndex.Rooms.Sum(room => room.States.Length),
                "room-art guide covers every selectable retail state");
            AssertEqual(artIndex.Rooms.Length,
                artIndex.Rooms.Select(room => room.RoomId).Distinct().Count(),
                "room-art guide uses unique logical room IDs");
            AssertEqual(RoomSkyTilemapFormat.PageCount, artIndex.ScrollingSkyArtwork.Length,
                "room-art guide exposes every streaming-sky page");
            foreach (string relativePath in artIndex.ScrollingSkyArtwork)
                AssertTrue(File.Exists(Path.Combine(installed.ContentDirectory, relativePath)),
                    $"room-art guide references installed sky page {relativePath}");
            foreach (RoomArtEntry room in artIndex.Rooms)
            foreach (RoomArtStateEntry state in room.States)
            {
                foreach (string relativePath in new[] { state.Characters, state.Blocks,
                             state.Layout, state.Palette }.Concat(state.BackgroundArtwork))
                    AssertTrue(File.Exists(Path.Combine(installed.ContentDirectory, relativePath)),
                        $"room-art guide entry {room.RoomId}/{state.Variant} references installed {relativePath}");
            }
            VerifyRoomArtworkRenderParity(installed, sourceRom);
            VerifyLibraryBackgroundInstalledParity(sourceRom, installed);
            AssertTrue(File.Exists(Path.Combine(installed.RoomCharacterDirectory,
                    RoomCharacterArtworkFiles.ManifestFileName)),
                "room-character stock manifest is installed");
            RoomCharacterAtlasCatalog stock = installed.LoadRoomCharacters();
            SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
            CartridgeRoomHeader landing = CartridgeRoomHeader.Load(bus, 0x91f8);
            VerifyRoomVisualLayouts(installed, bus, landing);
            VerifyXrayRevealVisualInstallation(installed);
            RoomArtStateEntry landingGuide = artIndex.Rooms.Single(room => room.RoomId == "00/00")
                .States.Single(state => state.Variant == "default");
            AssertEqual("room-characters/" + RoomCharacterAtlasFormat.SourceFileName(
                    RoomTilesetDefinitions.Get(landing.State.GraphicsSet).CharacterAddress),
                landingGuide.Characters, "Landing Site room-ID guide selects its real character sheet");
            string artIndexText = File.ReadAllText(installed.RoomArtIndexPath);
            File.WriteAllText(installed.RoomArtIndexPath, "damaged index");
            _ = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Room-art index repair lost the installation.");
            AssertEqual(artIndexText, File.ReadAllText(installed.RoomArtIndexPath),
                "corrupt room-art guide is repaired from the installed cartridge");
            CartridgeRoomAssets native = CartridgeRoomAssets.Load(bus, landing);
            CartridgeRoomAssets installedRoom = CartridgeRoomAssets.Load(bus, landing, stock);
            AssertTrue(installedRoom.CreCharacters.AsSpan().SequenceEqual(native.CreCharacters) &&
                installedRoom.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                "installed stock room PNGs retain Landing Site character bytes");

            int source = native.Tileset.CharacterAddress;
            string name = RoomCharacterAtlasFormat.SourceFileName(source);
            int tileCount = native.RoomCharacters.Length / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            IndexedPngImage image;
            using (var stockFile = File.OpenRead(Path.Combine(installed.RoomCharacterDirectory, name)))
                image = IndexedPng.Read(stockFile, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            Directory.CreateDirectory(installed.RoomCharacterOverrideDirectory);
            string overridePath = Path.Combine(installed.RoomCharacterOverrideDirectory, name);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            RoomCharacterAtlasCatalog edited = installed.LoadRoomCharacters();
            CartridgeRoomAssets editedRoom = CartridgeRoomAssets.Load(bus, landing, edited);
            AssertTrue(!editedRoom.RoomCharacters.AsSpan().SequenceEqual(native.RoomCharacters),
                "installed PNG override changes the real room-loader output");
            AssertTrue(stock.Get(source).Transfer.Span.SequenceEqual(native.RoomCharacters),
                "room override does not mutate stock artwork already loaded");

            int ghostSource = RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress;
            string ghostName = RoomCharacterAtlasFormat.SourceFileName(ghostSource);
            string ghostStockPath = Path.Combine(installed.RoomCharacterDirectory, ghostName);
            byte[] nativeGhost = SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus,
                ghostSource, RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount);
            AssertTrue(stock.Get(ghostSource).Transfer.Span.SequenceEqual(nativeGhost),
                "installed statue-ghost PNG preserves every native character byte");
            LibraryBackgroundSource ghostTransfer = LibraryBackgroundSourceInventory.Scan(bus)
                .Single(entry => entry.SourceAddress == ghostSource);
            AssertEqual((ushort)RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount,
                ghostTransfer.TransferByteCount!.Value, "statue-ghost library upload length");
            AssertEqual(RoomAssetRomData.LibraryBackground.TourianStatueGhost.VramDestinationWord,
                ghostTransfer.VramDestination!.Value, "statue-ghost library VRAM destination");
            var nativeGhostVram = new SnesVram();
            LibraryBackgroundLoader.Execute(bus, nativeGhostVram, ghostTransfer.ListPointer,
                activeDoorPointer: 0);
            var installedGhostVram = new SnesVram();
            LibraryBackgroundLoader.Execute(bus, installedGhostVram, ghostTransfer.ListPointer,
                activeDoorPointer: 0, characterArt: stock);
            AssertTrue(nativeGhostVram.Bytes.SequenceEqual(installedGhostVram.Bytes),
                "installed statue-ghost PNG matches the complete native library upload");
            int ghostTileCount = nativeGhost.Length / RoomCharacterAtlasFormat.BytesPerTile;
            int ghostColumns = Math.Min(RoomCharacterAtlasFormat.TileColumns, ghostTileCount);
            int ghostRows = (ghostTileCount + ghostColumns - 1) / ghostColumns;
            IndexedPngImage ghostImage;
            using (var stockFile = File.OpenRead(ghostStockPath))
                ghostImage = IndexedPng.Read(stockFile, ghostColumns * 8, ghostRows * 8);
            ghostImage.Pixels[0] ^= 1;
            string ghostOverridePath = Path.Combine(installed.RoomCharacterOverrideDirectory, ghostName);
            using (var output = File.Create(ghostOverridePath))
                IndexedPng.Write(output, ghostImage.Width, ghostImage.Height,
                    ghostImage.Pixels, ghostImage.Palette);
            RoomCharacterAtlasCatalog editedGhost = installed.LoadRoomCharacters();
            var editedGhostVram = new SnesVram();
            LibraryBackgroundLoader.Execute(bus, editedGhostVram, ghostTransfer.ListPointer,
                activeDoorPointer: 0, characterArt: editedGhost);
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
            JsonNode firstColor = paletteDocument["colors"]![0]!;
            int stockRed = firstColor["red"]!.GetValue<int>();
            firstColor["red"] = stockRed ^ 1;
            Directory.CreateDirectory(installed.RoomPaletteOverrideDirectory);
            string paletteOverridePath = Path.Combine(installed.RoomPaletteOverrideDirectory, paletteName);
            File.WriteAllText(paletteOverridePath, paletteDocument.ToJsonString());
            RoomStaticPaletteCatalog editedPalettes = installed.LoadRoomPalettes();
            CartridgeRoomAssets editedPaletteRoom = CartridgeRoomAssets.Load(bus, landing,
                paletteArt: editedPalettes);
            var stockCgram = new SnesCgram();
            var editedCgram = new SnesCgram();
            native.LoadGraphics(new SnesVram(), stockCgram);
            editedPaletteRoom.LoadGraphics(new SnesVram(), editedCgram);
            AssertTrue(stockCgram.Colors[0] != editedCgram.Colors[0] &&
                stockCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]
                    .SequenceEqual(editedCgram.Colors[1..RoomStaticPaletteFormat.ColorCount]),
                "installed RGB5 override changes only its room CGRAM color");
            var compiledStockCgram = new SnesCgram();
            stockPalettes.Get(paletteSource).LoadTo(compiledStockCgram);
            AssertTrue(stockCgram.Colors.SequenceEqual(compiledStockCgram.Colors),
                "installed stock room palette matches native room CGRAM");

            AssertTrue(File.Exists(Path.Combine(installed.RoomMetatileDirectory,
                    RoomMetatileArtworkFiles.ManifestFileName)),
                "room-block stock manifest is installed");
            RoomMetatileCatalog stockBlocks = installed.LoadRoomMetatiles();
            CartridgeRoomAssets installedBlocks = CartridgeRoomAssets.Load(bus, landing,
                metatileArt: stockBlocks);
            AssertTrue(native.LevelData.BlockDefinitions.Span.SequenceEqual(
                    installedBlocks.LevelData.BlockDefinitions.Span),
                "installed stock room-block JSON retains native visual tile words");
            string blockName = RoomMetatileFormat.CreFileName;
            string blockStockPath = Path.Combine(installed.RoomMetatileDirectory, blockName);
            JsonNode blockDocument = JsonNode.Parse(File.ReadAllText(blockStockPath))
                ?? throw new InvalidDataException("Installed room-block JSON is empty.");
            JsonNode firstTile = blockDocument["blocks"]![0]!["topLeft"]!;
            firstTile["tileColumn"] = (firstTile["tileColumn"]!.GetValue<int>() + 1)
                % RoomMetatileFormat.TileColumns;
            Directory.CreateDirectory(installed.RoomMetatileOverrideDirectory);
            string blockOverridePath = Path.Combine(installed.RoomMetatileOverrideDirectory, blockName);
            File.WriteAllText(blockOverridePath, blockDocument.ToJsonString());
            CartridgeRoomAssets editedBlocks = CartridgeRoomAssets.Load(bus, landing,
                metatileArt: installed.LoadRoomMetatiles());
            AssertTrue(!native.LevelData.BlockDefinitions.Span.SequenceEqual(
                    editedBlocks.LevelData.BlockDefinitions.Span) &&
                native.LevelData.ForegroundEntries.Span.SequenceEqual(
                    editedBlocks.LevelData.ForegroundEntries.Span) &&
                native.LevelData.BehaviorBytes.Span.SequenceEqual(
                    editedBlocks.LevelData.BehaviorBytes.Span),
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
            JsonObject manifestEntries = alteredManifest["entries"]!.AsObject();
            string stockName = RoomBackgroundTilemapFormat.SourceFileName(stockSource);
            string substitutedName = RoomBackgroundTilemapFormat.SourceFileName(substitutedSource);
            JsonNode alteredEntry = manifestEntries[stockName]!.DeepClone();
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
            CartridgeRoomHeader ceres = CartridgeRoomHeader.Load(bus, 0xdf8d);
            int backgroundSource = LibraryBackgroundSourceInventory.Scan(bus)
                .Single(source => source.ListPointer == ceres.State.BackgroundDataPointer &&
                    source.Command == LibraryBackgroundCommand.DecompressToWorkRam).SourceAddress;
            string backgroundName = RoomBackgroundTilemapFormat.SourceFileName(backgroundSource);
            var nativeBackgroundVram = new SnesVram();
            var stockBackgroundVram = new SnesVram();
            LibraryBackgroundLoader.Execute(bus, nativeBackgroundVram,
                ceres.State.BackgroundDataPointer, activeDoorPointer: 0);
            LibraryBackgroundLoader.Execute(bus, stockBackgroundVram,
                ceres.State.BackgroundDataPointer, activeDoorPointer: 0,
                tilemapArt: stockBackgrounds);
            AssertTrue(nativeBackgroundVram.Bytes.SequenceEqual(stockBackgroundVram.Bytes),
                "installed Ceres background tilemap retains native VRAM words");
            string backgroundStockPath = Path.Combine(installed.RoomBackgroundTilemapDirectory,
                backgroundName);
            JsonNode backgroundDocument = JsonNode.Parse(File.ReadAllText(backgroundStockPath))
                ?? throw new InvalidDataException("Installed background JSON is empty.");
            JsonNode backgroundCell = backgroundDocument["pages"]![0]!["cells"]![0]!;
            backgroundCell["tileColumn"] = (backgroundCell["tileColumn"]!.GetValue<int>() + 1)
                % RoomBackgroundTilemapFormat.TileColumns;
            Directory.CreateDirectory(installed.RoomBackgroundTilemapOverrideDirectory);
            string backgroundOverridePath = Path.Combine(
                installed.RoomBackgroundTilemapOverrideDirectory, backgroundName);
            File.WriteAllText(backgroundOverridePath, backgroundDocument.ToJsonString());
            var editedBackgroundVram = new SnesVram();
            LibraryBackgroundLoader.Execute(bus, editedBackgroundVram,
                ceres.State.BackgroundDataPointer, activeDoorPointer: 0,
                tilemapArt: installed.LoadRoomBackgroundTilemaps());
            AssertTrue(nativeBackgroundVram.ReadWord(0x4800) != editedBackgroundVram.ReadWord(0x4800) &&
                nativeBackgroundVram.ReadWord(0x4801) == editedBackgroundVram.ReadWord(0x4801),
                "installed background override changes only its selected Ceres BG tile");

            RoomSkyTilemapCatalog stockSky = installed.LoadRoomSkyTilemaps();
            LandingSiteEntryState landingEntry = LandingSiteEntryState.LoadLandingCutscene(bus);
            VerifyLandingSiteInstalledArtwork(installed, bus, landingEntry,
                stock, edited, stockBlocks, stockSky);
            int skyPage = (landingEntry.SkySourceAddress -
                RoomSkyTilemapFormat.FirstSourceAddress) / RoomSkyTilemapFormat.PageByteCount;
            string skyName = RoomSkyTilemapFormat.FileName(skyPage);
            string skyStockPath = Path.Combine(installed.RoomBackgroundTilemapDirectory, skyName);
            AssertTrue(File.Exists(skyStockPath), "selected scrolling-sky page is installed");
            byte[] nativeSky = SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus,
                landingEntry.SkySourceAddress, landingEntry.SkyByteCount);
            AssertTrue(stockSky.TryResolve(landingEntry.SkySourceAddress,
                    landingEntry.SkyByteCount, out ReadOnlyMemory<byte> installedSky) &&
                installedSky.Span.SequenceEqual(nativeSky),
                "installed scrolling-sky page retains native door-selected bytes");
            JsonNode skyDocument = JsonNode.Parse(File.ReadAllText(skyStockPath))
                ?? throw new InvalidDataException("Installed scrolling-sky JSON is empty.");
            JsonNode skyCell = skyDocument["pages"]![0]!["cells"]![0]!;
            skyCell["tileColumn"] = (skyCell["tileColumn"]!.GetValue<int>() + 1)
                % RoomBackgroundTilemapFormat.TileColumns;
            string skyOverridePath = Path.Combine(
                installed.RoomBackgroundTilemapOverrideDirectory, skyName);
            File.WriteAllText(skyOverridePath, skyDocument.ToJsonString());
            RoomSkyTilemapCatalog editedSky = installed.LoadRoomSkyTilemaps();
            AssertTrue(editedSky.TryResolve(landingEntry.SkySourceAddress,
                    landingEntry.SkyByteCount, out ReadOnlyMemory<byte> changedSky) &&
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
            CartridgeRoomAssets repairedEdited = CartridgeRoomAssets.Load(bus, landing,
                repaired.LoadRoomCharacters());
            AssertTrue(repairedEdited.RoomCharacters.AsSpan().SequenceEqual(editedRoom.RoomCharacters),
                "repaired installation retains the selected user room artwork");
            CartridgeRoomAssets repairedPaletteRoom = CartridgeRoomAssets.Load(bus, landing,
                paletteArt: repaired.LoadRoomPalettes());
            var repairedCgram = new SnesCgram();
            repairedPaletteRoom.LoadGraphics(new SnesVram(), repairedCgram);
            AssertTrue(repairedCgram.Colors.SequenceEqual(editedCgram.Colors),
                "repaired installation retains the selected user room palette");
            CartridgeRoomAssets repairedBlocks = CartridgeRoomAssets.Load(bus, landing,
                metatileArt: repaired.LoadRoomMetatiles());
            AssertTrue(repairedBlocks.LevelData.BlockDefinitions.Span.SequenceEqual(
                    editedBlocks.LevelData.BlockDefinitions.Span),
                "repaired installation retains selected user visual blocks");
            var repairedBackgroundVram = new SnesVram();
            LibraryBackgroundLoader.Execute(bus, repairedBackgroundVram,
                ceres.State.BackgroundDataPointer, activeDoorPointer: 0,
                tilemapArt: repaired.LoadRoomBackgroundTilemaps());
            AssertTrue(repairedBackgroundVram.Bytes.SequenceEqual(editedBackgroundVram.Bytes),
                "repaired installation retains selected user background tilemap");
            AssertTrue(repaired.LoadRoomSkyTilemaps().TryResolve(landingEntry.SkySourceAddress,
                    landingEntry.SkyByteCount, out ReadOnlyMemory<byte> repairedSky) &&
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
}
