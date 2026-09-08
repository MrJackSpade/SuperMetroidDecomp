using System.Text.Json;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

namespace SuperMetroid.Android;

/// <summary>
/// Worker-owned game and diagnostic state. No Activity/View references are persisted.
/// A reset recording starts from SRAM; a recording after a state load includes the exact
/// seed file alongside it so desktop replay need not invent a reset-time equivalent.
/// </summary>
internal sealed class AndroidSessionData : IDisposable
{
    private readonly string root;
    private readonly string romPath;
    private readonly string savePath;
    private readonly ExtractedAudioAssetCatalog assets;
    private readonly DebuggerSaveStateStore states;
    private ControllerInputRecorder recorder;

    public AndroidSessionData(string root, string? cartridgePath = null, string? audioDirectory = null)
    {
        this.root = root;
        string gameRoot = Path.Combine(root, "game");
        romPath = cartridgePath ?? Path.Combine(gameRoot, "SuperMetroid.smc");
        savePath = Path.Combine(root, "SuperMetroid.save.json");
        string ini = Path.Combine(root, "SuperMetroid.ini");
        if (!File.Exists(ini)) File.WriteAllText(ini, SuperMetroidGameOptionsIni.DefaultFileContents);
        Options = SuperMetroidGameOptionsIni.Parse(File.ReadAllText(ini), ini);
        Bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        GameSaveFileStore.LoadOrMigrate(Bus, savePath, Path.Combine(root, "SuperMetroid.srm"));
        AndroidFileImport.ActivatePendingSave(root, Bus, savePath);
        Game = new SuperMetroidGame(Bus, Options);
        Game.SaveRamChanged += PersistSave;
        assets = ExtractedAudioAssetCatalog.Load(audioDirectory ?? Path.Combine(gameRoot, "audio"));
        Audio = new CartridgeAudioRenderer(assets);
        states = new DebuggerSaveStateStore(romPath, Bus.Rom, Path.Combine(root, "debug-states"));
        recorder = StartRecorder();
        WriteRecordingMetadata(seedFile: null);
    }

    public SuperMetroidGameOptions Options { get; }
    public SuperMetroidAddressSpace Bus { get; private set; }
    public SuperMetroidGame Game { get; private set; }
    public CartridgeAudioRenderer Audio { get; private set; }
    public long Generation { get; private set; } = 1;

    public void Record(ushort input) => recorder.RecordFrame(input);
    public void PersistSave() => GameSaveFileStore.WriteAtomic(Bus, savePath);
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    public string ImportState(string path, int slot) => AndroidFileImport.ImportState(root, romPath, path, slot);
    public string ImportSave(string path) => AndroidFileImport.StageRegularSave(root, romPath, path);

    public string SaveSlot(int slot)
    {
        DebuggerSaveStateMetadata metadata = states.Save(slot, Bus, Game, Audio.Player);
        FlushRecording();
        return $"Saved slot {slot}, frame {metadata.FrameNumber}, room {metadata.RoomPointer:X4}.";
    }

    public string LoadSlot(int slot)
    {
        if (!states.TryLoad(slot, out DebuggerSaveStateLoadResult loaded))
            return $"Slot {slot} is empty. No state to load.";
        if (loaded.AudioPlayer is null)
            throw new InvalidDataException("This state has no managed audio graph; cannot resume its audio accurately.");

        // All decoding/validation happens before replacing the live game. Retain the
        // precise seed before future saves can overwrite this user-visible slot.
        recorder.Dispose();
        Bus = loaded.AddressSpace;
        Game = loaded.Game;
        Game.SaveRamChanged += PersistSave;
        Audio = new CartridgeAudioRenderer(assets, loaded.AudioPlayer);
        Generation++;
        recorder = StartRecorder();
        string seed = Path.ChangeExtension(recorder.Path, ".seed.smstate");
        File.Copy(states.GetSlotPath(slot), seed, overwrite: false);
        WriteRecordingMetadata(Path.GetFileName(seed));
        return $"Loaded slot {slot}, frame {loaded.Metadata.FrameNumber}." +
            (loaded.Warnings.Count == 0 ? "" : "\nWARNING: " + string.Join("\n", loaded.Warnings));
    }

    private void WriteRecordingMetadata(string? seedFile)
    {
        // The .smrec format is unchanged. This explicit sidecar identifies whether replay
        // starts at reset or from an exact graph and records the build which generated it.
        File.WriteAllText(Path.ChangeExtension(recorder.Path, ".json"), JsonSerializer.Serialize(new
        {
            format = "SuperMetroid.Android.RecordingSeed.v1",
            recording = Path.GetFileName(recorder.Path),
            seedFile,
            coreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            diagnosticsBuild = typeof(DebuggerSaveStateStore).Module.ModuleVersionId,
            hostBuild = typeof(AndroidSessionData).Module.ModuleVersionId,
        }, new JsonSerializerOptions { WriteIndented = true }));
        FlushRecording();
    }

    private ControllerInputRecorder StartRecorder() => ControllerInputRecorder.Start(
        romPath, Bus.SaveRam, Options, Path.Combine(root, "input-recordings"));

    public void Dispose() => recorder.Dispose();
}
