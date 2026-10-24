using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>The frontend's shared state and its owners: the runtime, audio, display, save RAM, scenes, fades and demo counters that the game-state phases run against.</summary>
internal sealed class FrontendSession
{
    /// <summary>Creates a reset-state session, reading the selected save slot and capturing the native boot carry.</summary>
    /// <param name="saves">The battery-backed saves the session reads and writes for its whole lifetime.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    /// <param name="hostServices">The hardware-timing policies the session runs against for its whole lifetime.</param>
    /// <param name="random">The session's random-number word, which the frontend and every gameplay runtime share.</param>
    internal FrontendSession(SaveSlotStore saves, SuperMetroidGameOptions? gameOptions,
        SuperMetroidGameHostServices hostServices, CartridgeRandom random)
    {
        HostServices = Ensure.NotNull(hostServices);
        Random = Ensure.NotNull(random);
        ConfiguredOptions = gameOptions ?? new SuperMetroidGameOptions();
        display = new FrontendDisplay(hostServices.GameplayFrames);
        this.saves = Ensure.NotNull(saves);
        SelectedSaveSlot = saves.ReadSelectedSlot();
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
    /// <summary>The session's Mode 7 transfer queue residue; like WRAM, it outlives each gameplay runtime.</summary>
    internal readonly SuperMetroid.Core.Compatibility.FloorUpdateResidue floorQueueResidue = new();
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
    /// <summary>The frontend screen showing now: the one menu or cinematic the current state runs, or none.</summary>
    internal IFrontendScreen? Screen { get; private set; }
    /// <summary>Shows <paramref name="screen"/> in place of whatever screen was showing.</summary>
    internal T Show<T>(T screen)
        where T : class, IFrontendScreen
    {
        Screen = Ensure.NotNull(screen);
        return screen;
    }
    /// <summary>Closes the screen when it is a <typeparamref name="T"/>; any other screen stays.</summary>
    internal void Close<T>()
        where T : IFrontendScreen
    {
        if (Screen is T)
            Screen = null;
    }
    /// <summary>The screen showing when it is a <typeparamref name="T"/>, or null.</summary>
    internal T? ScreenAs<T>()
        where T : class, IFrontendScreen => Screen as T;
    /// <summary>The screen showing, which must be a <typeparamref name="T"/>.</summary>
    private T RequireScreen<T>(string what)
        where T : class, IFrontendScreen => Screen as T ?? throw new InvalidOperationException($"The {what} is not running.");
    /// <summary>Live gameplay runtime, absent during frontend-only phases.</summary>
    private SuperMetroidRuntime? runtime;
    /// <summary>Live gameplay runtime, absent during frontend-only phases.</summary>
    internal SuperMetroidRuntime? Runtime { get => runtime; set => runtime = value; }
    /// <summary>The state of the opening title sequence, which exists only while that phase runs.</summary>
    internal TitleSequenceState ActiveTitle => RequireScreen<TitleSequenceState>("opening title sequence");
    /// <summary>The state of the file-select menu, which exists only while that phase runs.</summary>
    internal FileSelectMenuState ActiveFileSelect => RequireScreen<FileSelectMenuState>("file-select menu");
    /// <summary>The state of the saved-game map, which exists only while that phase runs.</summary>
    internal FileSelectMapMenuState ActiveFileSelectMap => RequireScreen<FileSelectMapMenuState>("saved-game map");
    /// <summary>The state of the options menu, which exists only while that phase runs.</summary>
    internal GameOptionsMenuState ActiveOptions => RequireScreen<GameOptionsMenuState>("options menu");
    /// <summary>The state of the intro cinematic, which exists only while that phase runs.</summary>
    internal IntroCinematicState ActiveIntro => RequireScreen<IntroCinematicState>("intro cinematic");
    /// <summary>The state of the Ceres destruction cinematic, which exists only while that phase runs.</summary>
    internal CeresDestructionCinematicState ActiveCeresDestruction => RequireScreen<CeresDestructionCinematicState>("Ceres destruction cinematic");
    /// <summary>The state of the ending and credits, which exists only while that phase runs.</summary>
    internal EndingCreditsState ActiveEndingCredits => RequireScreen<EndingCreditsState>("ending and credits");
    /// <summary>The state of the pause menu, which exists only while that phase runs.</summary>
    internal PauseMenuState ActivePauseMenu => RequireScreen<PauseMenuState>("pause menu");
    /// <summary>The live gameplay runtime; gameplay phases run only while a game is loaded.</summary>
    internal SuperMetroidRuntime GameplayRuntime => runtime ??
        throw new InvalidOperationException("No gameplay runtime is loaded; this phase runs only during gameplay.");
    /// <summary>The choices the options menu closed with, which every selected game starts with; null before any.</summary>
    internal SelectedGameplayOptions? ChosenOptions { get; private set; }
    /// <summary>Keeps <paramref name="options"/>'s choices for the selected game the menu starts.</summary>
    internal void ChooseOptions(GameOptionsMenuState options)
    {
        Ensure.NotNull(options);
        ChosenOptions = new SelectedGameplayOptions(options.JapaneseText, options.ControllerBindings, options.MoonwalkEnabled,
            options.IconCancelEnabled);
    }
    /// <summary>Ceres elevator departure and blackout transition state.</summary>
    internal readonly CeresDepartureState ceresDeparture = new();
    /// <summary>Reserve-tank recovery state that can temporarily own the dispatcher.</summary>
    internal readonly SamusReserveAutoRecoveryState reserveRecovery = new();
    /// <summary>Door approach and destination loading state across gameplay states.</summary>
    internal readonly DoorTransitionState doorTransition = new();
    /// <summary>Current battery-backed save slot selected by the frontend.</summary>
    internal int SelectedSaveSlot { get; private set; }
    /// <summary>Distinguishes a saved-game load from fresh Ceres setup.</summary>
    internal bool LoadingExistingSave { get; set; }
    /// <summary>Selects <paramref name="slot"/>, which holds a save to load when <paramref name="existing"/>.</summary>
    internal void SelectSaveSlot(int slot, bool existing)
    {
        SelectedSaveSlot = slot;
        LoadingExistingSave = existing;
        saves.SelectSlot(slot);
    }
    /// <summary>The next load continues the selected slot's save.</summary>
    internal void ContinueSavedGame() => LoadingExistingSave = true;
    /// <summary>Save state retained for restart after the Space-Time intro sequence.</summary>
    private SuperMetroidSaveSlot? spacetimeIntroRestartSlot;
    /// <summary>Retains <paramref name="slot"/>'s live inventory for the Ceres setup after a SpaceTime-triggered intro.</summary>
    internal void RetainSpacetimeRestart(SuperMetroidSaveSlot slot) => spacetimeIntroRestartSlot = Ensure.NotNull(slot);
    /// <summary>A soft reset forgets any SpaceTime restart.</summary>
    internal void ForgetSpacetimeRestart() => spacetimeIntroRestartSlot = null;
    /// <summary>The fade into Landing Site or Ceres arrival, from black on a two-frame cadence.</summary>
    internal BrightnessFade GameplayFadeIn { get; } = new(0, 1);
    /// <summary>Selects Ceres arrival rather than Landing Site at the fade destination.</summary>
    internal bool GameplayFadeLeadsToCeresArrival { get; private set; }
    /// <summary>Gameplay brightness retained through pause and unpause fades, 0 (black) through 15.</summary>
    internal int PauseBrightness { get; set; } = PauseFadeTiming.FullyLit;
    /// <summary>Steps the shared screen fade one frame toward lit and records the level, without publishing it.</summary>
    internal int AdvanceScreenFadeIn() => PauseBrightness = screenFade.Brighten(PauseBrightness);
    /// <summary>Steps the shared screen fade one frame toward lit, publishes the level and returns it.</summary>
    internal int StepScreenFadeIn()
    {
        int brightness = AdvanceScreenFadeIn();
        ApplyDisplayBrightness(brightness);
        return brightness;
    }
    /// <summary>Steps the shared screen fade one frame toward black, publishes the level and returns it.</summary>
    internal int StepScreenFadeOut()
    {
        int brightness = PauseBrightness = screenFade.Darken(PauseBrightness);
        ApplyDisplayBrightness(brightness);
        return brightness;
    }
    /// <summary>Records the brightness register at full, as a fade-in that wrote $51 = $0F leaves it.</summary>
    internal void RecordFullBrightness() => PauseBrightness = PauseFadeTiming.FullyLit;
    /// <summary>Palette transition for Samus's death sequence, when active.</summary>
    internal CartridgePaletteTransition? DeathPaletteFade { get; private set; }
    /// <summary>Starts Samus's death palette transition.</summary>
    internal void BeginDeathPaletteFade(CartridgePaletteTransition fade) => DeathPaletteFade = Ensure.NotNull(fade);
    /// <summary>Palette transition for the escape time-up sequence.</summary>
    internal CartridgePaletteTransition? TimeUpPaletteFade { get; private set; }
    /// <summary>Starts the escape time-up white-out.</summary>
    internal void BeginTimeUpPaletteFade(CartridgePaletteTransition fade) => TimeUpPaletteFade = Ensure.NotNull(fade);
    /// <summary>Ends the escape time-up white-out.</summary>
    internal void EndTimeUpPaletteFade() => TimeUpPaletteFade = null;
    /// <summary>The death-state blackout, from full brightness on a two-frame cadence.</summary>
    internal BrightnessFade DeathFade { get; } = new(15, 0);
    /// <summary>The ending transition after the Zebes escape, one level darker every frame.</summary>
    internal BrightnessFade EscapeFade { get; } = new(15, 0);
    /// <summary>The most recent update's ordered semantic audio events; the session has already rendered them.</summary>
    internal IReadOnlyList<AudioEvent> LastAudioEvents { get; private set; } = Array.Empty<AudioEvent>();
    /// <summary>Advances the audio session by this update's commands and keeps the events it publishes.</summary>
    internal void AdvanceAudio(IReadOnlyList<CartridgeAudioCommand> commands) => LastAudioEvents = audioSession.Advance(commands);
    /// <summary>
    /// Runtime publication number last copied into the frontend audio queue. A runtime counts its completed gameplay
    /// publications from one, so zero means none has been copied and a runtime that has not completed one has nothing
    /// to copy.
    /// </summary>
    private ulong lastAudioRuntimeGameplayPublication;
    /// <summary>Room state associated with the last copied audio publication.</summary>
    internal CartridgeRoomState? LastAudioRoomState { get; set; }
    /// <summary>Forgets the publication and room whose audio was last queued, so the next runtime's queue from scratch.</summary>
    internal void ForgetQueuedAudio()
    {
        lastAudioRuntimeGameplayPublication = 0;
        LastAudioRoomState = null;
    }
    /// <summary>
    /// Raised after translated code changed save RAM. The handlers belong to the host, not the simulation: a debugger
    /// state leaves them out, and the host subscribes again to a restored game.
    /// </summary>
    [field: NonSerialized]
    internal event Action? SaveRamChanged;
    /// <summary>Raises the game's save-RAM-changed event after translated code changed save RAM.</summary>
    internal void RaiseSaveRamChanged() => SaveRamChanged?.Invoke();
    /// <summary>Current value of the native game-state word at WRAM $0998.</summary>
    public SuperMetroidGameState GameState { get; internal set; } = SuperMetroidGameState.Reset;
    /// <summary>The hardware-timing policies fixed at construction; production uses the lag-free defaults.</summary>
    internal SuperMetroidGameHostServices HostServices { get; }
    /// <summary>How far room loading has progressed; see <see cref="IRoomLoadProgress"/>.</summary>
    internal IRoomLoadProgress RoomLoadProgress => HostServices.RoomLoadProgress;
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
            roomState != LastAudioRoomState)
        {
            // $82:E071 loads data during door setup. $82:E664 selects the track
            // only after the opening scroll and this data queue have completed.
            if (GameState == SuperMetroidGameState.LoadingNextRoomB)
                audio.QueueRoomMusicData(roomState.MusicDataIndex);
            else
                audio.QueueRoomMusic(roomState.MusicDataIndex, roomState.MusicTrackIndex);
            LastAudioRoomState = roomState;
        }
        // Enemy initialization follows the room's music-data queueing in native loading. Before the first room load,
        // such as while a Ceres save starts its destruction cinematic, there are no enemies to initialize.
        if (runtime?.Room.Scope is { } scope)
            foreach (MusicCommand command in scope.Enemies.Audio.ConsumeInitializationMusicDelayed8())
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
        // An accepted save station resumes from the message close boundary too.
        if (runtime?.Room.Scope?.Plms.Stations.ConsumeSavingSoundRequest() is { } saving)
            audio.QueueSoundAndGetAccumulator(saving.SoundEffect, saving.MaximumQueued, soundSuppressed: saving.SoundSuppressed);
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
            // These state machines predate the shared SoundRequest list. Consume their
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

