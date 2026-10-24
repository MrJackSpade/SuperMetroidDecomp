using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Rooms.RoomEnemiesVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Text.Json;
using static SuperMetroid.Verification.Areas.Ceres.CeresEscapeOverlayTilemapsVerification;
using static SuperMetroid.Verification.Areas.Ceres.Artwork.CeresEscapeTileArtworkVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBg2ScrollVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBodyVisualsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.Artwork.CrocomireSkeletonArtworkVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoArtworkVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTourianCorpseArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemySpritemapArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkBabyMetroidCutsceneColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkBotwoonColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkCeresDoorVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkChozoAndTubeColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkCrocomireColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDachoraColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonBg2FramesVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkGunshipLiftoffVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkKraidColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkKraidVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkMagdollitePaletteVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkMotherBrainDeathColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkNorfairRidleyColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkPhantoonBg2FramesVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkPhantoonColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkProjectileSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkShitroidColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkSporeSpawnColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkTourianStatueColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkWorkRobotPaletteVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkZebetiteColorsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainCorpseArtworkVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainEscapeTextArtworkVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainSpecialSpriteArtworkVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.NinjaSpacePiratePaletteArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Harness.SuiteSchedulingVerification;
using static SuperMetroid.Verification.Bosses.Torizo.Artwork.TorizoSwapArtworkVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Coordinates retail tile and palette comparisons with installed enemy artwork and live edit checks.</summary>
internal static class EnemyTileArtworkVerification
{
    /// <summary>Reads the exact tile upload bytes named by an enemy definition for the retail oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for an enemy's native tile bytes")]
    private static byte[] ReadNativeEnemyTiles(ISnesAddressSpace bus, RoomEnemyDefinition definition, int byteCount) =>
        RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), definition.TileDataAddress, byteCount);

    /// <summary>Loads a definition's native palette into OBJ CGRAM slots starting at index 128.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for an enemy's native palette DMA")]
    private static SnesCgram LoadNativeEnemyPalette(ISnesAddressSpace bus, EnemyDefinitionId definition)
    {
        var cgram = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, bus,
            SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.PaletteAddress(bus, definition),
            EnemyPaletteSheet.ColorCount, destinationIndex: 8 * 16);
        return cgram;
    }

    /// <summary>
    /// The melt image the cartridge assembles for <paramref name="pass"/> from its retail graphics: each band's $0402-byte
    /// copy, $0200 bytes after the last, from its $0200-byte chunk of bank $A4, sized as the installed image.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Crocomire melting pass source bytes")]
    private static byte[] ReadNativeCrocomireMeltingPassImage(ISnesAddressSpace rom, CrocomireMeltingIllustration pass)
    {
        bool first = pass == CrocomireMeltingIllustration.First;
        var expected = new byte[first ? CrocomireMeltingArtworkFormat.FirstByteCount : CrocomireMeltingArtworkFormat.SecondByteCount];
        int firstChunk = first ? 0xa4a07d : 0xa4ac7d;
        for (int band = 0; band < (first ? 6 : 7); band++)
        {
            for (int index = 0; index < 0x402; index++)
                expected[band * 0x200 + index] = rom.ReadCartridgeByte(firstChunk + band * 0x200 + index);
        }
        return expected;
    }

    /// <summary>
    /// Stock enemy artwork freshly extracted from the retail cartridge into a private directory,
    /// which each enemy artwork suite may edit, so the suites run independently.
    /// </summary>
    private sealed class EnemyTileFixture : IDisposable
    {
        /// <summary>Retail cartridge oracle for native comparisons.</summary>
        private readonly CartridgeImportAddressSpace bus =
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        /// <summary>Private extraction directory deleted on disposal.</summary>
        private readonly string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "enemy-tiles-" + Guid.NewGuid().ToString("N"));
        /// <summary>Stock catalog loaded from the extraction.</summary>
        private readonly InstalledEnemyArtwork stock;

        /// <summary>Extracts and loads the stock enemy artwork.</summary>
        internal EnemyTileFixture()
        {
            EnemyTileArtworkFiles.Extract(bus, directory, SupportedCartridge.Sha256);
            stock = EnemyTileArtworkFiles.Load(directory, null);
        }

        /// <summary>Supplies the cartridge, extraction directory and stock catalog.</summary>
        internal void Deconstruct(out CartridgeImportAddressSpace bus, out string directory, out InstalledEnemyArtwork stock)
        {
            bus = this.bus;
            directory = this.directory;
            stock = this.stock;
        }

        /// <summary>Deletes the private extraction.</summary>
        public void Dispose()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Checks installed Torizo, Ceres escape, Mother Brain, corpse, Ninja Pirate, Zebetite and Norfair Ridley artwork against retail.</summary>
    private static void VerifyBossEnemyArtwork()
    {
        using var fixture = new EnemyTileFixture();
        var (bus, directory, stock) = fixture;
        Suite(nameof(VerifyTorizoSwapArtwork), () => VerifyTorizoSwapArtwork(bus, directory, stock));
        Suite(nameof(VerifyCeresEscapeTileArtwork), () => VerifyCeresEscapeTileArtwork(bus, directory, stock));
        Suite(nameof(VerifyCeresEscapeOverlayTilemaps), () => VerifyCeresEscapeOverlayTilemaps(bus, directory, stock));
        Suite(nameof(VerifyInstalledMotherBrainCorpseArtwork), () => VerifyInstalledMotherBrainCorpseArtwork(directory, stock));
        Suite(nameof(VerifyInstalledMotherBrainEscapeTextArtwork), () => VerifyInstalledMotherBrainEscapeTextArtwork(directory, stock));
        Suite(nameof(VerifyInstalledMotherBrainSpecialSpriteArtwork), () => VerifyInstalledMotherBrainSpecialSpriteArtwork(directory, stock));
        Suite(nameof(VerifyInstalledDeadTorizoArtwork), () => VerifyInstalledDeadTorizoArtwork(directory, stock));
        Suite(nameof(VerifyInstalledDeadTourianCorpseArtwork), () => VerifyInstalledDeadTourianCorpseArtwork(directory, stock));
        Suite(nameof(VerifyInstalledNinjaSpacePiratePalette), () => VerifyInstalledNinjaSpacePiratePalette(bus, directory, stock));
        Suite(nameof(VerifyInstalledZebetiteColors), () => VerifyInstalledZebetiteColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledNorfairRidleyColors), () => VerifyInstalledNorfairRidleyColors(bus, directory, stock));
    }

    /// <summary>Checks installed enemy and boss color, palette-cycle and Crocomire skeleton artwork against retail.</summary>
    private static void VerifyEnemyColorArtwork()
    {
        using var fixture = new EnemyTileFixture();
        var (bus, directory, stock) = fixture;
        Suite(nameof(VerifyInstalledTourianStatueColors), () => VerifyInstalledTourianStatueColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledMagdollitePaletteCycle), () => VerifyInstalledMagdollitePaletteCycle(bus, directory, stock.Enemies));
        Suite(nameof(VerifyInstalledWorkRobotPaletteCycle), () => VerifyInstalledWorkRobotPaletteCycle(bus, directory, stock.Enemies));
        Suite(nameof(VerifyInstalledCrocomireColors), () => VerifyInstalledCrocomireColors(bus, directory, stock));
        Suite(nameof(VerifyCrocomireBg2ScrollDefinitions), () => VerifyCrocomireBg2ScrollDefinitions(bus));
        Suite(nameof(VerifyCrocomireSkeletonArtwork), () => VerifyCrocomireSkeletonArtwork(bus, directory, stock));
        Suite(nameof(VerifyInstalledDraygonColors), () => VerifyInstalledDraygonColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledPhantoonColors), () => VerifyInstalledPhantoonColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledChozoAndTubeColors), () => VerifyInstalledChozoAndTubeColors(bus, directory, stock.Enemies));
        Suite(nameof(VerifyInstalledSporeSpawnColors), () => VerifyInstalledSporeSpawnColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledDachoraColors), () => VerifyInstalledDachoraColors(bus, directory, stock.Enemies));
        Suite(nameof(VerifyInstalledShitroidColors), () => VerifyInstalledShitroidColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledBabyMetroidCutsceneColors), () => VerifyInstalledBabyMetroidCutsceneColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledBotwoonColors), () => VerifyInstalledBotwoonColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledMotherBrainDeathColors), () => VerifyInstalledMotherBrainDeathColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledCeresDoorVisuals), () => VerifyInstalledCeresDoorVisuals(bus, directory, stock));
    }

    /// <summary>Checks installed enemy spritemaps, extended frames, Kraid, BG2 frame and gunship artwork against retail.</summary>
    private static void VerifyEnemyFrameArtwork()
    {
        using var fixture = new EnemyTileFixture();
        var (bus, directory, stock) = fixture;
        Suite(nameof(VerifyInstalledEnemyProjectileSpritemaps), () => VerifyInstalledEnemyProjectileSpritemaps(bus, directory, stock));
        Suite(nameof(VerifyInstalledKraidBackground), () => VerifyInstalledKraidBackground(bus, directory, stock));
        Suite(nameof(VerifyInstalledKraidColors), () => VerifyInstalledKraidColors(bus, directory, stock));
        Suite(nameof(VerifyInstalledEnemySpritemaps), () => VerifyInstalledEnemySpritemaps(bus, directory, stock));
        Suite(nameof(VerifyInstalledEnemyExtendedFrames), () => VerifyInstalledEnemyExtendedFrames(bus, directory, stock));
        Suite(nameof(VerifyInstalledCrocomireBodyVisuals), () => VerifyInstalledCrocomireBodyVisuals(bus, directory, stock));
        Suite(nameof(VerifyInstalledPhantoonBg2Frames), () => VerifyInstalledPhantoonBg2Frames(bus, directory, stock));
        Suite(nameof(VerifyInstalledDraygonBg2Frames), () => VerifyInstalledDraygonBg2Frames(bus, directory, stock));
        Suite(nameof(VerifyInstalledGunshipLiftoffArtwork), () => VerifyInstalledGunshipLiftoffArtwork(bus, directory, stock));
    }

    /// <summary>Extracts stock enemy art and runs tile, palette, BG, spritemap and live override checks for the supported retail revision.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyEnemyTileArtwork()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Enemy tile artwork: private ROM absent; retail extraction skipped.");
            return;
        }
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "enemy-tiles-" + Guid.NewGuid().ToString("N"));
        try
        {
            EnemyTileArtworkFiles.Extract(bus, directory, SupportedCartridge.Sha256);
            InstalledEnemyArtwork stock = EnemyTileArtworkFiles.Load(directory, null);
            string[] files = Directory.GetFiles(directory, "enemy-????-tiles.png");
            AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount, files.Length,
                "one indexed PNG per distinct retail enemy graphics definition");
            var stockMeltImages = new List<byte[]>();
            ushort[] stockFirstMeltTiles = VerifyInstalledCrocomireMeltingTilemap(
                bus, stock, CrocomireMeltingTilemapAddress.FirstTilemap,
                CrocomireBodyProgram.MeltingOneTopRow);
            ushort[] stockSecondMeltTiles = VerifyInstalledCrocomireMeltingTilemap(
                bus, stock, CrocomireMeltingTilemapAddress.SecondTilemap,
                CrocomireBodyProgram.MeltingTwoTopRow);
            foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
            {
                byte[] actual = VerifyInstalledCrocomireMeltingPass(stock, pass);
                byte[] expected = ReadNativeCrocomireMeltingPassImage(bus, pass);
                AssertTrue(actual.SequenceEqual(expected),
                    $"installed Crocomire {pass} melt image preserves the image the cartridge assembles");
                stockMeltImages.Add(actual);
            }
            foreach (string file in files)
            {
                EnemyDefinitionId pointer = EnemyDefinitionIds.FromHeaderPointer(
                    Convert.ToUInt16(Path.GetFileName(file).Substring(6, 4), 16));
                RoomEnemyDefinition definition = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.Load(bus, pointer);
                int byteCount = definition.TileDataSize & 0x7fff;
                byte[] native = ReadNativeEnemyTiles(bus, definition, byteCount);
                AssertTrue(stock.Enemies.TryResolve(definition.TileDataAddress, byteCount,
                        out ReadOnlyMemory<byte> queued) && queued.Span.SequenceEqual(native),
                    $"enemy ${(int)pointer:X4} queued VRAM DMA resolves its installed PNG");
                var vram = new SnesVram();
                stock.Enemies.LoadTo(pointer, byteCount, new VramRegion(vram, PpuMemoryLayout.Vram), 0);
                AssertTrue(vram.Bytes[..byteCount].SequenceEqual(native),
                    $"enemy ${(int)pointer:X4} PNG preserves every native tile byte");
                SnesCgram nativeCgram = LoadNativeEnemyPalette(bus, pointer);
                var installedCgram = new GameplayColours(new SpriteColourSets());
                stock.Enemies.LoadPaletteTo(pointer, installedCgram.SpriteSets.Set(0));
                AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram.AsPaletteColors())),
                    $"enemy ${(int)pointer:X4} RGB5 JSON preserves all native palette slots");
            }

            string editedFile = files[0];
            EnemyDefinitionId editedPointer = EnemyDefinitionIds.FromHeaderPointer(
                Convert.ToUInt16(Path.GetFileName(editedFile).Substring(6, 4), 16));
            RoomEnemyDefinition editedDefinition = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.Load(bus, editedPointer);
            int editedByteCount = editedDefinition.TileDataSize & 0x7fff;
            int tileCount = editedByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tileCount, RoomCharacterAtlasFormat.TileColumns);
            int rows = (tileCount + columns - 1) / columns;
            using var stockPng = new MemoryStream(File.ReadAllBytes(editedFile), writable: false);
            IndexedPngImage image = IndexedPng.Read(stockPng, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory, "overrides");
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, Path.GetFileName(editedFile));
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            string meltFile = Path.Combine(directory,
                CrocomireMeltingArtworkFormat.FirstFileName);
            using (var meltInput = new MemoryStream(File.ReadAllBytes(meltFile), writable: false))
            {
                IndexedPngImage meltImage = IndexedPng.Read(meltInput, new(256, 32));
                meltImage.Pixels[0] ^= 1;
                using var meltOutput = File.Create(Path.Combine(overrideDirectory,
                    CrocomireMeltingArtworkFormat.FirstFileName));
                IndexedPng.Write(meltOutput, new(meltImage.Size.X, meltImage.Size.Y),
                    meltImage.Pixels, meltImage.Colours);
            }
            InstalledEnemyArtwork editedMelt = EnemyTileArtworkFiles.Load(
                directory, overrideDirectory);
            byte[] changedMelt = VerifyInstalledCrocomireMeltingPass(editedMelt,
                CrocomireMeltingIllustration.First);
            AssertEqual((byte)(stockMeltImages[0][0] ^ 0x80), changedMelt[0],
                "Crocomire melt PNG edit changes the live first planar pixel");
            AssertTrue(changedMelt.AsSpan(1).SequenceEqual(stockMeltImages[0].AsSpan(1)),
                "Crocomire melt PNG edit leaves all other image bytes unchanged");
            AssertTrue(VerifyInstalledCrocomireMeltingPass(editedMelt,
                    CrocomireMeltingIllustration.Second)
                .SequenceEqual(stockMeltImages[1]),
                "first-melt PNG edit does not change the second pass");
            AssertTrue(VerifyInstalledCrocomireMeltingPass(EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                    CrocomireMeltingIllustration.First)
                .SequenceEqual(changedMelt),
                "Crocomire melt override survives catalog reload");
            string firstTilemapFile = Path.Combine(directory,
                CrocomireMeltingArtworkFormat.FirstTilemapFileName);
            CrocomireMeltingTilemapDocument tilemapDocument =
                RequiredJson.Deserialize<CrocomireMeltingTilemapDocument>(
                    File.ReadAllBytes(firstTilemapFile),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            InstalledEnemyArtwork editedMeltMap = EnemyTileArtworkFiles.Load(
                directory, overrideDirectory);
            ushort[] changedTiles = VerifyInstalledCrocomireMeltingTilemap(
                bus, editedMeltMap, CrocomireMeltingTilemapAddress.FirstTilemap,
                CrocomireBodyProgram.MeltingOneTopRow,
                compareRom: false);
            AssertEqual((ushort)(stockFirstMeltTiles[0] ^ 1), changedTiles[0],
                "Crocomire melt JSON edit changes its selected BG2 tile reference");
            AssertTrue(changedTiles.AsSpan(1).SequenceEqual(stockFirstMeltTiles.AsSpan(1)),
                "Crocomire melt JSON edit leaves other BG2 cells unchanged");
            AssertTrue(VerifyInstalledCrocomireMeltingTilemap(
                    bus, editedMeltMap,
                    CrocomireMeltingTilemapAddress.SecondTilemap,
                    CrocomireBodyProgram.MeltingTwoTopRow,
                    compareRom: false)
                .SequenceEqual(stockSecondMeltTiles),
                "first-melt tilemap edit does not change the second melt");
            AssertTrue(VerifyInstalledCrocomireMeltingTilemap(bus,
                    EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                    CrocomireMeltingTilemapAddress.FirstTilemap,
                    CrocomireBodyProgram.MeltingOneTopRow,
                    compareRom: false)
                .SequenceEqual(changedTiles),
                "Crocomire melt tilemap override survives catalog reload");
            var stockVram = new SnesVram();
            var editedVram = new SnesVram();
            stock.Enemies.LoadTo(editedPointer, editedByteCount, new VramRegion(stockVram, PpuMemoryLayout.Vram), 0);
            edited.Enemies.LoadTo(editedPointer, editedByteCount, new VramRegion(editedVram, PpuMemoryLayout.Vram), 0);
            AssertEqual((byte)(stockVram.ReadByte(0) ^ 0x80), editedVram.ReadByte(0),
                "edited enemy PNG changes its first indexed pixel at the native tile location");
            AssertTrue(stockVram.Bytes[1..].SequenceEqual(editedVram.Bytes[1..]),
                "enemy PNG edit leaves all neighboring VRAM bytes unchanged");
            var reloadedVram = new SnesVram();
            EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .Enemies.LoadTo(editedPointer, editedByteCount, new VramRegion(reloadedVram, PpuMemoryLayout.Vram), 0);
            AssertTrue(reloadedVram.ImageEquals(editedVram),
                "enemy override survives content reload without modifying stock PNG");

            string stockPalettePath = Path.Combine(directory,
                EnemyTileArtworkFormat.PaletteFileName(editedPointer));
            EnemyPaletteSheetDocument paletteDocument = RequiredJson.Deserialize<EnemyPaletteSheetDocument>(
                File.ReadAllBytes(stockPalettePath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            PaletteRgb5[] editedColors = (PaletteRgb5[])paletteDocument.Colors.Clone();
            editedColors[0] = editedColors[0] with { Red = editedColors[0].Red ^ 1 };
            string paletteOverridePath = Path.Combine(overrideDirectory,
                EnemyTileArtworkFormat.PaletteFileName(editedPointer));
            File.WriteAllBytes(paletteOverridePath, EnemyPaletteSheetFile.Write(
                new EnemyPaletteSheetDocument { Version = 1, Colors = editedColors }));
            InstalledEnemyArtwork editedPalette = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            var originalColors = new GameplayColours(new SpriteColourSets());
            var changedColors = new GameplayColours(new SpriteColourSets());
            stock.Enemies.LoadPaletteTo(editedPointer, originalColors.SpriteSets.Set(0));
            editedPalette.Enemies.LoadPaletteTo(editedPointer, changedColors.SpriteSets.Set(0));
            AssertEqual((ushort)(originalColors.Colors[128].ToWord() ^ 1), changedColors.Colors[128],
                "RGB5 override changes its selected enemy palette channel");
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color != 128)
                    AssertEqual(originalColors.Colors[color], changedColors.Colors[color],
                        "enemy palette edit leaves all other CGRAM slots unchanged");
            var reloadedColors = new GameplayColours(new SpriteColourSets());
            EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .Enemies.LoadPaletteTo(editedPointer, reloadedColors.SpriteSets.Set(0));
            AssertTrue(new CgramPalettes(reloadedColors.AsPaletteColors()).ImageEquals(new CgramPalettes(changedColors.AsPaletteColors())),
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
            AssertThrows<InvalidDataException>(() => EnemyPaletteSheetFile.Write(
                    new EnemyPaletteSheetDocument { Version = 1, Colors = editedColors }),
                "RGB5 palette channel outside native five-bit precision is rejected");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
            Console.WriteLine("  Enemy artwork: 122 retail tile/color sheets, Crocomire melts, Kraid body/head maps, backdrop PNG, palette effects and four HUD-derived death restores, plus Boyon/Cacatac/Boulder/Atomic/Skultera/Waver/Skree/Metaree/Zoa/Pipe Bug/Fake Kraid/Kraid nail/Owtch/Stoke/Ripper/Fireflea/Magdollite/Ceres door/Ceres Baby/Rio/Lower Norfair Rio/Norfair Rio/Puyo/Bull/Alcoon/Beetom/Sidehopper/Dessgeega/Choot/HZoomer/Sbug/Fune/Namihe/Kamer platform/Elevator/Zeela/Sova/Zoomer/Stone Zoomer visual frames pass stock parity, live edits, persistence, and invalid-resource checks.");
            Console.WriteLine("  Sciser, shared Mellow/Mella/Memu, Kago, face-block, Morph Ball eye, shutter, ordinary Metroid, Shaktool, Tripper/Kamer, Dragon, Multiviola, Norfair lava-jumper, Chozo statue, Viola, and Rinka OAM frames: cartridge parity, installed draws, and legacy composition overrides pass.");
    }

    /// <summary>
    /// The installed enemy artwork family. Each suite extracts its own stock artwork, so the full
    /// run schedules them independently.
    /// </summary>
    internal static RegisteredSuite[] EnemyTileArtworkSuites() =>
    [
        new(nameof(VerifyEnemyTileArtwork), () => VerifyEnemyTileArtwork()),
        new(nameof(VerifyBossEnemyArtwork), () => VerifyBossEnemyArtwork()),
        new(nameof(VerifyEnemyColorArtwork), () => VerifyEnemyColorArtwork()),
        new(nameof(VerifyEnemyFrameArtwork), () => VerifyEnemyFrameArtwork()),
    ];

    /// <summary>Runs only the installed Zebetite pulse-colour comparison against retail.</summary>
    [VerificationCommand("--zebetite-colors")]
    internal static int ZebetiteColorsCommand()
    {
        using var fixture = new EnemyTileFixture();
        var (bus, directory, stock) = fixture;
        VerifyInstalledZebetiteColors(bus, directory, stock);
        return 0;
    }

    /// <summary>Runs only the installed Phantoon colour comparison against retail.</summary>
    [VerificationCommand("--phantoon-colors")]
    internal static int PhantoonColorsCommand()
    {
        using var fixture = new EnemyTileFixture();
        var (bus, directory, stock) = fixture;
        VerifyInstalledPhantoonColors(bus, directory, stock);
        return 0;
    }

    /// <summary>Runs only the installed Dachora palette-colour comparison against retail.</summary>
    [VerificationCommand("--dachora-colors")]
    internal static int DachoraColorsCommand()
    {
        using var fixture = new EnemyTileFixture();
        var (bus, directory, stock) = fixture;
        VerifyInstalledDachoraColors(bus, directory, stock.Enemies);
        return 0;
    }

    /// <summary>Runs artwork comparisons alongside mapped source routing and room-enemy loading checks.</summary>
    [VerificationCommand("--enemy-tile-artwork")]
    internal static int EnemyTileArtworkCommand()
    {
        RunSuitesSerially(EnemyTileArtworkSuites());
        Suite(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting());
        Suite(nameof(VerifyRoomEnemyLoading), () => VerifyRoomEnemyLoading());
        return 0;
    }
}
