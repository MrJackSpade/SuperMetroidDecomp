using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingObjectArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares installed post-credits Samus and logo artwork through the native fade, DMA and assembled-logo sequence.</summary>
internal static class PostCreditsCharacterArtworkVerification
{
    /// <summary>Checks native versus installed VRAM and rendered pixels across the suitless post-shot handoff and all four logo actors.</summary>
    internal static void VerifyPostCreditsCharacterArtwork()
    {
        ImportEndingObjectArtwork stock = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects;
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var guardedBus = NewEndingObjectSourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
            var nativeAudio = AudioQueues.Standalone();
            var installedAudio = AudioQueues.Standalone();
            var native = CreateRetailEndingFixture(nativeAudio, hours, 0);
            var installed = CreateRetailEndingFixture(installedAudio, hours, 0);
            installed.InstalledArtwork.BindFixtureObjectArtwork(stock);
            var checkedPhases = new HashSet<EndingCreditsPhase>();
            for (int frame = 0; frame < 60_000 &&
                native.Stage.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
            {
                AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                    $"post-credits installed/native phase for {hours}h at frame {frame}");
                if (native.Stage.Phase >= EndingCreditsPhase.Credits &&
                    checkedPhases.Add(native.Stage.Phase))
                {
                    LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                    RgbColor[] expectedColours = EndingShownColours.Of(native);
                    byte[] expectedVram = native.ShownVram();
                    LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                    RgbColor[] actualColours = EndingShownColours.Of(installed);
                    byte[] actualVram = installed.ShownVram();
                    AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                            actualColours.SequenceEqual(expectedColours) &&
                            ScenePixels.Same(actual, expected),
                        $"installed post-credits art preserves VRAM, palette, and pixels in {hours}h {native.Stage.Phase}");
                }
                native.Step();
                installed.Step();
                nativeAudio.AdvanceFrame(default);
                installedAudio.AdvanceFrame(default);
            }
            AssertEqual(EndingCreditsPhase.PostCreditsShot, native.Stage.Phase,
                $"native {hours}h reward reaches the post-credit shot");
            AssertEqual(EndingCreditsPhase.PostCreditsShot, installed.Stage.Phase,
                $"installed {hours}h reward reaches the post-credit shot");
            AssertTrue(checkedPhases.Contains(EndingCreditsPhase.Credits) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsWaitingSamus) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsReward) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsJump),
                $"{hours}h reward exercised waiting, selected reward, and jump sheets");
            // The newly installed logo is not transferred until the Samus fade
            // admits six successive post-shot DMA calls. Compare the actual
            // native and installed VRAM/pixels across that handoff, not merely
            // the phase entry before the logo has appeared.
            for (int frame = 0; frame < 56; frame++)
            {
                native.Step();
                installed.Step();
                nativeAudio.AdvanceFrame(default);
                installedAudio.AdvanceFrame(default);
                if (frame < 47) continue;
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                RgbColor[] expectedColours = EndingShownColours.Of(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                RgbColor[] actualColours = EndingShownColours.Of(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actualColours.SequenceEqual(expectedColours) &&
                        ScenePixels.Same(actual, expected),
                    $"installed post-shot logo preserves native VRAM/pixels in {hours}h frame {frame}");
            }
            if (hours == 2)
            {
                // Continue the installed suitless route through all four logo
                // actors, not merely the initial logo upload after the shot.
                for (int frame = 0; frame < 2000 &&
                    native.Stage.Phase != EndingCreditsPhase.ItemPercentage; frame++)
                {
                    AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                        $"installed logo phase at frame {frame}");
                    if (frame % 29 == 0 || native.Stage.Phase == EndingCreditsPhase.PostCreditsLogo)
                    {
                        LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                        RgbColor[] expectedColours = EndingShownColours.Of(native);
                        byte[] expectedVram = native.ShownVram();
                        LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                        RgbColor[] actualColours = EndingShownColours.Of(installed);
                        byte[] actualVram = installed.ShownVram();
                        AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                                actualColours.SequenceEqual(expectedColours) &&
                                ScenePixels.Same(actual, expected),
                            $"installed logo pixels at frame {frame}");
                    }
                    native.Step();
                    installed.Step();
                    nativeAudio.AdvanceFrame(default);
                    installedAudio.AdvanceFrame(default);
                }
                AssertEqual(EndingCreditsPhase.ItemPercentage, native.Stage.Phase,
                    "native logo reaches item percentage");
                AssertEqual(EndingCreditsPhase.ItemPercentage, installed.Stage.Phase,
                    "compiled installed logo reaches item percentage");
            }
            AssertEqual(0, guardedBus.ForbiddenReadAttempts,
                $"installed {hours}h reward never rereads post-credit art sources");
        }
        Console.WriteLine("Post-credits art: three reward variants and the logo retain native phase, VRAM, palette and pixels without source reads.");
    }
}