        if (runtime.Room.LoadedScope.Projectiles.LastFrameResult.QueuedSoundEffect is { } projectileSound)
        {
            audio.QueueSoundAndGetAccumulator(
                projectileSound,
                runtime.Room.LoadedScope.Projectiles.LastFrameResult.QueuedSoundMaximum,
                soundSuppressed: runtime.Room.LoadedScope.Projectiles.LastFrameResult.QueuedSoundSuppressed);
        }

        foreach (SoundRequest request in
            runtime.Room.LoadedScope.Projectiles.LastFrameResult.AdditionalSoundRequests ?? [])
        {
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }

        foreach (SoundRequest request in runtime.Room.LoadedScope.Projectiles.ImpactAudio.ImpactSoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);

        foreach (SoundRequest request in
            runtime.Room.LoadedScope.BombProjectiles.LastFrameResult.SoundRequests ?? [])
        {
            audio.QueueSoundAndGetAccumulator(
                request.SoundEffect,
                request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        }

        foreach (SoundRequest request in runtime.Room.LoadedScope.Plms.Sounds.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (DelayedMusicRequest request in runtime.Room.LoadedScope.Plms.BombTorizoHand.MusicRequests)
            audio.QueueMusicDelayed(request.Command, request.Delay);
        if (runtime.Room.LoadedScope.Plms.CollectiblePlms.ConsumeCollectibleFanfareRequest())
            audio.QueuePermanentItemFanfare();

        foreach (SoundRequest request in runtime.Room.LoadedScope.Enemies.Audio.SoundRequests)
            audio.QueueSoundAndGetAccumulator(request.SoundEffect, request.MaximumQueued,
                soundSuppressed: request.SoundSuppressed);
        foreach (DelayedMusicRequest request in runtime.Room.LoadedScope.Enemies.Audio.MusicRequests)
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
        EscapeFade.Start(15, 0);
        GameState = SuperMetroidGameState.SamusEscapesFromZebes;
    }
    /// <summary>Writes frontend brightness and retains its last level for pause fades.</summary>
    internal void ApplyDisplayBrightness(int brightness)
    {
        // Every published level is a write of the brightness register $51; pause entry
        // fades from the last one.
        PauseBrightness = brightness;
        display.ApplyBrightness(checked((byte)brightness));
    }
    /// <summary>Starts a pause fade from <paramref name="initialBrightness"/> with both fade words reloaded.</summary>
    internal void BeginPauseFade(int initialBrightness)
    {
        PauseBrightness = initialBrightness;
        screenFade.Reload();
    }
    /// <summary>Allocates and binds the gameplay owner without selecting a room.</summary>
    /// <param name="purpose">Whether the runtime plays a selected game, which takes the host and menu options, or an attract demo.</param>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(runtime))]
    internal void CreateGameplayRuntime(GameplayRuntimePurpose purpose)
    {
        if (runtime is not null)
            menuLayer1.RetainFrom(runtime);
        bool attract = purpose == GameplayRuntimePurpose.AttractDemo;
        runtime = GameplayRuntimeFactory.Create(new GameplayRuntimeStart
        {
            Purpose = purpose,
            Overrides = ConfiguredOptions.RuntimeOverrides(attract),
            Random = Random,
            MainGameLoopCarry = bootMainLoopCarry,
            Layer1 = menuLayer1,
            Artwork = Artwork,
            Clock = frameClock,
            FloorQueueResidue = floorQueueResidue,
            // Attract scripts retain the default controls and options of their private runtime.
            Selected = attract ? null : ChosenOptions ?? new SelectedGameplayOptions(false, ControllerBindings.Default, false, false),
        });
    }
    /// <summary>Applies live menu controller and gameplay choices to a newly created runtime.</summary>
    internal void ApplySelectedGameOptions()
    {
        // Live edits in options take precedence over the snapshot loaded at file select.
        if (ChosenOptions is not { } options) return;
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
        CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);

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
            runtime.Tester.ApplyTesterInventory(target: TesterInventoryTarget.RestoredInventory);
            restartedSamus.SelectedHudItem = HudItem.Beam;
            restartedSamus.AutoCancelHudItemIndex = HudItem.Beam;
            AutomaticCheckpointSaver.SaveCeresArrival(saves, runtime, SelectedSaveSlot);
            RaiseSaveRamChanged();
            spacetimeIntroRestartSlot = null;
            LoadingExistingSave = false;
            return true;
        }

