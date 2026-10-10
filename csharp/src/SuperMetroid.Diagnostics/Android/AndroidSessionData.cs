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
    /// <summary>Application-private directory containing settings, saves, and installed assets.</summary>
    private readonly string root;
    /// <summary>Path to the JSON save file persisted when game SRAM changes.</summary>
    private readonly string savePath;
    /// <summary>Audio samples loaded from the installation or an explicitly selected directory.</summary>
    private readonly ExtractedAudioAssetCatalog assets;
    /// <summary>Installed area-map presentation catalog bound to the game.</summary>
    private readonly SuperMetroid.Core.Assets.AreaMapPresentationCatalog maps;
    /// <summary>Installed gameplay base palettes bound to the game.</summary>
    private readonly SuperMetroid.Core.Assets.GameplayBasePaletteCatalog gameplayBasePalettes;
    /// <summary>Standard room object graphics used by the game renderer.</summary>
    private readonly SuperMetroid.Core.Assets.RoomCharacterAtlas standardObjectArt;
    /// <summary>Intro cinematic artwork supplied to the frontend presentation.</summary>
    private readonly SuperMetroid.Core.Assets.IntroCinematicArtworkCatalog introCinematicArt;
    /// <summary>Samus body artwork loaded from the extracted installation.</summary>
    private readonly SuperMetroid.Core.Assets.SamusBodyArtworkCatalog samusBodyArt;
    /// <summary>Mode 7 artwork used for the ending sequence.</summary>
    private readonly SuperMetroid.Core.Assets.EndingMode7ArtworkCatalog endingMode7Art;
    /// <summary>Ending-sequence object artwork.</summary>
    private readonly SuperMetroid.Core.Assets.EndingObjectArtworkCatalog endingObjectArt;
    /// <summary>Palette resources used by ending sequences.</summary>
    private readonly SuperMetroid.Core.Assets.EndingPaletteCatalog endingPaletteArt;
    /// <summary>Per-room character graphics.</summary>
    private readonly SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog roomCharacters;
    /// <summary>Per-room static palettes.</summary>
    private readonly SuperMetroid.Core.Assets.RoomStaticPaletteCatalog roomPalettes;
    /// <summary>Room metatile graphics and layout data.</summary>
    private readonly SuperMetroid.Core.Assets.RoomMetatileCatalog roomMetatiles;
    /// <summary>Room visual layouts required by the game renderer.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog roomVisualLayouts;
    /// <summary>Installed shot-block PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog roomPlmShotBlockVisuals;
    /// <summary>Installed grapple-block PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmGrappleBlockVisualCatalog roomPlmGrappleBlockVisuals;
    /// <summary>Installed station PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog roomPlmStationVisuals;
    /// <summary>Installed blue-door PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmBlueDoorVisualCatalog roomPlmBlueDoorVisuals;
    /// <summary>Installed colored-door PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmColoredDoorVisualCatalog roomPlmColoredDoorVisuals;
    /// <summary>Installed grey-door PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmGreyDoorVisualCatalog roomPlmGreyDoorVisuals;
    /// <summary>Installed eye-door PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmEyeDoorVisualCatalog roomPlmEyeDoorVisuals;
    /// <summary>Installed Mother Brain glass PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog roomPlmMotherBrainGlassVisuals;
    /// <summary>Installed noob-tube PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog roomPlmNoobTubeVisuals;
    /// <summary>Installed downward-gate PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog roomPlmDownwardGateVisuals;
    /// <summary>Installed elevator-platform PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog roomPlmElevatorPlatformVisuals;
    /// <summary>Installed escape-gate PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmEscapeGateVisualCatalog roomPlmEscapeGateVisuals;
    /// <summary>Installed Bomb Torizo hand PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog roomPlmBombTorizoHandVisuals;
    /// <summary>Installed Draygon cannon PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog roomPlmDraygonCannonVisuals;
    /// <summary>Installed Chozo statue PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog roomPlmChozoStatueVisuals;
    /// <summary>Installed linked-restore PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog roomPlmLinkedRestoreVisuals;
    /// <summary>Installed Tourian-access PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmTourianAccessVisualCatalog roomPlmTourianAccessVisuals;
    /// <summary>Installed speed-booster PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmSpeedBoosterVisualCatalog roomPlmSpeedBoosterVisuals;
    /// <summary>Installed Maridia elevatube PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmMaridiaElevatubeVisualCatalog roomPlmMaridiaElevatubeVisuals;
    /// <summary>Installed Spore Spawn ceiling PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmSporeSpawnCeilingVisualCatalog roomPlmSporeSpawnCeilingVisuals;
    /// <summary>Installed Samus-eater PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmSamusEaterVisualCatalog roomPlmSamusEaterVisuals;
    /// <summary>Installed Botwoon wall PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog roomPlmBotwoonWallVisuals;
    /// <summary>Installed Kraid PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmKraidVisualCatalog roomPlmKraidVisuals;
    /// <summary>Installed Crocomire PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmCrocomireVisualCatalog roomPlmCrocomireVisuals;
    /// <summary>Installed Mother Brain fake-death PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmMotherBrainFakeDeathVisualCatalog roomPlmMotherBrainFakeDeathVisuals;
    /// <summary>Installed collectible PLM artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmCollectibleVisualCatalog roomPlmCollectibleVisuals;
    /// <summary>Installed dynamic collectible artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog roomPlmDynamicCollectibleArt;
    /// <summary>Installed X-ray reveal artwork.</summary>
    private readonly SuperMetroid.Core.Rooms.XrayRevealVisualCatalog xrayRevealVisuals;
    /// <summary>Installed room background tilemaps.</summary>
    private readonly SuperMetroid.Core.Assets.RoomBackgroundTilemapCatalog roomBackgroundTilemaps;
    /// <summary>Installed room sky tilemaps.</summary>
    private readonly SuperMetroid.Core.Assets.RoomSkyTilemapCatalog roomSkyTilemaps;
    /// <summary>Projectile presentation catalogs imported for this session.</summary>
    private readonly SuperMetroid.AssetExtraction.InstalledProjectilePresentation projectiles;
    /// <summary>Installed enemy tile artwork bound to the game.</summary>
    private readonly SuperMetroid.Core.Assets.EnemyTileArtworkCatalog enemyTiles;
    /// <summary>Debugger save-state persistence service for this session.</summary>
    private readonly DebuggerSaveStateStore states;
    /// <summary>Active controller-input recording, when input capture is enabled.</summary>
    private ControllerInputRecorder recorder;

    /// <summary>Creates a worker-owned game session and binds every installed presentation catalog.</summary>
    /// <param name="root">Application data directory.</param>
    /// <param name="cartridgePath">Optional source cartridge to import into the installation.</param>
    /// <param name="audioDirectory">Optional directory overriding installed audio assets.</param>
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
                    roomPlmDynamicCollectibleArt))
                .Concat(SuperMetroid.AssetExtraction.GameplayPresentationIdentity.Create(
                    gameplayBasePalettes, standardObjectArt, xrayRevealVisuals))
                .Concat(SuperMetroid.AssetExtraction.EndingPresentationIdentity.Create(
                    endingMode7Art, endingObjectArt, endingPaletteArt))
                .Append(KeyValuePair.Create(SuperMetroid.AssetExtraction.GameInstallationLayout.IntroCinematicDirectoryName,
                    introCinematicArt.ContentIdentity))
                .Append(KeyValuePair.Create(SuperMetroid.AssetExtraction.GameInstallationLayout.EnemyTileDirectoryName,
                    enemyTiles.ContentIdentity)));
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

    /// <summary>Effective host gameplay and audio options for this session.</summary>
    public SuperMetroidGameOptions Options { get; }
    /// <summary>Runtime address space for the live game, including its current SRAM state.</summary>
    public SuperMetroidAddressSpace Bus { get; private set; }
    /// <summary>Live game instance whose state may be replaced by a successful state load.</summary>
    public SuperMetroidGame Game { get; private set; }
    /// <summary>Audio renderer paired with the current game and managed audio-player graph.</summary>
    public CartridgeAudioRenderer Audio { get; private set; }
    /// <summary>Identity of the installed audio, map, projectile, and presentation content.</summary>
    public SuperMetroid.AssetExtraction.GameContentIdentity ContentIdentity { get; }
    /// <summary>Generation counter incremented when a saved game replaces the active session.</summary>
    public long Generation { get; private set; } = 1;

    /// <summary>Records one controller input word in the active session recording.</summary>
    /// <param name="input">SNES controller bitfield consumed for the current frame.</param>
    public void Record(ushort input) => recorder.RecordFrame(input);
    /// <summary>Persists current SRAM to the regular save file.</summary>
    public void PersistSave() => GameSaveFileStore.WriteAtomic(Bus, savePath, maps);
    /// <summary>Flushes buffered recording data after a failed frame.</summary>
    public void FlushRecording() => recorder.FlushAfterFrameFailure();

    /// <summary>Validates and imports a debugger state into a manual save slot.</summary>
    /// <param name="path">Source debugger-state file.</param>
    /// <param name="slot">Destination numbered slot.</param>
    /// <returns>User-facing import status.</returns>
    public string ImportState(string path, int slot) =>
        AndroidFileImport.ImportState(root, path, slot);
    /// <summary>Validates and stages a regular save for application at the next startup.</summary>
    /// <param name="path">Source regular-save JSON.</param>
    /// <returns>User-facing staging status.</returns>
    public string ImportSave(string path) =>
        AndroidFileImport.StageRegularSave(root, path);

    /// <summary>Saves automatically when the configured door-transition policy permits it.</summary>
    /// <param name="previousState">Game state before the door transition completed.</param>
    public void SaveCompletedDoor(SuperMetroidGameState previousState) =>
        DoorTransitionAutosave.TrySave(Options.DoorTransitionAutosave, replay: false,
            previousState, states, Bus, Game, Audio.Player);

    /// <summary>Saves a numbered state slot, or reports that the automatic slot is reserved.</summary>
    /// <param name="slot">State slot to save.</param>
    /// <returns>User-facing save result with frame and room metadata.</returns>
    public string SaveSlot(int slot)
    {
        if (slot == DebuggerStateFormat.AutomaticSlot)
            return "The auto slot is written at completed door transitions. Select 0-9 for a manual save.";
        DebuggerSaveStateMetadata metadata = states.Save(slot, Bus, Game, Audio.Player);
        FlushRecording();
        return $"Saved slot {slot}, frame {metadata.FrameNumber}, room {metadata.RoomPointer:X4}.";
    }

    /// <summary>Loads a saved graph and atomically rebinds its game to this session's installed catalogs.</summary>
    /// <param name="slot">State slot to load.</param>
    /// <returns>User-facing load result with any migration warnings.</returns>
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
        return $"Loaded slot {DebuggerSaveStateStore.SlotName(slot)}, frame {loaded.Metadata.FrameNumber}." +
            (loaded.Warnings.Count == 0 ? "" : "\nWARNING: " + string.Join("\n", loaded.Warnings));
    }

    /// <summary>Writes sidecar metadata identifying whether the recording begins at reset or a saved state.</summary>
    /// <param name="seedFile">Optional filename of the exact saved-state seed captured for replay.</param>
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

    /// <summary>Starts a recording bound to this session's SRAM seed, options, and installed content.</summary>
    /// <returns>The new input recorder.</returns>
    private ControllerInputRecorder StartRecorder() =>
        ControllerInputRecorder.StartInstalled(
            root,
            Bus.SaveRam,
            Options,
            ContentIdentity);

    /// <summary>Closes the active controller-input recording.</summary>
    public void Dispose() => recorder.Dispose();
}
