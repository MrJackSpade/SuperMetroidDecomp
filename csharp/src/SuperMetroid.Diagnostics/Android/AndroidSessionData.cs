using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Input;
using System.Text.Json;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Saves;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Diagnostics.Android;

/// <summary>
/// Worker-owned game and diagnostic state. No Activity/View references are persisted.
/// A reset recording starts from SRAM; a recording after a state load includes the exact
/// seed file alongside it so desktop replay need not invent a reset-time equivalent.
/// </summary>
internal sealed class AndroidSessionData : IDisposable
{
    /// <summary>JSON options: Indented output.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    /// <summary>Installation root used for saves, states, and recordings.</summary>
    private readonly string root;
    /// <summary>Atomic JSON save path for the session's live SRAM.</summary>
    private readonly string savePath;
    /// <summary>Installed audio catalog retained across debugger state loads.</summary>
    private readonly ClipAudioCatalog assets;
    /// <summary>Installed map presentation used for save migration and persistence.</summary>
    private readonly SuperMetroid.Core.Frontend.Map.Artwork.AreaMapPresentationCatalog maps;
    /// <summary>Installed projectile art contributing to the session content identity.</summary>
    private readonly SuperMetroid.AssetExtraction.Samus.InstalledProjectilePresentation projectiles;
    /// <summary>Installed visual catalog rebound to a game loaded from debugger state.</summary>
    private readonly GameArtwork artwork;
    /// <summary>State store tied to the session's options and installed content identity.</summary>
    private readonly DebuggerSaveStateStore states;
    /// <summary>Input recorder replaced after each successful debugger state load.</summary>
    private ControllerInputRecorder recorder;

    /// <summary>Opens installed assets and RAM state, binds art and audio, and begins a reset-seeded recording.</summary>
    public AndroidSessionData(string root, string? cartridgePath = null, string? audioDirectory = null)
    {
        this.root = root;
        // A caller-provided cartridge is an import input only. The session always
        // opens the extracted installation and a RAM-only address space.
        if (cartridgePath is not null)
            SuperMetroid.AssetExtraction.Installation.GameAssetInstaller.Install(cartridgePath, root);
        var installation = new SuperMetroid.AssetExtraction.Installation.GameInstallation(root);
        savePath = Path.Combine(root, "SuperMetroid.save.json");
        string ini = Path.Combine(root, "SuperMetroid.ini");
        if (!File.Exists(ini)) File.WriteAllText(ini, HostOptionsIni.DefaultFileContents);
        Options = HostOptionsIni.Parse(File.ReadAllText(ini), ini);
        Bus = installation.OpenRuntimeAddressSpace();
        maps = installation.LoadMaps();
        SaveSlotStore saves = GameSaveFileStore.LoadOrMigrate(savePath, Path.Combine(root, "SuperMetroid.srm"), maps).Saves;
        saves = AndroidFileImport.ActivatePendingSave(root, savePath, maps) ?? saves;
        // A caller-provided cartridge has already passed through the importer;
        // the running session binds only the installed presentation catalog.
        projectiles = installation.LoadProjectiles();
        artwork = installation.LoadGameArtwork(maps, projectiles);
        Game = GameSessionComposer.Create(saves, Options.Game, artwork);
        Console.WriteLine($"Projectile compositions: stock={projectiles.StockSha256}, selected={projectiles.SelectedSha256} ({root}).");
        Game.SaveRamChanged += PersistSave;
        assets = audioDirectory is null
            ? installation.LoadAudioClips()
            : ClipAudioCatalog.Load(audioDirectory);
        ContentIdentity = SuperMetroid.AssetExtraction.Installation.GameContentIdentity.Create(
            assets, maps, projectiles, SuperMetroid.AssetExtraction.Installation.InstalledArtworkIdentity.Create(artwork));
        Console.WriteLine(
            $"Installed content: {ContentIdentity.CompositeSha256}; " +
            $"definitions={ContentIdentity.CompiledDefinitionsBuildId:D}, " +
            $"audio={ContentIdentity.AudioContentSha256}, maps={ContentIdentity.MapContentSha256}, " +
            $"projectiles={ContentIdentity.ProjectileContentSha256}.");
        foreach ((string domain, string digest) in ContentIdentity.AdditionalContentSha256.OrderBy(pair => pair.Key))
            Console.WriteLine($"Installed content component: {domain}={digest}.");
        Game.BindAudio(assets);
        states = DebuggerSaveStateStore.ForInstalledGame(
            root,
            Options.Game,
            ContentIdentity);
        recorder = StartRecorder();
        WriteRecordingMetadata(seedFile: null);
    }

