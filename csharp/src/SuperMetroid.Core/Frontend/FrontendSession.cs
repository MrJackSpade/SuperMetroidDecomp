using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>The frontend's shared state and its owners: the runtime, audio, display, save RAM, scenes, fades and demo counters that the game-state phases run against.</summary>
internal sealed class FrontendSession
{
    /// <summary>Creates a reset-state session, reading the selected save slot and capturing the native boot carry.</summary>
    /// <param name="bus">Retained cartridge address space with <see cref="ISnesMutableMemory"/> support for WRAM changes.</param>
    /// <param name="saves">The battery-backed saves the session reads and writes for its whole lifetime.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    /// <param name="renderGameplayFrames">Whether gameplay frames are composited; verification hosts may skip them.</param>
    internal FrontendSession(ISnesAddressSpace bus, SaveSlotStore saves, SuperMetroidGameOptions? gameOptions, bool renderGameplayFrames)
    {
        this.bus = Ensure.NotNull(bus);
        ConfiguredOptions = gameOptions ?? new SuperMetroidGameOptions();
        display = new FrontendDisplay(renderGameplayFrames);
        this.saves = Ensure.NotNull(saves);
        selectedSaveSlot = saves.ReadSelectedSlot();
        // Construction is power-on: boot evaluates the saves once before entering MainGameLoop.
        bootMainLoopCarry = saves.DetermineBootMainLoopCarry();
        bootDemoSetCount = saves.DetermineNumberOfDemoSets();
    }

    /// <summary>Shared fade clock for frontend transitions between rendered phases.</summary>
    internal readonly ScreenFadeTimer screenFade = new();
    /// <summary>Accepted menu NMI counters used by frontend animation and input timing.</summary>
    internal readonly MenuLayer1Retention menuLayer1 = new();

