using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks the file-select swoosh request and audible PCM through isolated and full frontend paths.</summary>
internal static class FileSelectSoundVerification
{
    /// <summary>Checks the driver's delayed acknowledgement and audible output, then both save and accept routes.</summary>
    internal static void VerifyFileSelectSound()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var audio = new CartridgeAudioState();
        var menu = new FileSelectMenuState(new SuperMetroid.Core.Saves.SaveSlotStore(), audio, RetailPresentationFixture(), MenuSheetFixtures.For(RetailPresentationFixture()));
        var renderer = CartridgeAudioSessionAccess.CreateSynthesizing(ExtractedAudioAssetCatalog.Load(RepositoryInstallation.Installation.AudioDirectory));
        int writes = 0, nonzero = 0;
        bool accepted = false;
        for (int frame = 0; frame < 180; frame++)
        {
            // Accept on the first update the native main menu reads input.
            bool accept = !accepted && menu.Phase == FileSelectPhase.Main;
            menu.Step(accept ? SnesButton.A : 0);
            var commands = audio.AdvanceFrame(renderer.Acknowledgements);
            writes += commands.Count(command => command is CartridgeAudioCommand.WritePort { InputPort: ApuPort.SoundLibrary1 } write && write.Value == SoundEffectLibrary1Sounds.FileSelectSwoosh.Value);
            var pcm = renderer.RenderFrame(commands);
            if (!accepted && !accept) AssertTrue(pcm.All(value => value == 0), "isolated selection must be silent before accept");
            else nonzero += pcm.Count(value => value != 0);
            accepted |= accept;
        }
        AssertTrue(accepted, "isolated file select reaches its main menu");
        // The SPC driver echoes a request one sound service after reading it ($1EE7/$1621), so
        // $82:8A55 rewrites the unacknowledged request once; the driver's change detection
        // starts the swoosh only for the first.
        AssertEqual(2, writes, "one file selection writes its swoosh request until the driver echoes it");
        AssertTrue(nonzero > 0, "file selection generates audible PCM");
        Console.WriteLine($"File selection: {writes} sound writes, {nonzero} nonzero PCM samples.");
        foreach (bool existingSave in new[] { false, true })
        foreach (SnesButton accept in new[] { SnesButton.A, SnesButton.Start })
            VerifyFullFrontendFileSelectSound(existingSave, accept);
    }

    /// <summary>Compares captured PCM with the swoosh suppressed for one save state and accept button.</summary>
    private static void VerifyFullFrontendFileSelectSound(bool existingSave, SnesButton accept)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        if (existingSave)
            saves.SaveSlot(0, new SuperMetroidSaveSnapshot());
        var game = CreateRetailGameFixture(saves);
        var assets = RepositoryInstallation.Installation.LoadAudioClips();
        game.BindAudio(assets);
        var pcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var withoutSwoosh = new ClipAudioBackend(assets);
        var control = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        int starts = 0, differentSamples = 0;
        int firstSoundTick = -1, firstDifferenceTick = -1;
        bool reachedSavedMap = false;
        for (int tick = 0; tick < 500; tick++)
        {
            SnesButton button = game.GameState == SuperMetroidGameState.FileSelectMenus ? accept : SnesButton.Start;
            var frame = game.StepCaptured(tick % 47 == 0 ? button : SnesButton.None, tick + 1, 1).Frame;
            reachedSavedMap |= frame.GameState == SuperMetroidGameState.FileSelectMap;
            static bool IsSwoosh(AudioEvent audioEvent) => audioEvent is AudioEvent.StartSoundEffect started &&
                started.SoundEffect == SoundEffectLibrary1Sounds.FileSelectSwoosh;
            starts += game.AudioEventsForVerification.Count(IsSwoosh);
            if (game.AudioEventsForVerification.Any(IsSwoosh) && firstSoundTick < 0)
            {
                firstSoundTick = tick;
                AssertEqual(SuperMetroidGameState.FileSelectMenus, frame.GameState, "swoosh starts on the file screen, not after loading");
                AssertEqual(nameof(FileSelectPhase.TurnSelectedHelmet), frame.Phase, "swoosh accompanies selected helmet turn");
            }
            game.CopyAudioFrame(pcm);
            withoutSwoosh.Render(new AudioFrameEvents([.. game.AudioEventsForVerification.Where(audioEvent => !IsSwoosh(audioEvent))]), control);
            for (int sample = 0; sample < pcm.Length; sample++)
                if (pcm[sample] != control[sample])
                {
                    differentSamples++;
                    if (firstDifferenceTick < 0) firstDifferenceTick = tick;
                }
        }
        // $82:8A55 rewrites the request once while the driver's echo is a service behind; the
        // driver's change detection starts the swoosh once.
        AssertEqual(1, starts, "full captured frontend starts one file acceptance sound");
        AssertTrue(differentSamples > 0, "selection changes PCM even with frontend music and bank uploads");
        // Report onset rather than impose a guessed one-frame bound: the native SPC
        // instruction stream may deliberately delay its first non-silent sample.
        AssertTrue(firstDifferenceTick >= firstSoundTick,
            "suppressed sound cannot change PCM before the driver starts it");
        AssertEqual(existingSave, reachedSavedMap, "fixture actually distinguishes saved-map and new-game routes");
        Console.WriteLine($"Full frontend existing={existingSave} accept={accept}: {starts} start at tick {firstSoundTick}; PCM begins {firstDifferenceTick}; {differentSamples} samples differ.");
    }

    /// <summary>Runs the file-select sound verification command.</summary>
    [VerificationCommand("--file-select-sound")]
    internal static int FileSelectSoundCommand()
    {
        Suite(nameof(VerifyFileSelectSound), () => VerifyFileSelectSound());
        return 0;
    }
}
