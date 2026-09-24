using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyPostCreditsCharacterArtwork(GameInstallation installation)
    {
        EndingObjectArtworkCatalog stock = installation.LoadEndingObjectArt();
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var source = new SuperMetroidAddressSpace(rom);
        CreditsPresentation credits = CreditsPresentation.Load(new MemoryStream(
            CreditsPresentationExtractor.Extract(source)));
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var nativeBus = new SuperMetroidAddressSpace(rom);
            var guardedBus = new EndingObjectSourceReadGuard(new SuperMetroidAddressSpace(rom));
            var nativeAudio = new CartridgeAudioState();
            var installedAudio = new CartridgeAudioState();
            var native = new EndingCreditsState(nativeBus, nativeAudio, hours, 0);
            var installed = new EndingCreditsState(guardedBus, installedAudio, hours, 0);
            native.BindStaffCredits(credits);
            installed.BindStaffCredits(credits);
            installed.BindObjectArtwork(stock);
            var checkedPhases = new HashSet<EndingCreditsPhase>();
            for (int frame = 0; frame < 60_000 &&
                native.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
            {
                AssertEqual(native.Phase, installed.Phase,
                    $"post-credits installed/native phase for {hours}h at frame {frame}");
                if (native.Phase >= EndingCreditsPhase.Credits &&
                    checkedPhases.Add(native.Phase))
                {
                    LayeredRenderSnapshot expected = native.CaptureRenderSnapshot();
                    LayeredRenderSnapshot actual = installed.CaptureRenderSnapshot();
                    AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                            actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                            SoftwareLayeredSnapshotRenderer.Render(actual).AsSpan().SequenceEqual(
                                SoftwareLayeredSnapshotRenderer.Render(expected)),
                        $"installed post-credits art preserves VRAM, palette, and pixels in {hours}h {native.Phase}");
                }
                native.Step();
                installed.Step();
                nativeAudio.AdvanceFrame(nativeBus, default);
                installedAudio.AdvanceFrame(guardedBus, default);
            }
            AssertEqual(EndingCreditsPhase.PostCreditsShot, native.Phase,
                $"native {hours}h reward reaches the post-credit shot");
            AssertEqual(EndingCreditsPhase.PostCreditsShot, installed.Phase,
                $"installed {hours}h reward reaches the post-credit shot");
            AssertTrue(checkedPhases.Contains(EndingCreditsPhase.Credits) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsWaitingSamus) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsReward) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsJump),
                $"{hours}h reward exercised waiting, selected reward, and jump sheets");
            AssertEqual(0, guardedBus.ForbiddenReadAttempts,
                $"installed {hours}h reward never rereads the three character-art sources");
        }
        foreach ((string fileName, ushort hours, EndingCreditsPhase target) in new[]
        {
            (EndingObjectArtworkFormat.WaitingSamusFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsWaitingBackdrop),
            (EndingObjectArtworkFormat.ShootingScreenFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsShot),
            (EndingObjectArtworkFormat.SuitlessSamusFileName, (ushort)2,
                EndingCreditsPhase.PostCreditsReward),
        })
        {
            string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory,
                fileName);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.EndingObjectDirectory,
                       fileName)))
                image = IndexedPng.Read(input, 256, 128);
            Array.Fill(image.Pixels, (byte)0);
            try
            {
                using (var output = File.Create(overridePath))
                    IndexedPng.Write(output, image.Width, image.Height, image.Pixels,
                        image.Palette);
                EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
                var stockBus = new EndingObjectSourceReadGuard(new SuperMetroidAddressSpace(rom));
                var editedBus = new EndingObjectSourceReadGuard(new SuperMetroidAddressSpace(rom));
                var stockAudio = new CartridgeAudioState();
                var editedAudio = new CartridgeAudioState();
                var stockState = new EndingCreditsState(stockBus, stockAudio, hours, 0);
                var editedState = new EndingCreditsState(editedBus, editedAudio, hours, 0);
                stockState.BindStaffCredits(credits);
                editedState.BindStaffCredits(credits);
                stockState.BindObjectArtwork(stock);
                editedState.BindObjectArtwork(edited);
                for (int frame = 0; frame < 60_000 && stockState.Phase != target; frame++)
                {
                    AssertEqual(stockState.Phase, editedState.Phase,
                        $"edited {fileName} does not alter reward timing at frame {frame}");
                    stockState.Step();
                    editedState.Step();
                    stockAudio.AdvanceFrame(stockBus, default);
                    editedAudio.AdvanceFrame(editedBus, default);
                }
                AssertEqual(target, stockState.Phase,
                    $"edited {fileName} reaches the native reward scene");
                AssertEqual(target, editedState.Phase,
                    $"edited {fileName} retains reward scene timing");
                bool visible = false;
                for (int frame = 0; frame < 120 && !visible; frame++)
                {
                    if (frame % 8 == 0)
                    {
                        LayeredRenderSnapshot original = stockState.CaptureRenderSnapshot();
                        LayeredRenderSnapshot changed = editedState.CaptureRenderSnapshot();
                        AssertTrue(!original.Memory.Vram.SequenceEqual(changed.Memory.Vram) &&
                                original.Memory.Cgram.SequenceEqual(changed.Memory.Cgram),
                            $"edited {fileName} changes only art, not palette state");
                        visible = !SoftwareLayeredSnapshotRenderer.Render(original)
                            .AsSpan().SequenceEqual(
                                SoftwareLayeredSnapshotRenderer.Render(changed));
                    }
                    stockState.Step();
                    editedState.Step();
                    stockAudio.AdvanceFrame(stockBus, default);
                    editedAudio.AdvanceFrame(editedBus, default);
                }
                AssertTrue(visible,
                    $"edited {fileName} changes visible post-credits pixels");
                AssertEqual(0, stockBus.ForbiddenReadAttempts,
                    $"stock {fileName} never rereads source ROM art");
                AssertEqual(0, editedBus.ForbiddenReadAttempts,
                    $"edited {fileName} never rereads source ROM art");
            }
            finally
            {
                File.Delete(overridePath);
            }
        }
        Console.WriteLine("Post-credits character art: three reward variants retain native phase, VRAM, palette and pixels without source reads.");
    }
}
