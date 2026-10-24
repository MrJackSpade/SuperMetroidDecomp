using SuperMetroid.Core.Input;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>
/// Replays one deterministic controller journal with native SPC acknowledgements but no GUI.
/// </summary>
public static class AudioInputReplaySmokeTest
{
    /// <summary>Maximum neutral-input updates allowed for a journal ending before its first door finishes.</summary>
    private const int NeutralTailFrames = 360;

    /// <summary>Replays a ROM-validated journal through managed gameplay/audio until its first door completes, checking projectile firing and the power-beam acknowledgement.</summary>
    /// <param name="recordingPath">Controller journal supplying initial SRAM, game options, and controller words.</param>
    /// <param name="romPath">Retail cartridge whose SHA-256 must match the recording.</param>
    /// <param name="captureDirectory">Optional directory for diagnostic source-room and completed-door PNGs; null or blank disables capture.</param>
    /// <remarks>Allows up to 360 neutral-input updates after the journal ends to finish the door transition; this is not a complete-recording replay.</remarks>
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

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(fullRomPath);
        var game = new SuperMetroidGame(SuperMetroid.Diagnostics.Saves.SaveRamCodec.Decode(recording.InitialSaveRam, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), recording.GameOptions);
        game.BindAudio(DesktopAccess.LoadAudioClips());

        int framesExecuted = 0;
        bool projectileFired = false;
        bool powerBeamCommandObserved = false;
        bool powerBeamAcknowledged = false;
        bool firstDoorBegan = false;
        bool firstDoorCompleted = false;
        RoomHeaderDefinition? sourceRoom = null;
        RoomHeaderDefinition? destinationRoom = null;
        Rgba32[]? firstSourceFrame = null;
        Rgba32[]? lastSourceGameplayFrame = null;
        Rgba32[]? completedDestinationFrame = null;

