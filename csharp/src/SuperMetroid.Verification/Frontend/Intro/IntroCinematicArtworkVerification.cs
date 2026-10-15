using static SuperMetroid.Verification.Frontend.Intro.IntroArtworkPostSlicesVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Harness.SuiteSchedulingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Frontend.Intro.CinematicSpriteAnimationsVerification;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using System.Buffers.Binary;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoCartridgeFallbackVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroDiscoveryActorSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroEggEffectSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroEyeInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainCollisionSourceVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainInputDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroRinkaSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroScientistSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Samus.Poses.SamusAnimationDelayDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Artwork.SamusBodyArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed opening and Ceres cinematic artwork, resource validation, and ROM-read guards.</summary>
internal static class IntroCinematicArtworkVerification
{
    /// <summary>Exercises installed opening-scene art and tilemaps through the real VRAM loader.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro cinematic characters, tilemaps and palette")]
    internal static void VerifyIntroCinematicArtwork(string sourceRom)
    {
        // Edits, corruption and repairs run on a private copy of the repository installation;
        // ROM import itself is covered by the asset-import verification.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopy();
        string root = copy.Root;
        {
            GameInstallation installation = copy.Installation;
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(bus, sourceRom));
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            AssertTrue(stock.BackgroundCharacters.Transfer.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.BackgroundCharacters,
                        maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundByteCount)),
                "installed intro BG PNG preserves every native tile byte");
            AssertTrue(stock.IntroObjectCharacters.Transfer.Span.SequenceEqual(
                    RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), IntroCinematicRomData.Assets.IntroObjectCharacters,
                        IntroCinematicArtworkFormat.IntroObjectByteCount)),
                "installed fixed intro OBJ PNG preserves every native tile byte");
            AssertTrue(stock.CinematicObjectCharacters.Transfer.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.ObjectCharacters,
                        maximumOutputBytes: IntroCinematicArtworkFormat.CinematicObjectByteCount)),
                "installed compressed cinematic OBJ PNG preserves every native tile byte");
            AssertTrue(stock.BackgroundPages.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.BackgroundPageTilemaps,
                        maximumOutputBytes: IntroCinematicRomData.Vram.BackgroundPageTilemapBytes)),
                "four installed BG page JSON files preserve every native tilemap word");
            AssertTrue(stock.PortraitTilemap.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.SamusHeadTilemap,
                        maximumOutputBytes: IntroCinematicRomData.Vram.SamusHeadTilemapBytes)),
                "installed portrait JSON preserves every native tilemap word");
            AssertTrue(stock.InitialNarrationTilemap.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.FirstNarrationTilemap,
                        maximumOutputBytes: IntroCinematicRomData.Vram.NarrationTilemapBytes)),
                "installed initial narration JSON preserves every native tilemap word");
            for (int index = 0; index < IntroFinalLineTilemapFormat.CellCount; index++)
            {
                int source = IntroCinematicRomData.Assets.FinalTextLine + index * sizeof(ushort);
                ushort nativeWord = (ushort)(SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, source), CpuBusRegionRead.Read(bus, source + 1)));
                AssertEqual(nativeWord, stock.FinalLine.Words.Span[index],
                    $"opening divider tile {index} matches cartridge source");
            }
            Suite(nameof(VerifyIntroEyeInstructions), () => VerifyIntroEyeInstructions(bus));
            for (int frame = 0; frame < IntroEyeTilemapFormat.FrameCount; frame++)
            {
                int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    (ushort)(IntroEyeAnimationDefinitions.FrameStartPointer +
                        frame * IntroEyeAnimationDefinitions.FrameStride));
                for (int cell = 0; cell < IntroEyeTilemapFormat.CellsPerFrame; cell++)
                {
                    int wordSource = source + 4 + cell * sizeof(ushort);
                    ushort nativeWord = (ushort)(SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, wordSource), CpuBusRegionRead.Read(bus, wordSource + 1)));
                    AssertEqual(nativeWord, stock.EyeFrames.FrameWords(frame)[cell],
                        $"opening eye frame {frame} cell {cell} matches cartridge");
                }
            }
            Suite(nameof(VerifyIntroEyeArtwork), () => VerifyIntroEyeArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroCaretSpriteArtwork), () => VerifyIntroCaretSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroMotherBrainCollision), () => VerifyIntroMotherBrainCollision(bus, stock));
            Suite(nameof(VerifyIntroMotherBrainSpriteArtwork), () => VerifyIntroMotherBrainSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroMotherBrainExplosionSpriteArtwork), () => VerifyIntroMotherBrainExplosionSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroRinkaSpriteArtwork), () => VerifyIntroRinkaSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroEggEffectSpriteArtwork), () => VerifyIntroEggEffectSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroDiscoveryActorSpriteArtwork), () => VerifyIntroDiscoveryActorSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroScientistSpriteArtwork), () => VerifyIntroScientistSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroMotherBrainDemoInput), () => VerifyIntroMotherBrainDemoInput(bus, stock));
            AssertTrue(stock.Palette.Transfer.Span.SequenceEqual(
                    RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), IntroCinematicRomData.Assets.Palette,
                        CgramPalettes.ByteCount)),
                "installed opening palette preserves every native RGB5 word");

            var native = CreateRetailIntroFixture(bus);
            var completeInstalled = CreateRetailIntroFixture(
                NewFrontendCartridgeReadGuard(bus),
                introFont: installation.LoadMaps().IntroFont,
                characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles);
            var completeNativeMemory = native.CaptureTranslatedRenderSnapshot().Memory;
            var completeInstalledMemory = completeInstalled.CaptureTranslatedRenderSnapshot().Memory;
            AssertTrue(completeInstalledMemory.Vram.SequenceEqual(completeNativeMemory.Vram) &&
                completeInstalledMemory.Cgram.SequenceEqual(completeNativeMemory.Cgram),
                "fully installed opening upload preserves every native VRAM and CGRAM word without a cartridge read");
            var guarded = NewIntroArtworkSourceReadGuard(bus);
            var installed = CreateRetailIntroFixture(guarded, characterArtwork: stock);
            byte[] nativeVram = native.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
            AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                "three installed PNGs create exact native opening-cinematic VRAM, including overlapping OBJ uploads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed cinematic never reads cartridge character or BG-page sources");
            for (int frame = 0; frame < 360; frame++)
            {
                native.Step(0);
                installed.Step(0);
                AssertEqual(native.Phase, installed.Phase,
                    $"installed cinematic preserves native phase at frame {frame}");
                if (frame % 60 == 0)
                {
                    var expectedScene = native.CaptureTranslatedRenderSnapshot();
                    var actualScene = installed.CaptureTranslatedRenderSnapshot();
                    AssertTrue(actualScene.Brightness == expectedScene.Brightness &&
                            actualScene.Memory.Vram.SequenceEqual(expectedScene.Memory.Vram) &&
                            actualScene.Memory.Cgram.SequenceEqual(expectedScene.Memory.Cgram),
                        $"installed cinematic preserves native graphics and colors at frame {frame}");
                }
            }
            Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
            string paletteName = IntroCinematicPaletteFormat.FileName;
            string paletteStockPath = Path.Combine(installation.IntroCinematicDirectory, paletteName);
            string paletteOverridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, paletteName);
            IntroCinematicPaletteDocument paletteDocument =
                JsonSerializer.Deserialize<IntroCinematicPaletteDocument>(
                    File.ReadAllBytes(paletteStockPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException("Extracted opening palette is empty.");
            PaletteRgb5 originalColor = paletteDocument.Colors[1];
            paletteDocument.Colors[1] = originalColor with
            {
                Red = originalColor.Red == 31 ? 30 : originalColor.Red + 1,
            };
            using (var output = File.Create(paletteOverridePath))
                IntroCinematicPalette.Write(output, paletteDocument);
            IntroCinematicArtworkCatalog editedPalette = installation.LoadIntroCinematicArt();
            var editedPaletteState = CreateRetailIntroFixture(guarded, characterArtwork: editedPalette);
            var stockPaletteState = CreateRetailIntroFixture(guarded, characterArtwork: stock);
            Bgr555[] originalCgram = stockPaletteState.CaptureTranslatedRenderSnapshot().Memory.Cgram.ToArray();
            Bgr555[] editedCgram = editedPaletteState.CaptureTranslatedRenderSnapshot().Memory.Cgram.ToArray();
            AssertTrue(!editedCgram.SequenceEqual(originalCgram) &&
                editedCgram.AsSpan(0, 1).SequenceEqual(originalCgram.AsSpan(0, 1)) &&
                editedCgram.AsSpan(2).SequenceEqual(originalCgram.AsSpan(2)),
                "edited opening RGB5 color reaches only its production CGRAM destination");
            stockPaletteState.BindCharacterArtwork(editedPalette);
            AssertTrue(stockPaletteState.CaptureTranslatedRenderSnapshot().Memory.Cgram.SequenceEqual(editedCgram),
                "restored initial narration rebinds the current palette without restarting its phase");
            for (int frame = 0; frame < 120; frame++)
            {
                editedPaletteState.Step(0);
                stockPaletteState.Step(0);
                AssertEqual(editedPaletteState.Phase, stockPaletteState.Phase,
                    $"opening palette edit preserves cinematic phase at frame {frame}");
            }
            // The initial single-color edit proves exact CGRAM targeting; it can be
            // hidden behind the first card's chosen palette indices. A broad color
            // edit proves the selected resource also reaches visible scene pixels.
            using (var output = File.Create(paletteOverridePath))
                IntroCinematicPalette.Write(output, paletteDocument with
                {
                    Colors = paletteDocument.Colors.Select(color => color with
                    {
                        Red = (color.Red + 7) & 31,
                        Green = (color.Green + 11) & 31,
                    }).ToArray(),
                });
            IntroCinematicArtworkCatalog visiblePalette = installation.LoadIntroCinematicArt();
            var stockVisualState = CreateRetailIntroFixture(guarded, characterArtwork: stock);
            var editedVisualState = CreateRetailIntroFixture(guarded, characterArtwork: visiblePalette);
            bool changedVisiblePixel = false;
            for (int frame = 0; frame < 400; frame++)
            {
                stockVisualState.Step(0);
                editedVisualState.Step(0);
                AssertEqual(stockVisualState.Phase, editedVisualState.Phase,
                    $"visible opening palette edit preserves cinematic phase at frame {frame}");
                if (frame % 20 == 0 && !SoftwareLayeredSnapshotRenderer.Render(
                        stockVisualState.CaptureTranslatedRenderSnapshot()).SequenceEqual(
                        SoftwareLayeredSnapshotRenderer.Render(
                            editedVisualState.CaptureTranslatedRenderSnapshot())))
                    changedVisiblePixel = true;
            }
            AssertTrue(changedVisiblePixel,
                "edited opening RGB5 colors change displayed narration pixels");
            AssertThrows<InvalidDataException>(() => IntroCinematicPalette.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(paletteDocument with
                {
                    Colors = paletteDocument.Colors.Take(255).ToArray(),
                }))), "opening palette rejects a truncated color table");
            AssertThrows<InvalidDataException>(() => IntroCinematicPalette.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(paletteDocument with
                {
                    Colors = paletteDocument.Colors.Select((color, index) =>
                        index == 1 ? color with { Red = 32 } : color).ToArray(),
                }))), "opening palette rejects out-of-range RGB5 channels");
            File.Delete(paletteOverridePath);
            AssertTrue(installation.LoadIntroCinematicArt().Palette.Transfer.Span.SequenceEqual(
                    stock.Palette.Transfer.Span),
                "removing opening palette override restores exact native colors");
            File.WriteAllBytes(paletteOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "malformed opening palette override fails instead of silently restoring stock");
            File.Delete(paletteOverridePath);
            File.WriteAllBytes(paletteStockPath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "opening palette stock corruption fails manifest validation");
            _ = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Opening palette stock repair lost the installation.");
            AssertTrue(installation.LoadIntroCinematicArt().Palette.Transfer.Span.SequenceEqual(
                    stock.Palette.Transfer.Span),
                "stock repair restores exact cartridge opening palette colors");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed opening palette never rereads its cartridge source");
            string dividerStockPath = Path.Combine(installation.IntroCinematicDirectory,
                IntroFinalLineTilemapFormat.FileName);
            string dividerOverridePath = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroFinalLineTilemapFormat.FileName);
            IntroFinalLineTilemapDocument dividerDocument =
                JsonSerializer.Deserialize<IntroFinalLineTilemapDocument>(
                    File.ReadAllBytes(dividerStockPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException("Extracted opening divider is empty.");
            const BindingFlags dividerSetupFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var nativeDividerState = CreateRetailIntroFixture(bus);
            var stockDividerState = CreateRetailIntroFixture(guarded, characterArtwork: stock);
            foreach (IntroCinematicState scene in new[] { nativeDividerState, stockDividerState })
                scene.SetupFirstIllustratedPage();
            byte[] stockDividerVram = stockDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
            AssertTrue(stockDividerVram.SequenceEqual(
                    nativeDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram),
                "installed opening divider produces exact native illustrated-page VRAM");
            int changedCell = Array.FindIndex(dividerDocument.Cells,
                cell => cell.TileColumn < RoomBackgroundTilemapFormat.TileColumns - 1);
            AssertTrue(changedCell >= 0, "opening divider has an editable tile column");
            RoomBackgroundTilemapCell originalDividerCell = dividerDocument.Cells[changedCell];
            dividerDocument.Cells[changedCell] = originalDividerCell with
            {
                TileColumn = originalDividerCell.TileColumn + 1,
            };
            using (var output = File.Create(dividerOverridePath))
                IntroFinalLineTilemap.Write(output, dividerDocument);
            IntroCinematicArtworkCatalog editedDivider = installation.LoadIntroCinematicArt();
            var editedDividerState = CreateRetailIntroFixture(guarded, characterArtwork: editedDivider);
            editedDividerState.SetupFirstIllustratedPage();
            byte[] expectedDividerVram = stockDividerVram.ToArray();
            int dividerByte = (IntroCinematicRomData.Layers.NarrationTilemapWord +
                IntroCinematicRomData.Text.FinalLineDestinationStart + changedCell) * sizeof(ushort);
            BinaryPrimitives.WriteUInt16LittleEndian(expectedDividerVram.AsSpan(dividerByte),
                editedDivider.FinalLine.Words.Span[changedCell]);
            AssertTrue(!expectedDividerVram.SequenceEqual(stockDividerVram) &&
                    editedDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                        expectedDividerVram),
                "one edited divider cell changes only its production BG3 tile word");
            stockDividerState.BindCharacterArtwork(editedDivider);
            AssertTrue(stockDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                    expectedDividerVram),
                "restored illustrated page rebinds divider without erasing live text elsewhere");
            using (var output = File.Create(dividerOverridePath))
                IntroFinalLineTilemap.Write(output, dividerDocument with
                {
                    Cells = dividerDocument.Cells.Select(cell => cell with
                    {
                        Palette = (cell.Palette + 3) % RoomBackgroundTilemapFormat.PaletteCount,
                    }).ToArray(),
                });
            IntroCinematicArtworkCatalog visibleDivider = installation.LoadIntroCinematicArt();
            var stockDividerVisual = CreateRetailIntroFixture(guarded, characterArtwork: stock);
            var editedDividerVisual = CreateRetailIntroFixture(guarded, characterArtwork: visibleDivider);
            foreach (IntroCinematicState scene in new[] { stockDividerVisual, editedDividerVisual })
            {
                scene.SetupFirstIllustratedPage();
                scene.inidisp = ScreenFade.FullyLit;
            }
            AssertEqual(stockDividerVisual.Phase, editedDividerVisual.Phase,
                "visual divider override leaves page phase unchanged");
            AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(
                    stockDividerVisual.CaptureTranslatedRenderSnapshot()).SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(
                        editedDividerVisual.CaptureTranslatedRenderSnapshot())),
                "divider palette edits change displayed illustrated-page pixels");
            File.Delete(dividerOverridePath);
            stockDividerState.BindCharacterArtwork(stock);
            AssertTrue(stockDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                    stockDividerVram),
                "removing opening divider override restores exact native BG3 words");
            AssertThrows<InvalidDataException>(() => IntroFinalLineTilemap.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(dividerDocument with
                {
                    Cells = dividerDocument.Cells.Take(127).ToArray(),
                }))), "opening divider rejects a truncated tilemap");
            File.WriteAllBytes(dividerOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "malformed opening divider override fails loudly");
            File.Delete(dividerOverridePath);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed opening divider never rereads its cartridge source");

            string[] names =
            [
                IntroCinematicArtworkFormat.BackgroundFileName,
                IntroCinematicArtworkFormat.IntroObjectFileName,
                IntroCinematicArtworkFormat.CinematicObjectFileName,
            ];
            Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
            foreach (string name in names)
            {
                string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
                string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
                int nativeByteCount = name == names[0]
                    ? IntroCinematicArtworkFormat.BackgroundByteCount
                    : name == names[1]
                        ? IntroCinematicArtworkFormat.IntroObjectByteCount
                        : IntroCinematicArtworkFormat.CinematicObjectByteCount;
                int tileRows = nativeByteCount / 32 / IntroCinematicArtworkFormat.TileColumns;
                IndexedPngImage image;
                using (var input = File.OpenRead(stockPath))
                    image = IndexedPng.Read(input, new(256, checked((ushort)(tileRows * 8))));
                image.Pixels[0] = (byte)((image.Pixels[0] + 1) & 15);
                using (var output = File.Create(overridePath))
                    IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);

                IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
                var editedState = CreateRetailIntroFixture(guarded, characterArtwork: edited);
                byte[] expected = nativeVram.ToArray();
                // $8B:A395 copies the fixed OBJ sheet before the compressed sheet;
                // the final $400 bytes of the first transfer are deliberately overwritten.
                edited.BackgroundCharacters.Transfer.Span.CopyTo(expected.AsSpan(
                    (IntroVramLayout.BackgroundCharacters.Offset * 2)));
                edited.BackgroundPages.Span.CopyTo(expected.AsSpan(
                    (IntroVramLayout.BackgroundPages.Offset * 2)));
                edited.IntroObjectCharacters.Transfer.Span.CopyTo(expected.AsSpan(
                    (IntroVramLayout.IntroObjectCharacters.Offset * 2)));
                edited.CinematicObjectCharacters.Transfer.Span.CopyTo(expected.AsSpan(
                    (IntroVramLayout.CinematicObjectCharacters.Offset * 2)));
                // The final beam DMA wins over the $C600-$C6FF portion of the fixed
                // OBJ sheet, including in a freshly constructed cinematic state.
                nativeVram.AsSpan(BeamTileAtlasDefinitions.DestinationWord * 2,
                    BeamTileAtlasDefinitions.ByteCount).CopyTo(expected.AsSpan(
                        BeamTileAtlasDefinitions.DestinationWord * 2));
                AssertTrue(!expected.SequenceEqual(nativeVram) &&
                        editedState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"edited {name} reaches the exact production VRAM transfer without changing other scene bytes");
                installed.BindCharacterArtwork(edited);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"restored cinematic state rebinds edited {name} with native transfer ordering");
                File.Delete(overridePath);
                installed.BindCharacterArtwork(stock);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                    $"removing {name} override restores stock cinematic VRAM");
            }
            for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
            {
                string name = IntroCinematicArtworkFormat.BackgroundPageFileName(page);
                string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
                string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
                RoomBackgroundTilemapDocument document;
                using (var input = File.OpenRead(stockPath))
                    document = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(input,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidDataException($"Empty cinematic BG page {name}.");
                // Choose a nonblank source cell so each isolated page edit has a
                // deterministic physical tilemap-word difference.
                int editedCell = Enumerable.Range(RoomBackgroundTilemapFormat.TileColumns,
                        26 * RoomBackgroundTilemapFormat.TileColumns)
                    .First(index => document.Pages[0].Cells[index] is
                        { TileColumn: not 31 } or { TileRow: not 31 });
                RoomBackgroundTilemapCell original = document.Pages[0].Cells[editedCell];
                document.Pages[0].Cells[editedCell] = original with
                {
                    TileColumn = 31,
                    TileRow = 31,
                };
                if (page == 0)
                    // A full-page high-contrast fixture crosses the native actor/text
                    // occlusion, proving the authored tilemap is actually rendered.
                    for (int cell = 0; cell < document.Pages[0].Cells.Length; cell++)
                        document.Pages[0].Cells[cell] = document.Pages[0].Cells[cell] with
                        { TileColumn = 0, TileRow = 0, Palette = 7, Priority = true };
                using (var output = File.Create(overridePath))
                    RoomBackgroundTilemapAtlas.Write(output, document,
                        IntroCinematicArtworkFormat.BackgroundPageByteCount);

                IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
                byte[] expected = nativeVram.ToArray();
                edited.BackgroundPages.Span.CopyTo(expected.AsSpan(
                    (IntroVramLayout.BackgroundPages.Offset * 2)));
                AssertTrue(!expected.SequenceEqual(nativeVram) &&
                        CreateRetailIntroFixture(guarded, characterArtwork: edited)
                            .CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"edited {name} reaches exactly its production BG page transfer");
                if (page == 0)
                {
                    const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                    var stockScene = CreateRetailIntroFixture(bus, characterArtwork: stock);
                    var editedScene = CreateRetailIntroFixture(bus, characterArtwork: edited);
                    foreach (IntroCinematicState scene in new[] { stockScene, editedScene })
                    {
                        scene.SetupFirstIllustratedPage();
                        scene.SetupMotherBrainFlashback();
                    }
                    // The setup begins with gameplay colors cleared; wait for the
                    // cartridge's 128-frame palette crossfade before checking pixels.
                    for (int fadeFrame = 0; fadeFrame < 128; fadeFrame++)
                    {
                        stockScene.Step(0);
                        editedScene.Step(0);
                    }
                    // The full game owns INIDISP during this synthetic setup; force
                    // the already-composed layer to visible brightness for this probe.
                    stockScene.inidisp = ScreenFade.FullyLit;
                    editedScene.inidisp = ScreenFade.FullyLit;
                    AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(stockScene.CaptureTranslatedRenderSnapshot())
                            .SequenceEqual(RenderForComparison(
                                editedScene.CaptureTranslatedRenderSnapshot())),
                        "edited opening BG page produces visible flashback pixels");
                }
                installed.BindCharacterArtwork(edited);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"restored cinematic rebinds edited {name} without changing another page");
                File.Delete(overridePath);
                installed.BindCharacterArtwork(stock);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                    $"removing {name} override restores stock BG VRAM");
            }
            foreach ((string name, int destination, Func<IntroCinematicArtworkCatalog, ReadOnlyMemory<byte>> transfer)
                in new (string, int, Func<IntroCinematicArtworkCatalog, ReadOnlyMemory<byte>>)[]
                {
                    (IntroCinematicArtworkFormat.PortraitTilemapFileName,
                        (IntroVramLayout.PortraitTilemap.Offset * 2),
                        artwork => artwork.PortraitTilemap),
                    (IntroCinematicArtworkFormat.InitialNarrationTilemapFileName,
                        (IntroVramLayout.NarrationTilemap.Offset * 2),
                        artwork => artwork.InitialNarrationTilemap),
                })
            {
                string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
                string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
                RoomBackgroundTilemapDocument document;
                using (var input = File.OpenRead(stockPath))
                    document = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(input,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidDataException($"Empty opening tilemap {name}.");
                RoomBackgroundTilemapCell original = document.Pages[0].Cells[0];
                document.Pages[0].Cells[0] = original with
                {
                    TileColumn = (original.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
                };
                using (var output = File.Create(overridePath))
                    RoomBackgroundTilemapAtlas.Write(output, document,
                        IntroCinematicArtworkFormat.BackgroundPageByteCount);

                IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
                byte[] expected = nativeVram.ToArray();
                transfer(edited).Span.CopyTo(expected.AsSpan(destination));
                AssertTrue(!expected.SequenceEqual(nativeVram) &&
                        CreateRetailIntroFixture(guarded, characterArtwork: edited)
                            .CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"edited {name} reaches only its initial cinematic VRAM page");
                var restoredEarly = CreateRetailIntroFixture(guarded, characterArtwork: stock);
                restoredEarly.BindCharacterArtwork(edited);
                AssertTrue(restoredEarly.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"restored first narration/portrait state receives edited {name}");

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var restoredLate = CreateRetailIntroFixture(guarded, characterArtwork: stock);
                restoredLate.SetupFirstIllustratedPage();
                byte[] lateBefore = restoredLate.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
                restoredLate.BindCharacterArtwork(edited);
                byte[] lateAfter = restoredLate.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
                if (name == IntroCinematicArtworkFormat.InitialNarrationTilemapFileName)
                    AssertTrue(lateAfter.SequenceEqual(lateBefore),
                        "later typewriter map survives rebinding an edited initial narration page");
                else
                {
                    transfer(edited).Span.CopyTo(lateBefore.AsSpan(destination));
                    AssertTrue(lateAfter.SequenceEqual(lateBefore),
                        "later illustrated page receives edited portrait map without changing typewriter text");
                }
                File.Delete(overridePath);
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "edited and rebound cinematic never reads cartridge character or tilemap sources");

            string backgroundStockPath = Path.Combine(installation.IntroCinematicDirectory, names[0]);
            string backgroundOverridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, names[0]);
            using (var input = File.OpenRead(backgroundStockPath))
            {
                IndexedPngImage image = IndexedPng.Read(input, new(256, 256));
                image.Pixels[0] = (byte)((image.Pixels[0] + 1) & 15);
                using var output = File.Create(backgroundOverridePath);
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            }
            IntroCinematicArtworkCatalog selected = installation.LoadIntroCinematicArt();
            File.WriteAllBytes(backgroundStockPath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "an override cannot hide corrupt stock cinematic artwork");
            GameInstallation repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro content disappeared during repair.");
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundCharacters.Transfer.Span.SequenceEqual(
                    selected.BackgroundCharacters.Transfer.Span),
                "stock repair preserves the external cinematic PNG override");
            File.Delete(backgroundStockPath);
            AssertThrows<FileNotFoundException>(() => repaired.LoadIntroCinematicArt(),
                "an override cannot hide missing stock cinematic artwork");
            repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro content disappeared during missing-file repair.");
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundCharacters.Transfer.Span.SequenceEqual(
                    selected.BackgroundCharacters.Transfer.Span),
                "missing-stock repair also preserves the external cinematic PNG override");
            File.Delete(backgroundOverridePath);
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundCharacters.Transfer.Span.SequenceEqual(
                    stock.BackgroundCharacters.Transfer.Span),
                "removing an override restores exact cartridge opening artwork");
            string pageStockPath = Path.Combine(installation.IntroCinematicDirectory,
                IntroCinematicArtworkFormat.BackgroundPageFileName(0));
            File.WriteAllBytes(pageStockPath, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "a corrupt stock BG page is rejected even without an override");
            repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro BG pages disappeared during repair.");
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundPages.Span.SequenceEqual(
                    stock.BackgroundPages.Span),
                "stock repair restores all four native cinematic BG pages");
            File.WriteAllBytes(backgroundOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro PNG fails instead of silently falling back");
            File.Delete(backgroundOverridePath);
            string invalidPage = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroCinematicArtworkFormat.BackgroundPageFileName(0));
            File.WriteAllBytes(invalidPage, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro BG page fails instead of silently falling back");
            File.Delete(invalidPage);
            string invalidEye = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroEyeTilemapFormat.FileName);
            File.WriteAllBytes(invalidEye, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected opening eye frames fail instead of silently falling back");
            File.Delete(invalidEye);
            string invalidCaret = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroCaretSpriteFormat.FileName);
            File.WriteAllBytes(invalidCaret, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected caret sprites fail instead of silently falling back");
            File.Delete(invalidCaret);
            string invalidMotherBrain = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroMotherBrainSpriteFormat.FileName);
            File.WriteAllBytes(invalidMotherBrain, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro Mother Brain sprites fail instead of silently falling back");
            Console.WriteLine(
                "Intro art: three indexed PNGs, seven full tilemaps, eye/caret/Mother Brain/explosion/Rinka/egg-effect/discovery/scientist compositions and full RGB5 palette; native parity, edits, rebind, repair and strict failures pass.");
        }
    }

    /// <summary>Compares installed eye tilemap frames and instruction timing with native VRAM output.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyIntroEyeArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var nativeVram = new SnesVram();
        var installedVram = new SnesVram();
        var native = new IntroCinematicObjectSystem(bus, nativeVram, new ushort[1024], new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration, eyeArtwork: stock.EyeFrames);
        var installed = new IntroCinematicObjectSystem(guarded, installedVram,
            new ushort[1024], new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration, eyeArtwork: stock.EyeFrames);
        native.Step(0);
        installed.Step(0);
        AssertTrue(installedVram.ImageEquals(nativeVram),
            "installed opening eye frame draws the exact native portrait rectangle");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed opening eye script and frame do not reread ROM");

        string eyePath = Path.Combine(installation.IntroCinematicDirectory,
            IntroEyeTilemapFormat.FileName);
        IntroEyeTilemapDocument document = RequiredJson.Deserialize<IntroEyeTilemapDocument>(
            File.ReadAllBytes(eyePath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var firstFrame = document.Frames[0];
        var firstCell = firstFrame.Cells[0];
        firstFrame.Cells[0] = firstCell with
        {
            TileColumn = (firstCell.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
        };
        using var editedJson = new MemoryStream();
        IntroEyeTilemapPresentation.Write(editedJson, document);
        editedJson.Position = 0;
        IntroEyeTilemapPresentation edited = IntroEyeTilemapPresentation.Load(editedJson);
        NativeCountdown timer = installed.eyeInstructionTimer;
        ushort pointer = installed.eyeInstructionPointer;
        installed.BindEyeArtwork(edited);
        AssertEqual(timer, installed.eyeInstructionTimer,
            "eye art rebind preserves blink timer");
        AssertEqual(pointer, installed.eyeInstructionPointer,
            "eye art rebind preserves blink script position");
        int packedPosition = SnesLittleEndian.Word(IntroEyeAnimationDefinitions.ReadByte(
            (ushort)(IntroEyeAnimationDefinitions.StartPointer + 2)),IntroEyeAnimationDefinitions.ReadByte(
                (ushort)(IntroEyeAnimationDefinitions.StartPointer + 3)));
        int destination = IntroCinematicRomData.Layers.PortraitTilemapWord +
            SnesLittleEndian.HighByte(unchecked((ushort)packedPosition)) * IntroCinematicRomData.Layers.TilemapWidth +
            (packedPosition & IntroCinematicRomData.ObjectSystem.PackedPositionXMask);
        AssertEqual(edited.FrameWords(0)[0], installedVram.ReadWord(destination),
            "eye JSON edit reaches the active portrait tile");
        installed.BindEyeArtwork(stock.EyeFrames);
        AssertTrue(installedVram.ImageEquals(nativeVram),
            "restoring stock eye art restores exact native VRAM without restarting script");

        for (int frame = 0; frame < 160; frame++)
        {
            native.Step(0);
            installed.Step(0);
            AssertTrue(installedVram.ImageEquals(nativeVram),
                $"installed normal eye blink preserves native VRAM at frame {frame}");
        }
        native.StartEnglishPageSix();
        installed.StartEnglishPageSix();
        for (int frame = 0; frame < 64; frame++)
        {
            native.Step(0);
            installed.Step(0);
            AssertTrue(installedVram.ImageEquals(nativeVram),
                $"installed page-six eye blink preserves native VRAM at frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed eye lists and all four frames avoid native source reads");
    }

    /// <summary>Compares caret sprite compositions and blink behavior with native OAM output.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyIntroCaretSpriteArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        Suite(nameof(VerifyCinematicSpriteAnimations), () => VerifyCinematicSpriteAnimations(bus));
        const ushort originX = 8;
        const ushort originY = 24;
        ushort paletteBits = IntroCinematicRomData.Objects.ScientistPalette.Raw;
        foreach (IntroCaretFrameDefinition definition in IntroCaretSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            DrawImportedSpritemap(bus, native,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer), new(originX, originY), paletteBits);
            native.FinalizeFrame();
            var installed = new OamBuffer();
            installed.BeginFrame();
            stock.CaretSprites.Draw(definition.Pointer, installed,
                new(originX, originY), paletteBits);
            installed.FinalizeFrame();
            AssertTrue(installed.ImageEquals(native) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"opening caret {definition.Name} produces exact native OAM");
        }

        string caretPath = Path.Combine(installation.IntroCinematicDirectory,
            IntroCaretSpriteFormat.FileName);
        IntroCaretSpriteDocument document = RequiredJson.Deserialize<IntroCaretSpriteDocument>(
            File.ReadAllBytes(caretPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        AssertEqual(1, document.Frames.Count,
            "caret asset excludes neighboring non-caret spritemaps");
        var previousFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [IntroCaretSpriteDefinitions.PreviousFrameNames[0]] = document.Frames["caret-visible"],
            [IntroCaretSpriteDefinitions.PreviousFrameNames[1]] = [],
            [IntroCaretSpriteDefinitions.PreviousFrameNames[2]] = [],
            [IntroCaretSpriteDefinitions.PreviousFrameNames[3]] = [],
        };
        using (var previousJson = new MemoryStream())
        {
            IntroCaretSpritePresentation.Write(previousJson, new IntroCaretSpriteDocument
            {
                Version = (int)IntroCaretSpriteVersion.Previous,
                Frames = previousFrames,
            });
            previousJson.Position = 0;
            IntroCaretSpritePresentation previous = IntroCaretSpritePresentation.Load(previousJson);
            var previousOam = new OamBuffer();
            previousOam.BeginFrame();
            previous.Draw(IntroCaretSpriteDefinitions.Still, previousOam,
                new(originX, originY), paletteBits);
            previousOam.FinalizeFrame();
            var stockOam = new OamBuffer();
            stockOam.BeginFrame();
            stock.CaretSprites.Draw(IntroCaretSpriteDefinitions.Still, stockOam,
                new(originX, originY), paletteBits);
            stockOam.FinalizeFrame();
            AssertTrue(previousOam.ImageEquals(stockOam),
                "prior four-frame override preserves its actual caret composition");
        }
        SpriteVisualPart part = document.Frames["caret-visible"][0];
        document.Frames["caret-visible"][0] = part with { TileColumn = part.TileColumn + 1 };
        using var editedJson = new MemoryStream();
        IntroCaretSpritePresentation.Write(editedJson, document);
        editedJson.Position = 0;
        IntroCaretSpritePresentation edited = IntroCaretSpritePresentation.Load(editedJson);
        var changed = new OamBuffer();
        changed.BeginFrame();
        edited.Draw(IntroCaretSpriteDefinitions.Still, changed, new(originX, originY), paletteBits);
        changed.FinalizeFrame();
        var original = new OamBuffer();
        original.BeginFrame();
        stock.CaretSprites.Draw(IntroCaretSpriteDefinitions.Still, original,
            new(originX, originY), paletteBits);
        original.FinalizeFrame();
        AssertTrue(!changed.LowTable.SequenceEqual(original.LowTable) &&
                changed.LowTable[..2].SequenceEqual(original.LowTable[..2]) &&
                changed.HighTable.SequenceEqual(original.HighTable),
            "caret JSON tile edit changes OAM art without changing sprite placement");

        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var nativeState = CreateRetailIntroFixture(bus);
        var installedState = CreateRetailIntroFixture(guarded, characterArtwork: stock);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        nativeState.SetupFirstIllustratedPage();
        installedState.SetupFirstIllustratedPage();
        ((nativeState.objects ?? throw new InvalidOperationException("Verification requires nativeState.objects."))).Step(0);
        ((installedState.objects ?? throw new InvalidOperationException("Verification requires installedState.objects."))).Step(0);
        var nativeOam = nativeState.PrepareIllustratedPageOam();
        var installedOam = installedState.PrepareIllustratedPageOam();
        AssertTrue(installedOam.ImageEquals(nativeOam),
            "installed illustrated page draws caret without cartridge spritemap reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed caret art avoids the native OAM composition source");

        var nativeBlink = new IntroCinematicObjectSystem(bus, new SnesVram(), new ushort[1024], new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration, eyeArtwork: stock.EyeFrames);
        var installedBlink = new IntroCinematicObjectSystem(guarded, new SnesVram(),
            new ushort[1024], new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration, eyeArtwork: stock.EyeFrames);
        nativeBlink.SetCaretBlinking();
        installedBlink.SetCaretBlinking();
        for (int frame = 0; frame < 20; frame++)
        {
            nativeBlink.Step(0);
            installedBlink.Step(0);
            ushort expected = (frame / 5) % 2 == 0 ? IntroCaretSpriteDefinitions.Still : (ushort)0;
            AssertEqual(expected, nativeBlink.SpriteMapPointer,
                $"native caret blink selects visible/blank frame at frame {frame}");
            AssertEqual(expected, installedBlink.SpriteMapPointer,
                $"installed caret blink selects visible/blank frame at frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed caret blink avoids cartridge instruction and visual reads");
    }

    /// <summary>Checks the installed flashback collision source without rereading its cartridge bytes.</summary>
    private static void VerifyIntroMotherBrainCollision(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        IntroCinematicArtworkCatalog stock)
    {
        byte[] native = VerifyIntroMotherBrainCollisionSource(CartridgeImportSource.Require(bus));
        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var state = CreateRetailIntroFixture(guarded, characterArtwork: stock);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        state.SetupFirstIllustratedPage();
        state.SetupMotherBrainFlashback();
        AssertTrue(state.MotherBrainLevelData is not null &&
                state.MotherBrainLevelData.SequenceEqual(native),
            "flashback setup retains the exact native physical level without ROM reads");
        var level = state.flashbackLevel;
        for (int index = 0; index < (level ?? throw new InvalidOperationException("Verification requires level.")).ForegroundEntries.Length; index++)
        {
            ushort expected = index * sizeof(ushort) < native.Length
                ? (ushort)(SnesLittleEndian.Word(native[index * 2], native[index * 2 + 1]))
                : (ushort)0;
            AssertEqual(expected, level.ForegroundEntries.Span[index],
                $"Mother Brain flashback collision block {index} preserves native level word");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "flashback setup never rereads the physical level source");
    }

    /// <summary>Compares installed Mother Brain flashback sprite frames with the cartridge's OAM layout.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyIntroMotherBrainSpriteArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        foreach (IntroMotherBrainSpriteFrameDefinition definition in
            IntroMotherBrainSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            DrawImportedSpritemap(bus, native,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer),
                new(IntroMotherBrainSpriteState.Position.X, IntroMotherBrainSpriteState.Position.Y),
                IntroMotherBrainSpriteState.PaletteBits);
            native.FinalizeFrame();
            var installed = new OamBuffer();
            installed.BeginFrame();
            stock.MotherBrainSprites.Draw(definition.Pointer, installed,
                IntroMotherBrainSpriteState.Position.ToOam(),
                IntroMotherBrainSpriteState.PaletteBits);
            installed.FinalizeFrame();
            AssertTrue(installed.ImageEquals(native) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"intro Mother Brain {definition.Name} produces exact native OAM");
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroMotherBrainSpriteFormat.FileName;
        IntroMotherBrainSpriteDocument document = RequiredJson.Deserialize<IntroMotherBrainSpriteDocument>(
            File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        document.Frames["mother-brain-frame-0"] = document.Frames["mother-brain-frame-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroMotherBrainSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
        var stockOam = new OamBuffer();
        stockOam.BeginFrame();
        stock.MotherBrainSprites.Draw(IntroMotherBrainSpriteDefinitions.FrameZero, stockOam,
            IntroMotherBrainSpriteState.Position.ToOam(),
            IntroMotherBrainSpriteState.PaletteBits);
        stockOam.FinalizeFrame();
        var editedOam = new OamBuffer();
        editedOam.BeginFrame();
        edited.MotherBrainSprites.Draw(IntroMotherBrainSpriteDefinitions.FrameZero, editedOam,
            IntroMotherBrainSpriteState.Position.ToOam(),
            IntroMotherBrainSpriteState.PaletteBits);
        editedOam.FinalizeFrame();
        AssertTrue(!editedOam.ImageEquals(stockOam),
            "intro Mother Brain palette edit changes OAM art but not sprite size or X high bits");
        for (int part = 0; part < IntroMotherBrainSpriteDefinitions.StockPartCount; part++)
            AssertTrue(editedOam.LowTable.Slice(part * 4, 2)
                    .SequenceEqual(stockOam.LowTable.Slice(part * 4, 2)),
                $"intro Mother Brain palette edit preserves part {part} screen position");

        var guarded = NewIntroArtworkSourceReadGuard(bus);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockState = CreateRetailIntroFixture(guarded, characterArtwork: stock);
        var editedState = CreateRetailIntroFixture(guarded, characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            state.SetupFirstIllustratedPage();
            state.SetupMotherBrainFlashback();
            (state.flashbackMotherBrain ?? throw new InvalidOperationException("Verification requires state.flashbackMotherBrain.")).Step();
        }
        Rgba32[] stockPixels = stockState.RenderMotherBrainFlashback();
        Rgba32[] editedPixels = editedState.RenderMotherBrainFlashback();
        AssertTrue(!editedPixels.SequenceEqual(stockPixels),
            "intro Mother Brain palette override changes visible production scene pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue((stockState.RenderMotherBrainFlashback()).SequenceEqual(editedPixels),
            "restored intro Mother Brain scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed intro Mother Brain rendering avoids all three native spritemap records");
        File.Delete(overridePath);
    }

    /// <summary>Checks explosion sprite compositions and rejects missing named override frames.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyIntroMotherBrainExplosionSpriteArtwork(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus, IntroCinematicArtworkCatalog stock,
        GameInstallation installation)
    {
        const ushort x = 120;
        const ushort y = 100;
        ushort palette = IntroCinematicRomData.Objects.ExplosionPalette.Raw;
        foreach (IntroMotherBrainExplosionSpriteFrameDefinition definition in
            IntroMotherBrainExplosionSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            DrawImportedSpritemap(bus, native,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer), new(x, y), palette);
            native.FinalizeFrame();
            var installed = new OamBuffer();
            installed.BeginFrame();
            stock.MotherBrainExplosionSprites.Draw(definition.Pointer, installed,
                new(x, y), palette);
            installed.FinalizeFrame();
            AssertTrue(installed.ImageEquals(native) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"intro Mother Brain explosion {definition.Name} produces exact native OAM");
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroMotherBrainExplosionSpriteFormat.FileName;
        IntroMotherBrainExplosionSpriteDocument document =
            RequiredJson.Deserialize<IntroMotherBrainExplosionSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        document.Frames["small-explosion-0"] = document.Frames["small-explosion-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroMotherBrainExplosionSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroMotherBrainExplosions);
        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        explosions.Step(introCrossfadeTimer: 1);
        var stockOam = new OamBuffer();
        stockOam.BeginFrame();
        explosions.Draw(stockOam, stock.MotherBrainExplosionSprites);
        stockOam.FinalizeFrame();
        var editedOam = new OamBuffer();
        editedOam.BeginFrame();
        explosions.Draw(editedOam, edited.MotherBrainExplosionSprites);
        editedOam.FinalizeFrame();
        AssertTrue(!editedOam.ImageEquals(stockOam),
            "intro explosion palette edit changes production OAM attributes, not actor placement");
        for (int part = 0; part < stockOam.LastFinalizedSpriteCount; part++)
            AssertTrue(editedOam.LowTable.Slice(part * 4, 2)
                    .SequenceEqual(stockOam.LowTable.Slice(part * 4, 2)),
                $"intro explosion palette edit preserves native part {part} X/Y");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed intro explosion actors never reread the twelve native compositions");

        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockState = CreateRetailIntroFixture(guarded, characterArtwork: stock);
        var editedState = CreateRetailIntroFixture(guarded, characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            state.SetupFirstIllustratedPage();
            state.SetupMotherBrainFlashback();
            state.flashbackMotherBrainExplosions = explosions;
        }
        Rgba32[] stockPixels = stockState.RenderMotherBrainFlashback();
        Rgba32[] editedPixels = editedState.RenderMotherBrainFlashback();
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "intro explosion art override changes visible production flashback pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue((stockState.RenderMotherBrainFlashback()).SequenceEqual(editedPixels),
            "restored intro explosion scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production intro explosion render avoids all twelve native compositions");

        document.Frames.Remove("small-explosion-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroMotherBrainExplosionSpritePresentation.Write(malformed, document);
        }, "intro explosion artwork rejects a missing named frame");
        File.Delete(overridePath);
    }

    /// <summary>Builds a guard that reports any cinematic read from selected migrated artwork ranges.</summary>
    internal static MutableCartridgeReadGuard NewIntroArtworkSourceReadGuard(ISnesAddressSpace source,
        IntroArtworkSources blocked = IntroArtworkSources.None)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => IntroArtworkForbiddenRead(address, blocked) is not null,
            address => (IntroArtworkForbiddenRead(address, blocked) ?? throw new InvalidOperationException("Verification requires IntroArtworkForbiddenRead(address, blocked).")));
        return guard;
    }

    /// <summary>The failure message for a cinematic read of migrated artwork, or null when the read is allowed.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static string? IntroArtworkForbiddenRead(int address, IntroArtworkSources blocked)
    {
        if (blocked.HasFlag(IntroArtworkSources.CeresFlightSprites))
        {
            foreach (CeresFlightSpriteFrameDefinition frame in CeresFlightSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    return $"Ceres flight reread installed spritemap ${address:X6}.";
                }
            }
        }
        if (blocked.HasFlag(IntroArtworkSources.CeresDestructionSprites))
        {
            foreach (CeresDestructionSpriteFrameDefinition frame in
                CeresDestructionSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    return $"Ceres destruction reread installed spritemap ${address:X6}.";
                }
            }
        }
        if (address is IntroCinematicRomData.Assets.BackgroundCharacters or
            IntroCinematicRomData.Assets.BackgroundPageTilemaps or
            IntroCinematicRomData.Assets.SamusHeadTilemap or
            IntroCinematicRomData.Assets.FirstNarrationTilemap or
            IntroCinematicRomData.Assets.IntroObjectCharacters or
            IntroCinematicRomData.Assets.ObjectCharacters or
            CeresFlightRomData.Assets.Mode7Characters or
            CeresFlightRomData.Assets.Mode7Maps or
            CeresFlightRomData.Assets.ObjectCharacters or
            CeresDestructionRomData.Assets.ZebesTilemap or
            CeresDestructionRomData.Assets.ZebesCharacters or
            CeresDestructionRomDataAssetsConstants.SharedObjectCharacters)
        {
            return $"Cinematic reread character source ${address:X6}.";
        }
        if (address is >= CeresFlightRomData.Assets.Palette and
            < (CeresFlightRomData.Assets.Palette + CgramPalettes.ByteCount))
        {
            return $"Cinematic reread Ceres flight palette ${address:X6}.";
        }
        if (address is >= IntroCinematicRomData.Assets.Palette and
            < (IntroCinematicRomData.Assets.Palette + CgramPalettes.ByteCount))
        {
            return $"Cinematic reread opening palette ${address:X6}.";
        }
        if (address is >= IntroCinematicRomData.Assets.FinalTextLine and
            < (IntroCinematicRomData.Assets.FinalTextLine +
                IntroFinalLineTilemapFormat.CellCount * sizeof(ushort)))
        {
            return $"Cinematic reread opening divider ${address:X6}.";
        }
        if (address >= IntroCinematicRomData.Assets.MotherBrainLevelData &&
            address < IntroCinematicRomData.Assets.MotherBrainLevelData +
                IntroCinematicRomData.Flashback.MotherBrainLevelByteCount)
        {
            return $"Cinematic reread Mother Brain collision source ${address:X6}.";
        }
        int eyeScriptStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
            IntroEyeAnimationDefinitions.StartPointer);
        if (CinematicSpriteNativeListAddresses.InListRegion(address))
            return $"Cinematic reread a native cinematic sprite list ${address:X6}.";
        int eyeFrameStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
            IntroEyeAnimationDefinitions.FrameStartPointer);
        foreach (IntroCaretFrameDefinition caret in IntroCaretSpriteDefinitions.Frames)
        {
            int sourceAddress = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps, caret.Pointer);
            int byteCount = 2 + caret.StockPartCount * 5;
            if (address >= sourceAddress && address < sourceAddress + byteCount)
            {
                return $"Cinematic reread caret composition ${address:X6}.";
            }
        }
        int motherBrainSpriteStart = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroMotherBrainSpriteDefinitions.FrameZero);
        int motherBrainSpriteEnd = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            (ushort)(IntroMotherBrainSpriteDefinitions.FrameTwo +
                2 + IntroMotherBrainSpriteDefinitions.StockPartCount * 5));
        if (address >= motherBrainSpriteStart && address < motherBrainSpriteEnd)
        {
            return $"Cinematic reread intro Mother Brain sprite ${address:X6}.";
        }
        int explosionSpriteStart = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroMotherBrainExplosionSpriteDefinitions.SmallStart);
        int explosionSpriteEnd = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroMotherBrainExplosionSpriteDefinitions.End);
        if (blocked.HasFlag(IntroArtworkSources.IntroMotherBrainExplosions) &&
            address >= explosionSpriteStart && address < explosionSpriteEnd)
        {
            return $"Cinematic reread intro Mother Brain explosion sprite ${address:X6}.";
        }
        int rinkaSpriteStart = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps, IntroRinkaSpriteDefinitions.First);
        int rinkaSpriteEnd = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps, IntroRinkaSpriteDefinitions.End);
        if (blocked.HasFlag(IntroArtworkSources.IntroRinkas) &&
            address >= rinkaSpriteStart && address < rinkaSpriteEnd)
        {
            return $"Cinematic reread intro Rinka sprite ${address:X6}.";
        }
        int eggEffectSpriteStart = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroEggEffectSpriteDefinitions.Start);
        int eggEffectSpriteEnd = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroEggEffectSpriteDefinitions.End);
        if (blocked.HasFlag(IntroArtworkSources.IntroEggEffects) &&
            address >= eggEffectSpriteStart && address < eggEffectSpriteEnd)
        {
            return $"Cinematic reread intro egg effect sprite ${address:X6}.";
        }
        if (blocked.HasFlag(IntroArtworkSources.IntroDiscoveryActors))
        {
            int eggStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroDiscoveryActorSpriteDefinitions.EggStart);
            int eggEnd = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroDiscoveryActorSpriteDefinitions.EggEnd);
            int babyStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroDiscoveryActorSpriteDefinitions.BabyStart);
            int babySmallEnd = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroDiscoveryActorSpriteDefinitions.BabySmallEnd);
            int babyLarge = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroDiscoveryActorSpriteDefinitions.BabyLarge);
            int babyEnd = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroDiscoveryActorSpriteDefinitions.BabyEnd);
            if (address >= eggStart && address < eggEnd ||
                address >= babyStart && address < babySmallEnd ||
                address >= babyLarge && address < babyEnd)
            {
                return $"Cinematic reread intro discovery actor sprite ${address:X6}.";
            }
        }
        int scientistSpriteStart = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroScientistSpriteDefinitions.Start);
        int scientistSpriteEnd = (int)new SnesAddress(
            IntroCinematicRomData.Banks.Spritemaps,
            IntroScientistSpriteDefinitions.End);
        if (blocked.HasFlag(IntroArtworkSources.IntroScientistSprites) &&
            address >= scientistSpriteStart && address < scientistSpriteEnd)
        {
            return $"Cinematic reread intro scientist sprite ${address:X6}.";
        }
        int motherBrainDemoListStart = DemoInputRomData.BankBase |
            IntroMotherBrainInputDefinitions.ListStart;
        int motherBrainDemoListEnd = DemoInputRomData.BankBase |
            IntroMotherBrainInputDefinitions.ListEnd;
        int motherBrainDemoHeaderStart = DemoInputRomData.BankBase |
            IntroMotherBrainInputDefinitions.HeaderStart;
        int motherBrainDemoHeaderEnd = DemoInputRomData.BankBase |
            IntroMotherBrainInputDefinitions.HeaderEnd;
        if (address >= motherBrainDemoListStart && address < motherBrainDemoListEnd ||
            address >= motherBrainDemoHeaderStart && address < motherBrainDemoHeaderEnd)
        {
            return $"Cinematic reread intro Mother Brain demo program ${address:X6}.";
        }
        if ((address >= eyeScriptStart &&
                address < eyeScriptStart + IntroEyeAnimationDefinitions.EndPointer -
                    IntroEyeAnimationDefinitions.StartPointer) ||
            (address >= eyeFrameStart &&
                address < eyeFrameStart + IntroEyeAnimationDefinitions.FrameCount *
                    IntroEyeAnimationDefinitions.FrameStride))
        {
            return $"Cinematic reread opening eye source ${address:X6}.";
        }
        return null;
    }

    /// <summary>The installed artwork sources an intro read guard forbids the cinematic to reread (#633).</summary>
    [Flags]
    internal enum IntroArtworkSources
    {
        /// <summary>Blocks only the artwork ranges unconditionally checked by the guard.</summary>
        None = 0,
        /// <summary>Blocks intro Mother Brain explosion sprite source records.</summary>
        IntroMotherBrainExplosions = 1 << 0,
        /// <summary>Blocks intro Rinka sprite source records.</summary>
        IntroRinkas = 1 << 1,
        /// <summary>Blocks intro egg-effect sprite source records.</summary>
        IntroEggEffects = 1 << 2,
        /// <summary>Blocks discovery actor sprite source records.</summary>
        IntroDiscoveryActors = 1 << 3,
        /// <summary>Blocks scientist sprite source records.</summary>
        IntroScientistSprites = 1 << 4,
        /// <summary>Blocks Ceres flight sprite source records.</summary>
        CeresFlightSprites = 1 << 5,
        /// <summary>Blocks Ceres destruction sprite source records.</summary>
        CeresDestructionSprites = 1 << 6,
    }

    /// <summary>
    /// The installed intro, Ceres and ending artwork family. Each suite that edits artwork runs on
    /// its own private installation copy, so the full run schedules them independently; the two
    /// read-only suites share the repository installation.
    /// </summary>
    internal static RegisteredSuite[] IntroCinematicArtworkSuites() =>
    [
        new(nameof(VerifyFrontendRomFreeStartup), () => VerifyFrontendRomFreeStartup(RepositoryInstallation.Installation, RepositoryRomPath)),
        new(nameof(VerifyGoldenTorizoCartridgeCombatFallback), () => VerifyGoldenTorizoCartridgeCombatFallback(RepositoryRomPath, RepositoryInstallation.Installation)),
        new(nameof(VerifySamusBodyArtwork), () => VerifySamusBodyArtworkOnPrivateCopy()),
        new(nameof(VerifyIntroCinematicArtwork), () => VerifyIntroCinematicArtwork(RepositoryRomPath)),
        new(nameof(VerifyCeresCinematicArtwork), () => VerifyCeresCinematicArtwork(RepositoryRomPath)),
        new(nameof(VerifyEndingArtwork), () => VerifyEndingArtwork()),
    ];

    /// <summary>Runs the Samus body artwork checks, which edit pose artwork, on a private installation copy.</summary>
    private static void VerifySamusBodyArtworkOnPrivateCopy()
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopy();
        VerifySamusBodyArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath),
            copy.Installation);
    }

    /// <summary>Runs every suite of the installed intro, Ceres and ending artwork family in this process.</summary>
    [VerificationCommand("--intro-cinematic-artwork")]
    internal static int IntroCinematicArtworkCommand()
    {
        RunSuitesSerially(IntroCinematicArtworkSuites());
        return 0;
    }

    /// <summary>Runs the Samus body artwork checks.</summary>
    [VerificationCommand("--samus-body-artwork")]
    internal static int SamusBodyArtworkCommand()
    {
        Suite(nameof(VerifySamusBodyArtwork), () => VerifySamusBodyArtworkOnPrivateCopy());
        return 0;
    }
}
