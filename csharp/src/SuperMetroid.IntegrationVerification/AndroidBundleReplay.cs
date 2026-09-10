using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

/// <summary>
/// Replays one explicitly selected journal from an Android export without extracting
/// arbitrary ZIP paths or touching desktop saves. Seeded journals use their preserved
/// graph; reset journals use the SRAM/options embedded in the recording itself.
/// </summary>
internal static class AndroidBundleReplay
{
    public static int Run(string bundlePath, string recordingName)
    {
        if (Path.GetFileName(recordingName) != recordingName || !recordingName.EndsWith(".smrec", StringComparison.Ordinal))
            throw new ArgumentException("Select a recording basename ending in .smrec.", nameof(recordingName));
        using var bundle = ZipFile.OpenRead(bundlePath);
        using var journal = Required(bundle, "input-recordings/" + recordingName).Open();
        var recording = ControllerInputRecording.Read(journal);
        using var sidecar = Required(bundle, "input-recordings/" + Path.ChangeExtension(recordingName, ".json")).Open();
        using var metadata = JsonDocument.Parse(sidecar);
        if (metadata.RootElement.GetProperty("format").GetString() != "SuperMetroid.Android.RecordingSeed.v1" ||
            metadata.RootElement.GetProperty("recording").GetString() != recordingName)
            throw new InvalidDataException("Android recording metadata identity does not match the selected journal.");
        string? seed = metadata.RootElement.GetProperty("seedFile").GetString();
        if (seed is not null && (Path.GetFileName(seed) != seed || !seed.EndsWith(".seed.smstate", StringComparison.Ordinal)))
            throw new InvalidDataException("Recording seed must name a sibling preserved debugger state.");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        if (!SHA256.HashData(bus.Rom).AsSpan().SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("Local ROM does not match Android recording digest.");
        var assets = ExtractedAudioAssetCatalog.Load(Path.GetFullPath("standalone-assets/audio"));
        string temporary = Directory.CreateTempSubdirectory("SuperMetroid-android-replay-").FullName;
        try
        {
            SuperMetroidGame game;
            CartridgeAudioRenderer audio;
            if (seed is null)
            {
                recording.InitialSaveRam.CopyTo(bus.SaveRam);
                game = new SuperMetroidGame(bus, recording.GameOptions);
                audio = new CartridgeAudioRenderer(assets);
            }
            else
            {
                var store = new DebuggerSaveStateStore(romPath, bus.Rom, temporary);
                using (var output = File.Create(store.GetSlotPath(0)))
                using (var input = Required(bundle, "input-recordings/" + seed).Open()) input.CopyTo(output);
                var loaded = store.Load(0);
                bus = loaded.AddressSpace;
                game = loaded.Game;
                audio = new CartridgeAudioRenderer(assets, loaded.AudioPlayer
                    ?? throw new InvalidDataException("Android seed omitted its managed audio graph."));
                if (!bus.SaveRam.SequenceEqual(recording.InitialSaveRam))
                    throw new InvalidDataException("Preserved seed SRAM differs from the journal reset header.");
            }

            using var videoHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var audioHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            for (int tick = 0; tick < recording.ControllerInputs.Length; tick++)
            {
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                var frame = game.StepCaptured(recording.ControllerInputs[tick], tick + 1, 1);
                var pixels = frame.Snapshot is { } snapshot
                    ? SoftwareFrameSnapshotRenderer.Render(snapshot) : frame.Frame.Pixels;
                short[] samples = audio.RenderFrame(frame.Frame.AudioCommands);
                videoHash.AppendData(MemoryMarshal.AsBytes(pixels.AsSpan()));
                audioHash.AppendData(MemoryMarshal.AsBytes(samples.AsSpan()));
            }
            Console.WriteLine($"Android bundle replay: {recordingName}; seed={seed ?? "reset SRAM"}; frames={recording.ControllerInputs.Length}; final={game.GameState}; room={game.GameplayActiveRoomPointer:X4}");
            Console.WriteLine($"Video SHA256: {Convert.ToHexString(videoHash.GetHashAndReset())}");
            Console.WriteLine($"Audio SHA256: {Convert.ToHexString(audioHash.GetHashAndReset())}");
            return 0;
        }
        finally
        {
            // The only extracted state lives in this unique, locally created directory.
            Directory.Delete(temporary, recursive: true);
        }
    }

    private static ZipArchiveEntry Required(ZipArchive bundle, string name)
    {
        var entries = bundle.Entries.Where(entry => entry.FullName == name).ToArray();
        return entries.Length == 1 ? entries[0]
            : throw new InvalidDataException($"Bundle must contain exactly one '{name}', found {entries.Length}.");
    }
}
