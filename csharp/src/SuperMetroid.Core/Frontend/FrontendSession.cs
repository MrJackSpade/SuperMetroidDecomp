using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Assets;
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
    /// <param name="audioOutput">What the session's audio produces for its host.</param>
    /// <param name="artwork">The installed catalogs every screen and gameplay runtime draws from until a host binds others.</param>
    internal FrontendSession(SaveSlotStore saves, GameplayAssists? assists,
        SuperMetroidGameHostServices hostServices, SharedRandom random, GameAudioOutput audioOutput, GameArtwork artwork)
    {
        HostServices = Ensure.NotNull(hostServices);
        Artwork = Ensure.NotNull(artwork);
        Random = Ensure.NotNull(random);
        Assists = assists ?? GameplayAssists.None;
        display = new FrontendDisplay(hostServices.GameplayFrames);
        // One set of request cycles for the session's whole lifetime, like the clock: each library's cycle keeps its
        // phase across menus and gameplay runtimes, and a soft reset returns it to ready with the queues. Every sound request
        // is tested against the live runtime's demo and power bomb as it is made.
        audio = new AudioQueues(new SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder),
            new SessionGameplaySoundState(this));
        audioSession = new AudioSession(soundClock, audioOutput);
        this.saves = Ensure.NotNull(saves);
        SelectedSaveSlot = saves.ReadSelectedSlot();
        // Construction is power-on: boot evaluates the saves once before entering the main game loop.
        earthquakeSoundDelay = saves.EarthquakeSoundDelayAtPowerOn();
        DemoPlaylist = StockAttractDemoScenes.Playlist(saves.DetermineCompletionDemoSetUnlocked());
        // Power-on starts the title sequence.
        Phase = new TitleResetPhase(this);
    }

    /// <summary>One phase for every sequence of the game, each of which names the phase it hands over to.</summary>
    internal GamePhases Phases { get; }

    /// <summary>The phase of the sequence the game is running.</summary>
    internal IGamePhase RunningPhase { get; private set; }

    /// <summary>
    /// The gameplay screen's fade, shared by the pause and unpause fades and the transitions between rendered phases: its
    /// level is the brightness gameplay was last shown at, which a pause fades from. Every return to main gameplay leaves its
    /// cadence stepping on every update, which a map-station pause then inherits; power-on waits one update first.
    /// </summary>
    internal readonly ScreenFade screenFade = new(DisplayBrightness.Full, stepDelay: 0, firstWait: PauseFadeTiming.WaitBeforeEachStep);
    /// <summary>
    /// The camera fractions the game keeps from one game or demo to the next: each runtime's first room's camera starts from
    /// them, and each released runtime leaves its camera's; the soft reset clears them.
    /// </summary>
    internal readonly CameraFractionsKeptAcrossGames cameraFractions = new();

    /// <summary>The session's frame counters: every runtime it creates counts on them.</summary>
    internal readonly FrameClock frameClock = new();
    /// <summary>
    /// The installed catalogs (#2224) every screen and gameplay runtime draws from: those the session was built with, until
    /// a host binds others (<see cref="SuperMetroidGame.BindArtwork"/>).
    /// </summary>
    public GameArtwork Artwork { get; internal set; }
    /// <summary>The session's unauthored Speed Booster cadence, whose delays Ceres Ridley's getaway leaves for the rest of the session.</summary>
    internal readonly UnauthoredBoostCadence unauthoredBoostCadence = new();
    /// <summary>When an open save confirmation reads the controller; every runtime's message box consults it.</summary>
    internal readonly SaveChoiceInputCadence saveChoiceInput = new();
    /// <summary>The updates gameplay stays paused before a message box opens and after it closes; every runtime's message box consults it.</summary>
    internal readonly MessageBoxPauses messageBoxPauses = new();
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
    /// <summary>Live gameplay runtime, absent during frontend-only phases.</summary>
    private SuperMetroidRuntime? runtime;
    /// <summary>Live gameplay runtime, absent during frontend-only phases.</summary>
    internal SuperMetroidRuntime? Runtime => runtime;
    /// <summary>The options every selected game starts with: unchosen until the options menu first closes.</summary>
    internal GameplayOptionsChoice OptionsChoice { get; private set; } = GameplayOptionsChoice.Unchosen;
    /// <summary>Keeps <paramref name="options"/>'s choices for the selected game the menu starts.</summary>
    internal void ChooseOptions(GameOptionsMenuState options)
    {
        Ensure.NotNull(options);
        OptionsChoice = new ChosenGameplayOptions(new SelectedGameplayOptions(options.JapaneseText, options.ControllerBindings,
            options.MoonwalkEnabled, options.IconCancelEnabled));
    }
    /// <summary>Ceres elevator departure and blackout transition state.</summary>
    internal readonly CeresDepartureState ceresDeparture = new();
    /// <summary>Reserve-tank recovery state that can temporarily own the game-state loop.</summary>
    internal readonly SamusReserveAutoRecoveryState reserveRecovery = new();
    /// <summary>Door approach and destination loading state across gameplay states.</summary>
    internal readonly DoorTransitionState doorTransition = new();
    /// <summary>The save slot the frontend has selected.</summary>
    internal SaveSlot SelectedSaveSlot { get; private set; }
    /// <summary>Distinguishes a saved-game load from fresh Ceres setup.</summary>
    internal bool LoadingExistingSave { get; private set; }
    /// <summary>Selects <paramref name="slot"/>, which holds a save to load when <paramref name="existing"/>.</summary>
    internal void SelectSaveSlot(SaveSlot slot, bool existing)
    {
        SelectedSaveSlot = slot;
        LoadingExistingSave = existing;
        saves.SelectSlot(slot);
        if (existing)
        {
            // Selecting a save loads it, its explored maps with it, and loads the live map from its area's.
            SuperMetroidSaveSlot save = saves.ReadSavedSlot(slot);
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
    /// <summary>
    /// The fade into Landing Site or Ceres arrival, from black, waiting <see cref="GameplayFadeDefinitions.FadeInWaitBeforeEachStep"/>
    /// updates before each step.
    /// </summary>
    internal ScreenFade GameplayFadeIn { get; } =
        new(0, stepDelay: GameplayFadeDefinitions.FadeInWaitBeforeEachStep, firstWait: GameplayFadeDefinitions.FadeInWaitBeforeEachStep);
    /// <summary>Selects Ceres arrival rather than Landing Site at the fade destination.</summary>
    internal bool GameplayFadeLeadsToCeresArrival { get; private set; }
    /// <summary>Gameplay brightness retained through pause and unpause fades, from black through full.</summary>
    internal int PauseBrightness => screenFade.Level;
    /// <summary>Steps the shared screen fade one frame toward lit, without publishing it; returns whether it is fully lit.</summary>
    internal bool AdvanceScreenFadeIn() => screenFade.Brighten();
    /// <summary>Steps the shared screen fade one frame toward lit and publishes the level; returns whether it is fully lit.</summary>
    internal bool StepScreenFadeIn()
    {
        bool lit = AdvanceScreenFadeIn();
        SetDisplayBrightness(screenFade.Level);
        return lit;
    }
    /// <summary>Steps the shared screen fade one frame toward black and publishes the level; returns whether it is black.</summary>
    internal bool StepScreenFadeOut()
    {
        bool black = screenFade.Darken();
        SetDisplayBrightness(screenFade.Level);
        return black;
    }
    /// <summary>Records the screen brightness at full, as a completed fade-in leaves it.</summary>
    internal void RecordFullBrightness() => screenFade.Show(DisplayBrightness.Full);
    /// <summary>The death-state blackout, from full brightness on a two-frame cadence.</summary>
    internal ScreenFade DeathFade { get; } = new(DisplayBrightness.Full);
    /// <summary>The ending transition after the Zebes escape, one level darker every frame.</summary>
    internal ScreenFade EscapeFade { get; } = new(DisplayBrightness.Full);
    /// <summary>The room state whose music was last queued.</summary>
    internal RoomStateSettings? LastAudioRoomState { get; private set; }
    /// <summary>The runtime whose room's enemies' start music was last queued (#2562).</summary>
    private SuperMetroidRuntime? _startMusicQueuedForRuntime;
    /// <summary>Which of that runtime's room loads it was: its count of rooms loaded then.</summary>
    private int _startMusicQueuedForRoomLoad;
    /// <summary>Forgets the room whose music was last queued, so the next runtime's room music is queued from scratch.</summary>
    internal void ForgetQueuedAudio() => LastAudioRoomState = null;
    /// <summary>
    /// Raised after the game changed its saves. The handlers belong to the host, not the game: a saved session leaves
    /// them out, and the host subscribes again when it binds a game to its save storage.
    /// </summary>
    [field: NonSerialized]
    internal event Action? SavesChanged;
    /// <summary>Raises <see cref="SavesChanged"/> after the game changed its saves.</summary>
    internal void RaiseSavesChanged() => SavesChanged?.Invoke();
    /// <summary>
    /// The sequence the game is running, at its current step (#2554): one phase, which carries what that step runs on and
    /// shows. A step that shows a menu or cinematic holds that screen, so the screen exists exactly while the game is at it.
    /// </summary>
    internal IGamePhase Phase { get; private set; }
    /// <summary>The sequence the game is running.</summary>
    public SuperMetroidGameState GameState => Phase.State;
    /// <summary>The gameplay phases, which the pause's brightening and the time-up's white-out also run (<see cref="Frontend.GameplayGamePhases"/>).</summary>
    internal GameplayGamePhases GameplayPhases => field ??= new GameplayGamePhases(this);
    /// <summary>
    /// Whether a door's transition is loading, scrolling into and fading in the next room: the step the door enters once its
    /// entry's stop requests are queued, during which it loads only the room's music data and sound effects are disabled.
    /// </summary>
    internal bool TransitioningThroughDoor => Phase is DoorTransitionGameSequence { CurrentStep: DoorTransitionStep.Transitioning };

    /// <summary>Enters <paramref name="phase"/>: the sequence it runs, at its step.</summary>
    internal void Enter(IGamePhase phase) => Phase = Ensure.NotNull(phase);

    /// <summary>Enters the fade into Landing Site or Ceres arrival; <see cref="BeginGameplayFadeIn"/> sets its fade.</summary>
    private void EnterGameplayFadeIn() => Enter(new GamePhase(Phases.GameplayFadeIn,
        PhaseRandomStep.RuntimeOwned,
        step: (controllerInput, live) => GameplayPhases.RunGameplayFadeIn(controllerInput, live),
        name: () => GameplayFadeLeadsToCeresArrival
            ? $"Ceres elevator fade-in (brightness {GameplayFadeIn.Level})"
            : $"Landing Site fade-in (brightness {GameplayFadeIn.Level})"));

    /// <summary>Enters main gameplay.</summary>
    internal void EnterMainGameplay() => Enter(new GamePhase(Phases.MainGameplay,
        PhaseRandomStep.RuntimeOwned,
        step: (controllerInput, live) => GameplayPhases.RunMainGameplay(controllerInput, live),
        name: () => "Gameplay"));

    /// <summary>Enters the AUTO reserves' refill.</summary>
    internal void EnterReserveTankRefill() => Enter(new GamePhase(SuperMetroidGameState.ReserveTankRefill,
        PhaseRandomStep.RuntimeOwned,
        step: (controllerInput, live) => GameplayPhases.RunReserveTankRefill(controllerInput, live),
        name: () => "Reserve tank refill"));

    /// <summary>Enters the Ceres elevator's departure, which <see cref="ceresDeparture"/> runs.</summary>
    internal void EnterCeresDeparture() => Enter(new CeresDepartureGameSequence(this));

    /// <summary>Enters the game-over menu, which opens on its first update.</summary>
    internal void EnterGameOverMenu() => Enter(new GameOverOpeningPhase(this));

    /// <summary>Enters the game load of <paramref name="route"/>.</summary>
    internal void Enter(GameDataLoadRoute route) => Enter(new LoadingGameSequence(this, route));

    /// <summary>Enters the door transition through <paramref name="door"/>, the door Samus entered, as she reaches it.</summary>
    internal void EnterDoor(DoorDefinition door)
    {
        Enter(DoorTransitionStep.EnteringDoor);
        doorTransition.Enter(door);
    }

    /// <summary>Enters the door transition at <paramref name="step"/>.</summary>
    internal void Enter(DoorTransitionStep step) => Enter(new DoorTransitionGameSequence(this, step));

    /// <summary>Enters the pause at <paramref name="step"/>, one that shows no pause screen.</summary>
    internal void Enter(PauseStep step) => Enter(new PauseGameSequence(this, GameplayPhases, step));

    /// <summary>Enters the pause at <paramref name="step"/>, showing <paramref name="menu"/>.</summary>
    internal void Enter(PauseMenuStep step, PauseMenuState menu) => Enter(new PauseMenuGameSequence(this, step, Ensure.NotNull(menu)));

    /// <summary>Enters the death sequence at <paramref name="step"/>.</summary>
    internal void Enter(DeathSequenceStep step) => Enter(new DeathGameSequence(this, step));

    /// <summary>
    /// Shows Ceres's destruction cinematic with <paramref name="outcome"/>, in the loaded game's text language, and enters
    /// it (#2541): each way in, the elevator's departure, a saved game resumed at the destruction and the escape's time-up
    /// with Samus aboard, has a gameplay runtime loaded.
    /// </summary>
    /// <param name="outcome">Whether Samus escaped Ceres or is aboard it.</param>
    /// <param name="gameplayRuntime">The runtime carrying Samus, whose game's text language the cinematic shows.</param>
    internal void BeginCeresDestruction(CeresDestructionOutcome outcome, SuperMetroidRuntime gameplayRuntime)
    {
        Ensure.NotNull(gameplayRuntime);
        Enter(new CeresDestructionGameSequence(this, outcome, new CeresDestructionCinematicState(
            audio, Artwork.ScreenContent.PowerBombGlowColors, Artwork.IntroCinematicArt, Artwork.SpriteSheets,
            outcome, japaneseText: gameplayRuntime.JapaneseText)));
    }

    /// <summary>Enters the time-up at <paramref name="step"/>.</summary>
    internal void Enter(TimeUpStep step) => Enter(new TimeUpGameSequence(this, GameplayPhases, step));

    /// <summary>Enters the attract demo at <paramref name="step"/>.</summary>
    internal void Enter(AttractDemoStep step) => Enter(new AttractDemoGameSequence(this, step));
    /// <summary>The compatibility services fixed at construction; production uses the game's own timing.</summary>
    internal SuperMetroidGameHostServices HostServices { get; }
    /// <summary>How far room loading has progressed; see <see cref="IRoomLoadProgress"/>.</summary>
    internal IRoomLoadProgress RoomLoadProgress => HostServices.RoomLoadProgress;
    /// <summary>
    /// Queues the audio the frontend's own boundaries request after the update's phase: the room's music when the room
    /// changed, the music the room's enemies start, and the one-shot sounds of a closing message box.
    /// </summary>
    /// <remarks>
    /// Gameplay's own sound and music requests are not collected here: every system queues each one into the session's
    /// queues as it makes it, through the runtime's <see cref="GameplayAudioRequests"/> (#1620).
    /// </remarks>
    internal void CollectFrontendAudioRequests()
    {
        // Demo loading suppresses room music; the demo's sound switch suppresses
        // scene SFX while the title track continues. Do not replace it with each
        // demo room's normal area music.
        if (runtime?.AttractDemo.IsAttractDemo == true)
            return;
        if (runtime?.Room.ActiveRoom?.State is { } roomState &&
            roomState != LastAudioRoomState)
        {
            // Door setup loads music data. The track is selected
            // only after the opening scroll and this data queue have completed.
            if (TransitioningThroughDoor)
                audio.QueueRoomMusicData(roomState.Music.DataSet);
            else
                audio.QueueRoomMusic(roomState.Music);
            LastAudioRoomState = roomState;
        }
        // The music this room's enemies start is queued after the room's own, with the shortest delay. Before the
        // first room load, such as while a Ceres save starts its destruction cinematic, there are no enemies.
        if (runtime?.Room.Scope is { Enemies: var enemies } &&
            (runtime != _startMusicQueuedForRuntime || runtime.Room.RoomsLoaded != _startMusicQueuedForRoomLoad))
        {
            foreach (MusicRequest request in enemies.Audio.RoomStartMusic)
                audio.QueueMusic(request);
            _startMusicQueuedForRuntime = runtime;
            _startMusicQueuedForRoomLoad = runtime.Room.RoomsLoaded;
        }

        // The message box runs outside gameplay: the save choice's move, the gunship's saving sound as its accepted prompt
        // closes and the gunship's pad opening as its prompt returns are events of the box's updates, taken once, so
        // updates that only show the box cannot repeat them. Each update of the box makes at most one of them.
        if (runtime is not null)
            foreach (SoundRequest sound in runtime.MessageBoxes.TakeSounds())
                audio.QueueSound(sound);
    }
    /// <summary>
    /// The game leaves Zebes once the gunship's top hull has risen past Y=256, starting the screen fade from full
    /// brightness. It does not freeze time: leaving Zebes keeps running full gameplay, so Samus and the RNG advance behind
    /// the fade. This update's gameplay has already run; the fade's first step is the next update's.
    /// </summary>
    internal void BeginZebesEscapeFade()
    {
        EscapeFade.Restart(DisplayBrightness.Full, 0);
        Enter(new LeavingZebesPhase(this));
    }
    /// <summary>
    /// The escape countdown expired, or the game returns to the time-up it started (#2518). The whole gameplay display, Samus
    /// and the HUD among it, fades to white through the shared fade step count in the time-up's gradual change (#1512).
    /// </summary>
    /// <param name="live">The runtime whose escape countdown expired.</param>
    internal void BeginTimeUp(SuperMetroidRuntime live)
    {
        Ensure.NotNull(live);
        // A white-out the AUTO reserves' refill interrupted goes on from where it stood when the game returns to the time-up
        // (#2518): the refill neither steps nor restarts it, so its colours stay part-whitened. Only the time-up's own
        // black-out ends the white-out, and the death that ends the other gameplay fade releases the runtime.
        if (!live.FadeOut.IsUnderWay)
            live.FadeOut.Begin(RgbColor.White, TimeUpDefinitions.WhiteOutParts, GameplayFadeSpares.Nothing);
        Enter(TimeUpStep.WhiteningOut);
    }
    /// <summary>
    /// Enters the death sequence's final black-out already black, as both time-up endings do. Its fade-out finds no
    /// brightness left, so that update opens the game-over menu.
    /// </summary>
    internal void BeginBlackDeathBlackOut()
    {
        DeathFade.Restart(0, 0);
        Enter(DeathSequenceStep.FinalBlackOut);
        display.PublishBlack();
    }
    /// <summary>Sets the screen's brightness on the published frame and retains it for pause fades.</summary>
    internal void SetDisplayBrightness(int brightness)
    {
        // Pause entry fades from the last level the screen was shown at.
        screenFade.Show(brightness);
        display.SetBrightness(brightness);
    }
    /// <summary>
    /// Starts a pause fade from <paramref name="initialBrightness"/>, waiting the pause fades' wait before each step
    /// (<see cref="PauseFadeTiming.WaitBeforeEachStep"/>).
    /// </summary>
    internal void BeginPauseFade(int initialBrightness)
    {
        screenFade.Restart(initialBrightness, PauseFadeTiming.WaitBeforeEachStep);
    }
    /// <summary>Allocates and binds the gameplay owner without selecting a room.</summary>
    /// <param name="purpose">Whether the runtime plays a selected game, which takes the assists and menu options, or an attract demo.</param>
    /// <returns>The new runtime, which the session now holds.</returns>
    internal SuperMetroidRuntime CreateGameplayRuntime(GameplayRuntimePurpose purpose)
    {
        runtime?.LeaveCameraFractions();
        bool attract = purpose == GameplayRuntimePurpose.AttractDemo;
        SuperMetroidRuntime created = GameplayRuntimeFactory.Create(new GameplayRuntimeStart
        {
            Purpose = purpose,
            Assists = attract ? GameplayAssists.None : Assists,
            Carryover = new GameplaySessionCarryover
            {
                Clock = frameClock,
                GameState = new SharedGameStateCarryover
                {
                    Random = Random,
                    EarthquakeSoundDelay = earthquakeSoundDelay,
                    StoredExploredMaps = storedExploredMaps,
                },
                Compatibility = new CompatibilityKeptAcrossGames
                {
                    UnauthoredBoostCadence = unauthoredBoostCadence,
                    SaveChoiceInput = saveChoiceInput,
                    MessageBoxPauses = messageBoxPauses,
                    CameraFractions = cameraFractions,
                },
                Audio = audio,
            },
            Artwork = Artwork,
            // Attract scripts retain the default controls and options of their private runtime.
            Selected = attract ? null : OptionsChoice.StartingOptions,
        });
        runtime = created;
        return created;
    }
    /// <summary>Applies live menu controller and gameplay choices to a newly created runtime.</summary>
    /// <param name="selected">The runtime of the selected game.</param>
    internal void ApplySelectedGameOptions(SuperMetroidRuntime selected)
    {
        Ensure.NotNull(selected);
        // Live edits in options take precedence over the snapshot loaded at file select.
        if (OptionsChoice is not ChosenGameplayOptions { Options: var options }) return;
        selected.Options.ControllerBindings = options.ControllerBindings;
        selected.Options.MoonwalkEnabled = options.MoonwalkEnabled;
        selected.Options.IconCancelEnabled = options.IconCancelEnabled;
    }
    /// <summary>
    /// Shared saved-game HUD construction for playable and cinematic checkpoints.
    /// The saved inventory supplies counters/icons; loading the game clears
    /// the persisted HUD selection before the first live gameplay update.
    /// </summary>
    /// <param name="slot">The save the game loads.</param>
    /// <param name="selected">The runtime of the selected game.</param>
    internal static void InitializeSelectedGameHud(SuperMetroidSaveSlot slot, SuperMetroidRuntime selected) =>
        Ensure.NotNull(selected).HudSetup.InitializeHud(new HudSnapshot(
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
            SelectedItem: HudItem.Beam,
            slot.ReserveEnergy,
            slot.ReserveMode));
    /// <summary>Builds fresh Ceres or saved-game runtime state, including the selected HUD and room.</summary>
    internal bool SetupSelectedGame()
    {
        SuperMetroidRuntime selected = CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);

        if (spacetimeIntroRestartSlot is { } restartSlot)
        {
            // The game loaded the old inventory before the intro was selected. Ceres
            // setup then starts from fresh progression while retaining equipment and other
            // resources. The flashback specifically replaces and then clears Missiles.
            selected.HudSetup.InitializeHud(new HudSnapshot(
                Health: restartSlot.Health,
                MaxHealth: restartSlot.MaxHealth,
                Missiles: 0,
                MaxMissiles: 0,
                SuperMissiles: restartSlot.SuperMissiles,
                MaxSuperMissiles: restartSlot.MaxSuperMissiles,
                PowerBombs: restartSlot.PowerBombs,
                MaxPowerBombs: restartSlot.MaxPowerBombs,
                EquippedItems: restartSlot.EquippedItems,
                SelectedItem: HudItem.Beam,
                ReserveHealth: restartSlot.ReserveEnergy,
                ReserveMode: restartSlot.ReserveMode));
            selected.FrameBoundary.PublishWhileLoading();
            selected.RoomLoader.InitializeCeresStartSamus(selected.RoomLoader.InitializeStartingCeresRoom());
            SamusState restartedSamus = selected.Player.Samus;
            RestoreSpacetimeRestartInventory(restartedSamus, restartSlot);
            selected.Assists.GrantLoadoutToRestoredSamus(restartedSamus);
            restartedSamus.SelectBeam();
            AutomaticCheckpointSaver.SaveCeresArrival(saves, selected, SelectedSaveSlot);
            RaiseSavesChanged();
            spacetimeIntroRestartSlot = null;
            LoadingExistingSave = false;
            return true;
        }

        if (LoadingExistingSave)
        {
            SuperMetroidSaveSlot slot = saves.ReadSavedSlot(SelectedSaveSlot);
            InitializeSelectedGameHud(slot, selected);
            selected.FrameBoundary.PublishWhileLoading();

            // A Ceres elevator save starts with the Ceres elevator's arrival, as a new game does; every other station
            // loads through the room loader below.
            if (slot.StartupRoute == SaveStartupRoute.CeresElevatorArrival)
            {
                // The save's progress is restored before the room loads, so the room's state and its enemies see it.
                selected.RoomLoader.RestoreSavedPlayerState(slot);
                selected.RoomLoader.InitializeCeresStartSamus(selected.RoomLoader.InitializeStartingCeresRoom());
                SamusState ceresSamus = selected.Player.Samus;
                slot.ApplyTo(ceresSamus);
                selected.Assists.GrantLoadoutToRestoredSamus(ceresSamus);
                ceresSamus.SelectBeam();
                ApplySelectedGameOptions(selected);
                return true;
            }

            RoomScope loaded = selected.RoomLoader.InitializeSavedGame(slot);
            // Loading the save restores the controls and options it holds; the choices the player has made in the
            // options menu since replace them, as the game keeps them live.
            ApplySelectedGameOptions(selected);
            RoomStateSettings loadedState = loaded.Room.State;
            // Samus's appearance starts her fanfare and then the room's track behind it
            // (SavedGameAppearanceMusicDefinitions). This room state is marked as already handled, so the room's
            // change of music does not replace the fanfare with the room's track at once.
            // Loading from file select still has the title's music data set loaded.
            // The appearance tracks still play in their order, but the room's data set loads first,
            // just as ordinary room loading does; a set's own songs belong to that data set.
            audio.QueueRoomMusicData(loadedState.Music.DataSet);
            audio.ScheduleMusic(MusicRequest.SelectTrack(MusicTrack.SamusFanfare), SavedGameAppearanceMusicDefinitions.FanfareDelay);
            audio.ScheduleRoomTrack(
                loadedState.Music.Track,
                SavedGameAppearanceMusicDefinitions.RoomTrackDelay);
            LastAudioRoomState = loadedState;
            return false;
        }

        // A new game first shows the standard HUD, then loads the Ceres elevator room and finally sets Samus up for the
        // new game.
        selected.HudSetup.InitializeHud(HudSnapshot.NewGame);
        selected.FrameBoundary.PublishWhileLoading();
        selected.RoomLoader.InitializeCeresStartSamus(selected.RoomLoader.InitializeStartingCeresRoom());
        // The intro's end publishes Ceres and its elevator checkpoint
        // and saves immediately before the Ceres game state. Both the
        // full cinematic and host-configured skip converge here, so neither path can omit
        // the automatic checkpoint.
        AutomaticCheckpointSaver.SaveCeresArrival(saves, selected, SelectedSaveSlot);
        RaiseSavesChanged();
        return true;
    }
    /// <summary>
    /// Enters the fade-in game state with a fully dark screen and the fade-in's brightness cadence
    /// (<see cref="GameplayFadeDefinitions.FadeInWaitBeforeEachStep"/>). A fresh Ceres game must execute complete gameplay frames in
    /// this state before the Ceres elevator state owns the elevator arrival; otherwise the level-data pad
    /// is briefly visible before its concealing enemy projectile is drawn.
    /// </summary>
    internal void BeginGameplayFadeIn(GameplayFadeTarget target)
    {
        GameplayFadeIn.Restart(0, GameplayFadeDefinitions.FadeInWaitBeforeEachStep);
        GameplayFadeLeadsToCeresArrival = target == GameplayFadeTarget.CeresArrival;
        EnterGameplayFadeIn();
    }
    /// <summary>
    /// The end of the fade-in selects the next state, which then enters main gameplay; the Ceres arrival is
    /// ordinary gameplay with Samus locked. The fade-in wrote full brightness this frame; the level is published before it
    /// rises, so the brightness is recorded directly.
    /// </summary>
    internal void FinishGameplayFadeIn()
    {
        EnterMainGameplay();
        GameplayFadeLeadsToCeresArrival = false;
        screenFade.StepEvery(GameplayFadeDefinitions.MainGameplayUpdatesPerStep);
        RecordFullBrightness();
    }
    /// <summary>
    /// Writes the save an update's closing message box completed at a save station or the gunship (#2514) to the selected
    /// slot, after the update, as the session owns the slot.
    /// </summary>
    /// <param name="outcome">The update's outcome, which carries the save it completed, if any.</param>
    /// <param name="outcome">What the update completed.</param>
    /// <param name="saved">The runtime whose update completed the save.</param>
    internal void WriteCompletedSave(RuntimeFrameOutcome outcome, SuperMetroidRuntime saved)
    {
        Ensure.NotNull(saved);
        if (outcome.CompletedSave is not { } save)
            return;
        saves.SaveSlot(SelectedSaveSlot, saved.SaveSnapshotAt(save.Station));
        RaiseSavesChanged();
    }
    /// <summary>Persists the automatic Landing Site checkpoint after the gunship lands.</summary>
    /// <param name="live">The gameplay whose update may have completed the gunship's landing.</param>
    internal void HandleGunshipLandingSave(LoadedGameplay live)
    {
        Ensure.NotNull(live);
        if (!AutomaticCheckpointSaver.TrySaveGunshipLanding(saves, live.Runtime, live.Room, SelectedSaveSlot))
            return;
        RaiseSavesChanged();
    }
    /// <summary>Where the attract demos are.</summary>
    internal AttractDemoSchedule Demo { get; } = new();
    /// <summary>The demo sets the title cycles through, fixed at boot: the completion set plays only after a valid completed-game save exists.</summary>
    internal IReadOnlyList<AttractDemoSet> DemoPlaylist { get; }
    /// <summary>The waiting updates the current game load still owes.</summary>
    internal GameLoadingWait GameLoading { get; } = new();
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
        runtime?.LeaveCameraFractions();
        runtime = null;
    }

    /// <summary>
    /// Applies the inventory that remains live across a Spacetime-triggered intro. The
    /// Mother Brain flashback temporarily replaces both missile counts and
    /// the intro clears them afterward; nothing touches Super Missiles or Power
    /// Bombs. The frontend cinematic uses isolated actor state, so its gameplay consequence
    /// is made explicit at the shared Ceres handoff.
    /// </summary>
    private static void RestoreSpacetimeRestartInventory(
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