    /// <summary>Parsed host options; their game values are shared by the runtime, the debugger state store and recordings.</summary>
    public HostOptions Options { get; }
    /// <summary>Live RAM-only address space, replaced when a debugger state is loaded.</summary>
    public SuperMetroidAddressSpace Bus { get; private set; }
    /// <summary>Live game graph owned by the session worker.</summary>
    public SuperMetroidGame Game { get; private set; }
    /// <summary>Fingerprint of installed audio, maps, projectiles, and artwork.</summary>
    public SuperMetroid.AssetExtraction.Installation.GameContentIdentity ContentIdentity { get; }
    /// <summary>Capture generation incremented whenever the live game graph is replaced.</summary>
    public long Generation { get; private set; } = 1;

    /// <summary>Appends one consumed controller word to the active session recording.</summary>
    public void Record(SnesButton input) => recorder.RecordFrame(input);
    /// <summary>Atomically writes the live saves as JSON.</summary>
    public void PersistSave() => GameSaveFileStore.WriteAtomic(Game.Saves, savePath);
    /// <summary>Flushes accepted input frames after a worker failure.</summary>
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    /// <summary>Stages an external debugger state into the selected Android slot.</summary>
    public string ImportState(string path, int slot) =>
        AndroidFileImport.ImportState(root, path, slot);
    /// <summary>Stages a regular save file for import on the next session open.</summary>
    public string ImportSave(string path) =>
        AndroidFileImport.StageRegularSave(root, path);

    /// <summary>Writes an automatic state only after an eligible completed door transition.</summary>
    public void SaveCompletedDoor(SuperMetroidGameState previousState) =>
        DoorTransitionAutosave.TrySave(Options.DoorTransitionAutosave, replay: false,
            previousState, states, Bus, Game);

    /// <summary>Saves a manual debugger slot and flushes its accompanying input recording.</summary>
    public string SaveSlot(int slot)
    {
        if (slot == DebuggerStateFormat.AutomaticSlot)
            return "The auto slot is written at completed door transitions. Select 0-9 for a manual save.";
        DebuggerSaveStateMetadata metadata = states.Save(slot, Bus, Game);
        FlushRecording();
        return $"Saved slot {slot}, frame {metadata.FrameNumber}, room {metadata.Room}.";
    }

    /// <summary>Replaces the game, RAM, and audio graph with a validated slot and starts a seeded recording.</summary>
    public string LoadSlot(int slot)
    {
        if (!states.TryLoad(slot, out DebuggerSaveStateLoadResult loaded))
            return $"Slot {DebuggerSaveStateStore.SlotName(slot)} is empty. No state to load.";
        // All decoding/validation happens before replacing the live game, including the
        // audio binding, which rejects a state whose session never bound clips. Retain the
        // precise seed before future saves can overwrite this user-visible slot.
        loaded.Game.BindAudio(assets);
        recorder.Dispose();
        Bus = loaded.AddressSpace;
        Game = loaded.Game;
        GameSessionComposer.Rebind(Game, artwork);
        Game.SaveRamChanged += PersistSave;
        Generation++;
        recorder = StartRecorder();
        string seed = Path.ChangeExtension(recorder.Path, ".seed.smstate");
        File.Copy(states.GetSlotPath(slot), seed, overwrite: false);
        WriteRecordingMetadata(Path.GetFileName(seed));
        return $"Loaded slot {DebuggerSaveStateStore.SlotName(slot)}, frame {loaded.Metadata.FrameNumber}." +
            (loaded.Warnings.Count == 0 ? "" : "\nWARNING: " + string.Join("\n", loaded.Warnings));
    }

    /// <summary>Writes content and assembly identity alongside a reset or debugger-seeded recording.</summary>
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
        }, IndentedJson));
        FlushRecording();
    }

    /// <summary>Begins an installed-content recording seeded from the SRAM image of the current saves.</summary>
    private ControllerInputRecorder StartRecorder() =>
        ControllerInputRecorder.StartInstalled(
            root,
            SaveRamCodec.Encode(Game.Saves, maps),
            Options.Game,
            ContentIdentity);

    /// <summary>Closes the active input recording and finishes queued state writes when the worker session ends.</summary>
    public void Dispose()
    {
        recorder.Dispose();
        states.Dispose();
    }
}
