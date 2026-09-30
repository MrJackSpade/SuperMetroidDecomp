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
    private readonly string savePath;
    private readonly ExtractedAudioAssetCatalog assets;
    private readonly SuperMetroid.Core.Assets.AreaMapPresentationCatalog maps;
    private readonly SuperMetroid.Core.Assets.GameplayBasePaletteCatalog gameplayBasePalettes;
    private readonly SuperMetroid.Core.Assets.RoomCharacterAtlas standardObjectArt;
    private readonly SuperMetroid.Core.Assets.IntroCinematicArtworkCatalog introCinematicArt;
    private readonly SuperMetroid.Core.Assets.SamusBodyArtworkCatalog samusBodyArt;
    private readonly SuperMetroid.Core.Assets.EndingMode7ArtworkCatalog endingMode7Art;
    private readonly SuperMetroid.Core.Assets.EndingObjectArtworkCatalog endingObjectArt;
    private readonly SuperMetroid.Core.Assets.EndingPaletteCatalog endingPaletteArt;
    private readonly SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog roomCharacters;
    private readonly SuperMetroid.Core.Assets.RoomStaticPaletteCatalog roomPalettes;
    private readonly SuperMetroid.Core.Assets.RoomMetatileCatalog roomMetatiles;
    private readonly SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog roomVisualLayouts;
    private readonly SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog roomPlmShotBlockVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog roomPlmGrappleBlockVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog roomPlmStationVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog roomPlmBlueDoorVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog roomPlmColoredDoorVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog roomPlmGreyDoorVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog roomPlmEyeDoorVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog roomPlmMotherBrainGlassVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog roomPlmNoobTubeVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog roomPlmDownwardGateVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog roomPlmElevatorPlatformVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog roomPlmEscapeGateVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog roomPlmBombTorizoHandVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog roomPlmDraygonCannonVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog roomPlmChozoStatueVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog roomPlmLinkedRestoreVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog roomPlmTourianAccessVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog roomPlmSpeedBoosterVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog roomPlmMaridiaElevatubeVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog roomPlmSporeSpawnCeilingVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog roomPlmSamusEaterVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog roomPlmBotwoonWallVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog roomPlmKraidVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog roomPlmCrocomireVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog roomPlmMotherBrainFakeDeathVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog roomPlmCollectibleVisuals;
    private readonly SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog roomPlmDynamicCollectibleArt;
    private readonly SuperMetroid.Core.Rooms.XrayRevealVisualCatalog xrayRevealVisuals;
    private readonly SuperMetroid.Core.Assets.RoomBackgroundTilemapCatalog roomBackgroundTilemaps;
    private readonly SuperMetroid.Core.Assets.RoomSkyTilemapCatalog roomSkyTilemaps;
    private readonly SuperMetroid.AssetExtraction.InstalledProjectilePresentation projectiles;
    private readonly SuperMetroid.Core.Assets.EnemyTileArtworkCatalog enemyTiles;
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
        GameSaveFileStore.LoadOrMigrate(Bus, savePath, Path.Combine(root, "SuperMetroid.srm"));
        AndroidFileImport.ActivatePendingSave(root, Bus, savePath);
        Game = new SuperMetroidGame(Bus, Options);
        // A caller-provided cartridge has already passed through the importer;
        // the running session binds only the installed presentation catalog.
        maps = installation.LoadMaps();
        Game.BindMapPresentation(maps);
        gameplayBasePalettes = installation.LoadGameplayBasePalettes();
        Game.BindGameplayBasePalettes(gameplayBasePalettes);
        standardObjectArt = installation.LoadStandardObjects();
        Game.BindStandardObjectArt(standardObjectArt);
        introCinematicArt = installation.LoadIntroCinematicArt();
        Game.BindIntroCinematicArt(introCinematicArt);
        samusBodyArt = installation.LoadSamusBodyArt();
        Game.BindSamusBodyArt(samusBodyArt);
        endingMode7Art = installation.LoadEndingMode7Art();
        Game.BindEndingMode7Art(endingMode7Art);
        endingObjectArt = installation.LoadEndingObjectArt();
        Game.BindEndingObjectArt(endingObjectArt);
        endingPaletteArt = installation.LoadEndingPalettes();
        Game.BindEndingPaletteArt(endingPaletteArt);
        roomCharacters = installation.LoadRoomCharacters();
        Game.BindRoomCharacterArt(roomCharacters);
        roomPalettes = installation.LoadRoomPalettes();
        Game.BindRoomPaletteArt(roomPalettes);
        roomMetatiles = installation.LoadRoomMetatiles();
        Game.BindRoomMetatileArt(roomMetatiles);
        roomVisualLayouts = installation.LoadRoomVisualLayouts();
        Game.BindRoomVisualLayouts(roomVisualLayouts);
        roomPlmShotBlockVisuals = installation.LoadRoomPlmShotBlockVisuals();
        Game.BindRoomPlmShotBlockVisuals(roomPlmShotBlockVisuals);
        roomPlmGrappleBlockVisuals = installation.LoadRoomPlmGrappleBlockVisuals();
        Game.BindRoomPlmGrappleBlockVisuals(roomPlmGrappleBlockVisuals);
        roomPlmStationVisuals = installation.LoadRoomPlmStationVisuals();
        Game.BindRoomPlmStationVisuals(roomPlmStationVisuals);
        roomPlmBlueDoorVisuals = installation.LoadRoomPlmBlueDoorVisuals();
        Game.BindRoomPlmBlueDoorVisuals(roomPlmBlueDoorVisuals);
        roomPlmColoredDoorVisuals = installation.LoadRoomPlmColoredDoorVisuals();
        Game.BindRoomPlmColoredDoorVisuals(roomPlmColoredDoorVisuals);
        roomPlmGreyDoorVisuals = installation.LoadRoomPlmGreyDoorVisuals();
        Game.BindRoomPlmGreyDoorVisuals(roomPlmGreyDoorVisuals);
        roomPlmEyeDoorVisuals = installation.LoadRoomPlmEyeDoorVisuals();
        Game.BindRoomPlmEyeDoorVisuals(roomPlmEyeDoorVisuals);
        roomPlmMotherBrainGlassVisuals = installation.LoadRoomPlmMotherBrainGlassVisuals();
        Game.BindRoomPlmMotherBrainGlassVisuals(roomPlmMotherBrainGlassVisuals);
        roomPlmNoobTubeVisuals = installation.LoadRoomPlmNoobTubeVisuals();
        Game.BindRoomPlmNoobTubeVisuals(roomPlmNoobTubeVisuals);
        roomPlmDownwardGateVisuals = installation.LoadRoomPlmDownwardGateVisuals();
        Game.BindRoomPlmDownwardGateVisuals(roomPlmDownwardGateVisuals);
        roomPlmElevatorPlatformVisuals = installation.LoadRoomPlmElevatorPlatformVisuals();
        Game.BindRoomPlmElevatorPlatformVisuals(roomPlmElevatorPlatformVisuals);
        roomPlmEscapeGateVisuals = installation.LoadRoomPlmEscapeGateVisuals();
        Game.BindRoomPlmEscapeGateVisuals(roomPlmEscapeGateVisuals);
        roomPlmBombTorizoHandVisuals = installation.LoadRoomPlmBombTorizoHandVisuals();
        Game.BindRoomPlmBombTorizoHandVisuals(roomPlmBombTorizoHandVisuals);
        roomPlmDraygonCannonVisuals = installation.LoadRoomPlmDraygonCannonVisuals();
        Game.BindRoomPlmDraygonCannonVisuals(roomPlmDraygonCannonVisuals);
        roomPlmChozoStatueVisuals = installation.LoadRoomPlmChozoStatueVisuals();
        Game.BindRoomPlmChozoStatueVisuals(roomPlmChozoStatueVisuals);
        roomPlmLinkedRestoreVisuals = installation.LoadRoomPlmLinkedRestoreVisuals();
        Game.BindRoomPlmLinkedRestoreVisuals(roomPlmLinkedRestoreVisuals);
        roomPlmTourianAccessVisuals = installation.LoadRoomPlmTourianAccessVisuals();
        Game.BindRoomPlmTourianAccessVisuals(roomPlmTourianAccessVisuals);
        roomPlmSpeedBoosterVisuals = installation.LoadRoomPlmSpeedBoosterVisuals();
        Game.BindRoomPlmSpeedBoosterVisuals(roomPlmSpeedBoosterVisuals);
        roomPlmMaridiaElevatubeVisuals = installation.LoadRoomPlmMaridiaElevatubeVisuals();
        Game.BindRoomPlmMaridiaElevatubeVisuals(roomPlmMaridiaElevatubeVisuals);
        roomPlmSporeSpawnCeilingVisuals = installation.LoadRoomPlmSporeSpawnCeilingVisuals();
        Game.BindRoomPlmSporeSpawnCeilingVisuals(roomPlmSporeSpawnCeilingVisuals);
        roomPlmSamusEaterVisuals = installation.LoadRoomPlmSamusEaterVisuals();
        Game.BindRoomPlmSamusEaterVisuals(roomPlmSamusEaterVisuals);
        roomPlmBotwoonWallVisuals = installation.LoadRoomPlmBotwoonWallVisuals();
        Game.BindRoomPlmBotwoonWallVisuals(roomPlmBotwoonWallVisuals);
        roomPlmKraidVisuals = installation.LoadRoomPlmKraidVisuals();
        Game.BindRoomPlmKraidVisuals(roomPlmKraidVisuals);
        roomPlmCrocomireVisuals = installation.LoadRoomPlmCrocomireVisuals();
        Game.BindRoomPlmCrocomireVisuals(roomPlmCrocomireVisuals);
        roomPlmMotherBrainFakeDeathVisuals = installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
        Game.BindRoomPlmMotherBrainFakeDeathVisuals(roomPlmMotherBrainFakeDeathVisuals);
        roomPlmCollectibleVisuals = installation.LoadRoomPlmCollectibleVisuals();
        Game.BindRoomPlmCollectibleVisuals(roomPlmCollectibleVisuals);
        roomPlmDynamicCollectibleArt = installation.LoadRoomPlmDynamicCollectibleArt();
        Game.BindRoomPlmDynamicCollectibleArt(roomPlmDynamicCollectibleArt);
        xrayRevealVisuals = installation.LoadXrayRevealVisuals();
        Game.BindXrayRevealVisuals(xrayRevealVisuals);
        roomBackgroundTilemaps = installation.LoadRoomBackgroundTilemaps();
        Game.BindRoomBackgroundTilemapArt(roomBackgroundTilemaps);
        roomSkyTilemaps = installation.LoadRoomSkyTilemaps();
        Game.BindRoomSkyTilemapArt(roomSkyTilemaps);
        projectiles = installation.LoadProjectiles();
        enemyTiles = installation.LoadEnemyTiles();
        Game.BindProjectileCompositions(projectiles.Catalog);
        Game.BindProjectileFrameBindings(projectiles.FrameBindings);
        Game.BindBeamArtwork(projectiles.BeamTiles);
        Game.BindEnemyTileArtwork(enemyTiles);
        Game.BindTrailArtwork(projectiles.Trails);
        Game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        Game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        Game.BindGrappleArtwork(projectiles.GrappleTiles);
        Console.WriteLine($"Projectile compositions: stock={projectiles.StockSha256}, selected={projectiles.SelectedSha256} ({root}).");
        Game.SaveRamChanged += PersistSave;
        assets = audioDirectory is null
            ? installation.LoadAudio()
            : ExtractedAudioAssetCatalog.Load(audioDirectory);
        ContentIdentity = SuperMetroid.AssetExtraction.GameContentIdentity.Create(
            assets, maps, projectiles,
            SuperMetroid.AssetExtraction.RoomPresentationIdentity.Create(roomCharacters, roomPalettes,
                roomMetatiles, roomBackgroundTilemaps, roomSkyTilemaps, roomVisualLayouts)
                .Append(KeyValuePair.Create(SuperMetroid.AssetExtraction.GameInstallationLayout.SamusBodyDirectoryName,
                    samusBodyArt.ContentIdentity))
                .Concat(SuperMetroid.AssetExtraction.RoomPlmPresentationIdentity.Create(
                    roomPlmShotBlockVisuals,
                    roomPlmGrappleBlockVisuals,
                    roomPlmStationVisuals,
                    roomPlmBlueDoorVisuals,
                    roomPlmColoredDoorVisuals,
                    roomPlmGreyDoorVisuals,
                    roomPlmEyeDoorVisuals,
                    roomPlmMotherBrainGlassVisuals,
                    roomPlmNoobTubeVisuals,
                    roomPlmDownwardGateVisuals,
                    roomPlmElevatorPlatformVisuals,
                    roomPlmEscapeGateVisuals,
                    roomPlmBombTorizoHandVisuals,
                    roomPlmDraygonCannonVisuals,
                    roomPlmChozoStatueVisuals,
                    roomPlmLinkedRestoreVisuals,
                    roomPlmTourianAccessVisuals,
                    roomPlmSpeedBoosterVisuals,
                    roomPlmMaridiaElevatubeVisuals,
                    roomPlmSporeSpawnCeilingVisuals,
                    roomPlmSamusEaterVisuals,
                    roomPlmBotwoonWallVisuals,
                    roomPlmKraidVisuals,
                    roomPlmCrocomireVisuals,
                    roomPlmMotherBrainFakeDeathVisuals,
                    roomPlmCollectibleVisuals,
                    roomPlmDynamicCollectibleArt)));
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
    public void PersistSave() => GameSaveFileStore.WriteAtomic(Bus, savePath);
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    public string ImportState(string path, int slot) =>
        AndroidFileImport.ImportState(root, path, slot);
    public string ImportSave(string path) =>
        AndroidFileImport.StageRegularSave(root, path);

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
        Game.BindGameplayBasePalettes(gameplayBasePalettes);
        Game.BindStandardObjectArt(standardObjectArt);
        Game.BindIntroCinematicArt(introCinematicArt);
        Game.BindSamusBodyArt(samusBodyArt);
        Game.BindEndingMode7Art(endingMode7Art);
        Game.BindEndingObjectArt(endingObjectArt);
        Game.BindEndingPaletteArt(endingPaletteArt);
        Game.BindRoomCharacterArt(roomCharacters);
        Game.BindRoomPaletteArt(roomPalettes);
        Game.BindRoomMetatileArt(roomMetatiles);
        Game.BindRoomVisualLayouts(roomVisualLayouts);
        Game.BindRoomPlmShotBlockVisuals(roomPlmShotBlockVisuals);
        Game.BindRoomPlmGrappleBlockVisuals(roomPlmGrappleBlockVisuals);
        Game.BindRoomPlmStationVisuals(roomPlmStationVisuals);
        Game.BindRoomPlmBlueDoorVisuals(roomPlmBlueDoorVisuals);
        Game.BindRoomPlmColoredDoorVisuals(roomPlmColoredDoorVisuals);
        Game.BindRoomPlmGreyDoorVisuals(roomPlmGreyDoorVisuals);
        Game.BindRoomPlmEyeDoorVisuals(roomPlmEyeDoorVisuals);
        Game.BindRoomPlmMotherBrainGlassVisuals(roomPlmMotherBrainGlassVisuals);
        Game.BindRoomPlmNoobTubeVisuals(roomPlmNoobTubeVisuals);
        Game.BindRoomPlmDownwardGateVisuals(roomPlmDownwardGateVisuals);
        Game.BindRoomPlmElevatorPlatformVisuals(roomPlmElevatorPlatformVisuals);
        Game.BindRoomPlmEscapeGateVisuals(roomPlmEscapeGateVisuals);
        Game.BindRoomPlmBombTorizoHandVisuals(roomPlmBombTorizoHandVisuals);
        Game.BindRoomPlmDraygonCannonVisuals(roomPlmDraygonCannonVisuals);
        Game.BindRoomPlmChozoStatueVisuals(roomPlmChozoStatueVisuals);
        Game.BindRoomPlmLinkedRestoreVisuals(roomPlmLinkedRestoreVisuals);
        Game.BindRoomPlmTourianAccessVisuals(roomPlmTourianAccessVisuals);
        Game.BindRoomPlmSpeedBoosterVisuals(roomPlmSpeedBoosterVisuals);
        Game.BindRoomPlmMaridiaElevatubeVisuals(roomPlmMaridiaElevatubeVisuals);
        Game.BindRoomPlmSporeSpawnCeilingVisuals(roomPlmSporeSpawnCeilingVisuals);
        Game.BindRoomPlmSamusEaterVisuals(roomPlmSamusEaterVisuals);
        Game.BindRoomPlmBotwoonWallVisuals(roomPlmBotwoonWallVisuals);
        Game.BindRoomPlmKraidVisuals(roomPlmKraidVisuals);
        Game.BindRoomPlmCrocomireVisuals(roomPlmCrocomireVisuals);
        Game.BindRoomPlmMotherBrainFakeDeathVisuals(roomPlmMotherBrainFakeDeathVisuals);
        Game.BindRoomPlmCollectibleVisuals(roomPlmCollectibleVisuals);
        Game.BindRoomPlmDynamicCollectibleArt(roomPlmDynamicCollectibleArt);
        Game.BindXrayRevealVisuals(xrayRevealVisuals);
        Game.BindRoomBackgroundTilemapArt(roomBackgroundTilemaps);
        Game.BindRoomSkyTilemapArt(roomSkyTilemaps);
        Game.BindProjectileCompositions(projectiles.Catalog);
        Game.BindProjectileFrameBindings(projectiles.FrameBindings);
        Game.BindBeamArtwork(projectiles.BeamTiles);
        Game.BindEnemyTileArtwork(enemyTiles);
        Game.BindTrailArtwork(projectiles.Trails);
        Game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        Game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        Game.BindGrappleArtwork(projectiles.GrappleTiles);
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

    private ControllerInputRecorder StartRecorder() =>
        ControllerInputRecorder.StartInstalled(
            root,
            Bus.SaveRam,
            Options,
            ContentIdentity);

    public void Dispose() => recorder.Dispose();
}
