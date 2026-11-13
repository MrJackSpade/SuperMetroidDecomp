using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Options.GameOptionsCursorPhasesVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

namespace SuperMetroid.Verification.Frontend.Options.Artwork;

/// <summary>Checks installed game-options art against cartridge rendering.</summary>
internal static class GameOptionsPresentationVerification
{
    /// <summary>Compares menu frames, audio, and controller changes with native output.</summary>
    internal static void VerifyGameOptionsPresentationAssets(
        ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(bus);
        _ = GameOptionsSourcePresentation.Load(new MemoryStream(extracted));

        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var native = new GameOptionsMenuState(nativeAudio,
            mapPresentation: original, spriteSheets: MenuSheetFixtures.For(original));
        var installed = new GameOptionsMenuState(
            installedAudio, mapPresentation: original, spriteSheets: MenuSheetFixtures.For(original));
        int comparedFrames = 0;

        Until(GameOptionsPhase.Main);
        Press(ControllerButton.Down);
        Press(ControllerButton.A); // Switch language and rewrite the primary highlights.
        for (int row = 0; row < 3; row++)
            Press(ControllerButton.Down);
        Press(ControllerButton.A);
        Until(GameOptionsPhase.ControllerSettings);
        Press(ControllerButton.R); // Reassign Shoot and swap the displaced action label.
        for (int row = 0; row < 7; row++)
        {
            Press(ControllerButton.Down);
            Until(GameOptionsPhase.ControllerSettings);
        }
        Press(ControllerButton.A);
        Until(GameOptionsPhase.Main);
        for (int row = 0; row < 4; row++)
            Press(ControllerButton.Down);
        Press(ControllerButton.A);
        Until(GameOptionsPhase.SpecialSettings);
        Press(ControllerButton.A);
        Press(ControllerButton.Down);
        Press(ControllerButton.Right);
        Press(ControllerButton.Down);
        Press(ControllerButton.A);
        Until(GameOptionsPhase.Main);

        AssertTrue(native.JapaneseText && installed.JapaneseText,
            "installed options path retains language behavior");
        AssertEqual(native.ControllerBindings, installed.ControllerBindings,
            "installed options path retains controller swaps");
        AssertTrue(native.IconCancelEnabled && installed.IconCancelEnabled &&
            native.MoonwalkEnabled && installed.MoonwalkEnabled,
            "installed options path retains special toggles");

        Console.WriteLine(
            "Options presentation: deterministic stock and native parity pass.");

        void Until(GameOptionsPhase expected)
        {
            for (int frame = 0; frame < 120 && native.Phase != expected; frame++)
                Frame(0);
            AssertEqual(expected, native.Phase, $"native options reaches {expected}");
            AssertEqual(expected, installed.Phase, $"installed options reaches {expected}");
        }

        void Press(ControllerButton button)
        {
            Frame(button);
            Frame(0);
        }

        void Frame(ControllerButton input)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed options phase matches cartridge frame {comparedFrames}");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                $"installed options selection matches cartridge frame {comparedFrames}");
            if (native.Phase is GameOptionsPhase.DissolveOut or GameOptionsPhase.DissolveIn)
            {
                AssertTrue(LastSpriteX(native.CaptureRenderSnapshot()) >= 256,
                    $"ROM options hides cursor in {native.Phase}");
                AssertTrue(LastSpriteX(installed.CaptureRenderSnapshot()) >= 256,
                    $"editable options hides cursor in {installed.Phase}");
            }
            AssertTrue(ScenePixels.Same(native.CaptureRenderSnapshot(), installed.CaptureRenderSnapshot()),
                $"installed options frame {comparedFrames} matches cartridge pixels");
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<AudioRequest> nativeCommands =
                nativeAudio.AdvanceFrame(default);
            IReadOnlyList<AudioRequest> installedCommands =
                installedAudio.AdvanceFrame(default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"installed options audio matches cartridge frame {comparedFrames}");
            comparedFrames++;
        }
    }
}
