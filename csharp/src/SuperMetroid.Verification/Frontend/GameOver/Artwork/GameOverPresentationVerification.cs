using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

namespace SuperMetroid.Verification.Frontend.GameOver.Artwork;

/// <summary>Checks installed game-over artwork and Baby animation against cartridge playback.</summary>
internal static class GameOverPresentationVerification
{
    /// <summary>Compares a No-to-title sequence with native output.</summary>
    internal static void VerifyGameOverPresentationAssets(
        ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.GameOverPresentationExtractor.Extract(bus);
        _ = GameOverSourcePresentation.Load(new MemoryStream(extracted));
        AssertEqual(60, GameOverBabyAnimationDefinitions.InstructionCount,
            "compiled game-over Baby instruction count");

        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var native = new GameOverMenuState(nativeAudio, original, MenuSheetFixtures.For(original));
        var installed = new GameOverMenuState(installedAudio, original, MenuSheetFixtures.For(original));
        bool choseNo = false;
        bool released = false;
        int comparedFrames = 0;
        for (int tick = 0; tick < 700 && !native.TitleRequested; tick++)
        {
            AssertEqual(native.Phase, installed.Phase,
                "installed game-over phase matches cartridge path");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                "installed game-over selection matches cartridge path");
            AssertEqual(native.BabyPlace, installed.BabyPlace,
                "the Baby's animation reaches the same place");
            AssertEqual(native.BabyFrame, installed.BabyFrame,
                "compiled Baby sequence selects the same pose");
            AssertTrue(ScenePixels.Same(native.CaptureRenderSnapshot(), installed.CaptureRenderSnapshot()),
                $"installed game-over frame {tick} matches cartridge pixels");
            comparedFrames++;

            ControllerButton input = 0;
            if (native.Phase == GameOverMenuPhase.Main && tick >= 500)
            {
                if (!choseNo)
                {
                    input = ControllerButton.Down;
                    choseNo = true;
                }
                else if (!released)
                {
                    released = true;
                }
                else
                {
                    input = ControllerButton.A;
                }
            }
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<AudioRequest> nativeCommands =
                nativeAudio.AdvanceFrame(default);
            IReadOnlyList<AudioRequest> installedCommands =
                installedAudio.AdvanceFrame(default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"compiled game-over audio dispatch matches cartridge frame {tick}");
        }
        AssertTrue(native.TitleRequested && installed.TitleRequested,
            "both game-over paths complete the No fade to title");
        AssertTrue(comparedFrames > 500,
            "game-over parity covers animation, music wait, input and fade");

        Console.WriteLine(
            "Game-over presentation: deterministic stock and native parity pass.");
    }
}
