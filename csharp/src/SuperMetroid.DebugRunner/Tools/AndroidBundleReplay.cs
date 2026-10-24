using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Diagnostics.Recording;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>
/// Replays one explicitly selected journal from an Android export without extracting
/// arbitrary ZIP paths or touching desktop saves. Seeded journals use their preserved
/// graph; reset journals use the SRAM/options embedded in the recording itself.
/// </summary>
internal static class AndroidBundleReplay
{
    /// <summary>Replays a selected Android journal from its saved seed or reset SRAM and prints video and audio digests.</summary>
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
        var importBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        SuperMetroidAddressSpace bus = importBus;
        if (!SHA256.HashData(importBus.Rom).SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("Local ROM does not match Android recording digest.");
        ClipAudioCatalog assets = RepositoryInstallation.Installation.LoadAudioClips();
        SuperMetroid.Core.Frontend.Map.Artwork.AreaMapPresentationCatalog maps = RepositoryInstallation.Installation.LoadMaps();
        SaveSlotStore journalSaves = SaveRamCodec.Decode(recording.InitialSaveRam, maps);
        string temporary = Directory.CreateTempSubdirectory("SuperMetroid-android-replay-").FullName;
        try
        {
            SuperMetroidGame game;
            if (seed is null)
            {
                game = new SuperMetroidGame(journalSaves, recording.GameOptions);
            }
            else
            {
                GameInstallation installation = RepositoryInstallation.Installation;
                var identity = GameContentIdentity.Create(installation.LoadAudioClips(), installation.LoadMaps(), installation.LoadProjectiles());
                var store = DebuggerSaveStateStoreTooling.ForCartridge(romPath, importBus.Rom, identity, temporary);
                using (var output = File.Create(store.GetSlotPath(0)))
                using (var input = Required(bundle, "input-recordings/" + seed).Open()) input.CopyTo(output);
                var loaded = store.Load(0);
                bus = loaded.AddressSpace;
                game = loaded.Game;
                // Compare decoded saves: the journal's image may hold padding or noncanonical marker bytes the store
                // does not keep, so its canonical encoding is what a matching seed reproduces.
                if (!SaveRamCodec.Encode(game.Saves, maps).SequenceEqual(SaveRamCodec.Encode(journalSaves, maps)))
                    throw new InvalidDataException("Preserved seed saves differ from the journal reset header.");
            }

            game.BindAudio(assets);
            var samples = new short[CartridgeAudioSession.PcmSamplesPerFrame];
            using var videoHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var audioHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            for (int tick = 0; tick < recording.ControllerInputs.Length; tick++)
            {
                var frame = game.StepCaptured(recording.InputAt(tick), tick + 1, 1);
                var pixels = SoftwareFrameSnapshotRenderer.Render(frame.Snapshot);
                game.CopyAudioFrame(samples);
                videoHash.AppendData(MemoryMarshal.AsBytes(pixels.AsSpan()));
                audioHash.AppendData(MemoryMarshal.AsBytes(samples.AsSpan()));
            }
            Console.WriteLine($"Android bundle replay: {recordingName}; seed={seed ?? "reset SRAM"}; frames={recording.ControllerInputs.Length}; final={game.GameState}; room={game.GameplayActiveRoom?.Name}");
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

    /// <summary>Finds exactly one ZIP entry by its fixed internal path, rejecting absent or duplicate entries.</summary>
    private static ZipArchiveEntry Required(ZipArchive bundle, string name)
    {
        var entries = bundle.Entries.Where(entry => entry.FullName == name).ToArray();
        return entries.Length == 1 ? entries[0]
            : throw new InvalidDataException($"Bundle must contain exactly one '{name}', found {entries.Length}.");
    }
}
