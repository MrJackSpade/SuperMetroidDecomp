using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Input;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Saves;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Diagnostics.Sessions;

/// <summary>
/// A game opened from an installation, with its saves, debugger states and input recording, for the tools that run it
/// headless to make state fixtures. A reset recording starts from SRAM; a recording after a state load includes the exact seed file alongside
/// it so replay need not invent a reset-time equivalent.
/// </summary>
internal sealed class InstalledGameSession : IDisposable
{
    /// <summary>JSON options: Indented output.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    /// <summary>Installation root used for saves, states, and recordings.</summary>
    private readonly string root;
    /// <summary>The JSON save file that keeps the session's saves.</summary>
    private readonly GameSaveFile saveFile;
    /// <summary>Installed artwork and audio, bound to every game this session composes or restores.</summary>
    private readonly InstalledSessionContent content;
    /// <summary>Installed map presentation used for save migration and persistence.</summary>
    private readonly SuperMetroid.Core.Frontend.Artwork.InstalledScreenContent maps;
    /// <summary>Installed projectile art contributing to the session content identity.</summary>
    private readonly SuperMetroid.AssetExtraction.Samus.InstalledProjectilePresentation projectiles;
    /// <summary>State store tied to the session's options and installed content identity.</summary>
    private readonly DebuggerSaveStateStore states;
    /// <summary>The session's input recording, seeded from the saves it opened with.</summary>
    private readonly ControllerInputRecorder recorder;

    /// <summary>Opens installed assets and saves, binds art and audio, and begins a reset-seeded recording.</summary>
    public InstalledGameSession(string root, string? cartridgePath = null, string? audioDirectory = null)
    {
        this.root = root;
        // A caller-provided cartridge is an import input only. The session always
        // opens the extracted installation, which the game runs on alone.
        if (cartridgePath is not null)
            SuperMetroid.AssetExtraction.Installation.GameAssetInstaller.Install(cartridgePath, root);
        var installation = new SuperMetroid.AssetExtraction.Installation.GameInstallation(root);
        string savePath = Path.Combine(root, "SuperMetroid.save.json");
        saveFile = new GameSaveFile(savePath);
        string ini = Path.Combine(root, "SuperMetroid.ini");
        if (!File.Exists(ini)) File.WriteAllText(ini, HostOptionsIni.DefaultFileContents);
        Options = HostOptionsIni.Parse(File.ReadAllText(ini), ini);
        installation.RequireExtractedContent();
        maps = installation.LoadMaps();
        SaveSlotStore saves = GameSaveFileStore.LoadOrMigrate(savePath, Path.Combine(root, "SuperMetroid.srm"), maps).Saves;
        // A caller-provided cartridge has already passed through the importer;
        // the running session binds only the installed presentation catalog.
        projectiles = installation.LoadProjectiles();
        var enemyArtwork = installation.LoadEnemyTiles();
        var samusBody = installation.LoadSamusBody();
        GameArtwork artwork = installation.LoadGameArtwork(maps, projectiles, enemyArtwork, samusBody);
        Console.WriteLine($"Projectile compositions: {projectiles.ContentSha256} ({root}).");
        InstalledClipAudioLibrary assets = audioDirectory is null
            ? installation.LoadAudioClips()
            : InstalledAudioClips.Load(audioDirectory);
        ContentIdentity = SuperMetroid.AssetExtraction.Installation.GameContentIdentity.Create(
            assets, maps, projectiles, SuperMetroid.AssetExtraction.Installation.InstalledArtworkIdentity.Create(artwork, enemyArtwork, samusBody));
        Console.WriteLine(
            $"Installed content: {ContentIdentity.CompositeSha256}; " +
            $"definitions={ContentIdentity.CompiledDefinitionsBuildId:D}, " +
            $"audio={ContentIdentity.AudioContentSha256}, maps={ContentIdentity.MapContentSha256}, " +
            $"projectiles={ContentIdentity.ProjectileContentSha256}.");
        foreach ((string domain, string digest) in ContentIdentity.AdditionalContentSha256.OrderBy(pair => pair.Key))
            Console.WriteLine($"Installed content component: {domain}={digest}.");
        content = new InstalledSessionContent(artwork, assets.Clips);
        Game = GameSessionComposer.Create(saves, Options.Game.ToAssists(), content, saveFile);
        states = DebuggerSaveStateStore.ForInstalledGame(
            root,
            Options.Game,
            ContentIdentity,
            content);
        recorder = StartRecorder();
        WriteRecordingMetadata(seedFile: null);
    }

