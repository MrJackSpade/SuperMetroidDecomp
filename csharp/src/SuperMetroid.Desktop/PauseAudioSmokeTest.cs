using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop;

/// <summary>PCM-level regression for gameplay-to-pause-to-gameplay audio continuity.</summary>
public readonly record struct PauseAudioSmokeTestResult(
    int FramesGenerated,
    int PauseFrames,
    int AudioCommandCount,
    int MaximumAdjacentSampleDelta,
    int MaximumFrameBoundaryDelta);

public static class PauseAudioSmokeTest
{
    public static PauseAudioSmokeTestResult Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SeedCrateriaSave(bus);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        using var audio = new SpcAudioEngine(bus);
        FrontendFrame frame = default;
        int frames = 0;
        int pauseFrames = 0;
        int commandCount = 0;
        int maximumAdjacentDelta = 0;
        int maximumBoundaryDelta = 0;
        var pauseSetupPortWrites = new List<(byte Port, byte Value)>();
        short? precedingSample = null;

        FrontendFrame Step(ushort input)
        {
            FrontendFrame next = game.Step(input);
            ReadOnlySpan<short> samples = audio.RenderFrame(next.AudioCommands);
            commandCount += next.AudioCommands.Count;
            if (precedingSample is short previous)
                maximumBoundaryDelta = Math.Max(maximumBoundaryDelta, Math.Abs(samples[0] - previous));
            for (int index = 1; index < samples.Length; index++)
                maximumAdjacentDelta = Math.Max(maximumAdjacentDelta, Math.Abs(samples[index] - samples[index - 1]));
            precedingSample = samples[^1];
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            frames++;
            if (next.GameState is >= SuperMetroidGameState.PausingDarkening and
                <= SuperMetroidGameState.Unpausing)
            {
                pauseFrames++;
                if (next.GameState == SuperMetroidGameState.PausedA)
                {
                    pauseSetupPortWrites.AddRange(next.AudioCommands
                        .Where(command => command.Kind == CartridgeAudioCommandKind.WritePort)
                        .Select(command => (command.Port, command.Value)));
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
        frame = StepUntil(frame, candidate => candidate.GameState == SuperMetroidGameState.SetUpNewGame, 180);
        frame = Step(0);
        frame = StepUntil(frame, _ => game.GameplayMovementEnabled, 400);

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
        (byte Port, byte Value)[] expectedCancellationWrites =
        [
            (1, SoundEffectLibrary1Sounds.CancelAll.Value),
            (2, SoundEffectLibrary2Sounds.CancelAll.Value),
            (3, SoundEffectLibrary3Sounds.CancelAll.Value),
        ];
        int cancellationCursor = 0;
        foreach ((byte port, byte value) in pauseSetupPortWrites)
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
        new SuperMetroidSaveRam(bus).SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(samus, system, area: 0, saveStation: 0));
    }
}