        // Continue briefly with neutral input after the journal ends. A recording stopped
        // while state $0B was waiting cannot itself contain the later fade/scroll frames;
        // the neutral tail proves the transition now makes autonomous forward progress.
        int totalAvailableFrames = recording.ControllerInputs.Length + NeutralTailFrames;
        for (int frameIndex = 0; frameIndex < totalAvailableFrames; frameIndex++)
        {
            SnesButton input = frameIndex < recording.ControllerInputs.Length
                ? recording.InputAt(frameIndex)
                : SnesButton.None;
            FrontendFrame frame = game.Step(input);
            SoundAcknowledgements acknowledgements = game.AudioAcknowledgementsForVerification;
            framesExecuted++;

            RoomHeaderDefinition? activeRoom = game.GameplayActiveRoom;
            if (sourceRoom is null && activeRoom is { } firstRoom)
                sourceRoom = firstRoom;
            if (sourceRoom is not null &&
                activeRoom == sourceRoom &&
                game.GameState == SuperMetroidGameState.MainGameplay)
            {
                // Clone because SuperMetroidGame may replace or reuse its current frame on
                // the following call. These are diagnostics only; replay state never reads
                // pixels back and therefore remains independent of an optional output path.
                firstSourceFrame ??= game.PublishedPixels();
                lastSourceGameplayFrame = game.PublishedPixels();
            }

            projectileFired |= game.GameplayLastFiredProjectileSlot is not null;
            powerBeamCommandObserved |= game.AudioEventsForVerification.Any(audioEvent =>
                audioEvent is AudioEvent.StartSoundEffect { SoundEffect: var started } &&
                started == SoundEffectLibrary1Sounds.PowerBeam);
            powerBeamAcknowledged |= acknowledgements[0] == 0x0b;

            if (game.GameState == SuperMetroidGameState.LoadingNextRoomB)
                firstDoorBegan = true;
            if (firstDoorBegan && activeRoom is { } room && room != sourceRoom)
                destinationRoom = room;
            if (destinationRoom is not null &&
                game.GameState == SuperMetroidGameState.MainGameplay)
            {
                firstDoorCompleted = true;
                completedDestinationFrame = game.PublishedPixels();
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
                $"source={sourceRoom?.Name}, destination={destinationRoom?.Name}, " +
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
            sourceRoom?.Name,
            destinationRoom?.Name);
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

        var game = new SuperMetroidGame(SuperMetroid.Diagnostics.Saves.SaveRamCodec.Decode(recording.InitialSaveRam, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), recording.GameOptions);
        game.BindAudio(DesktopAccess.LoadAudioClips());
        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            try
            {
                game.Step(recording.InputAt(frameIndex));
            }
            catch (Exception exception)
            {
                throw new InvalidDataException(
                    $"Replay with managed audio failed on recorded call {frameIndex}, " +
                    $"state=${(ushort)game.GameState:X2}, " +
                    $"room={game.GameplayActiveRoom?.Name}.",
                    exception);
            }
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
        string romPath,
        int maximumFrames = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFrames);
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] actualRomDigest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            actualRomDigest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(actualRomDigest, recording.RomSha256))
            throw new InvalidDataException("Replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(fullRomPath);
        var game = new SuperMetroidGame(SuperMetroid.Diagnostics.Saves.SaveRamCodec.Decode(recording.InitialSaveRam, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), recording.GameOptions);
        game.BindAudio(DesktopAccess.LoadAudioClips());
        var samples = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var completed = new List<RecordedPauseAudioResult>();
        PauseAccumulator? active = null;

        int framesToReplay = Math.Min(recording.ControllerInputs.Length, maximumFrames);
        for (int frameIndex = 0; frameIndex < framesToReplay; frameIndex++)
        {
            FrontendFrame frame = game.Step(recording.InputAt(frameIndex));
            bool pauseOwned = frame.GameState is >= SuperMetroidGameState.PausingDarkening and
                <= SuperMetroidGameState.Unpausing;
            game.CopyAudioFrame(samples);

            if (pauseOwned && active is null)
            {
                active = new PauseAccumulator(
                    frameIndex,
                    game.GameplayActiveRoom?.Name ?? throw new InvalidDataException("Pause began without an active room."));
            }
            if (active is { } pause)
            {
                AudioEvent[] events = [.. game.AudioEventsForVerification.Where(audioEvent => audioEvent is not AudioEvent.DriverTick)];
                pause.AudioEvents += events.Length;
                if (events.Length != 0)
                    pause.RecordEvents(frameIndex, frame.GameState, events);
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

    /// <summary>Accumulates one recorded pause's PCM levels and audio commands until gameplay resumes.</summary>
    private sealed class PauseAccumulator
    {
        /// <summary>Lower bound of fully paused frame RMS levels.</summary>
        private double minimumRms = double.MaxValue;
        /// <summary>Upper bound of fully paused frame RMS levels.</summary>
        private double maximumRms;
        /// <summary>Sum of fully paused frame RMS levels for the per-frame mean.</summary>
        private double sumRms;
        /// <summary>Number of fully paused audio frames included in RMS measurements.</summary>
        private int pausedFrames;
        /// <summary>Frame/state-tagged command lines in recorded order.</summary>
        private readonly List<string> commandTrace = [];

        /// <summary>Starts a pause interval at its recorded entry frame and room.</summary>
        public PauseAccumulator(int entryFrame, string room)
        {
            EntryFrame = entryFrame;
            Room = room;
        }

        /// <summary>Recorded update at which this pause began.</summary>
        public int EntryFrame { get; }
        /// <summary>Room retained across this pause interval.</summary>
        public string Room { get; }
        /// <summary>Count of frontend updates owned by pause states, including transitions.</summary>
        public int PauseOwnedFrames { get; set; }
        /// <summary>Number of bank loads, music commands and sound-effect starts during pause ownership.</summary>
        public int AudioEvents { get; set; }

        /// <summary>Measures one fully paused stereo PCM frame before waveOut receives it.</summary>
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

        /// <summary>Captures bank loads, music commands and sound-effect starts with their recorded frame and frontend state.</summary>
        public void RecordEvents(
            int frameIndex,
            SuperMetroidGameState state,
            IReadOnlyList<AudioEvent> events)
        {
            commandTrace.Add($"{frameIndex}/{state}={string.Join(',', events)}");
        }

        /// <summary>Finalizes a complete interval, requiring at least one fully paused PCM frame.</summary>
        public RecordedPauseAudioResult Complete(int resumeFrame)
        {
            if (pausedFrames == 0)
                throw new InvalidDataException($"Pause at recorded frame {EntryFrame} never reached PausedB.");
            return new RecordedPauseAudioResult(
                EntryFrame,
                resumeFrame,
                Room,
                PauseOwnedFrames,
                minimumRms,
                maximumRms,
                sumRms / pausedFrames,
                AudioEvents,
                string.Join(';', commandTrace));
        }
    }

    /// <summary>Writes an owned diagnostic gameplay frame as a 256-by-224 PNG after ensuring replay produced it.</summary>
    private static void WriteCapture(Rgba32[]? pixels, string directory, string fileName)
    {
        if (pixels is null)
            throw new InvalidDataException($"Replay did not produce capture '{fileName}'.");
        PngWriterTooling.WriteRgba(
            Path.Combine(directory, fileName),
            new Extent16<ScreenPixels>(FrontendFrame.Width, FrontendFrame.Height),
            pixels);
    }
}
