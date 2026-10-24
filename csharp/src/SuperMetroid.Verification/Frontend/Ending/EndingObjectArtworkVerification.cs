using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Verification.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingCompletionTextInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingCompletionTextSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingExplosionSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardInstructionsVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingRewardSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Frontend.Ending.PostCreditsCharacterArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks installed ending actors and sheets against cartridge sources and guarded runtime use.</summary>
internal static class EndingObjectArtworkVerification
{
    /// <summary>Matches four explosion-fragment source pointers and filenames to their native loader operands.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending object fragment metadata")]
    internal static void VerifyEndingObjectFragmentMetadata(ISnesAddressSpace bus)
    {
        string[] names = ["ending-explosion-fragment-70.png", "ending-explosion-fragment-74.png",
            "ending-explosion-fragment-78.png", "ending-explosion-fragment-7c.png"];
        AssertEqual(names.Length, EndingObjectArtworkFormat.FragmentCount, "ending fragment count");
        for (int index = 0; index < names.Length; index++)
        {
            int load = 0x8bd5af + 17 * index;
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(load), "native fragment bank LDA");
            AssertEqual((byte)0xa9, bus.ReadCartridgeByte(load + 5), "native fragment address LDA");
            int source = (int)new SnesAddress(bus.ReadCartridgeByte(load + 2), CartridgeImportSource.Require(bus).ReadCartridgeWord(load + 6));
            AssertEqual(source, EndingCreditsRomData.Assets.ObjectFragmentSource((EndingObjectFragmentId)index), "native fragment compressed source");
            AssertEqual(names[index], EndingObjectArtworkFormat.FragmentFileName(index), "published fragment filename");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingObjectArtworkFormat.FragmentFileName(invalid), "fragment filename bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingCreditsRomData.Assets.ObjectFragmentSource((EndingObjectFragmentId)invalid), "fragment source bounds");
        }
    }

    /// <summary>Compares the six cloud compositions and grids with native OAM.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending cloud definitions")]
    internal static void VerifyEndingCloudDefinitions(ISnesAddressSpace bus)
    {
        string[] names = ["scene-b-upper-a", "scene-b-upper-b", "scene-b-lower-a", "scene-b-lower-b", "scene-a-right", "scene-a-left"];
        var catalog = EndingCloudSpriteDefinitions.Frames;
        AssertEqual(6, catalog.Count, "cloud catalog count");
        for (int index = 0; index < catalog.Count; index++)
        {
            int operand = 0x8becef + index * 8;
            ushort map = CartridgeImportSource.Require(bus).ReadCartridgeWord(operand);
            int header = 0x8c0000 | map;
            int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(header);
            AssertEqual(map, CinematicSpritemapSources.Of(catalog[index].Pose), "cloud native pointer operand");
            AssertEqual(count, catalog[index].StockPartCount, "cloud native OAM count");
            AssertEqual(names[index], catalog[index].Name, "cloud published asset key");
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus, map, count, names[index]);
            SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, names[index]);
            var grid = new EndingCloudGridParts((EndingCloudSpriteDefinitions.Role)index);
            SpriteComposition calculated = supplied.CalculateIfMatching(grid);
            AssertTrue(!ReferenceEquals(supplied, calculated), "all original cloud parts use calculated grid");
            string Identity(SpriteComposition value) => SelectedPresentationHash.Create("cloud", value.AppendIdentity);
            AssertEqual(Identity(supplied), Identity(calculated), "cloud grid complete visual identity");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var originalOam = new OamBuffer();
                var calculatedOam = new OamBuffer();
                originalOam.BeginFrame(); calculatedOam.BeginFrame();
                DrawImportedSpritemap(bus, originalOam, header, new(120, y), 0x0800, originIsOnScreen: y == 72);
                if (y == 72) calculated.DrawOnScreen(calculatedOam, new(120, y), 0x0800);
                else calculated.DrawOffScreen(calculatedOam, new(120, y), 0x0800);
                originalOam.FinalizeFrame(); calculatedOam.FinalizeFrame();
                AssertTrue(originalOam.ImageEquals(calculatedOam),
                    "cloud grid preserves native OAM and clipping");
            }
            SpriteVisualPart first = visual[0];
            foreach (SpriteVisualPart edit in new[]
            {
                first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
                first with { TileColumn = first.TileColumn + 1 }, first with { TileRow = first.TileRow + 1 },
                first with { Size = 16 }, first with { Priority = 2 }, first with { Palette = 3 },
                first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
            })
            {
                visual[0] = edit;
                SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(visual, "edited cloud");
                AssertTrue(ReferenceEquals(edited, edited.CalculateIfMatching(grid)), "independent cloud edits stay supplied");
            }
            foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = grid[invalid], "cloud grid part bounds");
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "cloud catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[invalid], "cloud catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = new EndingCloudGridParts((EndingCloudSpriteDefinitions.Role)invalid), "cloud grid role bounds");
        }
    }

    /// <summary>Checks ending OBJ sheets, sprite actors, backgrounds, edits, and guarded runtime reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending object sheets and BG maps")]
    internal static void VerifyEndingObjectArtwork(GameInstallation installation)
    {
        EndingObjectArtworkCatalog stock = installation.LoadEndingObjectArt();
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        Suite(nameof(VerifyEndingObjectFragmentMetadata), () => VerifyEndingObjectFragmentMetadata(bus));
        AssertSheet(stock.Clouds, EndingCreditsRomData.Assets.EscapeCloudCharacters,
            EndingObjectArtworkFormat.CloudByteCount, "clouds");
        AssertSheet(stock.Explosion, EndingCreditsRomData.Assets.EndingObjectCharacters,
            EndingObjectArtworkFormat.ExplosionByteCount, "explosion");
        for (int index = 0; index < EndingObjectArtworkFormat.FragmentCount; index++)
            AssertSheet(stock.Fragment((EndingObjectFragmentId)index),
                EndingCreditsRomData.Assets.ObjectFragmentSource((EndingObjectFragmentId)index), EndingObjectArtworkFormat.FragmentByteCount,
                $"explosion fragment {index}");
        AssertSheet(stock.WaitingSamus,
            EndingCreditsRomData.Assets.WaitingForCreditsCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "waiting Samus");
        AssertSheet(stock.ShootingScreen,
            EndingCreditsRomData.Assets.ShootingScreenCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "shooting screen");
        AssertSheet(stock.SuitlessSamus,
            EndingCreditsRomData.Assets.SuitlessSamusCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "suitless Samus");
        byte[] nativeWaitingMap = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.WaitingForCreditsTilemap,
            EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        AssertTrue(stock.WaitingTilemap.Bytes.Span.SequenceEqual(
                nativeWaitingMap.AsSpan(0, EndingObjectArtworkFormat.WaitingTilemapByteCount)),
            "installed waiting-Samus map preserves all native BG2 tile words");
        AssertSheet(stock.PostCreditsFragmentA,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentA,
            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount,
            "post-credits fragment A");
        AssertSheet(stock.PostCreditsFragmentB,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentB,
            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount,
            "post-credits fragment B");
        AssertSheet(stock.PostShotLogoTiles, EndingPostShotDefinitions.LogoTiles,
            EndingObjectArtworkFormat.PostShotLogoTileByteCount,
            "post-shot logo tiles");
        byte[] nativeLogoMap = RomDataReader.Decompress(bus,
            EndingPostShotDefinitions.LogoMap,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        AssertTrue(stock.PostShotLogoMap.Bytes.Span.SequenceEqual(
                nativeLogoMap.AsSpan(0, EndingObjectArtworkFormat.PostShotLogoMapByteCount)),
            "installed post-shot logo map preserves all native BG2 tile words");

        Suite(nameof(VerifyEndingCloudDefinitions), () => VerifyEndingCloudDefinitions(bus));
        for (int index = 0; index < EndingCloudSpriteDefinitions.Frames.Count; index++)
        {
            CinematicSpriteFrameDefinition definition = EndingCloudSpriteDefinitions.Frames[index];
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask)) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(definition.Pose));
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = ScreenSpritesFixtures.Blank();
                installedOam.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.CloudSprites).Draw(definition.Pose, installedOam,
                    new(120, y), 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending cloud {definition.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
        Suite(nameof(VerifyEndingExplosionActorArtwork), () => VerifyEndingExplosionActorArtwork(bus, stock));
        Suite(nameof(VerifyEndingCompletionTextInstructions), () => VerifyEndingCompletionTextInstructions(bus));
        Suite(nameof(VerifyEndingCompletionTextSpriteArtwork), () => VerifyEndingCompletionTextSpriteArtwork(bus, stock));
        Suite(nameof(VerifyEndingRewardInstructions), () => VerifyEndingRewardInstructions(bus));
        Suite(nameof(VerifyEndingRewardSpriteArtwork), () => VerifyEndingRewardSpriteArtwork(bus, stock));
        Suite(nameof(VerifyEndingLogoSpriteArtwork), () => VerifyEndingLogoSpriteArtwork(bus, stock));

        var guard = NewEndingObjectSourceReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        installed.InstalledArtwork.BindFixtureObjectArtwork(stock);
        var phases = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 12000 &&
            native.Stage.Phase != EndingCreditsPhase.FadeOutToCredits; frame++)
        {
            AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                $"installed ending OBJ art preserves phase at frame {frame}");
            if (phases.Add(native.Stage.Phase) || frame % 211 == 0)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actual.Paint.Colors.SequenceEqual(expected.Paint.Colors) &&
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
                    $"ending OBJ PNGs preserve native VRAM/pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.FadeOutToCredits, native.Stage.Phase,
            "ending OBJ fixture reaches the completed-time credits handoff");
        AssertEqual(EndingCreditsPhase.FadeOutToCredits, installed.Stage.Phase,
            "installed OBJ art retains the native completed-time credits handoff");
        AssertTrue(phases.Contains(EndingCreditsPhase.WaitForEscapeMusic) &&
                phases.Contains(EndingCreditsPhase.FadeInEscapeSceneB) &&
                phases.Contains(EndingCreditsPhase.FadeInZebesExplosion) &&
                phases.Contains(EndingCreditsPhase.PlanetEscapeFast) &&
                phases.Contains(EndingCreditsPhase.OperationSuccessfulText),
            "ending clouds, explosion and completion text were exercised in their actual scenes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed ending OBJ scenes never reread compiled instructions, sprite maps or character sources");

        Suite(nameof(VerifyEndingExplosionVisualOverride), () => VerifyEndingExplosionVisualOverride(installation, stock, guard));
        Suite(nameof(VerifyEndingCompletionTextVisualOverride), () => VerifyEndingCompletionTextVisualOverride(installation, stock, guard));
        Suite(nameof(VerifyEndingRewardVisualOverride), () => VerifyEndingRewardVisualOverride(installation, stock, guard));

        string cloudSpriteName = EndingCloudSpriteFormat.FileName;
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        string cloudSpritePath = Path.Combine(installation.EndingObjectDirectory, cloudSpriteName);
        string cloudSpriteOverride = Path.Combine(
            installation.EndingObjectOverrideDirectory, cloudSpriteName);
        EndingCloudSpriteDocument cloudDocument =
            JsonSerializer.Deserialize<EndingCloudSpriteDocument>(
                File.ReadAllBytes(cloudSpritePath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending cloud sprite JSON is empty.");
        cloudDocument.Frames["scene-a-right"] = cloudDocument.Frames["scene-a-right"]
            .Select(part => part with { OffsetX = part.OffsetX + 8 }).ToArray();
        using (var output = File.Create(cloudSpriteOverride))
            EndingCloudSpritePresentation.Write(output, cloudDocument);
        byte[] editedCloudSpriteBytes = File.ReadAllBytes(cloudSpriteOverride);
        EndingObjectArtworkCatalog editedCloudSprites = installation.LoadEndingObjectArt();
        var cloudAudio = new CartridgeAudioState();
        var cloudScene = CreateRetailEndingFixture(cloudAudio, 0, 0);
        cloudScene.InstalledArtwork.BindFixtureObjectArtwork(stock);
        for (int frame = 0; frame < 2000 &&
            cloudScene.Stage.Phase != EndingCreditsPhase.EscapeSceneA; frame++)
        {
            cloudScene.Step();
            cloudAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.EscapeSceneA, cloudScene.Stage.Phase,
            "ending cloud edit reaches visible first atmospheric scene");
        bool visibleCloudEdit = false;
        for (int frame = 0; frame < 700 &&
            cloudScene.Stage.Phase < EndingCreditsPhase.FadeInEscapeSceneB &&
            !visibleCloudEdit; frame++)
        {
            cloudScene.InstalledArtwork.BindFixtureObjectArtwork(stock);
            LayeredScene beforeCloudEdit = EndingCreditsPresentation.CaptureRenderSnapshot(cloudScene);
            cloudScene.InstalledArtwork.BindFixtureObjectArtwork(editedCloudSprites);
            LayeredScene afterCloudEdit = EndingCreditsPresentation.CaptureRenderSnapshot(cloudScene);
            visibleCloudEdit = !beforeCloudEdit.Paint.SpriteParts().SequenceEqual(
                    afterCloudEdit.Paint.SpriteParts()) &&
                !DisplayRendering.Render(beforeCloudEdit).SequenceEqual(
                    DisplayRendering.Render(afterCloudEdit));
            if (!visibleCloudEdit)
            {
                cloudScene.Step();
                cloudAudio.AdvanceFrame(default);
            }
        }
        AssertTrue(visibleCloudEdit,
            "edited cloud composition changes live OAM and visible ending pixels");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "cloud art rebind never rereads installed bank-$8C sprite maps");
        cloudDocument.Frames.Remove("scene-a-right");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            EndingCloudSpritePresentation.Write(invalid, cloudDocument);
        }, "ending cloud sprites reject missing named art");
        File.Delete(cloudSpriteOverride);

        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        string[] names =
        [
            EndingObjectArtworkFormat.CloudFileName,
            EndingObjectArtworkFormat.ExplosionFileName,
            .. Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
                .Select(EndingObjectArtworkFormat.FragmentFileName),
        ];
        for (int resource = 0; resource < names.Length; resource++)
        {
            string name = names[resource];
            int byteCount = resource switch
            {
                0 => EndingObjectArtworkFormat.CloudByteCount,
                1 => EndingObjectArtworkFormat.ExplosionByteCount,
                _ => EndingObjectArtworkFormat.FragmentByteCount,
            };
            var sceneBus = NewEndingObjectSourceReadGuard(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
            var audio = new CartridgeAudioState();
            var scene = CreateRetailEndingFixture(audio, 0, 0);
            scene.InstalledArtwork.BindFixtureObjectArtwork(stock);
            EndingCreditsPhase target = resource == 0
                ? EndingCreditsPhase.WaitForEscapeMusic :
                EndingCreditsPhase.FadeInZebesExplosion;
            for (int frame = 0; frame < 10000 && scene.Stage.Phase != target; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(default);
            }
            AssertEqual(target, scene.Stage.Phase, $"ending {name} reaches its actual scene");
            for (int frame = 0; frame < 1200 && scene.Stage.Brightness == 0; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(default);
            }
            LayeredScene baseline = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
            byte[] baselineVram = scene.ShownVram();
            EndingCreditsPhase baselinePhase = scene.Stage.Phase;
            AssertTrue(baseline.Brightness > 0,
                $"ending {name} reaches a visible override frame");

            int tiles = byteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int rows = tiles / RoomCharacterAtlasFormat.TileColumns;
            string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.EndingObjectDirectory, name)))
                image = IndexedPng.Read(input,
                    new(RoomCharacterAtlasFormat.TileColumns * 8, checked((ushort)(rows * 8))));
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            scene.InstalledArtwork.BindFixtureObjectArtwork(edited);
            LayeredScene changed = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
            byte[] changedVram = scene.ShownVram();
            AssertTrue(!baselineVram.SequenceEqual(changedVram) &&
                    baselineVram[..(EndingVramLayout.ExplosionObjects.Offset * 2)]
                        .SequenceEqual(changedVram
                            [..(EndingVramLayout.ExplosionObjects.Offset * 2)]) &&
                    baseline.Paint.Colors.SequenceEqual(changed.Paint.Colors) &&
                    scene.Stage.Phase == baselinePhase,
                $"edited {name} changes OBJ VRAM without touching Mode-7 graphics or CGRAM");
            bool visibleEdit = false;
            for (int frame = 0; frame < 2400 && !visibleEdit &&
                scene.Stage.Phase < (resource == 0
                    ? EndingCreditsPhase.FadeInZebesExplosion :
                    EndingCreditsPhase.PlanetEscapeFast); frame++)
            {
                if (frame % 6 == 0)
                {
                    scene.InstalledArtwork.BindFixtureObjectArtwork(stock);
                    LayeredScene nativeArt = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                    scene.InstalledArtwork.BindFixtureObjectArtwork(edited);
                    LayeredScene editedArt = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                    visibleEdit = !DisplayRendering.Render(nativeArt)
                        .SequenceEqual(
                            DisplayRendering.Render(editedArt));
                }
                scene.Step();
                audio.AdvanceFrame(default);
            }
            AssertTrue(visibleEdit, $"edited {name} changes visible ending pixels");
            File.Delete(overridePath);
            AssertEqual(0, sceneBus.ForbiddenReadAttempts,
                $"edited/rebound {name} does not reread cartridge OBJ sources");
        }

        string invalidPath = Path.Combine(installation.EndingObjectOverrideDirectory,
            EndingObjectArtworkFormat.CloudFileName);
        File.WriteAllBytes(invalidPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingObjectArt(),
            "malformed ending OBJ override fails loudly");
        File.Delete(invalidPath);
        IndexedPngImage persistentClouds;
        using (var input = File.OpenRead(Path.Combine(installation.EndingObjectDirectory,
                   EndingObjectArtworkFormat.CloudFileName)))
            persistentClouds = IndexedPng.Read(input, new(256, 128));
        persistentClouds.Pixels[0] = (byte)((persistentClouds.Pixels[0] + 1) & 15);
        using (var output = File.Create(invalidPath))
            IndexedPng.Write(output, new(persistentClouds.Size.X, persistentClouds.Size.Y),
                persistentClouds.Pixels, persistentClouds.Colours);
        byte[] editedClouds = installation.LoadEndingObjectArt().Clouds.Characters.ToArray();
        string stockExplosion = Path.Combine(installation.EndingObjectDirectory,
            EndingObjectArtworkFormat.ExplosionFileName);
        File.WriteAllBytes(stockExplosion, [0]);
        File.WriteAllBytes(cloudSpriteOverride, editedCloudSpriteBytes);
        File.WriteAllBytes(cloudSpritePath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingObjectArt(),
            "valid ending OBJ overrides cannot hide corrupt stock art or sprite JSON");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ending OBJ content vanished during stock repair.");
        AssertTrue(repaired.LoadEndingObjectArt().Clouds.Characters.Span.SequenceEqual(editedClouds) &&
                repaired.LoadEndingObjectArt().Explosion.Characters.Span.SequenceEqual(
                    stock.Explosion.Characters.Span),
            "stock OBJ repair restores native pixels and preserves the player's cloud override");
        var preservedCloudOam = ScreenSpritesFixtures.Blank();
        preservedCloudOam.BeginFrame();
        CinematicSheetFixtures.Drawer(repaired.LoadEndingObjectArt().CloudSprites).Draw(EndingCloudSpriteDefinitions.Frames[(int)EndingCloudSpriteDefinitions.Role.Right].Pose, preservedCloudOam,
            new(120, 72), 0x0800, originIsOnScreen: true);
        preservedCloudOam.FinalizeFrame();
        var editedCloudOam = ScreenSpritesFixtures.Blank();
        editedCloudOam.BeginFrame();
        CinematicSheetFixtures.Drawer(editedCloudSprites.CloudSprites).Draw(EndingCloudSpriteDefinitions.Frames[(int)EndingCloudSpriteDefinitions.Role.Right].Pose, editedCloudOam,
            new(120, 72), 0x0800, originIsOnScreen: true);
        editedCloudOam.FinalizeFrame();
        AssertTrue(preservedCloudOam.ImageEquals(editedCloudOam),
            "ending cloud sprite override survives stock repair with edited OAM");
        File.Delete(invalidPath);
        File.Delete(cloudSpriteOverride);
        Suite(nameof(VerifyPostCreditsCharacterArtwork), () => VerifyPostCreditsCharacterArtwork(repaired));
        Console.WriteLine("Ending/credits art: compiled cloud, explosion, completion-text, reward and logo actors, editable cloud/explosion/text/reward/logo OAM, twelve native sheets and two BG maps, visible independent edits, guarded runtime and stock repair pass.");

        void AssertSheet(RoomCharacterAtlas sheet, int source, int bytes, string name)
        {
            byte[] native = RomDataReader.Decompress(bus, source,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            AssertTrue(sheet.Characters.Span.SequenceEqual(native.AsSpan(0, bytes)),
                $"installed ending {name} PNG preserves every native OBJ byte");
        }
    }

    /// <summary>Forbids runtime reads of compiled ending lists, installed spritemaps, and extracted art sources.</summary>
    internal static MutableCartridgeReadGuard NewEndingObjectSourceReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(CinematicSpriteNativeListAddresses.InListRegion,
            address => $"Ending actor reread native cinematic sprite list ${address:X6}.");
        guard.Forbid(address => EndingCloudSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending cloud reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingExplosionSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending explosion reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingCompletionTextSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending completion text reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingRewardSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending reward reread installed spritemap ${address:X6}.");
        guard.Forbid(address => EndingLogoSpriteDefinitions.Frames.Any(frame =>
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(frame.Pose));
                    return address >= start && address < start + 2 + frame.StockPartCount * 5;
                }),
            address => $"Ending logo reread installed spritemap ${address:X6}.");
        guard.Forbid(address => address is EndingCreditsRomData.Assets.EscapeCloudCharacters or
                EndingCreditsRomData.Assets.EndingObjectCharacters or
                EndingCreditsRomData.Assets.EndingObjectCharacters70 or
                EndingCreditsRomData.Assets.EndingObjectCharacters74 or
                EndingCreditsRomData.Assets.EndingObjectCharacters78 or
                EndingCreditsRomData.Assets.EndingObjectCharacters7C or
                EndingCreditsRomData.Assets.WaitingForCreditsCharacters or
                EndingCreditsRomData.Assets.ShootingScreenCharacters or
                EndingCreditsRomData.Assets.SuitlessSamusCharacters or
                EndingCreditsRomData.Assets.WaitingForCreditsTilemap or
                EndingCreditsRomData.Assets.PostCreditsTileFragmentA or
                EndingCreditsRomData.Assets.PostCreditsTileFragmentB or
                EndingCreditsRomData.Assets.PostCreditsFloorCharacters or
                EndingPostShotDefinitions.LogoTiles or
                EndingPostShotDefinitions.LogoMap,
            address => $"Ending OBJ reread cartridge source ${address:X6}.");
        return guard;
    }
}
