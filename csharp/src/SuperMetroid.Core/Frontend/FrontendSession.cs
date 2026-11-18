using SuperMetroid.Core.Input;
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
    internal FrontendSession(SaveSlotStore saves, GameplayAssists? assists,
        SuperMetroidGameHostServices hostServices, SharedRandom random)
    {
        HostServices = Ensure.NotNull(hostServices);
        Random = Ensure.NotNull(random);
        Assists = assists ?? GameplayAssists.None;
        display = new FrontendDisplay(hostServices.GameplayFrames);
        // One set of request cycles for the session's whole lifetime, like the clock: each library's cycle keeps its
        // phase across menus and gameplay runtimes, and a soft reset returns it to ready with the queues. Every sound request
        // is tested against the live runtime's demo and power bomb as it is made.
        audio = new AudioQueues(new SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder),
            new SessionGameplaySoundState(this));
        audioSession = new AudioSession(soundClock);
        this.saves = Ensure.NotNull(saves);
        SelectedSaveSlot = saves.ReadSelectedSlot();
        // Construction is power-on: boot evaluates the saves once before entering the main game loop.
        earthquakeSoundDelay = saves.EarthquakeSoundDelayAtPowerOn();
        DemoPlaylist = StockAttractDemoScenes.Playlist(saves.DetermineCompletionDemoSetUnlocked());
    }

    /// <summary>
    /// The gameplay screen's fade, shared by the pause and unpause fades and the transitions between rendered phases: its
    /// level is the brightness gameplay was last shown at, which a pause fades from. Every return to main gameplay leaves its
    /// cadence stepping on every update, which a map-station pause then inherits; power-on waits one update first.
    /// </summary>
    internal readonly ScreenFade screenFade = new(DisplayBrightness.Full, stepDelay: 0, firstWait: PauseFadeTiming.CounterReload);
    /// <summary>
    /// The camera fractions the game keeps from one game or demo to the next: each runtime's first room's camera starts from
    /// them, and each released runtime leaves its camera's; the soft reset clears them.
    /// </summary>
    internal readonly CameraFractionsKeptAcrossGames cameraFractions = new();

    /// <summary>The session's frame counters: every runtime it creates counts on them.</summary>
    internal readonly FrameClock frameClock = new();
    /// <summary>
    /// The installed catalogs the host bound (#2224), which every screen and gameplay runtime draws from: a host binds them
    /// before the session's first update (<see cref="GameSessionComposer"/>), and a rebinding replaces them with other
    /// installed catalogs, never with none.
    /// </summary>
    /// <exception cref="InvalidOperationException">Read before a host first bound the session's installed artwork.</exception>
    public GameArtwork Artwork
    {
        get => boundArtwork ?? throw new InvalidOperationException("The session's installed artwork has not been bound.");
        internal set => boundArtwork = Ensure.NotNull(value);
    }

    /// <summary>Whether a host has bound the session's installed artwork: from its first binding on.</summary>
    internal bool ArtworkBound => boundArtwork is not null;

    /// <summary>The installed catalogs the host bound; none only between the session's construction and its first binding.</summary>
    private GameArtwork? boundArtwork;
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
    internal SuperMetroidRuntime? Runtime => runtime;
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
    internal ScreenFade GameplayFadeIn { get; } = new(0, stepDelay: 1, firstWait: 1);
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
    internal RoomStateSettings? LastAudioRoomState { get; set; }
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
    /// <summary>The sequence the game is running.</summary>
    public SuperMetroidGameState GameState { get; private set; } = SuperMetroidGameState.Title;
    /// <summary>The title sequence's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.Title"/>.</summary>
    internal TitleStep TitleStep { get; private set; }
    /// <summary>The screen of choosing a game; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.FileSelect"/>.</summary>
    internal FileSelectStep FileSelectStep { get; private set; }
    /// <summary>What the game load loads; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.LoadingGame"/>.</summary>
    internal GameDataLoadRoute LoadingRoute { get; private set; }
    /// <summary>The door transition's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.DoorTransition"/>.</summary>
    internal DoorTransitionStep DoorTransitionStep { get; private set; }
    /// <summary>The pause's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.Pause"/>.</summary>
    internal PauseStep PauseStep { get; private set; }
    /// <summary>The death sequence's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.DeathSequence"/>.</summary>
    internal DeathSequenceStep DeathStep { get; private set; }
    /// <summary>How Ceres's destruction ends; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.CeresDestruction"/>.</summary>
    internal CeresDestructionOutcome CeresDestructionOutcome { get; private set; }
    /// <summary>The time-up's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.TimeUp"/>.</summary>
    internal TimeUpStep TimeUpStep { get; private set; }
    /// <summary>The ending's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.Ending"/>.</summary>
    internal EndingStep EndingStep { get; private set; }
    /// <summary>The attract demo's step; meaningful while <see cref="GameState"/> is <see cref="SuperMetroidGameState.AttractDemo"/>.</summary>
    internal AttractDemoStep AttractDemoStep { get; private set; }

    /// <summary>
    /// Enters <paramref name="sequence"/> at its first step: the title starts, choosing a game shows the save slots, the
    /// game load loads the chosen game, a door is entered, the pause darkens gameplay, the death sequence makes its final
    /// gameplay pass, Ceres is destroyed after Samus escaped, the time-up whitens, the ending leaves Zebes and an attract
    /// demo loads.
    /// </summary>
    internal void Enter(SuperMetroidGameState sequence)
    {
        GameState = sequence;
        TitleStep = TitleStep.Starting;
        FileSelectStep = FileSelectStep.SaveSlots;
        LoadingRoute = GameDataLoadRoute.SelectedGame;
        DoorTransitionStep = DoorTransitionStep.EnteringDoor;
        PauseStep = PauseStep.DarkeningGameplay;
        DeathStep = DeathSequenceStep.FinalGameplayPass;
        CeresDestructionOutcome = CeresDestructionOutcome.SamusEscaped;
        TimeUpStep = TimeUpStep.WhiteningOut;
        EndingStep = EndingStep.LeavingZebes;
        AttractDemoStep = AttractDemoStep.Loading;
    }

    /// <summary>Enters the title sequence at <paramref name="step"/>.</summary>
    internal void Enter(TitleStep step)
    {
        Enter(SuperMetroidGameState.Title);
        TitleStep = step;
    }

    /// <summary>Enters choosing a game at <paramref name="step"/>.</summary>
    internal void Enter(FileSelectStep step)
    {
        Enter(SuperMetroidGameState.FileSelect);
        FileSelectStep = step;
    }

    /// <summary>Enters the game load of <paramref name="route"/>.</summary>
    internal void Enter(GameDataLoadRoute route)
    {
        Enter(SuperMetroidGameState.LoadingGame);
        LoadingRoute = route;
    }

    /// <summary>Enters the door transition at <paramref name="step"/>.</summary>
    internal void Enter(DoorTransitionStep step)
    {
        Enter(SuperMetroidGameState.DoorTransition);
        DoorTransitionStep = step;
    }

    /// <summary>Enters the pause at <paramref name="step"/>.</summary>
    internal void Enter(PauseStep step)
    {
        Enter(SuperMetroidGameState.Pause);
        PauseStep = step;
    }

    /// <summary>Enters the death sequence at <paramref name="step"/>.</summary>
    internal void Enter(DeathSequenceStep step)
    {
        Enter(SuperMetroidGameState.DeathSequence);
        DeathStep = step;
    }

    /// <summary>Enters Ceres's destruction with <paramref name="outcome"/>.</summary>
    internal void Enter(CeresDestructionOutcome outcome)
    {
        Enter(SuperMetroidGameState.CeresDestruction);
        CeresDestructionOutcome = outcome;
    }

    /// <summary>Enters the time-up at <paramref name="step"/>.</summary>
    internal void Enter(TimeUpStep step)
    {
        Enter(SuperMetroidGameState.TimeUp);
        TimeUpStep = step;
    }

    /// <summary>Enters the ending at <paramref name="step"/>.</summary>
    internal void Enter(EndingStep step)
    {
        Enter(SuperMetroidGameState.Ending);
        EndingStep = step;
    }

    /// <summary>Enters the attract demo at <paramref name="step"/>.</summary>
    internal void Enter(AttractDemoStep step)
    {
        Enter(SuperMetroidGameState.AttractDemo);
        AttractDemoStep = step;
    }
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
            if (GameState == SuperMetroidGameState.DoorTransition && DoorTransitionStep == DoorTransitionStep.Transitioning)
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

        // The message-box save choice runs outside gameplay. Its one-shot request is consumed
        // once, so updates that only show the box cannot repeat it.
        if (runtime?.MessageBoxes.ConsumeMessageBoxSelectionSoundRequest() == true)
            audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
        if (runtime?.MessageBox.ConsumeSavingSoundRequest() == true)
            audio.QueueSound(SamusSound.Saving, priority: SoundRequestPriority.Normal);
        if (runtime?.MessageBoxes.ConsumeGunshipExitSoundRequest() == true)
            audio.QueueSound(RoomSound.GunshipPadOpening, priority: SoundRequestPriority.Normal);
    }
    /// <summary>
    /// The game leaves Zebes once the gunship's top hull has risen past Y=256, starting the screen fade from full
    /// brightness. It does not freeze time: leaving Zebes keeps running full gameplay, so Samus and the RNG advance behind
    /// the fade. This update's gameplay has already run; the fade's first step is the next update's.
    /// </summary>
    internal void BeginZebesEscapeFade()
    {
        EscapeFade.Restart(DisplayBrightness.Full, 0);
        Enter(EndingStep.LeavingZebes);
    }
    /// <summary>
    /// The escape countdown expired. The whole gameplay display, Samus and the HUD among it, fades to white through the
    /// shared fade step count in the time-up's gradual change (#1512).
    /// </summary>
    internal void BeginTimeUp()
    {
        SuperMetroidRuntime live = runtime ?? throw new InvalidOperationException("Time-up requires a gameplay runtime.");
        live.FadeOut.Begin(RgbColor.White, TimeUpDefinitions.WhiteOutLastStep, GameplayFadeSpares.Nothing);
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
    /// <summary>Starts a pause fade from <paramref name="initialBrightness"/> with both fade counters reloaded.</summary>
    internal void BeginPauseFade(int initialBrightness)
    {
        screenFade.Restart(initialBrightness, PauseFadeTiming.CounterReload);
    }
    /// <summary>Allocates and binds the gameplay owner without selecting a room.</summary>
    /// <param name="purpose">Whether the runtime plays a selected game, which takes the assists and menu options, or an attract demo.</param>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(runtime))]
    internal void CreateGameplayRuntime(GameplayRuntimePurpose purpose)
    {
        runtime?.LeaveCameraFractions();
        bool attract = purpose == GameplayRuntimePurpose.AttractDemo;
        runtime = GameplayRuntimeFactory.Create(new GameplayRuntimeStart
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
            SelectedItem: HudItem.Beam,
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
                SelectedItem: HudItem.Beam,
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
            restartedSamus.SelectBeam();
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

            // A Ceres elevator save starts with the Ceres elevator's arrival, as a new game does; every other station
            // loads through the room loader below.
            if (slot.StartupRoute == SaveLoadingGameState.CeresElevatorArrival)
            {
                // The save's progress is restored before the room loads, so the room's state and its enemies see it.
                runtime.RoomLoader.RestoreSavedPlayerState(slot);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                slot.ApplyTo(runtime.Player.Samus ?? throw new InvalidOperationException(
                    "Ceres initialization did not create Samus."));
                runtime.Assists.GrantLoadoutToRestoredSamus();
                runtime.Player.Samus.SelectBeam();
                ApplySelectedGameOptions();
                return true;
            }

            runtime.RoomLoader.InitializeSavedGame(slot);
            // Loading the save restores the controls and options it holds; the choices the player has made in the
            // options menu since replace them, as the game keeps them live.
            ApplySelectedGameOptions();
            RoomStateSettings loadedState = runtime.Room.ActiveRoom?.State
                ?? throw new InvalidOperationException(
                    "Saved-game appearance started without an active room state.");
            // Samus's appearance starts her fanfare and then the room's track behind it
            // (SavedGameAppearanceMusicDefinitions). This room state is marked as already handled, so the room's
            // change of music does not replace the fanfare with the room's track at once.
            // Loading from file select still has the title's music data set loaded.
            // The appearance tracks still play in their order, but the room's data set loads first,
            // just as ordinary room loading does; a set's own songs belong to that data set.
            audio.QueueRoomMusicData(loadedState.Music.DataSet);
            audio.ScheduleMusic(MusicRequest.SelectTrack(MusicTrack.SamusFanfare), SavedGameAppearanceMusicDefinitions.FanfareDelay);
            audio.ScheduleMusic(
                loadedState.Music.Track.Request,
                SavedGameAppearanceMusicDefinitions.RoomTrackDelay);
            LastAudioRoomState = loadedState;
            return false;
        }

        // A new game first shows the standard HUD, then loads the Ceres elevator room and finally sets Samus up for the
        // new game.
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.PublishWhileLoading();
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The intro's end publishes Ceres and its elevator checkpoint
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
        GameplayFadeIn.Restart(0, 1);
        GameplayFadeLeadsToCeresArrival = target == GameplayFadeTarget.CeresArrival;
        Enter(SuperMetroidGameState.GameplayFadeIn);
    }
    /// <summary>
    /// The end of the fade-in selects the next state, which then enters main gameplay; the Ceres arrival is
    /// ordinary gameplay with Samus locked. The fade-in wrote full brightness this frame; the level is published before it
    /// rises, so the brightness is recorded directly.
    /// </summary>
    internal void FinishGameplayFadeIn()
    {
        Enter(SuperMetroidGameState.MainGameplay);
        GameplayFadeLeadsToCeresArrival = false;
        screenFade.StepEvery(1);
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
