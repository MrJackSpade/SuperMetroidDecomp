using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingMode7ArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>Checks installed and edited reward-icon artwork through native partial VRAM uploads and the ending shot.</summary>
internal static class EndingRewardIconArtworkVerification
{
    /// <summary>Compares retail pixels and VRAM, then checks map and PNG override rebinds during the landing upload.</summary>
    internal static void VerifyEndingRewardIconArtwork(GameInstallation installation)
    {
        EndingFloorArtwork stock = installation.LoadEndingMode7Art();
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var source = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        byte[] native = RomDataReader.Decompress(source,
            EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters,
            EndingSourceDefinitions.Artwork.DecompressionLimit);
        AssertTrue(stock.RewardIcon.Bytes.Span.SequenceEqual(native.AsSpan(0,
                EndingRewardIconArtworkFormat.ByteCount)),
            "reward icon JSON/PNG preserves all interleaved native map and character bytes");

        var nativeBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        var installedBus = NewEndingMode7SourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
        var nativeAudio = new AudioQueues();
        var installedAudio = new AudioQueues();
        var nativeScene = CreateRetailEndingFixture(nativeAudio, 3, 0);
        var installedScene = CreateRetailEndingFixture(installedAudio, 3, 0);
        installedScene.Stage.BindFloorArtwork(stock);
        int jumpFrames = 0;
        for (int frame = 0; frame < 60_000 &&
            nativeScene.Stage.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
        {
            AssertEqual(nativeScene.Stage.Phase, installedScene.Stage.Phase,
                $"reward icon installed/native phase at frame {frame}");
            if (nativeScene.Stage.Phase == EndingCreditsPhase.PostCreditsJump)
            {
                jumpFrames++;
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(nativeScene);
                byte[] expectedVram = nativeScene.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installedScene);
                byte[] actualVram = installedScene.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actual.Paint.Colors.SequenceEqual(expected.Paint.Colors) &&
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
                    $"reward icon preserves native partial-upload VRAM and pixels at jump frame {jumpFrames}");
            }
            nativeScene.Step();
            installedScene.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.PostCreditsShot, nativeScene.Stage.Phase,
            "native reward reaches Mode-7 shot");
        AssertEqual(EndingCreditsPhase.PostCreditsShot, installedScene.Stage.Phase,
            "installed reward reaches Mode-7 shot");
        AssertTrue(jumpFrames >= 200,
            "reward icon integration covers the full native jump and landing upload interval");
        AssertEqual(0, installedBus.ForbiddenReadAttempts,
            "installed reward icon never rereads its compressed cartridge stream");
        AssertTrue(installedScene.ShownVram()
                [..EndingRewardIconArtworkFormat.ByteCount].SequenceEqual(
                    stock.RewardIcon.Bytes.Span),
            "all sixteen native landing chunks reconstruct the installed icon image");

