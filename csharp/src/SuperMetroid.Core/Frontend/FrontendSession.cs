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
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>The frontend's shared state and its owners: the runtime, audio, display, saves, scenes, fades and demo counters that the game-state phases run against.</summary>
internal sealed class FrontendSession
{
    /// <summary>Creates a reset-state session, reading the selected save slot and the earthquake rumble delay power-on gives.</summary>
    /// <param name="saves">The saves the session reads and writes for its whole lifetime.</param>
    /// <param name="assists">The assists the session plays under, or <see langword="null"/> for the unassisted game.</param>
    /// <param name="hostServices">The compatibility services and display policy the session runs against for its whole lifetime.</param>
    /// <param name="random">The session's random number generator, which the frontend and every gameplay runtime share.</param>
    internal FrontendSession(SaveSlotStore saves, GameplayAssists? assists,
        SuperMetroidGameHostServices hostServices, SharedRandom random)
    {
        HostServices = Ensure.NotNull(hostServices);
        Random = Ensure.NotNull(random);
        Assists = assists ?? GameplayAssists.None;
        display = new FrontendDisplay(hostServices.GameplayFrames);
        // One set of handshakes for the session's whole lifetime, like the clock: each library's handler carries its
        // phase across menus and gameplay runtimes, and a soft reset returns it to ready with the queues.
        audio = new AudioQueues(new SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder));
        audioSession = new AudioSession(soundClock);
        this.saves = Ensure.NotNull(saves);
        SelectedSaveSlot = saves.ReadSelectedSlot();
        // Construction is power-on: boot evaluates the saves once before entering the main game loop.
        earthquakeSoundDelay = saves.EarthquakeSoundDelayAtPowerOn();
        bootDemoSetCount = saves.DetermineNumberOfDemoSets();
    }

    /// <summary>Shared fade clock for frontend transitions between rendered phases.</summary>
    internal readonly ScreenFadeTimer screenFade = new();
    /// <summary>The menu's retained camera position, handed to and from each gameplay runtime.</summary>
    internal readonly MenuCameraRetention menuCamera = new();

    /// <summary>The session's frame counters: every runtime it creates counts on them.</summary>
    internal readonly FrameClock frameClock = new();
    /// <summary>The installed catalogs currently bound; <see cref="GameArtwork.Unbound"/> until a host binds them.</summary>
    public GameArtwork Artwork { get => field ?? GameArtwork.Unbound; internal set; }
    /// <summary>The session's unauthored Speed Booster cadence, whose delays Ceres Ridley's getaway leaves for the rest of the session.</summary>
    internal readonly UnauthoredBoostCadence unauthoredBoostCadence = new();
    /// <summary>When an open save confirmation reads the controller; every runtime's message box consults it.</summary>
    internal readonly SaveChoiceInputCadence saveChoiceInput = new();
    /// <summary>The live and stored explored maps the game keeps from power-on across every game and demo of the session.</summary>
    internal readonly StoredExploredMaps<AreaId> storedExploredMaps = StoredExploredMapLoads.PowerOn();
    /// <summary>The assists a selected game plays under; attract demos always play unassisted.</summary>
    public GameplayAssists Assists { get; internal set; }
    /// <summary>Owns scene capture and raster publication for the current frontend frame.</summary>
    internal readonly FrontendDisplay display;
    /// <summary>The save slots and the menu selection; the host persists them.</summary>
    internal readonly SaveSlotStore saves;
    /// <summary>The frame the save file found at power-on adds to every earthquake rumble's delay; fixed for the session.</summary>
    internal readonly PowerOnEarthquakeSoundDelay earthquakeSoundDelay;
    /// <summary>Boot-time number of demo sets; fixed for the session.</summary>
    internal readonly int bootDemoSetCount;
    /// <summary>The music queue and the sound libraries' request queues, shared across menus and gameplay.</summary>
    internal readonly AudioQueues audio;
    /// <summary>
    /// When each sound library accepts the game's requests and what it acknowledges: one clock for the session's whole
    /// lifetime, so its requests and request-check phase carry across menus and gameplay runtimes. It serves the libraries
    /// in queue order, and the Samus library reports a music load.
    /// </summary>
    internal readonly SoundAcceptanceClock<SoundEffectLibrary, SoundEffectId> soundClock =
        new(SoundEffectLibraries.InQueueOrder, SoundEffectLibrary.Samus);
    /// <summary>The audio advance on <see cref="soundClock"/> and clip playback, advanced at the end of every update.</summary>
    internal readonly AudioSession audioSession;
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
    /// <summary>The save slot the frontend has selected.</summary>
    internal int SelectedSaveSlot { get; private set; }
    /// <summary>Distinguishes a saved-game load from fresh Ceres setup.</summary>
    internal bool LoadingExistingSave { get; set; }
    /// <summary>Selects <paramref name="slot"/>, which holds a save to load when <paramref name="existing"/>.</summary>
    internal void SelectSaveSlot(int slot, bool existing)
    {
        SelectedSaveSlot = slot;
        LoadingExistingSave = existing;
        saves.SelectSlot(slot);
        if (existing)
        {
            // Selecting a save loads it, its explored maps with it, and loads the live map from its area's.
            SuperMetroidSaveSlot save = saves.ReadSlot(slot)
                ?? throw new InvalidDataException($"Selected save slot {slot} holds no valid save.");
            StoredExploredMapLoads.LoadFile(storedExploredMaps, save.Progression);
            storedExploredMaps.LoadLiveMap(save.Checkpoint.Area);
        }
        else
        {
            // Selecting an empty slot starts a new file, which clears every stored map and leaves the live map as it was.
            StoredExploredMapLoads.ClearForNewFile(storedExploredMaps);
        }
    }
    /// <summary>The next load continues the selected slot's save.</summary>
    internal void ContinueSavedGame() => LoadingExistingSave = true;
    /// <summary>Save state retained for restart after the Space-Time intro sequence.</summary>
    private SuperMetroidSaveSlot? spacetimeIntroRestartSlot;
    /// <summary>Retains <paramref name="slot"/>'s live inventory for the Ceres setup after a Spacetime-triggered intro.</summary>
    internal void RetainSpacetimeRestart(SuperMetroidSaveSlot slot) => spacetimeIntroRestartSlot = Ensure.NotNull(slot);
    /// <summary>A soft reset forgets any Spacetime Beam restart.</summary>
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
        SetDisplayBrightness(brightness);
        return brightness;
    }
    /// <summary>Steps the shared screen fade one frame toward black, publishes the level and returns it.</summary>
    internal int StepScreenFadeOut()
    {
        int brightness = PauseBrightness = screenFade.Darken(PauseBrightness);
        SetDisplayBrightness(brightness);
        return brightness;
    }
    /// <summary>Records the screen brightness at full, as a completed fade-in leaves it.</summary>
    internal void RecordFullBrightness() => PauseBrightness = PauseFadeTiming.FullyLit;
    /// <summary>The death-state blackout, from full brightness on a two-frame cadence.</summary>
    internal BrightnessFade DeathFade { get; } = new(15, 0);
    /// <summary>The ending transition after the Zebes escape, one level darker every frame.</summary>
    internal BrightnessFade EscapeFade { get; } = new(15, 0);
    /// <summary>The most recent update's ordered semantic audio events; the session has already rendered them.</summary>
    internal IReadOnlyList<AudioEvent> LastAudioEvents { get; private set; } = Array.Empty<AudioEvent>();
    /// <summary>Advances the audio session by this update's commands and keeps the events it publishes.</summary>
    internal void AdvanceAudio(IReadOnlyList<AudioRequest> requests) => LastAudioEvents = audioSession.Advance(requests);
    /// <summary>
    /// Runtime publication number last copied into the frontend audio queue. A runtime counts its completed gameplay
    /// publications from one, so zero means none has been copied and a runtime that has not completed one has nothing
    /// to copy.
    /// </summary>
    private ulong lastAudioRuntimeGameplayPublication;
    /// <summary>Room state associated with the last copied audio publication.</summary>
    internal RoomStateSettings? LastAudioRoomState { get; set; }
    /// <summary>Forgets the publication and room whose audio was last queued, so the next runtime's queue from scratch.</summary>
    internal void ForgetQueuedAudio()
    {
        lastAudioRuntimeGameplayPublication = 0;
        LastAudioRoomState = null;
    }
    /// <summary>
    /// Raised after the game changed its saves. The handlers belong to the host, not the game: a saved session leaves
    /// them out, and the host subscribes again when it binds a game to its save storage.
    /// </summary>
    [field: NonSerialized]
    internal event Action? SavesChanged;
    /// <summary>Raises <see cref="SavesChanged"/> after the game changed its saves.</summary>
    internal void RaiseSavesChanged() => SavesChanged?.Invoke();
    /// <summary>The phase the game is in.</summary>
    public SuperMetroidGameState GameState { get; internal set; } = SuperMetroidGameState.Reset;
    /// <summary>The compatibility services fixed at construction; production uses the game's own timing.</summary>
    internal SuperMetroidGameHostServices HostServices { get; }
    /// <summary>How far room loading has progressed; see <see cref="IRoomLoadProgress"/>.</summary>
    internal IRoomLoadProgress RoomLoadProgress => HostServices.RoomLoadProgress;
    /// <summary>
    /// Moves the update's published sound and music requests into the audio queues.
    /// </summary>
    /// <remarks>
    /// This is intentionally a collection seam, not sound policy. Samus, projectiles, and
    /// room objects own the sound and priority of each request; the frontend merely queues the
    /// requests those producers published, in their order.
    /// </remarks>
    internal void CollectTranslatedAudioRequests(GameplayAudioFramePublication gameplayAudio)
    {
        // Demo loading suppresses room music; the demo's sound switch suppresses
        // scene SFX while the title track continues. Do not let runtime publishers
        // replace it with each demo room's normal area music.
        if (runtime?.AttractDemo.IsAttractDemo == true)
            return;
        if (runtime?.Room.ActiveRoom?.State is { } roomState &&
            roomState != LastAudioRoomState)
        {
            // Door setup loads music data. The track is selected
            // only after the opening scroll and this data queue have completed.
            if (GameState == SuperMetroidGameState.DoorTransition)
                audio.QueueRoomMusicData(roomState.Music.DataSet);
            else
                audio.QueueRoomMusic(roomState.Music);
            LastAudioRoomState = roomState;
        }
        // The music this room's enemies start is queued after the room's own, with the shortest delay. Before the
        // first room load, such as while a Ceres save starts its destruction cinematic, there are no enemies.
        if (runtime?.Room.Scope is { } scope)
            foreach (MusicRequest request in scope.Enemies.Audio.ConsumeRoomEnemyStartMusic())
                audio.QueueMusic(request);

        // The message-box save choice runs outside gameplay, not part of the gameplay
        // publication generation below. It has explicit consume semantics so its one-shot
        // request remains audible without making stale enemy and room-object lists eligible again.
        if (runtime?.MessageBoxes.ConsumeMessageBoxSelectionSoundRequest() == true)
            audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
        if (runtime?.MessageBox.ConsumeSavingSoundRequest() == true)
            audio.QueueSound(SamusSound.Saving, priority: SoundRequestPriority.Normal);
        if (runtime?.MessageBoxes.ConsumeGunshipExitSoundRequest() == true)
            audio.QueueSound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
        // An accepted save station resumes from the message close boundary too.
        if (runtime?.Room.Scope?.RoomObjects.Stations.ConsumeSavingSoundRequest() is { } saving)
            audio.QueueSound(saving);
        // Suit setup resumes from the message close boundary, outside normal gameplay
        // publication generations. Consume once so updates without gameplay cannot repeat it.
        if (runtime?.SuitPickup.ConsumeTransformationSoundRequest() == true)
            audio.QueueSound(EnemySound.SuitAcquired, priority: SoundRequestPriority.Normal,
                soundSuppressed: runtime.SuitPickup.TransformationSoundSuppressed);

        // Frontend states sometimes run updates without running gameplay.
        // In particular, the door coroutine waits for all unread sound-ring entries to
        // drain. The update count cannot guard this handoff: re-reading the last enemy or room-object
        // publication during each waiting update perpetually refills that ring. Consume only a
        // newly completed gameplay publication instead.
        if (runtime is null ||
            runtime.FrameBoundary.CompletedGameplayAudioPublication ==
                lastAudioRuntimeGameplayPublication)
            return;
        lastAudioRuntimeGameplayPublication =
            runtime.FrameBoundary.CompletedGameplayAudioPublication;

        // Room-effect actors and palette objects run before Samus,
        // projectiles, room objects, and enemies in main gameplay. Queue their calls first so a full
        // sound ring retains the same higher-priority request the game would retain
        // when several producers fire during one frame.
        gameplayAudio.PublishPrefix(runtime);

        if (runtime.Player.Samus is { } samus)
        {
            // These state machines predate the shared SoundRequest list. Consume their
            // one-shot publications here, retaining the exact library and queue-limit entry.
            if (samus.HorizontalSpeed.ConsumeEchoSoundRequest())
                audio.QueueSound(RoomSound.SpeedBooster, priority: SoundRequestPriority.Normal);
            if (samus.Shinespark.ConsumeStoredShineWarningSoundRequest())
                audio.QueueSound(RoomSound.StoredShinespark, SoundRequestPriority.High,
                    soundSuppressed: samus.Shinespark.StoredShineWarningSoundSuppressed);
            if (samus.Shinespark.ConsumeChargeCancellationSoundRequest())
                audio.QueueSound(SamusSound.Silence, SoundRequestPriority.High,
                    soundSuppressed: samus.Shinespark.ChargeCancellationSoundSuppressed);
            if (samus.Shinespark.ConsumeLaunchSoundRequest())
                audio.QueueSound(RoomSound.Shinespark, SoundRequestPriority.High,
                    soundSuppressed: samus.Shinespark.LaunchSoundSuppressed);
            if (samus.Shinespark.ConsumeCrashSoundRequest())
            {
                audio.QueueSound(SamusSound.SamusDamaged, SoundRequestPriority.Normal,
                    soundSuppressed: samus.Shinespark.CrashSoundSuppressed);
                audio.QueueSound(RoomSound.ShinesparkEnd, SoundRequestPriority.Normal,
                    soundSuppressed: samus.Shinespark.CrashSoundSuppressed);
            }
            if (samus.CrystalFlash.ConsumeActivationSoundRequest())
                audio.QueueSound(RoomSound.Silence, priority: SoundRequestPriority.Essential);
            if (samus.Xray.ConsumeActivationSoundRequest())
                audio.QueueSound(SamusSound.XrayStart, priority: SoundRequestPriority.Normal);
            if (samus.Xray.ConsumeDeactivationSoundRequest())
                audio.QueueSound(SamusSound.XrayEnd, priority: SoundRequestPriority.Normal);
            if (samus.DeathSequence.ConsumeSpinJumpSoundRequest())
                audio.QueueSound(SamusSound.SpinJumpEnd, priority: SoundRequestPriority.Normal);
        }

        if (runtime.Room.LoadedScope.Projectiles.LastFrameResult.QueuedSoundEffect is { } projectileSound)
        {
            audio.QueueSound(
                projectileSound,
                runtime.Room.LoadedScope.Projectiles.LastFrameResult.QueuedSoundMaximum
                    ?? throw new InvalidDataException("A projectile sound was queued without its queue limit."),
                soundSuppressed: runtime.Room.LoadedScope.Projectiles.LastFrameResult.QueuedSoundSuppressed);
        }

        foreach (SoundRequest request in
            runtime.Room.LoadedScope.Projectiles.LastFrameResult.AdditionalSoundRequests ?? [])
        {
            audio.QueueSound(request);
        }

        foreach (SoundRequest request in runtime.Room.LoadedScope.Projectiles.ImpactAudio.ImpactSoundRequests)
            audio.QueueSound(request);

        foreach (SoundRequest request in
            runtime.Room.LoadedScope.BombProjectiles.LastFrameResult.SoundRequests ?? [])
        {
            audio.QueueSound(request);
        }

        foreach (SoundRequest request in runtime.Room.LoadedScope.RoomObjects.Sounds.SoundRequests)
            audio.QueueSound(request);
        foreach (DelayedMusicRequest request in runtime.Room.LoadedScope.RoomObjects.BombTorizoHand.MusicRequests)
            audio.ScheduleMusic(request.Request, request.Delay);
        if (runtime.Room.LoadedScope.RoomObjects.CollectibleObjects.ConsumeCollectibleFanfareRequest())
            audio.QueuePermanentItemFanfare();

        foreach (SoundRequest request in runtime.Room.LoadedScope.Enemies.Audio.SoundRequests)
            audio.QueueSound(request);
        foreach (DelayedMusicRequest request in runtime.Room.LoadedScope.Enemies.Audio.MusicRequests)
            audio.ScheduleMusic(request.Request, request.Delay);
    }
    /// <summary>
    /// The state-publication half of Samus running out of health and the game timer. The
    /// runtime's main-gameplay update has already completed its room-main and clock work when this is called.
    /// </summary>
    /// <summary>
    /// The gunship writes the liftoff game state only after the top hull has crossed
    /// Y=256, zeroing the screen fade's delay and counter. It does not freeze time: the liftoff state keeps
    /// running full gameplay, so Samus and the RNG advance behind the fade. The main-gameplay
    /// update has already completed on this publication frame; the following call owns the fade.
    /// </summary>
    internal void BeginZebesEscapeFade()
    {
        EscapeFade.Start(15, 0);
        GameState = SuperMetroidGameState.SamusEscapesFromZebes;
    }
    /// <summary>Sets the screen's brightness on the published frame and retains it for pause fades.</summary>
    internal void SetDisplayBrightness(int brightness)
    {
        // Pause entry fades from the last level the screen was shown at.
        PauseBrightness = brightness;
        display.SetBrightness(brightness);
    }
    /// <summary>Starts a pause fade from <paramref name="initialBrightness"/> with both fade words reloaded.</summary>
    internal void BeginPauseFade(int initialBrightness)
    {
        PauseBrightness = initialBrightness;
        screenFade.Reload();
    }
    /// <summary>Allocates and binds the gameplay owner without selecting a room.</summary>
    /// <param name="purpose">Whether the runtime plays a selected game, which takes the assists and menu options, or an attract demo.</param>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(runtime))]
    internal void CreateGameplayRuntime(GameplayRuntimePurpose purpose)
    {
        if (runtime is not null)
            menuCamera.RetainFrom(runtime);
        bool attract = purpose == GameplayRuntimePurpose.AttractDemo;
        runtime = GameplayRuntimeFactory.Create(new GameplayRuntimeStart
        {
            Purpose = purpose,
            Assists = attract ? GameplayAssists.None : Assists,
            Carryover = new GameplaySessionCarryover
            {
                Clock = frameClock,
                Random = Random,
                UnauthoredBoostCadence = unauthoredBoostCadence,
                EarthquakeSoundDelay = earthquakeSoundDelay,
                SaveChoiceInput = saveChoiceInput,
                StoredExploredMaps = storedExploredMaps,
            },
            MenuCamera = menuCamera,
            Artwork = Artwork,
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
    /// The saved inventory supplies counters/icons; loading the game clears
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
            // Loading clears the selection before the visible HUD is built.
            SelectedItem: 0,
            slot.ReserveEnergy,
            slot.ReserveMode));
    /// <summary>Builds fresh Ceres or saved-game runtime state, including the selected HUD and room.</summary>
    internal bool SetupSelectedGame()
    {
        CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);

        if (spacetimeIntroRestartSlot is { } restartSlot)
        {
            // The game loaded the old inventory before the intro was selected. Ceres
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
            runtime.FrameBoundary.PublishWhileLoading();
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            SamusState restartedSamus = runtime.Player.Samus
                ?? throw new InvalidOperationException(
                    "SpaceTime restart did not create the Ceres Samus state.");
            RestoreSpacetimeRestartInventory(restartedSamus, restartSlot);
            runtime.Assists.GrantLoadoutToRestoredSamus();
            restartedSamus.SelectedHudItem = HudItem.Beam;
            restartedSamus.AutoCancelHudItemIndex = HudItem.Beam;
            AutomaticCheckpointSaver.SaveCeresArrival(saves, runtime, SelectedSaveSlot);
            RaiseSavesChanged();
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
            runtime.FrameBoundary.PublishWhileLoading();

            // Preserve the already-translated Ceres elevator entrance for its checkpoint.
            // Every other station uses the general room loader below.
            if (slot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
            {
                // Room selection and actors must see the restored mirror, not a
                // fresh runtime followed by an after-the-fact progression overwrite.
                runtime.RoomLoader.RestoreSavedPlayerState(slot);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                slot.ApplyTo(runtime.Player.Samus ?? throw new InvalidOperationException(
                    "Ceres initialization did not create Samus."));
                runtime.Assists.GrantLoadoutToRestoredSamus();
                runtime.Player.Samus.SelectedHudItem = HudItem.Beam;
                runtime.Player.Samus.AutoCancelHudItemIndex = HudItem.Beam;
                ApplySelectedGameOptions();
                return true;
            }

            runtime.RoomLoader.InitializeSavedGame(slot);
            // File selection loaded the saved options before the options menu ran.
            // InitializeSavedGame also serves direct diagnostic loads, so it restores
            // those fields itself. At this frontend boundary, the player's live edits
            // take precedence over that original snapshot, just as the game keeps them live.
            ApplySelectedGameOptions();
            RoomStateSettings loadedState = runtime.Room.ActiveRoom?.State
                ?? throw new InvalidOperationException(
                    "Saved-game appearance started without an active room state.");
            // Samus's appearance starts her fanfare after 14 frames and, on its fifth
            // call, schedules the room track 360 frames later. Mark this room state as
            // already handled so the generic room-change collector cannot overwrite the
            // fanfare with an immediate track request.
            // Loading from file select still has the title's music data set loaded.
            // Preserve the appearance track sequence, but switch to the room's data set first
            // just as ordinary room loading does; a set's own songs belong to that data set.
            audio.QueueRoomMusicData(loadedState.Music.DataSet);
            audio.ScheduleMusic(MusicRequest.SelectTrack(MusicTrack.SamusFanfare), MusicQueueDelay.AtLeastShortest(14));
            audio.ScheduleMusic(
                loadedState.Music.Track.Request,
                MusicQueueDelay.AtLeastShortest(360 + 5));
            LastAudioRoomState = loadedState;
            return false;
        }

        // New-game loading first publishes the standard HUD/sprite artwork, then
        // loads the area-six station-zero room and finally runs Samus's new-game setup. This is the
        // same sequence used by the room diagnostic; there is no host
        // terrain image, hand-placed Samus, or alternate playable-only initialization.
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.PublishWhileLoading();
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The intro's end publishes area six/station zero
        // and saves immediately before the Ceres game state. Both the
        // full cinematic and host-configured skip converge here, so neither path can omit
        // the automatic checkpoint.
        AutomaticCheckpointSaver.SaveCeresArrival(saves, runtime, SelectedSaveSlot);
        RaiseSavesChanged();
        return true;
    }
    /// <summary>
    /// Enters the fade-in game state with a fully dark screen and the two-frame
    /// brightness cadence. A fresh Ceres game must execute complete gameplay frames in
    /// this state before the Ceres elevator state owns the elevator arrival; otherwise the level-data pad
    /// is briefly visible before its concealing enemy projectile is drawn.
    /// </summary>
    internal void BeginGameplayFadeIn(GameplayFadeTarget target)
    {
        GameplayFadeIn.Start(0, 1);
        GameplayFadeLeadsToCeresArrival = target == GameplayFadeTarget.CeresArrival;
        GameState = SuperMetroidGameState.MainGameplayFadeIn;
    }
    /// <summary>
    /// The end of the fade-in selects the next state, which then enters main gameplay; the Ceres arrival is
    /// ordinary gameplay with Samus locked. The fade-in wrote full brightness this frame; the level is published before it
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
        RaiseSavesChanged();
    }
    /// <summary>Where the attract demos are.</summary>
    internal AttractDemoSchedule Demo { get; } = new();
    /// <summary>Unlocks the fourth demo set only after a valid completed-game save exists.</summary>
    internal int AvailableDemoSetCount() => bootDemoSetCount;
    /// <summary>The waiting updates the current game load still owes.</summary>
    internal GameLoadingWait GameLoading { get; } = new();
    /// <summary>
    /// What the game-data load loads: a save continued from the file-select map, or the Ceres destruction's Zebes
    /// landing. Nothing until a load is routed.
    /// </summary>
    internal GameDataLoadRoute? LoadingRoute { get; private set; }
    /// <summary>Selects what the game-data load loads.</summary>
    internal void RouteGameDataLoad(GameDataLoadRoute route) => LoadingRoute = route;
    /// <summary>
    /// True while the load waits out frames of the display: the next update continues that wait instead of starting a new
    /// update of the loading phase (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool ResumesGameLoadingWait => GameLoading.Resumes;
    /// <summary>The downward elevator's wait before destination loading resumes.</summary>
    internal DownwardsElevatorWait DownwardsElevator { get; } = new();
    /// <summary>The session's random number generator, which the frontend and every gameplay runtime share.</summary>
    internal SharedRandom Random { get; }
    /// <summary>A soft reset discards every gameplay owner; the menu owners keep the random generator and the frame clock.</summary>
    internal void DiscardRuntime() => runtime = null;
    /// <summary>Transfers the camera position back to the frontend before dropping gameplay state.</summary>
    internal void ReleaseRuntime()
    {
        // File-map selection may release when no runtime was ever created.
        if (runtime is not null)
            menuCamera.RetainFrom(runtime);
        runtime = null;
    }

    /// <summary>
    /// Applies the inventory that remains live across a Spacetime-triggered intro. The
    /// Mother Brain flashback temporarily replaces both missile counts and
    /// the intro clears them afterward; nothing touches Super Missiles or Power
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
