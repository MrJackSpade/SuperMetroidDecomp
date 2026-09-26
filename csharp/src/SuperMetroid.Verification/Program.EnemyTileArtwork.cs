using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using System.Text.Json;

internal static partial class Program
{
    private static void VerifyEnemyTileArtwork()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Enemy tile artwork: private ROM absent; retail extraction skipped.");
            return;
        }
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "enemy-tiles-" + Guid.NewGuid().ToString("N"));
        try
        {
            EnemyTileArtworkFiles.Extract(bus, directory, SupportedCartridge.Sha256);
            EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(directory, null);
            VerifyInstalledCeresDoorVisuals(bus, directory, stock);
            VerifyInstalledEnemyProjectileSpritemaps(bus, directory, stock);
            VerifyInstalledKraidBackground(bus, directory, stock);
            VerifyInstalledKraidColors(bus, directory, stock);
            VerifyInstalledEnemySpritemaps(bus, directory, stock);
            VerifyInstalledEnemyExtendedFrames(bus, directory, stock);
            VerifyInstalledGunshipLiftoffArtwork(bus, directory, stock);
            string[] files = Directory.GetFiles(directory, "enemy-????-tiles.png");
            AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount, files.Length,
                "one indexed PNG per distinct retail enemy graphics definition");
            var stockMeltImages = new List<byte[]>();
            ushort[] stockFirstMeltTiles = VerifyInstalledCrocomireMeltingTilemap(
                bus, stock, CrocomireMeltingArtworkAddresses.FirstTilemap,
                CrocomireInstructionProgramDefinitions.MeltingOneTopRow);
            ushort[] stockSecondMeltTiles = VerifyInstalledCrocomireMeltingTilemap(
                bus, stock, CrocomireMeltingArtworkAddresses.SecondTilemap,
                CrocomireInstructionProgramDefinitions.MeltingTwoTopRow);
            foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
            {
                byte[] actual = VerifyInstalledCrocomireMeltingPass(bus, stock, pass);
                var expected = new byte[CrocomireDeathState.MeltingGraphicsByteCount];
                foreach (CrocomireMeltingCopy copy in pass.Copies.Span)
                {
                    int destination = copy.DestinationWord - 0x4000;
                    for (int index = 0; index < (pass.WordsToCopy + 1) * 2; index++)
                        expected[destination + index] = bus.ReadByte(
                            (pass.SourceBank << 16) |
                            unchecked((ushort)(copy.SourceWord + index)));
                }
                AssertTrue(actual.SequenceEqual(expected),
                    $"installed Crocomire melt pass ${pass.HeaderOffset:X4} preserves native scratch bytes");
                stockMeltImages.Add(actual);
            }
            foreach (string file in files)
            {
                ushort pointer = Convert.ToUInt16(Path.GetFileName(file).Substring(6, 4), 16);
                RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, pointer);
                int byteCount = definition.TileDataSize & 0x7fff;
                byte[] native = RomDataReader.ReadFixedBank(bus, definition.TileDataAddress, byteCount);
                AssertTrue(stock.TryResolve(definition.TileDataAddress, byteCount,
                        out ReadOnlyMemory<byte> queued) && queued.Span.SequenceEqual(native),
                    $"enemy ${pointer:X4} queued VRAM DMA resolves its installed PNG");
                var vram = new SnesVram();
                stock.LoadTo(pointer, byteCount, vram, 0);
                AssertTrue(vram.Bytes[..byteCount].SequenceEqual(native),
                    $"enemy ${pointer:X4} PNG preserves every native tile byte");
                var nativeCgram = new SnesCgram();
                var installedCgram = new SnesCgram();
                nativeCgram.LoadFromBus(bus, (definition.Bank << 16) | definition.PalettePointer,
                    EnemyPaletteSheet.ColorCount, destinationIndex: 8 * 16);
                stock.LoadPaletteTo(pointer, installedCgram, 8 * 16);
                AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                    $"enemy ${pointer:X4} RGB5 JSON preserves all native palette slots");
            }

            string editedFile = files[0];
            ushort editedPointer = Convert.ToUInt16(Path.GetFileName(editedFile).Substring(6, 4), 16);
            RoomEnemyDefinition editedDefinition = RoomEnemySystem.ReadDefinition(bus, editedPointer);
            int editedByteCount = editedDefinition.TileDataSize & 0x7fff;
            int tileCount = editedByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tileCount, RoomCharacterAtlasFormat.TileColumns);
            int rows = (tileCount + columns - 1) / columns;
            using var stockPng = new MemoryStream(File.ReadAllBytes(editedFile), writable: false);
            IndexedPngImage image = IndexedPng.Read(stockPng, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory, "overrides");
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, Path.GetFileName(editedFile));
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            string meltFile = Path.Combine(directory,
                CrocomireMeltingArtworkFormat.FirstFileName);
            using (var meltInput = new MemoryStream(File.ReadAllBytes(meltFile), writable: false))
            {
                IndexedPngImage meltImage = IndexedPng.Read(meltInput, 256, 32);
                meltImage.Pixels[0] ^= 1;
                using var meltOutput = File.Create(Path.Combine(overrideDirectory,
                    CrocomireMeltingArtworkFormat.FirstFileName));
                IndexedPng.Write(meltOutput, meltImage.Width, meltImage.Height,
                    meltImage.Pixels, meltImage.Palette);
            }
            EnemyTileArtworkCatalog editedMelt = EnemyTileArtworkFiles.Load(
                directory, overrideDirectory);
            byte[] changedMelt = VerifyInstalledCrocomireMeltingPass(bus, editedMelt,
                CrocomireMeltingTransferDefinitions.Passes[0]);
            AssertEqual((byte)(stockMeltImages[0][0] ^ 0x80), changedMelt[0],
                "Crocomire melt PNG edit changes the live first planar pixel");
            AssertTrue(changedMelt.AsSpan(1).SequenceEqual(stockMeltImages[0].AsSpan(1)),
                "Crocomire melt PNG edit leaves all other scratch bytes unchanged");
            AssertTrue(VerifyInstalledCrocomireMeltingPass(bus, editedMelt,
                    CrocomireMeltingTransferDefinitions.Passes[1])
                .SequenceEqual(stockMeltImages[1]),
                "first-melt PNG edit does not change the second pass");
            AssertTrue(VerifyInstalledCrocomireMeltingPass(bus,
                    EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                    CrocomireMeltingTransferDefinitions.Passes[0])
                .SequenceEqual(changedMelt),
                "Crocomire melt override survives catalog reload");
            string firstTilemapFile = Path.Combine(directory,
                CrocomireMeltingArtworkFormat.FirstTilemapFileName);
            CrocomireMeltingTilemapDocument tilemapDocument =
                JsonSerializer.Deserialize<CrocomireMeltingTilemapDocument>(
                    File.ReadAllBytes(firstTilemapFile),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            CrocomireMeltingTilemapCell[] editedCells =
                (CrocomireMeltingTilemapCell[])tilemapDocument.Cells.Clone();
            editedCells[0] = editedCells[0] with
            {
                TileIndex = editedCells[0].TileIndex ^ 1,
            };
            string tilemapOverride = Path.Combine(overrideDirectory,
                CrocomireMeltingArtworkFormat.FirstTilemapFileName);
            using (var tilemapOutput = File.Create(tilemapOverride))
                CrocomireMeltingArtwork.WriteTilemap(tilemapOutput,
                    tilemapDocument with { Cells = editedCells });
            EnemyTileArtworkCatalog editedMeltMap = EnemyTileArtworkFiles.Load(
                directory, overrideDirectory);
            ushort[] changedTiles = VerifyInstalledCrocomireMeltingTilemap(
                bus, editedMeltMap, CrocomireMeltingArtworkAddresses.FirstTilemap,
                CrocomireInstructionProgramDefinitions.MeltingOneTopRow,
                compareRom: false);
            AssertEqual((ushort)(stockFirstMeltTiles[0] ^ 1), changedTiles[0],
                "Crocomire melt JSON edit changes its selected BG2 tile reference");
            AssertTrue(changedTiles.AsSpan(1).SequenceEqual(stockFirstMeltTiles.AsSpan(1)),
                "Crocomire melt JSON edit leaves other BG2 cells unchanged");
            AssertTrue(VerifyInstalledCrocomireMeltingTilemap(
                    bus, editedMeltMap,
                    CrocomireMeltingArtworkAddresses.SecondTilemap,
                    CrocomireInstructionProgramDefinitions.MeltingTwoTopRow,
                    compareRom: false)
                .SequenceEqual(stockSecondMeltTiles),
                "first-melt tilemap edit does not change the second melt");
            AssertTrue(VerifyInstalledCrocomireMeltingTilemap(bus,
                    EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                    CrocomireMeltingArtworkAddresses.FirstTilemap,
                    CrocomireInstructionProgramDefinitions.MeltingOneTopRow,
                    compareRom: false)
                .SequenceEqual(changedTiles),
                "Crocomire melt tilemap override survives catalog reload");
            var stockVram = new SnesVram();
            var editedVram = new SnesVram();
            stock.LoadTo(editedPointer, editedByteCount, stockVram, 0);
            edited.LoadTo(editedPointer, editedByteCount, editedVram, 0);
            AssertEqual((byte)(stockVram.ReadByte(0) ^ 0x80), editedVram.ReadByte(0),
                "edited enemy PNG changes its first indexed pixel at the native tile location");
            AssertTrue(stockVram.Bytes[1..].SequenceEqual(editedVram.Bytes[1..]),
                "enemy PNG edit leaves all neighboring VRAM bytes unchanged");
            var reloadedVram = new SnesVram();
            EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .LoadTo(editedPointer, editedByteCount, reloadedVram, 0);
            AssertTrue(reloadedVram.Bytes.SequenceEqual(editedVram.Bytes),
                "enemy override survives content reload without modifying stock PNG");

            string stockPalettePath = Path.Combine(directory,
                EnemyTileArtworkFormat.PaletteFileName(editedPointer));
            EnemyPaletteSheetDocument paletteDocument = JsonSerializer.Deserialize<EnemyPaletteSheetDocument>(
                File.ReadAllBytes(stockPalettePath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            PaletteRgb5[] editedColors = (PaletteRgb5[])paletteDocument.Colors.Clone();
            editedColors[0] = editedColors[0] with { Red = editedColors[0].Red ^ 1 };
            string paletteOverridePath = Path.Combine(overrideDirectory,
                EnemyTileArtworkFormat.PaletteFileName(editedPointer));
            File.WriteAllBytes(paletteOverridePath, EnemyPaletteSheet.Write(
                new EnemyPaletteSheetDocument { Version = 1, Colors = editedColors }));
            EnemyTileArtworkCatalog editedPalette = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            var originalColors = new SnesCgram();
            var changedColors = new SnesCgram();
            stock.LoadPaletteTo(editedPointer, originalColors, 8 * 16);
            editedPalette.LoadPaletteTo(editedPointer, changedColors, 8 * 16);
            AssertEqual((ushort)(originalColors.Colors[128] ^ 1), changedColors.Colors[128],
                "RGB5 override changes its selected enemy palette channel");
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color != 128)
                    AssertEqual(originalColors.Colors[color], changedColors.Colors[color],
                        "enemy palette edit leaves all other CGRAM slots unchanged");
            var reloadedColors = new SnesCgram();
            EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .LoadPaletteTo(editedPointer, reloadedColors, 8 * 16);
            AssertTrue(reloadedColors.Colors.SequenceEqual(changedColors.Colors),
                "enemy palette override survives catalog reload");

            byte[] original = File.ReadAllBytes(editedFile);
            File.WriteAllBytes(editedFile, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
                "corrupt stock enemy artwork fails its manifest hash");
            File.WriteAllBytes(editedFile, original);
            File.WriteAllBytes(overridePath, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                "malformed enemy override fails loudly");
            File.WriteAllBytes(overridePath, File.ReadAllBytes(editedFile));
            string meltOverride = Path.Combine(overrideDirectory,
                CrocomireMeltingArtworkFormat.FirstFileName);
            File.WriteAllBytes(meltOverride, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(
                    directory, overrideDirectory),
                "malformed Crocomire melt override fails loudly");
            File.WriteAllBytes(meltOverride, File.ReadAllBytes(meltFile));
            File.WriteAllBytes(tilemapOverride, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(
                    directory, overrideDirectory),
                "malformed Crocomire tilemap override fails loudly");
            File.WriteAllBytes(tilemapOverride, File.ReadAllBytes(firstTilemapFile));
            File.WriteAllBytes(paletteOverridePath, new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                "malformed enemy palette override fails loudly");
            File.WriteAllText(paletteOverridePath, "{\"version\":1,\"version\":1,\"colors\":[]}");
            AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                "duplicate enemy palette keys fail loudly");
            editedColors[0] = editedColors[0] with { Red = 32 };
            AssertThrows<InvalidDataException>(() => EnemyPaletteSheet.Write(
                    new EnemyPaletteSheetDocument { Version = 1, Colors = editedColors }),
                "RGB5 palette channel outside native five-bit precision is rejected");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
        Console.WriteLine("  Enemy artwork: 122 retail tile/color sheets, Crocomire melts, Kraid body/head maps, backdrop PNG, palette effects and four HUD-derived death restores, plus Boyon/Cacatac/Boulder/Atomic/Skultera/Waver/Skree/Metaree/Zoa/Pipe Bug/Fake Kraid/Kraid nail/Owtch/Stoke/Ripper/Fireflea/Magdollite visual frames pass stock parity, live edits, persistence, and invalid-resource checks.");
    }
}
