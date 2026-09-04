using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Observable milestones from replaying a desktop journal through the real SPC.</summary>
public readonly record struct AudioInputReplaySmokeTestResult(
    int FramesExecuted,
    bool ProjectileFired,
    bool PowerBeamCommandObserved,
    bool PowerBeamAcknowledged,
    bool FirstDoorBegan,
    bool FirstDoorCompleted,
    ushort SourceRoom,
    ushort DestinationRoom);

/// <summary>PCM measurements for one complete pause found in a recorded player session.</summary>
public readonly record struct RecordedPauseAudioResult(
    int EntryFrame,
    int ResumeFrame,
    ushort RoomPointer,
    int PauseOwnedFrames,
    double MinimumPausedRms,
    double MaximumPausedRms,
    double MeanPausedRms,
    int AudioCommands,
    string CommandTrace);

/// <summary>
/// Replays one deterministic controller journal with native SPC acknowledgements but no GUI.
/// </summary>
public static class AudioInputReplaySmokeTest
{
    private const int NeutralTailFrames = 360;

    public static AudioInputReplaySmokeTestResult Run(
        string recordingPath,
        string romPath,
        string? captureDirectory = null)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] actualRomDigest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            actualRomDigest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(actualRomDigest, recording.RomSha256))
            throw new InvalidDataException("Replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        using var audio = new SpcAudioEngine();

        int framesExecuted = 0;
        bool projectileFired = false;
        bool powerBeamCommandObserved = false;
        bool powerBeamAcknowledged = false;
        bool firstDoorBegan = false;
        bool firstDoorCompleted = false;
        ushort sourceRoom = 0;
        ushort destinationRoom = 0;
        Rgba32[]? firstSourceFrame = null;
        Rgba32[]? lastSourceGameplayFrame = null;
        Rgba32[]? completedDestinationFrame = null;

        // Continue briefly with neutral input after the journal ends. A recording stopped
        // while state $0B was waiting cannot itself contain the later fade/scroll frames;
        // the neutral tail proves the transition now makes autonomous forward progress.
        int totalAvailableFrames = recording.ControllerInputs.Length + NeutralTailFrames;
        for (int frameIndex = 0; frameIndex < totalAvailableFrames; frameIndex++)
        {
            ushort input = frameIndex < recording.ControllerInputs.Length
                ? recording.ControllerInputs[frameIndex]
                : (ushort)0;
            FrontendFrame frame = game.Step(input);
            audio.RenderFrame(frame.AudioCommands);
            CartridgeAudioAcknowledgements acknowledgements = audio.ReadAcknowledgements();
            game.SetAudioAcknowledgements(acknowledgements);
            framesExecuted++;

            ushort? activeRoom = game.GameplayActiveRoomPointer;
            if (sourceRoom == 0 && activeRoom is { } firstRoom)
                sourceRoom = firstRoom;
            if (sourceRoom != 0 &&
                activeRoom == sourceRoom &&
                game.GameState == SuperMetroidGameState.MainGameplay)
            {
                // Clone because SuperMetroidGame may replace or reuse its current frame on
                // the following call. These are diagnostics only; replay state never reads
                // pixels back and therefore remains independent of an optional output path.
                firstSourceFrame ??= frame.Pixels.ToArray();
                lastSourceGameplayFrame = frame.Pixels.ToArray();
            }

            projectileFired |= game.GameplayLastFiredProjectileSlot is not null;
            foreach (CartridgeAudioCommand command in frame.AudioCommands)
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort &&
                    command.Port == 1 &&
                    command.Value == 0x0b)
                {
                    powerBeamCommandObserved = true;
                }
            }
            powerBeamAcknowledged |= acknowledgements[1] == 0x0b;

            if (game.GameState == SuperMetroidGameState.LoadingNextRoomB)
                firstDoorBegan = true;
            if (firstDoorBegan && activeRoom is { } room && room != sourceRoom)
                destinationRoom = room;
            if (destinationRoom != 0 &&
                game.GameState == SuperMetroidGameState.MainGameplay)
            {
                firstDoorCompleted = true;
                completedDestinationFrame = frame.Pixels.ToArray();
                break;
            }
        }

        if (!projectileFired)
            throw new InvalidDataException("Replay never fired a Samus projectile.");
        if (!powerBeamCommandObserved || !powerBeamAcknowledged)
        {
            throw new InvalidDataException(
                "Replay did not complete the recorded power-beam command acknowledgement.");
        }
        if (!firstDoorBegan || !firstDoorCompleted)
        {
            throw new InvalidDataException(
                $"Replay did not complete its first ordinary door: began={firstDoorBegan}, " +
                $"source=${sourceRoom:X4}, destination=${destinationRoom:X4}, " +
                $"state=${(ushort)game.GameState:X2} ({game.GameState}).");
        }

        if (!string.IsNullOrWhiteSpace(captureDirectory))
        {
            string fullCaptureDirectory = Path.GetFullPath(captureDirectory);
            Directory.CreateDirectory(fullCaptureDirectory);
            WriteCapture(firstSourceFrame, fullCaptureDirectory, "LandingSite.FirstGameplay.png");
            WriteCapture(lastSourceGameplayFrame, fullCaptureDirectory, "LandingSite.BeforeDoor.png");
            WriteCapture(completedDestinationFrame, fullCaptureDirectory, "Parlor.DoorComplete.png");
        }

        return new AudioInputReplaySmokeTestResult(
            framesExecuted,
            projectileFired,
            powerBeamCommandObserved,
            powerBeamAcknowledged,
            firstDoorBegan,
            firstDoorCompleted,
            sourceRoom,
            destinationRoom);
    }

    /// <summary>
    /// Replays an entire desktop journal through both the public frontend and the managed
    /// SPC/DSP implementation. Unlike the short first-door smoke test, this diagnostic keeps
    /// every later room-music upload, sound-effect overlap, and acknowledgement delay intact.
    /// </summary>
    public static int RunCompleteManagedAudio(string recordingPath, string romPath)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] actualRomDigest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            actualRomDigest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(actualRomDigest, recording.RomSha256))
            throw new InvalidDataException("Replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        using var audio = new SpcAudioEngine();
        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            FrontendFrame frame = game.Step(recording.ControllerInputs[frameIndex]);
            try
            {
                audio.RenderFrame(frame.AudioCommands);
            }
            catch (Exception exception)
            {
                throw new InvalidDataException(
                    $"Managed audio replay failed on recorded call {frameIndex}, " +
                    $"state=${(ushort)game.GameState:X2}, " +
                    $"room=$8F:{game.GameplayActiveRoomPointer.GetValueOrDefault():X4}.",
                    exception);
            }
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
        }
        return recording.ControllerInputs.Length;
    }

    /// <summary>
    /// Replays a player's complete journal and measures every actual pause interval before
    /// PCM reaches waveOut. This is the issue-#54 reproduction boundary: it retains the
    /// recorded room, music, active sound effects, input timing, and frontend pause states
    /// instead of substituting a newly constructed pause scenario.
    /// </summary>
    public static IReadOnlyList<RecordedPauseAudioResult> AnalyzeRecordedPauses(
        string recordingPath,
        string romPath)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] actualRomDigest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            actualRomDigest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(actualRomDigest, recording.RomSha256))
            throw new InvalidDataException("Replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        using var audio = new SpcAudioEngine();
        var completed = new List<RecordedPauseAudioResult>();
        PauseAccumulator? active = null;

        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            FrontendFrame frame = game.Step(recording.ControllerInputs[frameIndex]);
            bool pauseOwned = frame.GameState is >= SuperMetroidGameState.PausingDarkening and
                <= SuperMetroidGameState.Unpausing;
            ReadOnlySpan<short> samples = audio.RenderFrame(frame.AudioCommands);
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());

            if (pauseOwned && active is null)
            {
                active = new PauseAccumulator(
                    frameIndex,
                    game.GameplayActiveRoomPointer.GetValueOrDefault());
            }
            if (active is { } pause)
            {
                pause.AudioCommands += frame.AudioCommands.Count;
                if (frame.AudioCommands.Count != 0)
                    pause.RecordCommands(frameIndex, frame.GameState, frame.AudioCommands);
                if (pauseOwned)
                    pause.PauseOwnedFrames++;
                if (frame.GameState == SuperMetroidGameState.PausedB)
                    pause.AddPausedFrame(samples);

                if (!pauseOwned && frame.GameState == SuperMetroidGameState.MainGameplay)
                {
                    completed.Add(pause.Complete(frameIndex));
                    active = null;
                }
            }
        }

        if (active is not null)
            throw new InvalidDataException("Recorded pause did not return to gameplay before the journal ended.");
        if (completed.Count == 0)
            throw new InvalidDataException("Recording contains no complete gameplay pause interval.");
        return completed;
    }

    private sealed class PauseAccumulator
    {
        private double minimumRms = double.MaxValue;
        private double maximumRms;
        private double sumRms;
        private int pausedFrames;
        private readonly List<string> commandTrace = [];

        public PauseAccumulator(int entryFrame, ushort roomPointer)
        {
            EntryFrame = entryFrame;
            RoomPointer = roomPointer;
        }

        public int EntryFrame { get; }
        public ushort RoomPointer { get; }
        public int PauseOwnedFrames { get; set; }
        public int AudioCommands { get; set; }

        public void AddPausedFrame(ReadOnlySpan<short> samples)
        {
            double squareSum = 0;
            foreach (short sample in samples)
                squareSum += (double)sample * sample;
            double rms = Math.Sqrt(squareSum / samples.Length);
            minimumRms = Math.Min(minimumRms, rms);
            maximumRms = Math.Max(maximumRms, rms);
            sumRms += rms;
            pausedFrames++;
        }

        public void RecordCommands(
            int frameIndex,
            SuperMetroidGameState state,
            IReadOnlyList<CartridgeAudioCommand> commands)
        {
            string formatted = string.Join(',', commands.Select(command => command.Kind switch
            {
                CartridgeAudioCommandKind.Upload => $"upload:${command.UploadAddress:X6}",
                CartridgeAudioCommandKind.WritePort => $"port{command.Port}:${command.Value:X2}",
                _ => throw new InvalidDataException($"Unknown audio command {command.Kind}."),
            }));
            commandTrace.Add($"{frameIndex}/{state}={formatted}");
        }

        public RecordedPauseAudioResult Complete(int resumeFrame)
        {
            if (pausedFrames == 0)
                throw new InvalidDataException($"Pause at recorded frame {EntryFrame} never reached PausedB.");
            return new RecordedPauseAudioResult(
                EntryFrame,
                resumeFrame,
                RoomPointer,
                PauseOwnedFrames,
                minimumRms,
                maximumRms,
                sumRms / pausedFrames,
                AudioCommands,
                string.Join(';', commandTrace));
        }
    }

    private static void WriteCapture(Rgba32[]? pixels, string directory, string fileName)
    {
        if (pixels is null)
            throw new InvalidDataException($"Replay did not produce capture '{fileName}'.");
        PngWriter.WriteRgba(
            Path.Combine(directory, fileName),
            FrontendFrame.Width,
            FrontendFrame.Height,
            pixels);
    }
}