        Directory.CreateDirectory(installation.EndingMode7OverrideDirectory);
        foreach (string name in new[]
        {
            EndingRewardIconArtworkFormat.MapFileName,
            EndingRewardIconArtworkFormat.CharacterFileName,
        })
        {
            string overridePath = Path.Combine(installation.EndingMode7OverrideDirectory, name);
            try
            {
                if (name == EndingRewardIconArtworkFormat.MapFileName)
                {
                    using var output = File.Create(overridePath);
                    EndingRewardIconArtwork.WriteMap(output, new EndingFloorMapDocument
                    {
                        Version = EndingFloorArtworkFormat.Version,
                        Width = EndingRewardIconArtworkFormat.MapWidth,
                        Height = EndingRewardIconArtworkFormat.MapHeight,
                        Tiles = new int[EndingRewardIconArtworkFormat.MapBytes],
                    });
                }
                else
                {
                    IndexedPngImage image;
                    using (var input = File.OpenRead(Path.Combine(
                               installation.EndingMode7Directory, name)))
                        image = IndexedPng.Read(input, new(128, 128));
                    Array.Fill(image.Pixels, (byte)0);
                    using var output = File.Create(overridePath);
                    IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                        image.Pixels, image.Colours);
                }
                EndingFloorArtwork edited = installation.LoadEndingMode7Art();
                var uploadBus = NewEndingMode7SourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
                var partial = new EndingScreen();
                for (int chunk = 0; chunk < 8; chunk++)
                    partial.LoadRewardChunk(chunk);
                byte[] partialVram = EndingScreenMemory.Compose(partial, edited, null, null, null, EndingReward.Armored);
                AssertTrue(partialVram[..(8 * EndingRewardGraphicsUploadDefinitions.ChunkBytes)].AsSpan()
                        .SequenceEqual(edited.RewardIcon.Bytes.Span
                            [..(8 * EndingRewardGraphicsUploadDefinitions.ChunkBytes)]) &&
                        partialVram[(8 * EndingRewardGraphicsUploadDefinitions.ChunkBytes)..]
                            .All(value => value == 0),
                    $"{name} rebinds eight completed chunks without writing pending ones");
                AssertEqual(0, uploadBus.ForbiddenReadAttempts,
                    $"{name} partial upload never reads compressed ROM icon art");
                var sceneBus = NewEndingMode7SourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
                var sceneAudio = new AudioQueues();
                var scene = CreateRetailEndingFixture(sceneAudio, 3, 0);
                scene.Stage.BindFloorArtwork(stock);
                for (int frame = 0; frame < 60_000 &&
                    scene.Stage.Phase != EndingCreditsPhase.PostCreditsJump; frame++)
                {
                    scene.Step();
                    sceneAudio.AdvanceFrame(default);
                }
                AssertEqual(EndingCreditsPhase.PostCreditsJump, scene.Stage.Phase,
                    $"{name} override reaches the native reward jump");
                for (int frame = 0; frame < 190; frame++)
                {
                    scene.Step();
                    sceneAudio.AdvanceFrame(default);
                }
                AssertEqual(EndingCreditsPhase.PostCreditsJump, scene.Stage.Phase,
                    $"{name} override fixture remains in the native landing upload");
                LayeredScene before = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                byte[] beforeVram = scene.ShownVram();
                scene.Stage.BindFloorArtwork(edited);
                // The ending draws its Mode 7 characters from the sheets the edited art composes (#1392).
                scene.InstalledArtwork.BindSpriteSheets(EndingSheetFixtures.WithEndingMode7(edited));
                LayeredScene rebound = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                byte[] reboundVram = scene.ShownVram();
                AssertTrue(scene.Stage.Phase == EndingCreditsPhase.PostCreditsJump &&
                        before.Paint.Colors.SequenceEqual(rebound.Paint.Colors) &&
                        !beforeVram.SequenceEqual(reboundVram),
                    $"{name} rebind replaces completed chunks without resetting the jump");
                for (int frame = 0; frame < 400 &&
                    scene.Stage.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
                {
                    scene.Step();
                    sceneAudio.AdvanceFrame(default);
                }
                AssertEqual(EndingCreditsPhase.PostCreditsShot, scene.Stage.Phase,
                    $"{name} edited icon reaches the native shooting phase");
                var shownScene = scene.ShownVram();
                AssertTrue(shownScene
                        [..EndingRewardIconArtworkFormat.ByteCount]
                        .SequenceEqual(edited.RewardIcon.Bytes.Span),
                    $"{name} supplies every final native upload byte");
                LayeredScene baseline = EndingCreditsPresentation.CaptureRenderSnapshot(installedScene);
                LayeredScene changed = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                AssertTrue(!DisplayRendering.Render(baseline)
                        .SequenceEqual(
                            DisplayRendering.Render(changed)),
                    $"edited {name} changes visible Mode-7 reward-shot pixels");
                AssertEqual(0, sceneBus.ForbiddenReadAttempts,
                    $"edited {name} never rereads cartridge icon graphics");
            }
            finally
            {
                File.Delete(overridePath);
            }
        }
        foreach (string name in new[]
        {
            EndingRewardIconArtworkFormat.MapFileName,
            EndingRewardIconArtworkFormat.CharacterFileName,
        })
        {
            string invalid = Path.Combine(installation.EndingMode7OverrideDirectory, name);
            File.WriteAllBytes(invalid, [0]);
            try
            {
                AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
                    $"malformed reward icon override {name} fails loudly");
            }
            finally
            {
                File.Delete(invalid);
            }
        }
        string userPng = Path.Combine(installation.EndingMode7OverrideDirectory,
            EndingRewardIconArtworkFormat.CharacterFileName);
        IndexedPngImage persistentImage;
        using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory,
                   EndingRewardIconArtworkFormat.CharacterFileName)))
            persistentImage = IndexedPng.Read(input, new(128, 128));
        persistentImage.Pixels[0] = (byte)(persistentImage.Pixels[0] ^ 1);
        using (var output = File.Create(userPng))
            IndexedPng.Write(output, new(persistentImage.Size.X, persistentImage.Size.Y),
                persistentImage.Pixels, persistentImage.Colours);
        byte[] selected = installation.LoadEndingMode7Art().RewardIcon.Bytes.ToArray();
        string stockMap = Path.Combine(installation.EndingMode7Directory,
            EndingRewardIconArtworkFormat.MapFileName);
        File.WriteAllBytes(stockMap, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "an icon override cannot mask corrupt installed stock map JSON");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Reward icon content vanished during stock repair.");
        AssertTrue(repaired.LoadEndingMode7Art().RewardIcon.Bytes.Span.SequenceEqual(selected),
            "stock icon repair preserves the player's separately stored PNG override");
        File.Delete(userPng);
        Console.WriteLine("Reward icon: native sixteen-chunk VRAM/pixel parity, mid-upload rebind and independently visible map/PNG edits pass.");
    }
}
