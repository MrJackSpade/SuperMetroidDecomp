using System.Security.Cryptography;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

/// <summary>Restores progress from recorded inputs with the real host audio and frame-publication paths.</summary>
internal static class RecordingRecoveryState
{
    public static int Export(string recordingPath, string romPath, string audioDirectory, string destination, int lastFrame)
    {
        var recording = ControllerInputRecording.Read(recordingPath);
        if (lastFrame < 0 || lastFrame >= recording.ControllerInputs.Length)
            throw new ArgumentOutOfRangeException(nameof(lastFrame));
        if (!SHA256.HashData(File.ReadAllBytes(romPath)).AsSpan().SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("Recording ROM identity differs from recovery ROM.");
        // Refuse an existing directory so recovery never overwrites a player's slot.
        if (Directory.Exists(destination)) throw new IOException("Recovery destination already exists.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: true);
        var assets = ExtractedAudioAssetCatalog.Load(audioDirectory);
        var audio = new CartridgeAudioRenderer(assets);
        for (int frame = 0; frame <= lastFrame; frame++)
        {
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            var result = game.StepCaptured(recording.ControllerInputs[frame], frame + 1, 1);
            audio.RenderFrame(result.Frame.AudioCommands);
            if (frame % 3000 == 0 || frame == lastFrame)
                Console.WriteLine($"Recovered input {frame}: room={game.GameplayActiveRoomPointer:X4}, state={game.GameState}");
        }
        var store = new DebuggerSaveStateStore(romPath, bus.Rom, destination);
        if (game.GetRetainedDisplay(lastFrame + 2, 1)?.Layers is null)
            throw new InvalidDataException("Recovery requires a captured gameplay scene, not a stale menu display.");
        var metadata = store.Save(1, bus, game, audio.Player);
        Console.WriteLine($"Saved {metadata}");
        return Verify(recordingPath, romPath, audioDirectory, destination, lastFrame);
    }

    public static int Verify(string recordingPath, string romPath, string audioDirectory, string destination, int lastFrame)
    {
        var recording = ControllerInputRecording.Read(recordingPath);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var assets = ExtractedAudioAssetCatalog.Load(audioDirectory);
        var store = new DebuggerSaveStateStore(romPath, bus.Rom, destination);
        // Deserialize the exact file and advance past its boundary using the recorded
        // next input. This verifies the exported graph, not just the live replay.
        var loaded = store.Load(1);
        Console.WriteLine($"Reload warnings: {string.Join("; ", loaded.Warnings)}");
        if (loaded.Game.GetRetainedDisplay(lastFrame + 2, 2)?.Layers is null)
            throw new InvalidDataException("Reloaded state lacks its gameplay scene.");
        Console.WriteLine($"Loaded Core {typeof(SuperMetroidGame).Assembly.GetName().Version}: room={loaded.Game.GameplayActiveRoomPointer:X4}");
        var restoredAudio = new CartridgeAudioRenderer(assets, loaded.AudioPlayer);
        for (int frame = lastFrame + 1; frame < recording.ControllerInputs.Length; frame++)
        {
            loaded.Game.SetAudioAcknowledgements(restoredAudio.ReadAcknowledgements());
            var result = loaded.Game.StepCaptured(recording.ControllerInputs[frame], frame + 1, 2);
            restoredAudio.RenderFrame(result.Frame.AudioCommands);
            if (result.Snapshot is null) throw new InvalidDataException("Recovered frame lacks a render snapshot.");
        }
        Console.WriteLine("Recovery state reload and remaining recorded inputs passed.");
        return 0;
    }
}