    /// <summary>The session's accepted-frame counters <c>$05B5/$05B6</c>: every runtime it creates counts on them.</summary>
    internal readonly FrameClock frameClock = new();
    /// <summary>The installed catalogs currently bound; <see cref="GameArtwork.Unbound"/> until a host binds them.</summary>
    [field: NonSerialized]
    public GameArtwork Artwork { get => field ?? GameArtwork.Unbound; internal set; }
    /// <summary>Mutable emulated address space retained for the whole game session.</summary>
    internal readonly ISnesAddressSpace bus;
    /// <summary>The session's Mode 7 transfer queue residue; like WRAM, it outlives each gameplay runtime.</summary>
    internal readonly SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue mode7QueueResidue = new();
    /// <summary>Current host options; interactive state restoration rebinds these to the active session.</summary>
    public SuperMetroidGameOptions ConfiguredOptions { get; internal set; }
    /// <summary>Owns scene capture and raster publication for the current frontend frame.</summary>
    internal readonly FrontendDisplay display;
    /// <summary>Battery-backed save slots and the menu selection; the host persists them.</summary>
    internal readonly SaveSlotStore saves;
    /// <summary>Boot-time <see cref="Bank80SystemState.MainGameLoopCarry"/>; fixed for the session.</summary>
    internal readonly bool bootMainLoopCarry;
    /// <summary>Boot-time <c>NumberOfDemoSets</c> ($1F59); fixed for the session.</summary>
    internal readonly int bootDemoSetCount;
    /// <summary>Native music and SFX queues shared across menus and gameplay.</summary>
    internal readonly CartridgeAudioState audio = new();
    /// <summary>Core-owned SPC port model and synthesizer advanced at the end of every update.</summary>
    internal readonly CartridgeAudioSession audioSession = new();
    /// <summary>Opening logo and title sequence while the corresponding state is active.</summary>
    internal TitleSequenceState? title;
    /// <summary>Save-slot selection menu state.</summary>
    internal FileSelectMenuState? fileSelect;
    /// <summary>Saved-game map selection and loading screen state.</summary>
    internal FileSelectMapMenuState? fileSelectMap;
    /// <summary>Controller and display option menu state.</summary>
    internal GameOptionsMenuState? options;
    /// <summary>Game-over choice screen after death.</summary>
    internal GameOverMenuState? gameOver;
    /// <summary>New-game prologue and flashback cinematic.</summary>
    internal IntroCinematicState? intro;
    /// <summary>Ceres destruction scene after Ridley's escape.</summary>
    internal CeresDestructionCinematicState? ceresDestruction;
    /// <summary>Post-escape ending and staff credits.</summary>
    internal EndingCreditsState? endingCredits;
    /// <summary>Pause-map and equipment menu state.</summary>
    internal PauseMenuState? pauseMenu;
    /// <summary>Live gameplay runtime, absent during frontend-only phases.</summary>
    internal SuperMetroidRuntime? runtime;
    /// <summary>The state of the opening title sequence, which exists only while that phase runs.</summary>
    internal TitleSequenceState ActiveTitle => title ??
        throw new InvalidOperationException("The opening title sequence is not running.");
    /// <summary>The state of the file-select menu, which exists only while that phase runs.</summary>
    internal FileSelectMenuState ActiveFileSelect => fileSelect ??
        throw new InvalidOperationException("The file-select menu is not running.");
    /// <summary>The state of the options menu, which exists only while that phase runs.</summary>
    internal GameOptionsMenuState ActiveOptions => options ??
        throw new InvalidOperationException("The options menu is not running.");
    /// <summary>The state of the intro cinematic, which exists only while that phase runs.</summary>
    internal IntroCinematicState ActiveIntro => intro ??
        throw new InvalidOperationException("The intro cinematic is not running.");
    /// <summary>The state of the ending and credits, which exists only while that phase runs.</summary>
    internal EndingCreditsState ActiveEndingCredits => endingCredits ??
        throw new InvalidOperationException("The ending and credits is not running.");
    /// <summary>The state of the pause menu, which exists only while that phase runs.</summary>
    internal PauseMenuState ActivePauseMenu => pauseMenu ??
        throw new InvalidOperationException("The pause menu is not running.");
    /// <summary>The live gameplay runtime; gameplay phases run only while a game is loaded.</summary>
    internal SuperMetroidRuntime GameplayRuntime => runtime ??
        throw new InvalidOperationException("No gameplay runtime is loaded; this phase runs only during gameplay.");
    /// <summary>Ceres elevator departure and blackout transition state.</summary>
    internal readonly CeresDepartureState ceresDeparture = new();
    /// <summary>Reserve-tank recovery state that can temporarily own the dispatcher.</summary>
    internal readonly SamusReserveAutoRecoveryState reserveRecovery = new();
    /// <summary>Door approach and destination loading state across gameplay states.</summary>
    internal readonly DoorTransitionState doorTransition = new();
    /// <summary>Current battery-backed save slot selected by the frontend.</summary>
    internal int selectedSaveSlot;
    /// <summary>Distinguishes a saved-game load from fresh Ceres setup.</summary>
    internal bool loadingExistingSave;
    /// <summary>Save state retained for restart after the Space-Time intro sequence.</summary>
    internal SuperMetroidSaveSlot? spacetimeIntroRestartSlot;
    /// <summary>Brightness during the fade into Landing Site or Ceres arrival.</summary>
    internal byte postCeresFadeBrightness;
    /// <summary>Cadence counter for the post-Ceres fade.</summary>
    internal int postCeresFadeCounter = 1;
    /// <summary>Selects Ceres arrival rather than Landing Site at the fade destination.</summary>
    internal bool gameplayFadeLeadsToCeresArrival;
    /// <summary>Gameplay brightness retained through pause and unpause fades.</summary>
    internal byte pauseBrightness = PauseFadeTiming.FullyLit;
    /// <summary>Palette transition for Samus's death sequence, when active.</summary>
    internal CartridgePaletteTransition? deathPaletteFade;
    /// <summary>Palette transition for the escape time-up sequence.</summary>
    internal CartridgePaletteTransition? timeUpPaletteFade;
    /// <summary>Brightness during the death-state blackout.</summary>
    internal byte deathFadeBrightness = 15;
    /// <summary>Cadence counter for the death fade.</summary>
    internal int deathFadeCounter;
    /// <summary>Brightness during the ending transition after Zebes escape.</summary>
    internal byte endingFadeBrightness = 15;
    /// <summary>Cadence counter for the ending fade.</summary>
    internal int endingFadeCounter;
    /// <summary>The most recent update's ordered semantic audio events; the session has already rendered them.</summary>
    internal IReadOnlyList<AudioEvent> lastAudioEvents = Array.Empty<AudioEvent>();
    /// <summary>Runtime publication number last copied into the frontend audio queue.</summary>
    internal ulong? lastAudioRuntimeGameplayPublication;
    /// <summary>Room-state pointer associated with the last copied audio publication.</summary>
    internal ushort? lastAudioRoomStatePointer;
    /// <summary>The handlers of the game's save-RAM-changed event.</summary>
    internal Action? saveRamChanged;
    /// <summary>Raises the game's save-RAM-changed event after translated code changed save RAM.</summary>
    internal void RaiseSaveRamChanged() => saveRamChanged?.Invoke();
    /// <summary>Current value of the native game-state word at WRAM $0998.</summary>
    public SuperMetroidGameState GameState { get; internal set; } = SuperMetroidGameState.Reset;
    /// <summary>How far the cartridge's door loader has progressed; see <see cref="IDoorLoaderProgressSource"/>.</summary>
    public IDoorLoaderProgressSource DoorLoaderProgress { get; set; } = LagFreeDoorLoaderProgress.Instance;
    /// <summary>
    /// Moves already-translated per-frame publishers into the single cartridge queue.
    /// </summary>
    /// <remarks>
    /// This is intentionally a collection seam, not sound policy. Samus, projectiles, and
    /// PLMs retain ownership of the exact library/ID/cap chosen by their ROM routines; the
    /// frontend merely performs the global QueueSfx call those producers requested.
    /// </remarks>
    internal void CollectTranslatedAudioRequests(GameplayAudioFramePublication gameplayAudio)
    {
        // Demo loading suppresses room music; native debug_disable_sounds suppresses
        // scene SFX while the title track continues. Do not let runtime publishers
        // replace it with each demo room's normal area music.
        if (runtime?.AttractDemo.IsAttractDemo == true)
            return;
        if (runtime?.Room.ActiveRoom?.State is { } roomState &&
            roomState.Pointer != lastAudioRoomStatePointer)
        {
            // $82:E071 loads data during door setup. $82:E664 selects the track
            // only after the opening scroll and this data queue have completed.
            if (GameState == SuperMetroidGameState.LoadingNextRoomB)
                audio.QueueRoomMusicData(roomState.MusicDataIndex);
            else
                audio.QueueRoomMusic(roomState.MusicDataIndex, roomState.MusicTrackIndex);
            lastAudioRoomStatePointer = roomState.Pointer;
        }
        // Enemy initialization follows the room's music-data queueing in native loading.
        if (runtime is not null)
            foreach (MusicCommand command in runtime.Enemies.Audio.ConsumeInitializationMusicDelayed8())
                audio.QueueMusicDelayed8(command);

        // The message-box selector is an NMI-only owner, not part of the gameplay
        // publication generation below. It has explicit consume semantics so its one-shot
        // request remains audible without making stale enemy/PLM lists eligible again.
        if (runtime?.MessageBoxes.ConsumeMessageBoxSelectionSoundRequest() == true)
            audio.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
        if (runtime?.MessageBox.ConsumeSavingSoundRequest() == true)
            audio.QueueSound(SoundEffectLibrary1Sounds.Saving, maximumQueued: 6);
        if (runtime?.MessageBoxes.ConsumeGunshipExitSoundRequest() == true)
            audio.QueueSound(SoundEffectLibrary3Sounds.GunshipEntrancePad, maximumQueued: 6);
        // Suit setup resumes from the message close boundary, outside normal gameplay
        // publication generations. Consume once so HDMA-only frames cannot repeat it.
        if (runtime?.SuitPickup.ConsumeTransformationSoundRequest() == true)
            audio.QueueSoundAndGetAccumulator(SoundEffectLibrary2Sounds.SuitTransformation, maximumQueued: 6,
                soundSuppressed: runtime.SuitPickup.TransformationSoundSuppressed);

        // Frontend states sometimes accept NMIs without running the state-eight owner list.
        // In particular, the door coroutine waits for all unread sound-ring entries to
        // drain. NMI identity cannot guard this handoff: re-reading the last enemy or PLM
        // publication during each wait NMI perpetually refills that ring. Consume only a
        // newly completed gameplay publication instead.
        if (runtime is null ||
            runtime.FrameBoundary.CompletedGameplayAudioPublication ==
                lastAudioRuntimeGameplayPublication)
            return;
        lastAudioRuntimeGameplayPublication =
            runtime.FrameBoundary.CompletedGameplayAudioPublication;

        // Bank $88 room-effect actors and bank $8D palette objects run before Samus,
        // projectiles, PLMs, and enemies in state eight. Queue their calls first so a full
        // SFX ring retains the same higher-priority request the cartridge would retain
        // when several producers fire during one frame.
        gameplayAudio.PublishPrefix(runtime);

        if (runtime.Player.Samus is { } samus)
        {
            // These state machines predate the shared SamusSoundRequest list. Consume their
            // one-shot publications here, retaining the exact native library and MaxN entry.
            if (samus.HorizontalSpeed.ConsumeEchoSoundRequest())
                audio.QueueSound(SoundEffectLibrary3Sounds.SpeedBoosterEcho, maximumQueued: 6);
            if (samus.Shinespark.ConsumeStoredShineWarningSoundRequest())
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.StoredWarning, 9,
                    soundSuppressed: samus.Shinespark.StoredShineWarningSoundSuppressed);
            if (samus.Shinespark.ConsumeChargeCancellationSoundRequest())
                audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.CancelAll, 9,
                    soundSuppressed: samus.Shinespark.ChargeCancellationSoundSuppressed);
            if (samus.Shinespark.ConsumeLaunchSoundRequest())
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.Launch, 9,
                    soundSuppressed: samus.Shinespark.LaunchSoundSuppressed);
            if (samus.Shinespark.ConsumeCrashSoundRequest())
            {
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.CrashImpact, 6,
                    soundSuppressed: samus.Shinespark.CrashSoundSuppressed);
                audio.QueueSoundAndGetAccumulator(ShinesparkSounds.CrashEcho, 6,
                    soundSuppressed: samus.Shinespark.CrashSoundSuppressed);
            }
            if (samus.CrystalFlash.ConsumeActivationSoundRequest())
                audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
            if (samus.Xray.ConsumeActivationSoundRequest())
                audio.QueueSound(SoundEffectLibrary1Sounds.XrayStart, maximumQueued: 6);
            if (samus.Xray.ConsumeDeactivationSoundRequest())
                audio.QueueSound(SoundEffectLibrary1Sounds.XrayEnd, maximumQueued: 6);
            if (samus.DeathSequence.ConsumeSpinJumpSoundRequest())
                audio.QueueSound(SoundEffectLibrary1Sounds.StopSpinJump, maximumQueued: 6);
        }

        if (runtime.Projectiles.LastFrameResult.QueuedSoundEffect is { } projectileSound)
        {
            audio.QueueSoundAndGetAccumulator(
                projectileSound,
                runtime.Projectiles.LastFrameResult.QueuedSoundMaximum,
                soundSuppressed: runtime.Projectiles.LastFrameResult.QueuedSoundSuppressed);
        }

        foreach (SamusSoundRequest request in
            runtime.Projectiles.LastFrameResult.AdditionalSoundRequests ?? [])
        {
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }

        foreach (SamusSoundRequest request in runtime.Projectiles.ImpactAudio.ImpactSoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);

        foreach (SamusSoundRequest request in
            runtime.BombProjectiles.LastFrameResult.SoundRequests ?? [])
        {
            audio.QueueSoundAndGetAccumulator(
                request.SoundEffect,
                request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }

        foreach (PlmSoundRequest request in runtime.Plms.Sounds.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (PlmMusicRequest request in runtime.Plms.BombTorizoHand.MusicRequests)
            audio.QueueMusicDelayed(request.Command, request.Delay);
        if (runtime.Plms.CollectiblePlms.ConsumeCollectibleFanfareRequest())
            audio.QueuePermanentItemFanfare();

        foreach (EnemySoundRequest request in runtime.Enemies.Audio.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (EnemyMusicRequest request in runtime.Enemies.Audio.MusicRequests)
            audio.QueueMusicDelayed(request.Command, request.Delay);
    }
    /// <summary>
    /// Ports the state-publication half of <c>HandleSamusOutOfHealthAndGameTile</c>. Runtime
    /// state eight has already completed its room-main and clock work when this is called.
    /// </summary>
    /// <summary>
    /// Gunship function $A2:AD0E writes game state $26 only after the top hull has crossed
    /// Y=$0100, zeroing ScreenFadeDelay/Counter. It does not freeze time: state $26 keeps
    /// running full gameplay, so Samus and the RNG advance behind the fade. State eight has
    /// already completed on this publication frame; the following call owns the fade.
    /// </summary>
    internal void BeginZebesEscapeFade()
    {
        endingFadeBrightness = 15;
        endingFadeCounter = 0;
        GameState = SuperMetroidGameState.SamusEscapesFromZebes;
    }
    /// <summary>Writes frontend brightness and retains its last level for pause fades.</summary>
    internal void ApplyDisplayBrightness(byte brightness)
    {
        // Every published level is a write of the brightness register $51; pause entry
        // fades from the last one.
        pauseBrightness = brightness;
        display.ApplyBrightness(brightness);
    }
    /// <summary>Starts a pause fade from <paramref name="initialBrightness"/> with both fade words reloaded.</summary>
    internal void BeginPauseFade(byte initialBrightness)
    {
        pauseBrightness = initialBrightness;
        screenFade.Reload();
    }
    /// <summary>Allocates and binds the gameplay owner without selecting a room.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(runtime))]
    internal void CreateGameplayRuntime(bool forAttractDemo = false)
    {
        ushort incomingRandom = FrontendRandomOwner.RandomNumber;
        if (runtime is not null)
            menuLayer1.RetainFrom(runtime);
        runtime = new SuperMetroidRuntime(bus, ConfiguredOptions.RuntimeOverrides(forAttractDemo), Artwork.GameplayBasePalettes, frameClock, mode7QueueResidue);
        // Runtime allocation is a managed ownership change, not Vector_RESET.
        // Publish before room initialization so random-consuming enemies see it too.
        runtime.System.SetRandomNumber(incomingRandom);
        runtime.System.MainGameLoopCarry = bootMainLoopCarry;
        menuLayer1.PublishTo(runtime);
        runtime.Artwork.MapPresentation = Artwork.MapPresentation;
        runtime.Artwork.StandardObjectArt = Artwork.StandardObjectArt;
        runtime.Artwork.SamusBodyArt = Artwork.SamusBodyArt;
        runtime.Artwork.RoomCharacterArt = Artwork.RoomCharacterArt;
        runtime.Artwork.RoomPaletteArt = Artwork.RoomPaletteArt;
        runtime.Artwork.RoomMetatileArt = Artwork.RoomMetatileArt;
        runtime.Artwork.RoomVisualLayouts = Artwork.RoomVisualLayouts;
        runtime.Artwork.RoomLevelStreams = Artwork.RoomLevelStreams;
        runtime.ShotBlockVisuals.RoomPlmShotBlockVisuals = Artwork.RoomPlmShotBlockVisuals;
        runtime.ShotBlockVisuals.RoomPlmGrappleBlockVisuals = Artwork.RoomPlmGrappleBlockVisuals;
        runtime.ShotBlockVisuals.RoomPlmStationVisuals = Artwork.RoomPlmStationVisuals;
        runtime.ShotBlockVisuals.RoomPlmBlueDoorVisuals = Artwork.RoomPlmBlueDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmColoredDoorVisuals = Artwork.RoomPlmColoredDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmGreyDoorVisuals = Artwork.RoomPlmGreyDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmEyeDoorVisuals = Artwork.RoomPlmEyeDoorVisuals;
        runtime.ShotBlockVisuals.RoomPlmMotherBrainGlassVisuals = Artwork.RoomPlmMotherBrainGlassVisuals;
        runtime.ShotBlockVisuals.RoomPlmNoobTubeVisuals = Artwork.RoomPlmNoobTubeVisuals;
        runtime.ShotBlockVisuals.RoomPlmDownwardGateVisuals = Artwork.RoomPlmDownwardGateVisuals;
        runtime.ShotBlockVisuals.RoomPlmElevatorPlatformVisuals = Artwork.RoomPlmElevatorPlatformVisuals;
        runtime.ShotBlockVisuals.RoomPlmEscapeGateVisuals = Artwork.RoomPlmEscapeGateVisuals;
        runtime.ShotBlockVisuals.RoomPlmBombTorizoHandVisuals = Artwork.RoomPlmBombTorizoHandVisuals;
        runtime.ShotBlockVisuals.RoomPlmDraygonCannonVisuals = Artwork.RoomPlmDraygonCannonVisuals;
        runtime.ShotBlockVisuals.RoomPlmChozoStatueVisuals = Artwork.RoomPlmChozoStatueVisuals;
        runtime.ShotBlockVisuals.RoomPlmLinkedRestoreVisuals = Artwork.RoomPlmLinkedRestoreVisuals;
        runtime.ShotBlockVisuals.RoomPlmTourianAccessVisuals = Artwork.RoomPlmTourianAccessVisuals;
        runtime.ShotBlockVisuals.RoomPlmSpeedBoosterVisuals = Artwork.RoomPlmSpeedBoosterVisuals;
        runtime.ShotBlockVisuals.RoomPlmMaridiaElevatubeVisuals = Artwork.RoomPlmMaridiaElevatubeVisuals;
        runtime.ShotBlockVisuals.RoomPlmSporeSpawnCeilingVisuals = Artwork.RoomPlmSporeSpawnCeilingVisuals;
        runtime.ShotBlockVisuals.RoomPlmSamusEaterVisuals = Artwork.RoomPlmSamusEaterVisuals;
        runtime.ShotBlockVisuals.RoomPlmBotwoonWallVisuals = Artwork.RoomPlmBotwoonWallVisuals;
        runtime.ShotBlockVisuals.RoomPlmKraidVisuals = Artwork.RoomPlmKraidVisuals;
        runtime.ShotBlockVisuals.RoomPlmCrocomireVisuals = Artwork.RoomPlmCrocomireVisuals;
        runtime.ShotBlockVisuals.RoomPlmMotherBrainFakeDeathVisuals = Artwork.RoomPlmMotherBrainFakeDeathVisuals;
        runtime.ShotBlockVisuals.RoomPlmCollectibleVisuals = Artwork.RoomPlmCollectibleVisuals;
        runtime.ShotBlockVisuals.RoomPlmDynamicCollectibleArt = Artwork.RoomPlmDynamicCollectibleArt;
        runtime.Xray.XrayRevealVisuals = Artwork.XrayRevealVisuals;
        runtime.Artwork.RoomBackgroundTilemapArt = Artwork.RoomBackgroundTilemapArt;
        runtime.Artwork.RoomSkyTilemapArt = Artwork.RoomSkyTilemapArt;
        runtime.Artwork.ProjectileCompositions = Artwork.ProjectileCompositions;
        runtime.Artwork.ProjectileFrameBindings = Artwork.ProjectileFrameBindings;
        runtime.Artwork.TrailArtwork = Artwork.TrailArtwork;
        runtime.Artwork.ChargeFlarePlacement = Artwork.ChargeFlarePlacement;
        runtime.Artwork.ChargeFlareCompositions = Artwork.ChargeFlareCompositions;
        runtime.Artwork.GrappleArtwork = Artwork.GrappleArtwork;
        runtime.Artwork.BeamArtwork = Artwork.BeamArtwork;
        runtime.Enemies.TileArtwork = Artwork.EnemyTileArtwork;
        // Attract scripts retain the default controls and options of their private runtime.
        if (forAttractDemo)
            return;
        runtime.JapaneseText = options?.JapaneseText ?? false;
        runtime.Options.ControllerBindings = options?.ControllerBindings ?? ControllerBindings.Default;
        runtime.Options.MoonwalkEnabled = options?.MoonwalkEnabled ?? false;
        runtime.Options.IconCancelEnabled = options?.IconCancelEnabled ?? false;
    }
    /// <summary>Applies live menu controller and gameplay choices to a newly created runtime.</summary>
    internal void ApplySelectedGameOptions()
    {
        // Live edits in options take precedence over the snapshot loaded at file select.
        if (options is null) return;
        GameplayRuntime.Options.ControllerBindings = options.ControllerBindings;
        GameplayRuntime.Options.MoonwalkEnabled = options.MoonwalkEnabled;
        GameplayRuntime.Options.IconCancelEnabled = options.IconCancelEnabled;
    }
    /// <summary>
    /// Shared saved-game HUD construction for playable and cinematic checkpoints.
    /// The saved inventory supplies counters/icons; native initialization clears
    /// the persisted HUD selection before the first live gameplay update.
    /// </summary>
    internal void InitializeSelectedGameHud(SuperMetroidSaveSlot slot) =>
        GameplayRuntime.HudSetup.InitializeHud(new HudSnapshot(
            slot.Health,
            slot.MaxHealth,
            slot.Missiles,
            slot.MaxMissiles,
            slot.SuperMissiles,
            slot.MaxSuperMissiles,
            slot.PowerBombs,
            slot.MaxPowerBombs,
            slot.EquippedItems,
            // $91:E00D clears selection before $80:9A79 builds the visible HUD.
            SelectedItem: 0,
            slot.ReserveEnergy,
            slot.ReserveMode));
    /// <summary>Builds fresh Ceres or saved-game runtime state, including the selected HUD and room.</summary>
    internal bool SetupSelectedGame()
    {
        CreateGameplayRuntime();

        if (spacetimeIntroRestartSlot is { } restartSlot)
        {
            // The cart loaded the old inventory before $82:EEB4 selected the intro. Ceres
            // setup then starts from fresh progression while retaining equipment and other
            // resources. The flashback specifically replaces and then clears Missiles.
            runtime.HudSetup.InitializeHud(new HudSnapshot(
                Health: restartSlot.Health,
                MaxHealth: restartSlot.MaxHealth,
                Missiles: 0,
                MaxMissiles: 0,
                SuperMissiles: restartSlot.SuperMissiles,
                MaxSuperMissiles: restartSlot.MaxSuperMissiles,
                PowerBombs: restartSlot.PowerBombs,
                MaxPowerBombs: restartSlot.MaxPowerBombs,
                EquippedItems: restartSlot.EquippedItems,
                SelectedItem: 0,
                ReserveHealth: restartSlot.ReserveEnergy,
                ReserveMode: restartSlot.ReserveMode));
            runtime.FrameBoundary.PublishForcedBlankTransfers();
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            SamusState restartedSamus = runtime.Player.Samus
                ?? throw new InvalidOperationException(
                    "SpaceTime restart did not create the Ceres Samus state.");
            RestoreSpacetimeRestartInventory(restartedSamus, restartSlot);
            runtime.Tester.ApplyTesterInventory(restoredInventory: true);
            restartedSamus.SelectedHudItem = 0;
            restartedSamus.AutoCancelHudItemIndex = 0;
            AutomaticCheckpointSaver.SaveCeresArrival(saves, runtime, selectedSaveSlot);
            RaiseSaveRamChanged();
            spacetimeIntroRestartSlot = null;
            loadingExistingSave = false;
            return true;
        }

        if (loadingExistingSave)
        {
            SuperMetroidSaveSlot slot = saves.ReadSlot(selectedSaveSlot)
                ?? throw new InvalidDataException(
                    $"Selected save slot {selectedSaveSlot} became invalid during startup.");
            InitializeSelectedGameHud(slot);
            runtime.GameTime.Load(slot.GameTimeFrames, slot.GameTimeSeconds,
                slot.GameTimeMinutes, slot.GameTimeHours);
            runtime.FrameBoundary.PublishForcedBlankTransfers();

            // Preserve the already-translated Ceres elevator entrance for its checkpoint.
            // Every other station uses the general cartridge-backed loader below.
            if (slot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
            {
                // Room selection and actors must see the restored mirror, not a
                // fresh runtime followed by an after-the-fact progression overwrite.
                runtime.RoomLoader.RestoreSavedPlayerState(slot, ConfiguredOptions.ResetBossesOnLoad);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                slot.ApplyTo(runtime.Player.Samus ?? throw new InvalidOperationException(
                    "Ceres initialization did not create Samus."));
                runtime.Tester.ApplyTesterInventory(restoredInventory: true);
                runtime.Player.Samus.SelectedHudItem = 0;
                runtime.Player.Samus.AutoCancelHudItemIndex = 0;
                ApplySelectedGameOptions();
                return true;
            }

            runtime.RoomLoader.InitializeSavedGame(slot, ConfiguredOptions.ResetBossesOnLoad);
            // File selection loaded the saved options before the options menu ran.
            // InitializeSavedGame also serves direct diagnostic loads, so it restores
            // those fields itself. At this frontend boundary, the player's live edits
            // take precedence over that original snapshot, just like cartridge WRAM.
            ApplySelectedGameOptions();
            CartridgeRoomState loadedState = runtime.Room.ActiveRoom?.State
                ?? throw new InvalidOperationException(
                    "Saved-game appearance started without an active room state.");
            // `$92:ED24` starts appearance track one after 14 frames and, on its fifth
            // call, schedules the room track 360 frames later. Mark this room state as
            // already handled so the generic room-change collector cannot overwrite the
            // fanfare with an immediate track request.
            // Loading from file select still has the title's SPC song data installed.
            // Preserve the appearance track sequence, but upload the room's bank first
            // just as ordinary room loading does; track numbers alone are bank-local.
            audio.QueueRoomMusicData(loadedState.MusicDataIndex);
            audio.QueueMusicDelayed(MusicCommand.SelectTrack(1), MusicCommandDelay.FromDelayedYArgument(14));
            audio.QueueMusicDelayed(
                MusicCommand.SelectTrackOrStop(loadedState.MusicTrackIndex),
                MusicCommandDelay.FromDelayedYArgument(360 + 5));
            lastAudioRoomStatePointer = loadedState.Pointer;
            return false;
        }

        // Native new-game loading first publishes the standard HUD/OBJ transfer, then
        // loads the area-six station-zero room and finally runs SamusCode_08. This is the
        // same cartridge-backed sequence used by the room diagnostic; there is no host
        // terrain image, hand-placed Samus, or alternate playable-only initialization.
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.PublishForcedBlankTransfers();
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // CinematicFunction_Intro_Func73 at `$8B:C100` publishes area six/station zero
        // and calls the ordinary `$81:8000` saver immediately before state $1F. Both the
        // full cinematic and host-configured skip converge here, so neither path can omit
        // the automatic checkpoint.
        AutomaticCheckpointSaver.SaveCeresArrival(saves, runtime, selectedSaveSlot);
        RaiseSaveRamChanged();
        return true;
    }
    /// <summary>
    /// Enters cartridge game state seven with a fully dark screen and the native two-frame
    /// brightness cadence. A fresh Ceres game must execute complete gameplay/OAM frames in
    /// this state before state $20 owns the elevator arrival; otherwise the level-data pad
    /// is briefly visible before its bank-$86 concealer reaches displayed OAM.
    /// </summary>
    internal void BeginGameplayFadeIn(bool leadsToCeresArrival)
    {
        postCeresFadeBrightness = 0;
        postCeresFadeCounter = 1;
        gameplayFadeLeadsToCeresArrival = leadsToCeresArrival;
        GameState = SuperMetroidGameState.MainGameplayFadeIn;
    }
    /// <summary>Persists the automatic Landing Site checkpoint after the gunship lands.</summary>
    internal void HandleGunshipLandingSave()
    {
        if (runtime is null ||
            !AutomaticCheckpointSaver.TrySaveGunshipLanding(saves, runtime, selectedSaveSlot))
            return;
        RaiseSaveRamChanged();
    }
    /// <summary>Current attract-demo set selected by the title sequence.</summary>
    internal int demoSet;
    /// <summary>Next scene index within the current attract-demo set.</summary>
    internal int demoScene;
    /// <summary>Remaining scripted input frames in the active demo scene.</summary>
    internal int demoFramesRemaining;
    /// <summary>Remaining still-frame hold after a demo scene's scripted input.</summary>
    internal int demoHoldFramesRemaining;
    /// <summary>Remaining blank enemy-transfer waits before the demo becomes visible.</summary>
    internal int demoLoadFramesRemaining = -1;
    /// <summary>Whether live controller input canceled the current demo.</summary>
    internal bool demoCancelled;
    /// <summary>Whether another scene follows before returning to the title.</summary>
    internal bool demoHasNextScene;
    /// <summary>Unlocks the fourth demo set only after a valid completed-game save exists.</summary>
    internal int AvailableDemoSetCount() => bootDemoSetCount;
    // NMI continuations still owed by the current $82:8000 loading dispatch, and the
    // dispatcher state it selects after its final wait returns.
    /// <summary>Accepted blank NMI waits remaining in state-five game loading.</summary>
    internal int gameLoadingWaitsRemaining;
    /// <summary>Destination to enter when game-loading waits finish.</summary>
    internal GameLoadingCompletion gameLoadingCompletion;
    /// <summary>
    /// The loading state state six branches on (<c>SRAMMirror_LoadingGameState</c>): a save continued
    /// from the file-select map, or the Ceres destruction's Zebes landing.
    /// </summary>
    internal SaveLoadingGameState stateSixLoadingRoute;
    /// <summary>
    /// True when the next update is an NMI continuation inside the loading dispatch rather
    /// than a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesGameLoadingWait => gameLoadingWaitsRemaining > 0;
    // State $09's door function before state $0A. Every door hit installs $82:E17D; a
    // downward elevator replaces it with the $82:E19F delay.
    /// <summary>Whether room loading is paused for the downward elevator entry wait.</summary>
    internal bool waitingForDownwardsElevator;
    /// <summary>Countdown before a downward elevator resumes destination loading.</summary>
    internal int downwardsElevatorDelayTimer;
    /// <summary>Cartridge RNG owner before a gameplay room exists or after runtime release.</summary>
    internal Bank80SystemState? menuRandom;
    /// <summary>Uses gameplay RNG when live and otherwise retains the frontend RNG word.</summary>
    internal Bank80SystemState FrontendRandomOwner =>
        runtime?.System ?? (menuRandom ??= new Bank80SystemState());
    /// <summary>Transfers RNG and the layer-one position back to the frontend before dropping gameplay state.</summary>
    internal void ReleaseRuntimePreservingRandom()
    {
        (menuRandom ??= new()).SetRandomNumber(FrontendRandomOwner.RandomNumber);
        // File-map selection may release when no runtime was ever created.
        if (runtime is not null)
            menuLayer1.RetainFrom(runtime);
        runtime = null;
    }

    /// <summary>
    /// Applies the inventory that remains live across a SpaceTime-triggered intro. Native
    /// $8B:A395 temporarily replaces both missile words for the Mother Brain flashback and
    /// $8B:B76C clears them afterward; no equivalent stores touch Super Missiles or Power
    /// Bombs. The frontend cinematic uses isolated actor state, so its gameplay consequence
    /// is made explicit at the shared Ceres handoff.
    /// </summary>
    internal static void RestoreSpacetimeRestartInventory(
        SamusState samus,
        SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(slot);
        slot.ApplyTo(samus);
        samus.Missiles = 0;
        samus.MaxMissiles = 0;
    }
}
