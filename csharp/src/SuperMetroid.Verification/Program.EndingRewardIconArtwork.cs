using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingRewardIconArtwork(GameInstallation installation)
    {
        EndingMode7ArtworkCatalog stock = installation.LoadEndingMode7Art();
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var source = new SuperMetroidAddressSpace(rom);
        byte[] native = RomDataReader.Decompress(source,
            EndingCreditsRomData.Assets.PostCreditsMode7Characters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        AssertTrue(stock.RewardIcon.Transfer.Span.SequenceEqual(native.AsSpan(0,
                EndingRewardIconArtworkFormat.TransferBytes)),
            "reward icon JSON/PNG preserves all interleaved native map and character bytes");
        CreditsPresentation credits = CreditsPresentation.Load(new MemoryStream(
            CreditsPresentationExtractor.Extract(source)));

        var nativeBus = new SuperMetroidAddressSpace(rom);
        var installedBus = new EndingMode7SourceReadGuard(new SuperMetroidAddressSpace(rom));
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var nativeScene = new EndingCreditsState(nativeBus, nativeAudio, 3, 0);
        var installedScene = new EndingCreditsState(installedBus, installedAudio, 3, 0);
        nativeScene.BindStaffCredits(credits);
        installedScene.BindStaffCredits(credits);
        installedScene.BindMode7Artwork(stock);
        int jumpFrames = 0;
        for (int frame = 0; frame < 60_000 &&
            nativeScene.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
        {
            AssertEqual(nativeScene.Phase, installedScene.Phase,
                $"reward icon installed/native phase at frame {frame}");
            if (nativeScene.Phase == EndingCreditsPhase.PostCreditsJump)
            {
                jumpFrames++;
                LayeredRenderSnapshot expected = nativeScene.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installedScene.CaptureRenderSnapshot();
                AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(actual).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(expected)),
                    $"reward icon preserves native partial-upload VRAM and pixels at jump frame {jumpFrames}");
            }
            nativeScene.Step();
            installedScene.Step();
            nativeAudio.AdvanceFrame(nativeBus, default);
            installedAudio.AdvanceFrame(installedBus, default);
        }
        AssertEqual(EndingCreditsPhase.PostCreditsShot, nativeScene.Phase,
            "native reward reaches Mode-7 shot");
        AssertEqual(EndingCreditsPhase.PostCreditsShot, installedScene.Phase,
            "installed reward reaches Mode-7 shot");
        AssertTrue(jumpFrames >= 200,
            "reward icon integration covers the full native jump and landing upload interval");
        AssertEqual(0, installedBus.ForbiddenReadAttempts,
            "installed reward icon never rereads its compressed cartridge stream");
        AssertTrue(installedScene.CaptureRenderSnapshot().Memory.Vram
                [..EndingRewardIconArtworkFormat.TransferBytes].SequenceEqual(
                    stock.RewardIcon.Transfer.Span),
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
                    EndingRewardIconArtwork.WriteMap(output, new EndingMode7MapDocument
                    {
                        Version = EndingMode7ArtworkFormat.Version,
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
                        image = IndexedPng.Read(input, 128, 128);
                    Array.Fill(image.Pixels, (byte)0);
                    using var output = File.Create(overridePath);
                    IndexedPng.Write(output, image.Width, image.Height,
                        image.Pixels, image.Palette);
                }
                EndingMode7ArtworkCatalog edited = installation.LoadEndingMode7Art();
                var uploadBus = new EndingMode7SourceReadGuard(new SuperMetroidAddressSpace(rom));
                var upload = new EndingRewardGraphicsUpload(uploadBus, stock.RewardIcon);
                var partialVram = new SnesVram();
                partialVram.LoadBytes(0, Enumerable.Repeat((byte)0xa5,
                    SnesVram.ByteCount).ToArray());
                for (int chunk = 0; chunk < 8; chunk++)
                    upload.Upload(partialVram, chunk);
                upload.BindArtwork(edited.RewardIcon, partialVram);
                AssertTrue(partialVram.Bytes[..(8 * EndingRewardGraphicsUploadDefinitions.ChunkBytes)]
                        .SequenceEqual(edited.RewardIcon.Transfer.Span
                            [..(8 * EndingRewardGraphicsUploadDefinitions.ChunkBytes)]) &&
                        partialVram.Bytes[(8 * EndingRewardGraphicsUploadDefinitions.ChunkBytes)..]
                            .ToArray().All(value => value == 0xa5),
                    $"{name} rebinds eight completed chunks without touching pending VRAM");
                AssertEqual(0, uploadBus.ForbiddenReadAttempts,
                    $"{name} partial upload never reads compressed ROM icon art");
                var sceneBus = new EndingMode7SourceReadGuard(new SuperMetroidAddressSpace(rom));
                var sceneAudio = new CartridgeAudioState();
                var scene = new EndingCreditsState(sceneBus, sceneAudio, 3, 0);
                scene.BindStaffCredits(credits);
                scene.BindMode7Artwork(stock);
                for (int frame = 0; frame < 60_000 &&
                    scene.Phase != EndingCreditsPhase.PostCreditsJump; frame++)
                {
                    scene.Step();
                    sceneAudio.AdvanceFrame(sceneBus, default);
                }
                AssertEqual(EndingCreditsPhase.PostCreditsJump, scene.Phase,
                    $"{name} override reaches the native reward jump");
                for (int frame = 0; frame < 190; frame++)
                {
                    scene.Step();
                    sceneAudio.AdvanceFrame(sceneBus, default);
                }
                AssertEqual(EndingCreditsPhase.PostCreditsJump, scene.Phase,
                    $"{name} override fixture remains in the native landing upload");
                LayeredRenderSnapshot before = scene.CaptureRenderSnapshot();
                scene.BindMode7Artwork(edited);
                LayeredRenderSnapshot rebound = scene.CaptureRenderSnapshot();
                AssertTrue(scene.Phase == EndingCreditsPhase.PostCreditsJump &&
                        before.Memory.Cgram.SequenceEqual(rebound.Memory.Cgram) &&
                        !before.Memory.Vram.SequenceEqual(rebound.Memory.Vram),
                    $"{name} rebind replaces completed chunks without resetting the jump");
                for (int frame = 0; frame < 400 &&
                    scene.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
                {
                    scene.Step();
                    sceneAudio.AdvanceFrame(sceneBus, default);
                }
                AssertEqual(EndingCreditsPhase.PostCreditsShot, scene.Phase,
                    $"{name} edited icon reaches the native shooting phase");
                AssertTrue(scene.CaptureRenderSnapshot().Memory.Vram
                        [..EndingRewardIconArtworkFormat.TransferBytes]
                        .SequenceEqual(edited.RewardIcon.Transfer.Span),
                    $"{name} supplies every final native upload byte");
                LayeredRenderSnapshot baseline = installedScene.CaptureRenderSnapshot();
                LayeredRenderSnapshot changed = scene.CaptureRenderSnapshot();
                AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(baseline)
                        .AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(changed)),
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
            persistentImage = IndexedPng.Read(input, 128, 128);
        persistentImage.Pixels[0] = (byte)(persistentImage.Pixels[0] ^ 1);
        using (var output = File.Create(userPng))
            IndexedPng.Write(output, persistentImage.Width, persistentImage.Height,
                persistentImage.Pixels, persistentImage.Palette);
        byte[] selected = installation.LoadEndingMode7Art().RewardIcon.Transfer.ToArray();
        string stockMap = Path.Combine(installation.EndingMode7Directory,
            EndingRewardIconArtworkFormat.MapFileName);
        File.WriteAllBytes(stockMap, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "an icon override cannot mask corrupt installed stock map JSON");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Reward icon content vanished during stock repair.");
        AssertTrue(repaired.LoadEndingMode7Art().RewardIcon.Transfer.Span.SequenceEqual(selected),
            "stock icon repair preserves the player's separately stored PNG override");
        File.Delete(userPng);
        Console.WriteLine("Reward icon: native sixteen-chunk VRAM/pixel parity, mid-upload rebind and independently visible map/PNG edits pass.");
    }
}
