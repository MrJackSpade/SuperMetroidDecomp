using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using static SuperMetroid.Verification.Frontend.Intro.IntroArtworkPostSlicesVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Harness.SuiteSchedulingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Frontend.Intro.CinematicSpriteAnimationsVerification;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoCartridgeFallbackVerification;
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
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Checks installed opening and Ceres cinematic artwork, resource validation, and ROM-read guards.</summary>
internal static class IntroCinematicArtworkVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Exercises installed opening-scene art and tilemaps through the real VRAM loader.</summary>
    [PrivateInstallation("Corrupts and removes opening-scene stock files and BG pages on a private copy to check that missing and damaged stock fails where it is read.")]
    internal static void VerifyIntroCinematicArtwork(string sourceRom)
    {
        // Corruption and repairs run on a private copy of the installation's opening-cinematic artwork, the only content
        // the check edits or reads from the copy (#1532); ROM import itself is covered by the asset-import verification.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.IntroCinematicDirectory);
        string root = copy.Root;
        {
            GameInstallation installation = copy.Installation;
            // The opening's pages and colours are import data (#1636, #1637): the check corrupts and restores its own copy of them.
            string importData = Path.Combine(root, SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.ImportDataDirectoryName, SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName);
            PrivateInstallationCopy.CopyInstallation(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName), importData);
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(bus));
            ImportIntroCinematicArtwork stock = IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, importData);
            AssertTrue(stock.BackgroundCharacters.Characters.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicSourceDefinitions.BackgroundCharacters,
                        maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundByteCount)),
                "extracted intro BG page preserves every native tile byte");
            AssertTrue(stock.IntroObjectCharacters.Characters.Span.SequenceEqual(
                    RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.IntroObjectCharacters,
                        IntroCinematicArtworkFormat.IntroObjectByteCount)),
                "installed fixed intro OBJ PNG preserves every native tile byte");
            AssertTrue(stock.CinematicObjectCharacters.Characters.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicSourceDefinitions.ObjectCharacters,
                        maximumOutputBytes: IntroCinematicArtworkFormat.CinematicObjectByteCount)),
                "installed compressed cinematic OBJ PNG preserves every native tile byte");
            AssertTrue(BackgroundTileWordBytes.Of(stock.BackgroundPages.Span).AsSpan().SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicSourceDefinitions.BackgroundPageTilemaps,
                        maximumOutputBytes: IntroCinematicDefinitionsArtworkConstants.BackgroundPageTilemapCells * sizeof(ushort))),
                "four installed BG page JSON files, encoded through the codec, preserve every native tilemap word");
            AssertTrue(BackgroundTileWordBytes.Of(stock.PortraitTilemap.Span).AsSpan().SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicSourceDefinitions.SamusHeadTilemap,
                        maximumOutputBytes: IntroCinematicDefinitionsArtworkConstants.SamusHeadTilemapCells * sizeof(ushort))),
                "installed portrait JSON, encoded through the codec, preserves every native tilemap word");
            AssertTrue(BackgroundTileWordBytes.Of(stock.InitialNarrationTilemap.Span).AsSpan().SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicSourceDefinitions.FirstNarrationTilemap,
                        maximumOutputBytes: IntroArtworkExtentDefinitions.NarrationTilemapCells * sizeof(ushort))),
                "installed initial narration JSON, encoded through the codec, preserves every native tilemap word");
            for (int index = 0; index < IntroFinalLineTilemapFormat.CellCount; index++)
            {
                int source = IntroCinematicSourceDefinitions.FinalTextLine + index * sizeof(ushort);
                ushort nativeWord = LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, source), CpuBusRegionRead.Read(bus, source + 1));
                AssertEqual(nativeWord, stock.FinalLine.Tiles.Span[index].PackedWord,
                    $"opening divider tile {index} matches cartridge source");
            }
            Suite(nameof(VerifyIntroEyeInstructions), () => VerifyIntroEyeInstructions(bus));
            for (int frame = 0; frame < IntroEyeTilemapFormat.FrameCount; frame++)
            {
                int source = (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
                    (ushort)(FrontendSourceDefinitions.IntroEyeFrameRecords +
                        frame * FrontendSourceDefinitions.IntroEyeFrameRecordStride));
                for (int cell = 0; cell < IntroEyeTilemapFormat.CellsPerFrame; cell++)
                {
                    int wordSource = source + 4 + cell * sizeof(ushort);
                    ushort nativeWord = LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, wordSource), CpuBusRegionRead.Read(bus, wordSource + 1));
                    AssertEqual(nativeWord, stock.EyeFrames.FrameTiles(frame)[cell].PackedWord,
                        $"opening eye frame {frame} cell {cell} matches cartridge");
                }
            }
            Suite(nameof(VerifyIntroEyeArtwork), () => VerifyIntroEyeArtwork(bus));
            Suite(nameof(VerifyIntroCaretSpriteArtwork), () => VerifyIntroCaretSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroMotherBrainCollision), () => VerifyIntroMotherBrainCollision(bus, stock));
            Suite(nameof(VerifyIntroMotherBrainSpriteArtwork), () => VerifyIntroMotherBrainSpriteArtwork(bus, stock));
            Suite(nameof(VerifyIntroMotherBrainExplosionSpriteArtwork), () => VerifyIntroMotherBrainExplosionSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroRinkaSpriteArtwork), () => VerifyIntroRinkaSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroEggEffectSpriteArtwork), () => VerifyIntroEggEffectSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroDiscoveryActorSpriteArtwork), () => VerifyIntroDiscoveryActorSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroScientistSpriteArtwork), () => VerifyIntroScientistSpriteArtwork(bus, stock, installation));
            Suite(nameof(VerifyIntroMotherBrainDemoInput), () => VerifyIntroMotherBrainDemoInput(bus, stock));
            AssertTrue(ScreenColourWords.Words(stock.Palette.BackgroundColours.AsSpan(), stock.Palette.SpriteColours.AsSpan()).AsSpan().SequenceEqual(
                    RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.Palette,
                        CgramPalettes.ByteCount)),
                "installed opening palette preserves every native RGB5 word");

            var native = CreateRetailIntroFixture();
            var completeInstalled = CreateRetailIntroFixture(
                characterArtwork: stock);
            LayeredScene completeNativeScene = native.CaptureTranslatedRenderSnapshot();
            LayeredScene completeInstalledScene = completeInstalled.CaptureTranslatedRenderSnapshot();
            AssertTrue(completeInstalled.ShownVram().SequenceEqual(native.ShownVram()) &&
                completeInstalledScene.Paint.Backdrop == completeNativeScene.Paint.Backdrop &&
                SceneShownColours.Same(SceneShownColours.Of(completeInstalledScene), SceneShownColours.Of(completeNativeScene)),
                "fully installed opening upload preserves every native VRAM and CGRAM word without a cartridge read");
            var guarded = NewIntroArtworkSourceReadGuard(bus);
            var installed = CreateRetailIntroFixture(characterArtwork: stock);
            byte[] nativeVram = native.ShownVram().ToArray();
            AssertTrue(installed.ShownVram().SequenceEqual(nativeVram),
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
                    byte[] expectedSceneVram = native.ShownVram();
                    var actualScene = installed.CaptureTranslatedRenderSnapshot();
                    byte[] actualSceneVram = installed.ShownVram();
                    AssertTrue(actualScene.Brightness == expectedScene.Brightness &&
                            actualSceneVram.SequenceEqual(expectedSceneVram) &&
                            actualScene.Paint.Backdrop == expectedScene.Paint.Backdrop &&
                            SceneShownColours.Same(SceneShownColours.Of(actualScene), SceneShownColours.Of(expectedScene)),
                        $"installed cinematic preserves native graphics and colors at frame {frame}");
                }
            }
            string paletteStockPath = Path.Combine(importData, IntroCinematicPaletteFile.FileName);
            IntroCinematicPaletteDocument paletteDocument =
                JsonSerializer.Deserialize<IntroCinematicPaletteDocument>(
                    File.ReadAllBytes(paletteStockPath),
                    CaseInsensitiveJson)
                ?? throw new InvalidDataException("Extracted opening palette is empty.");
            AssertThrows<InvalidDataException>(() => IntroCinematicPaletteFile.Read(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(paletteDocument with
                {
                    Colors = paletteDocument.Colors.Take(255).ToArray(),
                }))), "opening palette rejects a truncated color table");
            AssertThrows<InvalidDataException>(() => IntroCinematicPaletteFile.Read(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(paletteDocument with
                {
                    Colors = paletteDocument.Colors.Select((color, index) =>
                        index == 1 ? color with { Red = 32 } : color).ToArray(),
                }))), "opening palette rejects out-of-range RGB5 channels");
            byte[] stockPalette = File.ReadAllBytes(paletteStockPath);
            File.WriteAllBytes(paletteStockPath, [0]);
            AssertThrows<InvalidDataException>(() => IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, importData),
                "opening palette stock corruption fails loudly where the import reads it");
            // Installed content is never repaired in place (#1532): damaged stock fails where it loads, and the check puts the stock bytes back.
            File.WriteAllBytes(paletteStockPath, stockPalette);
            AssertTrue(IntroPaletteWords(IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, importData).Palette).AsSpan().SequenceEqual(
                    ScreenColourWords.Words(stock.Palette.BackgroundColours.AsSpan(), stock.Palette.SpriteColours.AsSpan())),
                "the restored stock shows exact cartridge opening palette colors");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed opening palette never rereads its cartridge source");
            string dividerStockPath = Path.Combine(installation.IntroCinematicDirectory,
                IntroFinalLineTilemapFormat.FileName);
            IntroFinalLineTilemapDocument dividerDocument =
                JsonSerializer.Deserialize<IntroFinalLineTilemapDocument>(
                    File.ReadAllBytes(dividerStockPath),
                    CaseInsensitiveJson)
                ?? throw new InvalidDataException("Extracted opening divider is empty.");
            var nativeDividerState = CreateRetailIntroFixture();
            var stockDividerState = CreateRetailIntroFixture(characterArtwork: stock);
            foreach (IntroCinematicState scene in new[] { nativeDividerState, stockDividerState })
                scene.SetupFirstIllustratedPage();
            AssertTrue(stockDividerState.ShownVram().SequenceEqual(
                    nativeDividerState.ShownVram()),
                "installed opening divider produces exact native illustrated-page VRAM");
            AssertThrows<InvalidDataException>(() => IntroFinalLineTilemap.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(dividerDocument with
                {
                    Cells = dividerDocument.Cells.Take(127).ToArray(),
                }))), "opening divider rejects a truncated tilemap");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed opening divider never rereads its cartridge source");

            string pageStockPath = Path.Combine(installation.IntroCinematicDirectory,
                IntroCinematicArtworkFormat.BackgroundPageFileName(0));
            byte[] stockPage = File.ReadAllBytes(pageStockPath);
            File.WriteAllBytes(pageStockPath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "a corrupt stock BG page is rejected");
            File.WriteAllBytes(pageStockPath, stockPage);
            AssertTrue(installation.LoadIntroCinematicArt().BackgroundPages.Span.SequenceEqual(
                    stock.BackgroundPages.Span),
                "the restored stock shows all four native cinematic BG pages");
            Console.WriteLine(
                "Intro art: three extracted character pages, seven full tilemaps, eye/caret/Mother Brain/explosion/Rinka/egg-effect/discovery/scientist compositions and full RGB5 palette; native parity and strict failures pass.");
        }
    }

    /// <summary>Runs the installed eye program and confirms each update shows the frame the native program shows, without native reads.</summary>
    private static void VerifyIntroEyeArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var native = new IntroCinematicObjectSystem(new IntroNarrationText(), new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration);
        var installed = new IntroCinematicObjectSystem(new IntroNarrationText(),
            new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration);
        native.Step(0);
        installed.Step(0);
        AssertEqual(native.EyeFrame, installed.EyeFrame, "installed opening eye shows the native frame");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed opening eye script and frame do not reread ROM");

        for (int frame = 0; frame < 160; frame++)
        {
            native.Step(0);
            installed.Step(0);
            AssertEqual(native.EyeFrame, installed.EyeFrame,
                $"installed normal eye blink shows the native frame at frame {frame}");
        }
        native.StartEnglishPageSix();
        installed.StartEnglishPageSix();
        for (int frame = 0; frame < 64; frame++)
        {
            native.Step(0);
            installed.Step(0);
            AssertEqual(native.EyeFrame, installed.EyeFrame,
                $"installed page-six eye blink shows the native frame at frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed eye lists and all four frames avoid native source reads");
    }

    /// <summary>Compares caret sprite compositions and blink behavior with native OAM output.</summary>
    private static void VerifyIntroCaretSpriteArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        ImportIntroCinematicArtwork stock, GameInstallation installation)
    {
        Suite(nameof(VerifyCinematicSpriteAnimations), () => VerifyCinematicSpriteAnimations(bus));
        const ushort originX = 8;
        const ushort originY = 24;
        // The caret draws in the opening's cross-fade colours; the cartridge oracle takes their sprite set as its palette bits.
        PictureShade palette = IntroShades.Drawn(IntroSpriteColouring.CrossFadeObjects);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in IntroCaretSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            DrawImportedSpritemap(bus, native,
                (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
                    CinematicSpritemapSources.Of(definition.Pose)), new(originX, originY), paletteBits);
            native.FinalizeFrame();
            var installed = ScreenSpritesFixtures.Blank();
            installed.BeginFrame();
            CinematicSheetFixtures.Drawer(stock.CaretSprites).Draw(definition.Pose, installed,
                new(originX, originY), palette);
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

        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var nativeState = CreateRetailIntroFixture();
        var installedState = CreateRetailIntroFixture(characterArtwork: stock);
        nativeState.SetupFirstIllustratedPage();
        installedState.SetupFirstIllustratedPage();
        ((nativeState.CurrentObjects ?? throw new InvalidOperationException("Verification requires nativeState.objects."))).Step(0);
        ((installedState.CurrentObjects ?? throw new InvalidOperationException("Verification requires installedState.objects."))).Step(0);
        var nativeOam = nativeState.PrepareIllustratedPageSprites().FinalizedAsOam();
        var installedOam = installedState.PrepareIllustratedPageSprites().FinalizedAsOam();
        AssertTrue(installedOam.ImageEquals(nativeOam),
            "installed illustrated page draws caret without cartridge spritemap reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed caret art avoids the native OAM composition source");

        var nativeBlink = new IntroCinematicObjectSystem(new IntroNarrationText(), new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration);
        var installedBlink = new IntroCinematicObjectSystem(new IntroNarrationText(),
            new IntroJapaneseSubtitles(enabled: false), narrationPresentation: RetailPresentationFixture().IntroNarration);
        nativeBlink.SetCaretBlinking();
        installedBlink.SetCaretBlinking();
        for (int frame = 0; frame < 20; frame++)
        {
            nativeBlink.Step(0);
            installedBlink.Step(0);
            ushort expected = (frame / 5) % 2 == 0 ? CinematicSpritemapSources.Of(IntroCaretSpriteDefinitions.Visible.Pose) : (ushort)0;
            AssertEqual(expected, CinematicSpritePoseSpritemaps.Of(nativeBlink.CaretPose),
                $"native caret blink selects visible/blank frame at frame {frame}");
            AssertEqual(expected, CinematicSpritePoseSpritemaps.Of(installedBlink.CaretPose),
                $"installed caret blink selects visible/blank frame at frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed caret blink avoids cartridge instruction and visual reads");
    }

    /// <summary>Checks the installed flashback collision source without rereading its cartridge bytes.</summary>
    private static void VerifyIntroMotherBrainCollision(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        RoomTerrainBlock[] native = VerifyIntroMotherBrainCollisionSource(CartridgeImportSource.Require(bus));
        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var state = CreateRetailIntroFixture(characterArtwork: stock);
        state.SetupFirstIllustratedPage();
        state.SetupMotherBrainFlashback();
        var level = state.CurrentFlashbackLevel ?? throw new InvalidOperationException("Verification requires level.");
        for (int index = 0; index < native.Length; index++)
            AssertEqual(native[index], level.GetCollisionBlockByIndex(index).Terrain,
                $"Mother Brain flashback collision block {index} matches the decoded original without ROM reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "flashback setup never rereads the physical level source");
    }

    /// <summary>Compares installed Mother Brain flashback sprite frames with the cartridge's OAM layout.</summary>
    private static void VerifyIntroMotherBrainSpriteArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        // The cartridge oracle takes the sprite set Mother Brain's colouring resolves to as its attribute bits; the port's
        // draw takes the colouring itself.
        AssertEqual(IntroShades.Drawn(IntroSpriteColouring.MotherBrain), IntroMotherBrainSpriteState.Colouring,
            "Mother Brain draws in her own drawn shade");
        ushort motherBrainPaletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(IntroMotherBrainSpriteState.Colouring), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroMotherBrainSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            DrawImportedSpritemap(bus, native,
                (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
                    CinematicSpritemapSources.Of(definition.Pose)),
                new(IntroMotherBrainSpriteState.Position.X, IntroMotherBrainSpriteState.Position.Y),
                motherBrainPaletteBits);
            native.FinalizeFrame();
            var installed = ScreenSpritesFixtures.Blank();
            installed.BeginFrame();
            CinematicSheetFixtures.Drawer(stock.MotherBrainSprites).Draw(definition.Pose, installed,
                IntroMotherBrainSpriteState.Position.ToSpritePosition(),
                IntroMotherBrainSpriteState.Colouring);
            installed.FinalizeFrame();
            AssertTrue(installed.ImageEquals(native) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"intro Mother Brain {definition.Name} produces exact native OAM");
        }
    }

    /// <summary>Checks explosion sprite compositions and rejects a document missing a named frame.</summary>
    private static void VerifyIntroMotherBrainExplosionSpriteArtwork(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus, ImportIntroCinematicArtwork stock,
        GameInstallation installation)
    {
        const ushort x = 120;
        const ushort y = 100;
        PictureShade palette = IntroShades.Drawn(IntroSpriteColouring.Explosions);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroMotherBrainExplosionSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            DrawImportedSpritemap(bus, native,
                (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
                    CinematicSpritemapSources.Of(definition.Pose)), new(x, y), paletteBits);
            native.FinalizeFrame();
            var installed = ScreenSpritesFixtures.Blank();
            installed.BeginFrame();
            CinematicSheetFixtures.Drawer(stock.MotherBrainExplosionSprites).Draw(definition.Pose, installed,
                new(x, y), palette);
            installed.FinalizeFrame();
            AssertTrue(installed.ImageEquals(native) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"intro Mother Brain explosion {definition.Name} produces exact native OAM");
        }

        IntroMotherBrainExplosionSpriteDocument document =
            RequiredJson.Deserialize<IntroMotherBrainExplosionSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, IntroMotherBrainExplosionSpriteFormat.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroMotherBrainExplosions);
        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        explosions.Step(introCrossfadeTimer: 1);
        var stockOam = ScreenSpritesFixtures.Blank();
        stockOam.BeginFrame();
        explosions.Draw(stockOam, CinematicSheetFixtures.Drawer(stock.MotherBrainExplosionSprites));
        stockOam.FinalizeFrame();
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed intro explosion actors never reread the twelve native compositions");

        document.Frames.Remove("small-explosion-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroMotherBrainExplosionSpritePresentation.Write(malformed, document);
            _ = IntroMotherBrainExplosionSpritePresentation.Load(new MemoryStream(malformed.ToArray(), writable: false));
        }, "intro explosion artwork rejects a missing named frame on read");
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
    private static string? IntroArtworkForbiddenRead(int address, IntroArtworkSources blocked)
    {
        if (blocked.HasFlag(IntroArtworkSources.CeresFlightSprites))
        {
            foreach (CinematicSpriteFrameDefinition frame in CeresFlightSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    return $"Ceres flight reread installed spritemap ${address:X6}.";
                }
            }
        }
        if (blocked.HasFlag(IntroArtworkSources.CeresDestructionSprites))
        {
            foreach (CeresDestructionSpriteFrameDefinition frame in
                CeresDestructionSpriteFrameDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    return $"Ceres destruction reread installed spritemap ${address:X6}.";
                }
            }
        }
        if (address is IntroCinematicSourceDefinitions.BackgroundCharacters or
            IntroCinematicSourceDefinitions.BackgroundPageTilemaps or
            IntroCinematicSourceDefinitions.SamusHeadTilemap or
            IntroCinematicSourceDefinitions.FirstNarrationTilemap or
            IntroCinematicSourceDefinitions.ObjectCharacters or
            CeresCinematicSourceDefinitions.FlightFloorCharacters or
            CeresCinematicSourceDefinitions.CeresTilemaps or
            CeresCinematicSourceDefinitions.FlightObjectCharacters or
            CeresCinematicSourceDefinitions.ZebesTilemap or
            CeresCinematicSourceDefinitions.ZebesCharacters or
            IntroCinematicSourceDefinitions.IntroObjectCharacters)
        {
            return $"Cinematic reread character source ${address:X6}.";
        }
        if (address is >= CeresCinematicSourceDefinitions.FlightPalette and
            < (CeresCinematicSourceDefinitions.FlightPalette + CgramPalettes.ByteCount))
        {
            return $"Cinematic reread Ceres flight palette ${address:X6}.";
        }
        if (address is >= IntroCinematicSourceDefinitions.Palette and
            < (IntroCinematicSourceDefinitions.Palette + CgramPalettes.ByteCount))
        {
            return $"Cinematic reread opening palette ${address:X6}.";
        }
        if (address is >= IntroCinematicSourceDefinitions.FinalTextLine and
            < (IntroCinematicSourceDefinitions.FinalTextLine +
                IntroFinalLineTilemapFormat.CellCount * sizeof(ushort)))
        {
            return $"Cinematic reread opening divider ${address:X6}.";
        }
        if (address is >= IntroCinematicCheckSources.MotherBrainLevelData
            and < IntroCinematicCheckSources.MotherBrainLevelData + IntroMotherBrainFlashbackConstants.MotherBrainLevelByteCount)
        {
            return $"Cinematic reread Mother Brain collision source ${address:X6}.";
        }
        int eyeScriptStart = (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
            IntroEyeAnimationDefinitionsConstants.StartPointer);
        if (CinematicSpriteNativeListAddresses.InListRegion(address))
            return $"Cinematic reread a native cinematic sprite list ${address:X6}.";
        int eyeFrameStart = (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
            FrontendSourceDefinitions.IntroEyeFrameRecords);
        foreach (CinematicSpriteFrameDefinition caret in IntroCaretSpriteDefinitions.Frames)
        {
            int sourceAddress = (int)new SnesAddress(
                IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(caret.Pose));
            int byteCount = 2 + caret.StockPartCount * 5;
            if (address >= sourceAddress && address < sourceAddress + byteCount)
            {
                return $"Cinematic reread caret composition ${address:X6}.";
            }
        }
        if (RereadsFrame(IntroMotherBrainSpriteDefinitions.Frames))
            return $"Cinematic reread intro Mother Brain sprite ${address:X6}.";
        if (blocked.HasFlag(IntroArtworkSources.IntroMotherBrainExplosions) && RereadsFrame(IntroMotherBrainExplosionSpriteDefinitions.Frames))
            return $"Cinematic reread intro Mother Brain explosion sprite ${address:X6}.";
        if (blocked.HasFlag(IntroArtworkSources.IntroRinkas) && RereadsFrame(IntroRinkaSpriteDefinitions.Frames))
            return $"Cinematic reread intro Rinka sprite ${address:X6}.";
        if (blocked.HasFlag(IntroArtworkSources.IntroEggEffects) && RereadsFrame(IntroEggEffectSpriteDefinitions.Frames))
            return $"Cinematic reread intro egg effect sprite ${address:X6}.";
        if (blocked.HasFlag(IntroArtworkSources.IntroDiscoveryActors) && RereadsFrame(IntroDiscoveryActorSpriteDefinitions.Frames))
            return $"Cinematic reread intro discovery actor sprite ${address:X6}.";
        if (blocked.HasFlag(IntroArtworkSources.IntroScientistSprites) && RereadsFrame(IntroScientistSpriteDefinitions.Frames))
            return $"Cinematic reread intro scientist sprite ${address:X6}.";
        if ((address >= eyeScriptStart &&
                address < eyeScriptStart + IntroEyeAnimationDefinitionsConstants.EndPointer -
                    IntroEyeAnimationDefinitionsConstants.StartPointer) ||
            (address >= eyeFrameStart &&
                address < eyeFrameStart + IntroEyeAnimationDefinitionsConstants.FrameCount *
                    FrontendSourceDefinitions.IntroEyeFrameRecordStride))
        {
            return $"Cinematic reread opening eye source ${address:X6}.";
        }
        return null;

        // Whether the read lands inside one of the frames' stock spritemap records.
        bool RereadsFrame(IEnumerable<CinematicSpriteFrameDefinition> frames) => frames.Any(frame =>
            (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose)) is var start &&
            address >= start && address < start + 2 + frame.StockPartCount * 5);
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
    /// The installed intro, Ceres and ending artwork family, with the Samus body artwork. Each suite that corrupts artwork
    /// runs on its own private installation copy, so the full run schedules them independently; the read-only suites
    /// share the repository installation.
    /// </summary>
    internal static RegisteredSuite[] IntroCinematicArtworkSuites() =>
    [
        new(nameof(VerifyGoldenTorizoCartridgeCombatFallback), () => VerifyGoldenTorizoCartridgeCombatFallback(RepositoryRomPath, RepositoryInstallation.Installation)),
        new(nameof(VerifySamusBodyArtwork), VerifyInstalledSamusBodyArtwork),
        new(nameof(VerifyIntroCinematicArtwork), () => VerifyIntroCinematicArtwork(RepositoryRomPath)),
        new(nameof(VerifyCeresCinematicArtwork), () => VerifyCeresCinematicArtwork(RepositoryRomPath)),
        new(nameof(VerifyEndingArtwork), () => VerifyEndingArtwork()),
    ];

    /// <summary>Runs the Samus body artwork checks against the repository installation.</summary>
    private static void VerifyInstalledSamusBodyArtwork() =>
        VerifySamusBodyArtwork(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath));

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
        Suite(nameof(VerifySamusBodyArtwork), VerifyInstalledSamusBodyArtwork);
        return 0;
    }

    /// <summary>The installed opening palette's colours as the colour words the cartridge stores them in (#1512).</summary>
    private static byte[] IntroPaletteWords(IntroCinematicPalette palette) =>
        ScreenColourWords.Words(palette.BackgroundColours.AsSpan(), palette.SpriteColours.AsSpan());
}
