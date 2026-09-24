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
    private readonly bool installedSession;
    private readonly ExtractedAudioAssetCatalog assets;
    private readonly SuperMetroid.Core.Assets.AreaMapPresentationCatalog? maps;
    private readonly SuperMetroid.Core.Assets.IntroCinematicArtworkCatalog? introCinematicArt;
    private readonly SuperMetroid.Core.Assets.EndingMode7ArtworkCatalog? endingMode7Art;
    private readonly SuperMetroid.Core.Assets.EndingObjectArtworkCatalog? endingObjectArt;
    private readonly SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog? roomCharacters;
    private readonly SuperMetroid.Core.Assets.RoomStaticPaletteCatalog? roomPalettes;
    private readonly SuperMetroid.Core.Assets.RoomMetatileCatalog? roomMetatiles;
    private readonly SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog? roomVisualLayouts;
    private readonly SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog? roomPlmShotBlockVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog? roomPlmGrappleBlockVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog? roomPlmStationVisuals;
    private readonly SuperMetroid.Core.Rooms.XrayRevealVisualCatalog? xrayRevealVisuals;
    private readonly SuperMetroid.Core.Assets.RoomBackgroundTilemapCatalog? roomBackgroundTilemaps;
    private readonly SuperMetroid.Core.Assets.RoomSkyTilemapCatalog? roomSkyTilemaps;
    private readonly SuperMetroid.AssetExtraction.InstalledProjectilePresentation? projectiles;
    private readonly SuperMetroid.Core.Assets.EnemyTileArtworkCatalog? enemyTiles;
    private readonly DebuggerSaveStateStore states;
    private ControllerInputRecorder recorder;

    public AndroidSessionData(string root, string? cartridgePath = null, string? audioDirectory = null)
    {
        this.root = root;
        installedSession = cartridgePath is null;
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
        // Explicit diagnostic cartridge paths retain their legacy fixture setup;
        // ordinary installed Android sessions require the installed map catalog.
        maps = cartridgePath is null ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadMaps() : null;
        Game.BindMapPresentation(maps);
        introCinematicArt = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadIntroCinematicArt() : null;
        Game.BindIntroCinematicArt(introCinematicArt);
        endingMode7Art = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadEndingMode7Art() : null;
        Game.BindEndingMode7Art(endingMode7Art);
        endingObjectArt = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadEndingObjectArt() : null;
        Game.BindEndingObjectArt(endingObjectArt);
        roomCharacters = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomCharacters() : null;
        Game.BindRoomCharacterArt(roomCharacters);
        roomPalettes = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomPalettes() : null;
        Game.BindRoomPaletteArt(roomPalettes);
        roomMetatiles = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomMetatiles() : null;
        Game.BindRoomMetatileArt(roomMetatiles);
        roomVisualLayouts = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomVisualLayouts() : null;
        Game.BindRoomVisualLayouts(roomVisualLayouts);
        roomPlmShotBlockVisuals = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomPlmShotBlockVisuals() : null;
        Game.BindRoomPlmShotBlockVisuals(roomPlmShotBlockVisuals);
        roomPlmGrappleBlockVisuals = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomPlmGrappleBlockVisuals() : null;
        Game.BindRoomPlmGrappleBlockVisuals(roomPlmGrappleBlockVisuals);
        roomPlmStationVisuals = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomPlmStationVisuals() : null;
        Game.BindRoomPlmStationVisuals(roomPlmStationVisuals);
        xrayRevealVisuals = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadXrayRevealVisuals() : null;
        Game.BindXrayRevealVisuals(xrayRevealVisuals);
        roomBackgroundTilemaps = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomBackgroundTilemaps() : null;
        Game.BindRoomBackgroundTilemapArt(roomBackgroundTilemaps);
        roomSkyTilemaps = cartridgePath is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadRoomSkyTilemaps() : null;
        Game.BindRoomSkyTilemapArt(roomSkyTilemaps);
        projectiles = cartridgePath is null ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadProjectiles() : null;
        enemyTiles = cartridgePath is null ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadEnemyTiles() : null;
        Game.BindProjectileCompositions(projectiles?.Catalog);
        Game.BindProjectileFrameBindings(projectiles?.FrameBindings);
        Game.BindBeamArtwork(projectiles?.BeamTiles);
        Game.BindEnemyTileArtwork(enemyTiles);
        Game.BindTrailArtwork(projectiles?.Trails);
        Game.BindChargeFlarePlacement(projectiles?.FlarePlacement);
        Game.BindChargeFlareCompositions(projectiles?.FlareCompositions);
        Game.BindGrappleArtwork(projectiles?.GrappleTiles);
        if (projectiles is not null)
            Console.WriteLine($"Projectile compositions: stock={projectiles.StockSha256}, selected={projectiles.SelectedSha256} ({root}).");
        Game.SaveRamChanged += PersistSave;
        assets = audioDirectory is null
            ? new SuperMetroid.AssetExtraction.GameInstallation(root).LoadAudio()
            : ExtractedAudioAssetCatalog.Load(audioDirectory);
        if (cartridgePath is null)
        {
            ContentIdentity = SuperMetroid.AssetExtraction.GameContentIdentity.Create(
                assets,
                maps ?? throw new InvalidOperationException("Installed Android session has no map catalog."),
                projectiles ?? throw new InvalidOperationException("Installed Android session has no projectile catalog."));
            Console.WriteLine(
                $"Installed content: {ContentIdentity.CompositeSha256}; " +
                $"definitions={ContentIdentity.CompiledDefinitionsBuildId:D}, " +
                $"audio={ContentIdentity.AudioContentSha256}, maps={ContentIdentity.MapContentSha256}, " +
                $"projectiles={ContentIdentity.ProjectileContentSha256}.");
        }
        Audio = new CartridgeAudioRenderer(assets);
        states = installedSession
            ? DebuggerSaveStateStore.ForInstalledGame(
                root,
                Options,
                ContentIdentity ?? throw new InvalidOperationException("Installed Android session has no content identity."))
            : new DebuggerSaveStateStore(
                romPath,
                Bus.Rom,
                Path.Combine(root, "debug-states"),
                Options,
                ContentIdentity);
        recorder = StartRecorder();
        WriteRecordingMetadata(seedFile: null);
    }

    public SuperMetroidGameOptions Options { get; }
    public SuperMetroidAddressSpace Bus { get; private set; }
    public SuperMetroidGame Game { get; private set; }
    public CartridgeAudioRenderer Audio { get; private set; }
    public SuperMetroid.AssetExtraction.GameContentIdentity? ContentIdentity { get; }
    public long Generation { get; private set; } = 1;

    public void Record(ushort input) => recorder.RecordFrame(input);
    public void PersistSave() => GameSaveFileStore.WriteAtomic(Bus, savePath);
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    public string ImportState(string path, int slot) => installedSession
        ? AndroidFileImport.ImportState(root, path, slot)
        : AndroidFileImport.ImportState(root, romPath, path, slot);
    public string ImportSave(string path) => installedSession
        ? AndroidFileImport.StageRegularSave(root, path)
        : AndroidFileImport.StageRegularSave(root, romPath, path);

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
        Game.BindMapPresentation(maps);
        Game.BindIntroCinematicArt(introCinematicArt);
        Game.BindEndingMode7Art(endingMode7Art);
        Game.BindEndingObjectArt(endingObjectArt);
        Game.BindRoomCharacterArt(roomCharacters);
        Game.BindRoomPaletteArt(roomPalettes);
        Game.BindRoomMetatileArt(roomMetatiles);
        Game.BindRoomVisualLayouts(roomVisualLayouts);
        Game.BindRoomPlmShotBlockVisuals(roomPlmShotBlockVisuals);
        Game.BindRoomPlmGrappleBlockVisuals(roomPlmGrappleBlockVisuals);
        Game.BindRoomPlmStationVisuals(roomPlmStationVisuals);
        Game.BindXrayRevealVisuals(xrayRevealVisuals);
        Game.BindRoomBackgroundTilemapArt(roomBackgroundTilemaps);
        Game.BindRoomSkyTilemapArt(roomSkyTilemaps);
        Game.BindProjectileCompositions(projectiles?.Catalog);
        Game.BindProjectileFrameBindings(projectiles?.FrameBindings);
        Game.BindBeamArtwork(projectiles?.BeamTiles);
        Game.BindEnemyTileArtwork(enemyTiles);
        Game.BindTrailArtwork(projectiles?.Trails);
        Game.BindChargeFlarePlacement(projectiles?.FlarePlacement);
        Game.BindChargeFlareCompositions(projectiles?.FlareCompositions);
        Game.BindGrappleArtwork(projectiles?.GrappleTiles);
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

    private ControllerInputRecorder StartRecorder() => installedSession
        ? ControllerInputRecorder.StartInstalled(
            root,
            Bus.SaveRam,
            Options,
            ContentIdentity ?? throw new InvalidOperationException("Installed Android session has no content identity."))
        : ControllerInputRecorder.Start(
            romPath, Bus.SaveRam, Options, ContentIdentity, Path.Combine(root, "input-recordings"));

    public void Dispose() => recorder.Dispose();
}
