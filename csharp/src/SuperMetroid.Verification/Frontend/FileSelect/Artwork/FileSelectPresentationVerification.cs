using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Verification.Frontend.FileSelect.Artwork;

/// <summary>Checks extracted file-select presentation parity and strict document validation.</summary>
internal static class FileSelectPresentationVerification
{
    /// <summary>Compares native and installed COPY/CLEAR flows frame by frame.</summary>
    internal static void VerifyFileSelectPresentationAssets(
        ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        _ = FileSelectSourcePresentation.Load(SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(bus));

        var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
        {
            Health = 45,
            MaxHealth = 99,
            GameTimeHours = 12,
            GameTimeMinutes = 34,
        });
        saveRam.SelectSlot(0);

        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var native = new FileSelectMenuState(saveRam, nativeAudio, original, MenuSheetFixtures.For(original));
        var installed = new FileSelectMenuState(saveRam, installedAudio, original, MenuSheetFixtures.For(original));
        int comparedFrames = 0;
        Until(FileSelectPhase.Main);
        for (int row = 0; row < 3; row++) Press(ControllerButton.Down);
        Press(ControllerButton.A);
        Until(FileSelectPhase.CopySelectSource);
        Press(ControllerButton.A);
        AssertEqual(FileSelectPhase.CopySelectDestination, native.Phase,
            "copy source advances to destination");
        Press(ControllerButton.A);
        AssertEqual(FileSelectPhase.CopyConfirm, native.Phase,
            "copy destination advances to confirmation");
        Press(ControllerButton.A);
        AssertEqual(FileSelectPhase.CopyCompleted, native.Phase,
            "copy confirmation reaches completion");
        Press(ControllerButton.A);
        Until(FileSelectPhase.Main);
        for (int row = 0; row < 4; row++) Press(ControllerButton.Down);
        Press(ControllerButton.A);
        Until(FileSelectPhase.ClearSelectSlot);
        Press(ControllerButton.A);
        AssertEqual(FileSelectPhase.ClearConfirm, native.Phase,
            "clear selection advances to confirmation");
        Press(ControllerButton.A);
        AssertEqual(FileSelectPhase.ClearCompleted, native.Phase,
            "clear confirmation reaches completion");
        Press(ControllerButton.A);
        Until(FileSelectPhase.Main);
        Press(ControllerButton.Down);
        Press(ControllerButton.A);
        Until(FileSelectPhase.FadeOutToOptions, maximumFrames: 80);
        for (int frame = 0; frame < 16; frame++) Frame(0);
        AssertEqual(native.SelectedHelmetFrame, installed.SelectedHelmetFrame,
            "installed file-select helmet animation retains native frame");

        Console.WriteLine(
            "File-select presentation: deterministic stock and native COPY/CLEAR parity pass.");

        void Until(FileSelectPhase expected, int maximumFrames = 100)
        {
            for (int frame = 0; frame < maximumFrames && native.Phase != expected; frame++)
                Frame(0);
            AssertEqual(expected, native.Phase, $"native file select reaches {expected}");
            AssertEqual(expected, installed.Phase, $"installed file select reaches {expected}");
        }

        void Press(ControllerButton button)
        {
            Frame(button);
            Frame(0);
        }

        void Frame(ControllerButton input)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed file-select phase matches frame {comparedFrames}");
            AssertEqual(native.SelectedItem, installed.SelectedItem,
                $"installed file-select selection matches frame {comparedFrames}");
            AssertTrue(native.BackgroundTilemap(InstalledFixtures.ImportOf(original)).SequenceEqual(installed.BackgroundTilemap(InstalledFixtures.ImportOf(original))),
                $"installed file-select tilemap matches frame {comparedFrames}");
            AssertTrue(ScenePixels.Same(native.CaptureRenderSnapshot(), installed.CaptureRenderSnapshot()),
                $"installed file-select pixels match frame {comparedFrames}");
            native.Step(input);
            installed.Step(input);
            IReadOnlyList<AudioRequest> nativeCommands =
                nativeAudio.AdvanceFrame(default);
            IReadOnlyList<AudioRequest> installedCommands =
                installedAudio.AdvanceFrame(default);
            AssertTrue(nativeCommands.SequenceEqual(installedCommands),
                $"installed file-select audio matches frame {comparedFrames}");
            comparedFrames++;
        }
    }


}
