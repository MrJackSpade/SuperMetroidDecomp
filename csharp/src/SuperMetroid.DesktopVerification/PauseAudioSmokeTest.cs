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

namespace SuperMetroid.DesktopVerification;

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
        game.BindAudio(DesktopAccess.LoadAudioAssets());
        var samples = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        FrontendFrame frame = default;
        int frames = 0;
        int pauseFrames = 0;
        int commandCount = 0;
        var continuity = new StereoPcmContinuityMeter();
        var pauseSetupPortWrites = new List<(ApuPort Port, byte Value)>();

        FrontendFrame Step(ushort input)
        {
            FrontendFrame next = game.Step(input);
            game.CopyAudioFrame(samples);
            commandCount += next.AudioCommands.Count;
            continuity.Observe(samples);
            frames++;
            if (next.GameState is >= SuperMetroidGameState.PausingDarkening and
                <= SuperMetroidGameState.Unpausing)
            {
                pauseFrames++;
                if (next.GameState == SuperMetroidGameState.PausedA)
                {
                    pauseSetupPortWrites.AddRange(next.AudioCommands
                        .Where(command => command.Kind == CartridgeAudioCommandKind.WritePort)
                        .Select(command => (command.InputPort, command.Value)));
                }
                if (next.AudioCommands.Any(command =>
                        command.Kind == CartridgeAudioCommandKind.Upload))
                {
                    throw new InvalidDataException(
                        $"Pause state {next.GameState} restarted an SPC data upload.");
                }
            }
            return next;
        }

        frame = Step(0);
        frame = Step((ushort)SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen), 120);
        frame = Step((ushort)SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus, 120);
        for (int index = 0; index < 16; index++) frame = Step(0);
        frame = Step((ushort)SnesButton.A);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.GameOptionsMenu, 180);
        for (int index = 0; index < 16; index++) frame = Step(0);
        frame = Step((ushort)SnesButton.A);
        // This fixture loads existing SRAM, so follow both saved-game map confirmations.
        // Waiting for the new-game setup state would now stall on the area map and never
        // exercise pause audio. Keep the real frontend path and audio acknowledgements.
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.FileSelectMap &&
            candidate.Phase == nameof(FileSelectMapNavigationPhase.Area), 240);
        frame = Step((ushort)SnesButton.A);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.FileSelectMap &&
            candidate.Phase == nameof(FileSelectMapNavigationPhase.Room), 120);
        frame = Step((ushort)SnesButton.A);
        // Include the map's confirmation fade/black hold, gameplay fade-in, and the
        // cartridge's 360-call saved-game appearance before attempting to pause.
        frame = StepUntil(frame, _ => game.GameplayMovementEnabled, 600);

        // Establish ordinary room music before measuring the exact pause sequence.
        for (int index = 0; index < 120; index++) frame = Step(0);
        frame = Step((ushort)SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.PausedB, 128);
        for (int index = 0; index < 120; index++) frame = Step(0);
        frame = Step((ushort)SnesButton.Right);
        frame = Step(0);
        frame = Step((ushort)SnesButton.Left);
        frame = Step(0);
        for (int index = 0; index < 8; index++)
            frame = Step((ushort)SnesButton.Start);
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.MainGameplay, 128);
        for (int index = 0; index < 120; index++) frame = Step(0);

        if (pauseFrames == 0 || commandCount == 0)
            throw new InvalidDataException("Pause PCM audit did not exercise audio or pause states.");
        (ApuPort Port, byte Value)[] expectedCancellationWrites =
        [
            (ApuPort.SoundLibrary1, SoundEffectLibrary1Sounds.CancelAll.Value),
            (ApuPort.SoundLibrary2, SoundEffectLibrary2Sounds.CancelAll.Value),
            (ApuPort.SoundLibrary3, SoundEffectLibrary3Sounds.CancelAll.Value),
        ];
        int cancellationCursor = 0;
        foreach ((ApuPort port, byte value) in pauseSetupPortWrites)
        {
            if (cancellationCursor < expectedCancellationWrites.Length &&
                (port, value) == expectedCancellationWrites[cancellationCursor])
            {
                cancellationCursor++;
            }
        }
        if (cancellationCursor != expectedCancellationWrites.Length)
        {
            throw new InvalidDataException(
                "Pause setup did not send $82:BE17's library 1/2/3 cancellation commands " +
                $"in order; writes=[{string.Join(',', pauseSetupPortWrites.Select(write =>
                    $"{write.Port}:{write.Value:X2}"))}].");
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