        if (LoadingExistingSave)
        {
            SuperMetroidSaveSlot slot = saves.ReadSlot(SelectedSaveSlot)
                ?? throw new InvalidDataException(
                    $"Selected save slot {SelectedSaveSlot} became invalid during startup.");
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
                runtime.Tester.ApplyTesterInventory(target: TesterInventoryTarget.RestoredInventory);
                runtime.Player.Samus.SelectedHudItem = HudItem.Beam;
                runtime.Player.Samus.AutoCancelHudItemIndex = HudItem.Beam;
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
            LastAudioRoomState = loadedState;
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
        AutomaticCheckpointSaver.SaveCeresArrival(saves, runtime, SelectedSaveSlot);
        RaiseSaveRamChanged();
        return true;
    }
    /// <summary>
    /// Enters cartridge game state seven with a fully dark screen and the native two-frame
    /// brightness cadence. A fresh Ceres game must execute complete gameplay/OAM frames in
    /// this state before state $20 owns the elevator arrival; otherwise the level-data pad
    /// is briefly visible before its bank-$86 concealer reaches displayed OAM.
    /// </summary>
    internal void BeginGameplayFadeIn(GameplayFadeTarget target)
    {
        GameplayFadeIn.Start(0, 1);
        GameplayFadeLeadsToCeresArrival = target == GameplayFadeTarget.CeresArrival;
        GameState = SuperMetroidGameState.MainGameplayFadeIn;
    }
    /// <summary>
    /// <c>$82:8106</c> ends the fade-in by selecting state seven, which then enters state eight; the Ceres arrival is
    /// ordinary gameplay with Samus locked. HandleFadingIn wrote $51 = $0F this frame; the level is published before it
    /// rises, so the register is recorded directly.
    /// </summary>
    internal void FinishGameplayFadeIn()
    {
        GameState = SuperMetroidGameState.MainGameplay;
        GameplayFadeLeadsToCeresArrival = false;
        screenFade.Clear();
        RecordFullBrightness();
    }
    /// <summary>Persists the automatic Landing Site checkpoint after the gunship lands.</summary>
    internal void HandleGunshipLandingSave()
    {
        if (runtime is null ||
            !AutomaticCheckpointSaver.TrySaveGunshipLanding(saves, runtime, SelectedSaveSlot))
            return;
        RaiseSaveRamChanged();
    }
    /// <summary>Where the attract demos are.</summary>
    internal AttractDemoSchedule Demo { get; } = new();
    /// <summary>Unlocks the fourth demo set only after a valid completed-game save exists.</summary>
    internal int AvailableDemoSetCount() => bootDemoSetCount;
    /// <summary>The NMI waits the current loading dispatch still owes.</summary>
    internal GameLoadingWait GameLoading { get; } = new();
    /// <summary>
    /// The loading state state six branches on (<c>SRAMMirror_LoadingGameState</c>): a save continued
    /// from the file-select map, or the Ceres destruction's Zebes landing.
    /// </summary>
    internal SaveLoadingGameState StateSixLoadingRoute { get; private set; }
    /// <summary>Selects the branch state six loads by.</summary>
    internal void RouteLoading(SaveLoadingGameState route) => StateSixLoadingRoute = route;
    /// <summary>
    /// True when the next update is an NMI continuation inside the loading dispatch rather
    /// than a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesGameLoadingWait => GameLoading.Resumes;
    /// <summary>The downward elevator's wait before destination loading resumes.</summary>
    internal DownwardsElevatorWait DownwardsElevator { get; } = new();
    /// <summary>The session's random-number word, which the frontend and every gameplay runtime share.</summary>
    internal CartridgeRandom Random { get; }
    /// <summary>A soft reset clears bank $7E, discarding every gameplay owner; the menu owners keep the RNG and NMI counters.</summary>
    internal void DiscardRuntime() => runtime = null;
    /// <summary>Transfers the layer-one position back to the frontend before dropping gameplay state.</summary>
    internal void ReleaseRuntime()
    {
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
