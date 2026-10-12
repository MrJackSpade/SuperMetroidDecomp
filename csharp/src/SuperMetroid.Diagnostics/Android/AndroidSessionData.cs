using System.Text.Json;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
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
    private readonly string savePath;
    private readonly ExtractedAudioAssetCatalog assets;
    private readonly SuperMetroid.Core.Assets.AreaMapPresentationCatalog maps;
    private readonly SuperMetroid.AssetExtraction.InstalledProjectilePresentation projectiles;
    private readonly GameArtwork artwork;
    private readonly DebuggerSaveStateStore states;
    private ControllerInputRecorder recorder;

    public AndroidSessionData(string root, string? cartridgePath = null, string? audioDirectory = null)
    {
        this.root = root;
        // A caller-provided cartridge is an import input only. The session always
        // opens the extracted installation and a RAM-only address space.
        if (cartridgePath is not null)
            SuperMetroid.AssetExtraction.GameAssetInstaller.Install(cartridgePath, root);
        var installation = new SuperMetroid.AssetExtraction.GameInstallation(root);
        savePath = Path.Combine(root, "SuperMetroid.save.json");
        string ini = Path.Combine(root, "SuperMetroid.ini");
        if (!File.Exists(ini)) File.WriteAllText(ini, SuperMetroidGameOptionsIni.DefaultFileContents);
        Options = SuperMetroidGameOptionsIni.Parse(File.ReadAllText(ini), ini);
        Bus = installation.OpenRuntimeAddressSpace();
        maps = installation.LoadMaps();
        GameSaveFileStore.LoadOrMigrate(Bus, savePath, Path.Combine(root, "SuperMetroid.srm"), maps);
        AndroidFileImport.ActivatePendingSave(root, Bus, savePath, maps);
        Game = new SuperMetroidGame(Bus, Options);
        // A caller-provided cartridge has already passed through the importer;
        // the running session binds only the installed presentation catalog.
        projectiles = installation.LoadProjectiles();
        artwork = installation.LoadGameArtwork(maps, projectiles);
        Game.BindArtwork(artwork);
        Console.WriteLine($"Projectile compositions: stock={projectiles.StockSha256}, selected={projectiles.SelectedSha256} ({root}).");
        Game.SaveRamChanged += PersistSave;
        assets = audioDirectory is null
            ? installation.LoadAudio()
            : ExtractedAudioAssetCatalog.Load(audioDirectory);
        ContentIdentity = SuperMetroid.AssetExtraction.GameContentIdentity.Create(
            assets, maps, projectiles, SuperMetroid.AssetExtraction.InstalledArtworkIdentity.Create(artwork));
        Console.WriteLine(
            $"Installed content: {ContentIdentity.CompositeSha256}; " +
            $"definitions={ContentIdentity.CompiledDefinitionsBuildId:D}, " +
            $"audio={ContentIdentity.AudioContentSha256}, maps={ContentIdentity.MapContentSha256}, " +
            $"projectiles={ContentIdentity.ProjectileContentSha256}.");
        foreach ((string domain, string digest) in ContentIdentity.AdditionalContentSha256.OrderBy(pair => pair.Key))
            Console.WriteLine($"Installed content component: {domain}={digest}.");
        Audio = new CartridgeAudioRenderer(assets);
        states = DebuggerSaveStateStore.ForInstalledGame(
            root,
            Options,
            ContentIdentity);
        recorder = StartRecorder();
        WriteRecordingMetadata(seedFile: null);
    }

    public SuperMetroidGameOptions Options { get; }
    public SuperMetroidAddressSpace Bus { get; private set; }
    public SuperMetroidGame Game { get; private set; }
    public CartridgeAudioRenderer Audio { get; private set; }
    public SuperMetroid.AssetExtraction.GameContentIdentity ContentIdentity { get; }
    public long Generation { get; private set; } = 1;

    public void Record(ushort input) => recorder.RecordFrame(input);
    public void PersistSave() => GameSaveFileStore.WriteAtomic(Bus, savePath, maps);
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    public string ImportState(string path, int slot) =>
        AndroidFileImport.ImportState(root, path, slot);
    public string ImportSave(string path) =>
        AndroidFileImport.StageRegularSave(root, path);

    public void SaveCompletedDoor(SuperMetroidGameState previousState) =>
        DoorTransitionAutosave.TrySave(Options.DoorTransitionAutosave, replay: false,
            previousState, states, Bus, Game, Audio.Player);

    public string SaveSlot(int slot)
    {
        if (slot == DebuggerStateFormat.AutomaticSlot)
            return "The auto slot is written at completed door transitions. Select 0-9 for a manual save.";
        DebuggerSaveStateMetadata metadata = states.Save(slot, Bus, Game, Audio.Player);
        FlushRecording();
        return $"Saved slot {slot}, frame {metadata.FrameNumber}, room {metadata.RoomPointer:X4}.";
    }

    public string LoadSlot(int slot)
    {
        if (!states.TryLoad(slot, out DebuggerSaveStateLoadResult loaded))
            return $"Slot {DebuggerSaveStateStore.SlotName(slot)} is empty. No state to load.";
        if (loaded.AudioPlayer is null)
            throw new InvalidDataException("This state has no managed audio graph; cannot resume its audio accurately.");

        // All decoding/validation happens before replacing the live game. Retain the
        // precise seed before future saves can overwrite this user-visible slot.
        recorder.Dispose();
        Bus = loaded.AddressSpace;
        Game = loaded.Game;
        Game.BindArtwork(artwork);
        Game.SaveRamChanged += PersistSave;
        Audio = new CartridgeAudioRenderer(assets, loaded.AudioPlayer);
        Generation++;
        recorder = StartRecorder();
        string seed = Path.ChangeExtension(recorder.Path, ".seed.smstate");
        File.Copy(states.GetSlotPath(slot), seed, overwrite: false);
        WriteRecordingMetadata(Path.GetFileName(seed));
        return $"Loaded slot {DebuggerSaveStateStore.SlotName(slot)}, frame {loaded.Metadata.FrameNumber}." +
            (loaded.Warnings.Count == 0 ? "" : "\nWARNING: " + string.Join("\n", loaded.Warnings));
    }

    private void WriteRecordingMetadata(string? seedFile)
    {
        // The recording embeds installed-content identity. This sidecar additionally
        // identifies whether replay starts at reset or from an exact debugger graph and
        // records the Android-specific assemblies which generated that graph.
        File.WriteAllText(Path.ChangeExtension(recorder.Path, ".json"), JsonSerializer.Serialize(new
        {
            format = "SuperMetroid.Android.RecordingSeed.v1",
            recording = Path.GetFileName(recorder.Path),
            seedFile,
            coreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            diagnosticsBuild = typeof(DebuggerSaveStateStore).Module.ModuleVersionId,
            hostBuild = typeof(AndroidSessionData).Module.ModuleVersionId,
            contentIdentity = ContentIdentity,
        }, new JsonSerializerOptions { WriteIndented = true }));
        FlushRecording();
    }

    private ControllerInputRecorder StartRecorder() =>
        ControllerInputRecorder.StartInstalled(
            root,
            SaveRamImage.Capture(Bus),
            Options,
            ContentIdentity);

    public void Dispose() => recorder.Dispose();
}
