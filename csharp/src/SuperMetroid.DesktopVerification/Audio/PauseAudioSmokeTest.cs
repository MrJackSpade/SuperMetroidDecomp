using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.SaveMapFixtures;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>Headless saved-game pause/resume regression covering PCM continuity, sound-library cancellation order, and preservation of the resident SPC upload.</summary>
public static class PauseAudioSmokeTest
{
    /// <summary>Seeds a Crateria save, enters gameplay through the frontend, and measures managed audio while pausing, switching pages, and resuming.</summary>
    /// <param name="romPath">Retail cartridge used to construct the isolated game and seeded save.</param>
    /// <returns>PCM continuity and command counts after all pause assertions pass; mismatches throw.</returns>
    public static PauseAudioSmokeTestResult Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        SeedCrateriaSave(bus);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        // A bare game draws the title and file screens from installed artwork, as the hosts bind it.
        InstalledGameBindings.Create(RepositoryInstallation.Installation)(game);
        game.BindAudio(RepositoryInstallation.Installation.LoadAudioClips());
        var samples = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        FrontendFrame frame = default;
        int frames = 0;
        int pauseFrames = 0;
        int commandCount = 0;
        var continuity = new StereoPcmContinuityMeter();
        var pauseSoundStarts = new List<SoundEffectId>();

        FrontendFrame Step(SnesButton input)
        {
            FrontendFrame next = game.Step(input);
            game.CopyAudioFrame(samples);
            commandCount += game.AudioEventsForVerification.Count(audioEvent => audioEvent.Kind != AudioEventKind.DriverTick);
            continuity.Observe(samples);
            frames++;
            if (next.GameState is >= SuperMetroidGameState.PausingDarkening and
                <= SuperMetroidGameState.Unpausing)
            {
                pauseFrames++;
                // The driver starts the cancellations at a poll after the CPU writes them, so
                // collect every start while the pause owns the frame, not only PausedA's.
                pauseSoundStarts.AddRange(game.AudioEventsForVerification
                    .Where(audioEvent => audioEvent.Kind == AudioEventKind.StartSoundEffect)
                    .Select(audioEvent => audioEvent.SoundEffect));
                if (game.AudioEventsForVerification.Any(audioEvent => audioEvent.Kind == AudioEventKind.LoadAudioBank))
                {
                    throw new InvalidDataException(
                        $"Pause state {next.GameState} restarted an SPC data upload.");
                }
            }
            return next;
        }

        frame = Step(0);
        frame = Step(SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen), 120);
        frame = Step(SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus, 120);
        // This fixture loads existing SRAM, so follow both saved-game map confirmations.
        // Accept on the cadence FileSelectSoundVerification uses until the area map opens: the
        // menus take a press only once their transitions settle. Keep the real frontend path
        // and audio acknowledgements.
        for (int index = 0; !(frame.GameState == SuperMetroidGameState.FileSelectMap &&
            frame.Phase == nameof(FileSelectMapNavigationPhase.Area)); index++)
        {
            if (index == 600)
                throw new InvalidOperationException($"Pause PCM route stalled in {frame.GameState}/{frame.Phase}.");
            frame = Step(index % 47 == 46 ? SnesButton.A : SnesButton.None);
        }
        frame = Step(SnesButton.A);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.FileSelectMap &&
            candidate.Phase == nameof(FileSelectMapNavigationPhase.Room), 120);
        frame = Step(SnesButton.A);
        // Include the map's confirmation fade/black hold, gameplay fade-in, and the
        // cartridge's 360-call saved-game appearance before attempting to pause.
        frame = StepUntil(frame, _ => game.GameplayMovementEnabled, 600);

        // Establish ordinary room music before measuring the exact pause sequence.
        for (int index = 0; index < 120; index++) frame = Step(0);
        frame = Step(SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.PausedB, 128);
        for (int index = 0; index < 120; index++) frame = Step(0);
        frame = Step(SnesButton.Right);
        frame = Step(0);
        frame = Step(SnesButton.Left);
        frame = Step(0);
        for (int index = 0; index < 8; index++)
            frame = Step(SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.MainGameplay, 128);
        for (int index = 0; index < 120; index++) frame = Step(0);

        if (pauseFrames == 0 || commandCount == 0)
            throw new InvalidDataException("Pause PCM audit did not exercise audio or pause states.");
        SoundEffectId[] expectedCancellations =
        [
            SoundEffectLibrary1Sounds.CancelAll,
            SoundEffectLibrary2Sounds.CancelAll,
            SoundEffectLibrary3Sounds.CancelAll,
        ];
        int cancellationCursor = 0;
        foreach (SoundEffectId start in pauseSoundStarts)
        {
            if (cancellationCursor < expectedCancellations.Length &&
                start == expectedCancellations[cancellationCursor])
            {
                cancellationCursor++;
            }
        }
        if (cancellationCursor != expectedCancellations.Length)
        {
            throw new InvalidDataException(
                "The SPC did not start $82:BE17's library 1/2/3 cancellation commands " +
                $"in order during the pause; starts=[{string.Join(',', pauseSoundStarts)}].");
        }
        int maximumBoundaryDelta = continuity.MaximumBoundaryDelta;
        int maximumAdjacentDelta = continuity.MaximumWithinBufferDelta;
        if (maximumBoundaryDelta > maximumAdjacentDelta)
        {
            throw new InvalidDataException(
                $"Pause route introduced a {maximumBoundaryDelta}-level PCM discontinuity at " +
                $"an emulated frame seam, greater than its {maximumAdjacentDelta}-level " +
                "largest within-frame sample step.");
        }
        return new PauseAudioSmokeTestResult(
            frames,
            pauseFrames,
            commandCount,
            maximumAdjacentDelta,
            maximumBoundaryDelta);

        FrontendFrame StepUntil(
            FrontendFrame initial,
            Func<FrontendFrame, bool> predicate,
            int maximumFrames)
        {
            FrontendFrame current = initial;
            for (int index = 0; index < maximumFrames && !predicate(current); index++)
                current = Step(0);
            if (!predicate(current))
                throw new InvalidOperationException($"Pause PCM route stalled in {current.GameState}/{current.Phase}.");
            return current;
        }
    }

    /// <summary>Writes a Crateria station save with Morph Ball and Bombs so the frontend follows the saved-game route.</summary>
    private static void SeedCrateriaSave(SuperMetroidAddressSpace bus)
    {
        var system = new Bank80SystemState();
        system.SetBossBits(0, BossBits.AreaTorizo);
        system.MarkSaveStationUsed(0, 0);
        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        };
        new SuperMetroidSaveRam(bus, SaveMapPresentationFixture.Create(bus)).SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Area = 0, SaveStation = 0 }));
    }
}
