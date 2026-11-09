using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingMode7ArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>Checks installed reward-icon artwork through the reward jump's reveal and the ending shot.</summary>
internal static class EndingRewardIconArtworkVerification
{
    /// <summary>Compares retail pixels and VRAM through the reward jump and the icon's reveal.</summary>
    internal static void VerifyEndingRewardIconArtwork(GameInstallation installation)
    {
        EndingFloorCharacters stock = installation.LoadEndingMode7Art();
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var source = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        byte[] native = RomDataReader.Decompress(source,
            EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters,
            EndingSourceDefinitions.Artwork.DecompressionLimit);
        AssertTrue(EndingRewardIconCharacters.Of(stock.RewardIcon).Bytes.Span.SequenceEqual(native.AsSpan(0,
                EndingRewardIconArtworkFormat.ByteCount)),
            "reward icon JSON/PNG preserves all interleaved native map and character bytes");

        var installedBus = NewEndingMode7SourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var nativeScene = CreateRetailEndingFixture(nativeAudio, 3, 0);
        var installedScene = CreateRetailEndingFixture(installedAudio, 3, 0);
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
                RgbColor[] expectedColours = EndingShownColours.Of(nativeScene);
                byte[] expectedVram = nativeScene.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installedScene);
                RgbColor[] actualColours = EndingShownColours.Of(installedScene);
                byte[] actualVram = installedScene.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actualColours.SequenceEqual(expectedColours) &&
                        ScenePixels.Same(actual, expected),
                    $"reward icon preserves native VRAM and pixels at jump frame {jumpFrames}");
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
            "reward icon integration covers the full native jump and the icon's reveal");
        AssertEqual(0, installedBus.ForbiddenReadAttempts,
            "installed reward icon never rereads its compressed cartridge stream");
        AssertTrue(installedScene.ShownVram()
                [..EndingRewardIconArtworkFormat.ByteCount].SequenceEqual(
                    EndingRewardIconCharacters.Of(stock.RewardIcon).Bytes.Span),
            "the revealed icon is the installed icon image");

        Console.WriteLine("Reward icon: native VRAM/pixel parity through its reveal passes.");
    }
}