    /// <summary>Parsed host options; their game values are shared by the runtime, the debugger state store and recordings.</summary>
    public HostOptions Options { get; }
    /// <summary>Live game graph owned by the session worker.</summary>
    public SuperMetroidGame Game { get; private set; }
    /// <summary>Fingerprint of installed audio, maps, projectiles, and artwork.</summary>
    public SuperMetroid.AssetExtraction.Installation.GameContentIdentity ContentIdentity { get; }

    /// <summary>Appends one consumed controller word to the active session recording.</summary>
    public void Record(ControllerButton input) => recorder.RecordFrame(input);
    /// <summary>Flushes accepted input frames after a worker failure.</summary>
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    /// <summary>Saves a manual debugger slot and flushes its accompanying input recording.</summary>
    public string SaveSlot(int slot)
    {
        if (slot == DebuggerStateFormat.AutomaticSlot)
            return "The auto slot is written at completed door transitions. Select 0-9 for a manual save.";
        DebuggerSaveStateMetadata metadata = states.Save(slot, Game);
        FlushRecording();
        return $"Saved slot {slot}, frame {metadata.FrameNumber}, room {metadata.Room}.";
    }

    /// <summary>Writes content and assembly identity alongside a reset or debugger-seeded recording.</summary>
    private void WriteRecordingMetadata(string? seedFile)
    {
        // The recording embeds installed-content identity. This sidecar additionally
        // identifies whether replay starts at reset or from an exact debugger graph and
        // records the assemblies which generated that graph.
        File.WriteAllText(Path.ChangeExtension(recorder.Path, ".json"), JsonSerializer.Serialize(
            new RecordingSeed("SuperMetroid.InstalledSession.RecordingSeed.v1", Path.GetFileName(recorder.Path), seedFile,
                global::SuperMetroid.Core.BuildIdentity.Version, global::SuperMetroid.Diagnostics.BuildIdentity.Version,
                global::SuperMetroid.Tooling.BuildIdentity.Version, ContentIdentity),
            global::SuperMetroid.Tooling.ToolingJson.Info<RecordingSeed>(IndentedJson)));
        FlushRecording();
    }

    /// <summary>A recording's sidecar: how its replay starts and the builds and content that made it.</summary>
    /// <param name="Format">The sidecar's format name.</param>
    /// <param name="Recording">The recording's file name.</param>
    /// <param name="SeedFile">The debugger state the replay starts from, or null when it starts at reset.</param>
    /// <param name="CoreBuild">The simulation's build.</param>
    /// <param name="DiagnosticsBuild">The diagnostics' build, which wrote the debugger state.</param>
    /// <param name="HostBuild">The tooling's build.</param>
    /// <param name="ContentIdentity">The installed content's identity.</param>
    internal sealed record RecordingSeed(
        [property: JsonPropertyName("format")] string Format,
        [property: JsonPropertyName("recording")] string Recording,
        [property: JsonPropertyName("seedFile")] string? SeedFile,
        [property: JsonPropertyName("coreBuild")] Guid CoreBuild,
        [property: JsonPropertyName("diagnosticsBuild")] Guid DiagnosticsBuild,
        [property: JsonPropertyName("hostBuild")] Guid HostBuild,
        [property: JsonPropertyName("contentIdentity")] global::SuperMetroid.AssetExtraction.Installation.GameContentIdentity ContentIdentity);

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
