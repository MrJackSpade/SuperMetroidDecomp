using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The game: each update runs the phase of the current <see cref="SuperMetroidGameState"/> and the audio around it.
/// </summary>
/// <remarks>
/// Every state owns its own logic and display, and a transition always names the state it enters, so an
/// unexpected state stops the game with its name instead of continuing somewhere arbitrary.
/// </remarks>
public sealed class SuperMetroidGame
{
    /// <summary>The owner of FrontendSession's moved members.</summary>
    internal FrontendSession Session { get; }
    /// <summary>The owner of MenuGamePhases's moved members.</summary>
    private MenuGamePhases MenuPhases => field ??= new MenuGamePhases(Session);

    /// <summary>The owner of LoadingGamePhases's moved members.</summary>
    private LoadingGamePhases LoadingPhases => field ??= new LoadingGamePhases(Session);

    /// <summary>The owner of GameplayGamePhases's moved members.</summary>
    private GameplayGamePhases GameplayPhases => field ??= new GameplayGamePhases(Session);

    /// <summary>The owner of PauseGamePhases's moved members.</summary>
    private PauseGamePhases PausePhases => field ??= new PauseGamePhases(Session);

    /// <summary>The owner of DeathGamePhases's moved members.</summary>
    private DeathGamePhases DeathPhases => field ??= new DeathGamePhases(Session);

    /// <summary>The owner of DoorGamePhases's moved members.</summary>
    private DoorGamePhases DoorPhases => field ??= new DoorGamePhases(Session);

    /// <summary>The owner of CinematicGamePhases's moved members.</summary>
    private CinematicGamePhases CinematicPhases => field ??= new CinematicGamePhases(Session);

    /// <summary>The owner of AttractGamePhases's moved members.</summary>
    private AttractGamePhases AttractPhases => field ??= new AttractGamePhases(Session);

    /// <summary>
    /// Binds installed artwork whenever a host binds the game to its content (<see cref="GameSessionComposer.Bind"/>). Each
    /// catalog whose reference changed reaches the scenes that hold it; unchanged catalogs are left alone. The live runtime
    /// binds through <see cref="SuperMetroidRuntime.BindArtwork"/>.
    /// </summary>
    internal void BindArtwork(GameArtwork artwork)
    {
        Ensure.NotNull(artwork);
        GameArtwork previous = Session.Artwork;
        Session.Artwork = artwork;
        Session.Runtime?.BindArtwork(artwork, previous);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
            Session.saves.BindMapPresentation(artwork.MapPresentation);
        Session.Screen?.BindArtwork(previous, artwork, Session.GameState);
    }


    /// <summary>Creates a reset-state session, reading the selected save slot and capturing the boot carry before any dispatcher frame runs.</summary>
    /// <param name="saves">The saves the session owns; the host loads them and persists them on <see cref="SavesChanged"/>.</param>
    /// <param name="assists">The assists the session plays under, or <see langword="null"/> for the unassisted game.</param>
    public SuperMetroidGame(
        SaveSlotStore saves,
        GameplayAssists? assists = null)
        : this(saves, assists, SuperMetroidGameHostServices.Default, new SharedRandom())
    {
    }

    /// <summary>
    /// Creates the dispatcher with host-supplied timing and display policies, which verification hosts use to
    /// replay recorded evidence or to retain the display while they inspect game state.
    /// </summary>
    /// <remarks>
    /// Frontend screens are always rendered because their state machines own their own compositors; only
    /// gameplay updates publish through <see cref="SuperMetroidGameHostServices.GameplayFrames"/>.
    /// </remarks>
    /// <param name="saves">The saves the session owns.</param>
    /// <param name="assists">The assists the session plays under, or <see langword="null"/> for the unassisted game.</param>
    /// <param name="hostServices">The compatibility services and display policy; replay verification supplies recorded evidence here.</param>
    /// <param name="random">The session's random-number word, which the frontend and every gameplay runtime share.</param>
    internal SuperMetroidGame(
        SaveSlotStore saves,
        GameplayAssists? assists,
        SuperMetroidGameHostServices hostServices,
        SharedRandom random)
    {
        Session = new FrontendSession(saves, assists, hostServices, random);
    }

    /// <summary>The session's saves, which the host persists after <see cref="SavesChanged"/>.</summary>
    public SaveSlotStore Saves => Session.saves;

    /// <summary>
    /// Raised after the game changes its saves. The game owns them as <see cref="Saves"/>; the host chooses where they
    /// are kept.
    /// </summary>
    public event Action? SavesChanged
    {
        add => Session.SavesChanged += value;
        remove => Session.SavesChanged -= value;
    }

    /// <summary>
    /// Binds the extracted audio catalog whenever a host binds the game to its content, so
    /// every following update also synthesizes host PCM (<see cref="CopyAudioFrame"/>).
    /// </summary>
    /// <remarks>
    /// Binding affects presentation only. Each update advances the session's sound acceptance clock and
    /// its sound-effect acknowledgements whether or not audio is bound, so gameplay is identical
    /// with audio enabled, disabled, or without an output device.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="clips"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session already advanced without synthesis and has no player to continue.</exception>
    internal void BindAudio(ClipAudioLibrary clips) => Session.audioSession.BindClips(clips);

    /// <summary>Copies the last update's 1,600 interleaved 48-kHz stereo samples into host-owned storage.</summary>
    /// <exception cref="InvalidOperationException">No audio catalog is bound.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> holds fewer than 1,600 samples.</exception>
    public void CopyAudioFrame(Span<short> destination) => Session.audioSession.CopyFrame(destination);


    /// <summary>The phase the game is in.</summary>
    public SuperMetroidGameState GameState { get => Session.GameState; internal set => Session.GameState = value; }

    /// <summary>The wrapping count of updates the game has run, 0 through 65535.</summary>
    public int FrameNumber
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The active room, exposed for door-transition watches, or null before gameplay.</summary>
    public RoomHeaderDefinition? GameplayActiveRoom => Session.Runtime?.Room.ActiveRoom?.Definition;

    /// <summary>The active room's selected state, exposed beside the room identity, or null before gameplay.</summary>
    public RoomStateSettings? GameplayActiveRoomState => Session.Runtime?.Room.ActiveRoom?.State;

    /// <summary>Validated logical area/room pair, or null before gameplay.</summary>
    public RoomIdentity? GameplayActiveRoomIdentity => Session.Runtime?.Room.ActiveRoom?.Identity;

    /// <summary>The door the active room was entered through, exposed for door-transition watches.</summary>
    public DoorDefinition? GameplayActiveDoor => Session.Runtime?.Room.ActiveDoor;


    /// <summary>Runs one update and returns its status.</summary>
    public FrontendFrame Step(ControllerButton controllerInput)
    {
        // Derive whether the door transition has disabled sound effects before any producer
        // can queue audio, from the phase itself rather than a second serialized flag that
        // could disagree with it.
        GamePhaseAudio phaseAudio = Phases.Of(Session.GameState).Audio;
        Session.audio.DoorTransitionSoundsDisabled = phaseAudio == GamePhaseAudio.DoorTransitionSoundsDisabled;
        bool pauseInputEpilogue = phaseAudio == GamePhaseAudio.PauseInputEpilogue;
        bool messageWasActive = Session.Runtime?.MessageBox.IsActive == true;
        bool doorAudioDispatch = phaseAudio is GamePhaseAudio.DoorTransition or GamePhaseAudio.DoorTransitionSoundsDisabled;
        DoorTransitionPhase startingDoorPhase = Session.doorTransition.Phase;
        // An update that continues a phase's step waiting out a frame of the display runs
        // neither the update's music processing nor its sound processing.
        bool resumesFrameWait = NextUpdateResumesFrameWait;
        // Music processing runs before any phase, so a phase that waits on HasQueuedMusic
        // sees this update's queue; sound processing runs after the phase returns. A
        // message box's updates run only the box's own audio processing (below).
        bool prologueMusicDispatch = !resumesFrameWait && !messageWasActive &&
            (!doorAudioDispatch || phaseAudio != GamePhaseAudio.DoorTransitionSoundsDisabled ||
             startingDoorPhase is not (DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading));
        IReadOnlyList<AudioRequest> prologueMusicRequests = prologueMusicDispatch
            ? Session.audio.AdvanceMusicDispatch() : Array.Empty<AudioRequest>();
        // Frames pass on the frame counters while a door loads a new music data set
        // (IDoorMusicLoadFrames); nothing between this update's frame boundary and the load reads them.
        if (doorAudioDispatch && prologueMusicRequests.Any(request => request is AudioRequest.ChangeMusicDataSet))
            Session.GameplayRuntime.Clock.CountDoorMusicLoad(Session.HostServices.DoorMusicLoadFrames);
        var gameplayAudio = new GameplayAudioFramePublication(Session.audio);
        FrameNumber = WrappingCounter.IncrementWord(FrameNumber);
        Session.display.BeginUpdate(FrameNumber);
        if (Session.Runtime is null)
            Session.frameClock.AdvanceAcceptedFrame();
        AdvanceMenuRandom();
        if (Session.Runtime is not null)
            Session.Runtime.DispatchGameState = Session.GameState;
        Phases.Of(Session.GameState).Step(controllerInput, gameplayAudio);

        Session.CollectTranslatedAudioRequests(gameplayAudio);
        // Opening a message box cancels sound effects before the box's first update.
        // Publish earlier gameplay requests first, then cancel once at this entry
        // boundary; repeating it while the box is shown would cancel the message's
        // own cursor and saving sounds. A loaded active message is not an entry and
        // must not inject a new command merely because the host resumed.
        if (!messageWasActive && Session.Runtime?.MessageBox.IsActive == true)
            Session.audio.QueueCancelSoundEffects();
        bool musicHandler = false;
        int soundHandlers = !resumesFrameWait && (!doorAudioDispatch ||
            startingDoorPhase is not (DoorTransitionPhase.LoadMoreThingsAndOpenDoor or DoorTransitionPhase.WaitForDoorOpeningScroll))
            ? 1 : 0;
        if (messageWasActive)
        {
            // A message-box update runs exactly the box's own audio processing.
            MessageBoxFrameAudio boxAudio = Session.GameplayRuntime.MessageBox.LastFrameAudio;
            musicHandler = boxAudio.MusicHandlerCalls != 0;
            soundHandlers = boxAudio.SoundHandlerCalls;
        }
        else if (Session.Runtime?.MessageBox.IsActive == true)
        {
            // The opening update's sound processing runs only once the box closes.
            soundHandlers = 0;
        }
        IReadOnlyList<AudioRequest> trailingAudioRequests = Session.audio.AdvanceFrame(Session.HostServices.SoundAcceptance.ForNextUpdate(Session.audioSession.Acknowledgements, Session.audio.SentSoundRequests),
            advanceMusicQueue: musicHandler,
            soundEffectHandlerCalls: soundHandlers);
        IReadOnlyList<AudioRequest> audioRequests = prologueMusicRequests.Count == 0 ? trailingAudioRequests
            : [.. prologueMusicRequests, .. trailingAudioRequests];
        // The sound acceptance clock acts on this update's requests during the update's own
        // samples; its acknowledgements become visible to the next update's sound handlers.
        Session.AdvanceAudio(audioRequests);
        // The pause phases publish held input after they return, even though Samus's
        // draw-time epilogue did not run. Preserve its held-only publication;
        // updating the press edge or auto-jump timer here would invent gameplay.
        if (pauseInputEpilogue)
            Session.GameplayRuntime.Player.IntroducedSamus.InputSnapshot.PublishMainLoopHeldInput(Session.GameplayRuntime.Controller1.Current);
        return CurrentFrame;
    }

    /// <summary>One phase per game state; each runs that state's update.</summary>
    private GamePhaseTable Phases => field ??= new GamePhaseTable(
        [
            new GamePhase(SuperMetroidGameState.Reset,
                DispatchRandomStep.Reseed,
                step: (_, _) => MenuPhases.DispatchReset(),
                name: () => "Reset"),
            new GamePhase(SuperMetroidGameState.OpeningCinematic,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => MenuPhases.DispatchOpeningCinematic(controllerInput),
                name: () => Session.ActiveTitle.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.GameOptionsMenu,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => MenuPhases.DispatchGameOptionsMenu(controllerInput),
                name: () => Session.ActiveOptions.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.FileSelectMenus,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchFileSelectMenus(controllerInput),
                name: () => Session.ActiveFileSelect.Phase.ToString(),
                resumesFrameWait: () => Session.ScreenAs<FileSelectMenuState>()?.ResumesAfterFrameWait == true),
            // The map's final update latched its load request, blanked the display and waits
            // out one frame; the next update continues that step.
            new GamePhase(SuperMetroidGameState.FileSelectMap,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchFileSelectMap(controllerInput),
                name: () => Session.ScreenAs<FileSelectMapMenuState>()?.Phase.ToString() ?? "Preparing saved-game map",
                resumesFrameWait: () => Session.ScreenAs<FileSelectMapMenuState>()?.LoadRequested == true),
            new GamePhase(SuperMetroidGameState.LoadingGameData,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => LoadingPhases.DispatchLoadingGameData(controllerInput),
                name: () => $"Loading Landing Site ({Session.GameLoading.WaitsRemaining} updates)",
                resumesFrameWait: () => Session.ResumesGameLoadingWait),
            new GamePhase(SuperMetroidGameState.MainGameplayFadeIn,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => LoadingPhases.DispatchMainGameplayFadeIn(controllerInput, gameplayAudio),
                name: () => Session.GameplayFadeLeadsToCeresArrival ? $"Ceres elevator fade-in (brightness {Session.GameplayFadeIn.Brightness})" : $"Landing Site fade-in (brightness {Session.GameplayFadeIn.Brightness})"),
            new GamePhase(SuperMetroidGameState.MainGameplay,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchMainGameplay(controllerInput, gameplayAudio),
                name: () => "Gameplay"),
            new GamePhase(SuperMetroidGameState.HitDoorBlock,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchDoorCollisionAndLoading(controllerInput),
                name: () => "Door collision",
                audio: GamePhaseAudio.DoorTransition),
            new GamePhase(SuperMetroidGameState.LoadingNextRoomA,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchDoorCollisionAndLoading(controllerInput),
                name: () => "Loading destination room",
                audio: GamePhaseAudio.DoorTransition),
            new GamePhase(SuperMetroidGameState.LoadingNextRoomB,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchLoadingNextRoomB(controllerInput),
                name: () => "Destination room ready",
                audio: GamePhaseAudio.DoorTransitionSoundsDisabled),
            new GamePhase(SuperMetroidGameState.PausingDarkening,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchPausingDarkening(controllerInput, gameplayAudio),
                name: () => $"Pausing: gameplay darken ({Session.PauseBrightness})"),
            new GamePhase(SuperMetroidGameState.Pausing,
                DispatchRandomStep.NonGameplayLineEffectsThenAdvance,
                step: (controllerInput, _) => PausePhases.DispatchPausing(controllerInput),
                name: () => "Pausing: load pause assets",
                audio: GamePhaseAudio.PauseInputEpilogue),
            // The random generator advances once before each pause-only phase as well. The
            // pause fades run gameplay, which advances it itself.
            new GamePhase(SuperMetroidGameState.PausedA,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => PausePhases.DispatchPausedA(controllerInput),
                name: () => $"Paused: fade in ({Session.PauseBrightness})",
                audio: GamePhaseAudio.PauseInputEpilogue),
            new GamePhase(SuperMetroidGameState.PausedB,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => PausePhases.DispatchPausedB(controllerInput),
                name: () => Session.ScreenAs<PauseMenuState>()?.ScreenMode == PauseScreenMode.Equipment ? "Paused: equipment" : "Paused: map",
                audio: GamePhaseAudio.PauseInputEpilogue),
            new GamePhase(SuperMetroidGameState.UnpausingA,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => PausePhases.DispatchUnpausingA(controllerInput),
                name: () => $"Unpausing: fade out ({Session.PauseBrightness})",
                audio: GamePhaseAudio.PauseInputEpilogue),
            new GamePhase(SuperMetroidGameState.UnpausingB,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => PausePhases.DispatchUnpausingB(controllerInput),
                name: () => "Unpausing: restore gameplay",
                audio: GamePhaseAudio.PauseInputEpilogue),
            new GamePhase(SuperMetroidGameState.Unpausing,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchUnpausing(controllerInput, gameplayAudio),
                name: () => $"Unpausing: gameplay brighten ({Session.PauseBrightness})"),
            new GamePhase(SuperMetroidGameState.DeathSequenceStart,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => DeathPhases.DispatchDeathSequenceStart(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.DeathSequenceStart)),
            new GamePhase(SuperMetroidGameState.DeathBlackOutSurroundings,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => DeathPhases.DispatchDeathBlackOutSurroundings(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.DeathBlackOutSurroundings)),
            new GamePhase(SuperMetroidGameState.DeathWaitForMusic,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathWaitForMusic(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathWaitForMusic)),
            new GamePhase(SuperMetroidGameState.DeathPreFlashing,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathPreFlashing)),
            new GamePhase(SuperMetroidGameState.DeathFlashing,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathFlashing)),
            new GamePhase(SuperMetroidGameState.DeathExplosionWhiteOut,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathExplosionWhiteOut)),
            // The final black-out only fades; the random generator advances before it as before any phase.
            new GamePhase(SuperMetroidGameState.DeathFinalBlackOut,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFinalBlackOut(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathFinalBlackOut)),
            // The game-over menu's first two steps each wait out a frame of the display.
            new GamePhase(SuperMetroidGameState.GameOverMenu,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchGameOverMenu(controllerInput),
                name: () => nameof(SuperMetroidGameState.GameOverMenu),
                resumesFrameWait: () => Session.ScreenAs<GameOverMenuState>()?.ResumesAfterFrameWait == true),
            new GamePhase(SuperMetroidGameState.ReserveTanksAuto,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchReserveTanksAuto(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.ReserveTanksAuto)),
            new GamePhase(SuperMetroidGameState.IntroCinematic,
                DispatchRandomStep.Advance,
                step: (controllerInput, _) => CinematicPhases.DispatchIntroCinematic(controllerInput),
                name: () => Session.ActiveIntro.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.SetUpNewGame,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => LoadingPhases.DispatchSetUpNewGame(controllerInput),
                name: () => Session.LoadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
                resumesFrameWait: () => Session.ResumesGameLoadingWait),
            new GamePhase(SuperMetroidGameState.MadeItToCeresElevator,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchMadeItToCeresElevator(controllerInput, gameplayAudio),
                name: () => Session.ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator ? $"Ceres elevator departure ({Session.ceresDeparture.HoldFramesRemaining})" : Session.Runtime?.Room.CeresElevatorArrival is { IsComplete: false } ? "Ceres elevator arrival" : "Ceres controls unlocked"),
            new GamePhase(SuperMetroidGameState.BlackoutFromCeres,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchBlackoutFromCeres(controllerInput, gameplayAudio),
                name: () => $"Ceres blackout (brightness {Session.ceresDeparture.Brightness})"),
            // The Ceres destruction cinematics share the intro's cinematic phase and its random advance.
            new GamePhase(SuperMetroidGameState.CeresGoesBoom,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => CinematicPhases.DispatchCeresGoesBoom(controllerInput),
                name: () => $"Ceres destruction: {Session.ScreenAs<CeresDestructionCinematicState>()?.Phase}",
                resumesFrameWait: () => Session.ScreenAs<CeresDestructionCinematicState>()?.ResumesAfterFrameWait == true),
            new GamePhase(SuperMetroidGameState.TimeUp,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchTimeUp(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TimeUp)),
            // The time-up black-out fades without gameplay; its per-line effects still run until it clears them.
            new GamePhase(SuperMetroidGameState.WhitingOutFromTimeUp,
                DispatchRandomStep.NonGameplayLineEffectsThenAdvance,
                step: (controllerInput, _) => CinematicPhases.DispatchWhitingOutFromTimeUp(controllerInput),
                name: () => nameof(SuperMetroidGameState.WhitingOutFromTimeUp)),
            new GamePhase(SuperMetroidGameState.CeresGoesBoomWithSamus,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => CinematicPhases.DispatchCeresGoesBoomWithSamus(controllerInput),
                name: () => nameof(SuperMetroidGameState.CeresGoesBoomWithSamus),
                resumesFrameWait: () => Session.ScreenAs<CeresDestructionCinematicState>()?.ResumesAfterFrameWait == true),
            new GamePhase(SuperMetroidGameState.SamusEscapesFromZebes,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchSamusEscapesFromZebes(controllerInput, gameplayAudio),
                name: () => $"Zebes escape fade-out ({Session.EscapeFade.Brightness})"),
            // The ending runs after the random advance, except while its setup waits out
            // frames of the display.
            new GamePhase(SuperMetroidGameState.EndingAndCredits,
                DispatchRandomStep.AdvanceUnlessResumingWait,
                step: (_, _) => CinematicPhases.DispatchEndingAndCredits(),
                name: () => $"Ending: {Session.ScreenAs<EndingCreditsState>()?.Stage.Phase}",
                resumesFrameWait: () => Session.ScreenAs<EndingCreditsState>()?.ResumesAfterFrameWait == true),
            new GamePhase(SuperMetroidGameState.TransitionToDemoA,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionToDemoA)),
            new GamePhase(SuperMetroidGameState.TransitionToDemoB,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionToDemoB)),
            new GamePhase(SuperMetroidGameState.PlayingDemo,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.PlayingDemo)),
            new GamePhase(SuperMetroidGameState.TransitionFromDemoA,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionFromDemoA)),
            new GamePhase(SuperMetroidGameState.TransitionFromDemoB,
                DispatchRandomStep.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionFromDemoB)),
        ]);

    /// <summary>Reads the current status without advancing the dispatcher.</summary>
    public FrontendFrame CurrentFrame => new(Session.GameState, PhaseName, FrameNumber);

    /// <summary>Human-readable active subphase for diagnostics, without changing state.</summary>
    private string PhaseName => Phases.Of(Session.GameState).Name;

    /// <summary>
    /// The display the game last published, stamped with the current frame number, without advancing the game, its
    /// audio or any scene. A host shows it when it starts presenting a game that is not running, such as one it has
    /// just restored or one it has paused.
    /// </summary>
    /// <exception cref="InvalidOperationException">An update is being captured.</exception>
    public DisplayPacket GetRetainedDisplay() => Session.display.Retained();

    /// <summary>
    /// Runs one update and returns its status with the display it published, stamped with the update's frame number.
    /// </summary>
    public CapturedFrontendFrame StepCaptured(ControllerButton controllerInput)
    {
        Session.display.BeginCapture();
        try
        {
            FrontendFrame frame = Step(controllerInput);
            return new(frame, Session.display.CompleteCapture());
        }
        finally { Session.display.EndCapture(); }
    }

    /// <summary>
    /// Copies where the game is and what Samus is doing, without running the game. A host takes one before an update
    /// and another when the update fails, so its report shows how far the failed update got.
    /// </summary>
    public GameplaySnapshot CaptureGameplaySnapshot() => new()
    {
        FrameNumber = FrameNumber,
        GameState = Session.GameState,
        Room = GameplayActiveRoom?.Name,
        RoomState = GameplayActiveRoomState?.Name,
        Door = GameplayActiveDoor?.Name,
        Samus = Session.Runtime is { Player.Samus: { } samus } runtime
            ? new SamusSnapshot
            {
                Pose = samus.Pose,
                AnimationFrame = samus.AnimationFrame,
                AnimationFramesRemaining = samus.AnimationFrameTimer.RemainingFrames,
                AnimatedPose = samus.AnimatedPose,
                Position = samus.Position.Whole,
                HeldButtons = runtime.Controller1.Current,
                NewlyPressedButtons = runtime.Controller1.NewlyPressed,
                InputLocked = samus.Control.AlphaLocked,
                DrainedPhase = samus.Drained.Phase,
                DrainedGetUp = samus.Drained.GetUpHandler,
                EscapeTimer = runtime.EscapeTimer.Phase,
                Fx = samus.LiquidPhysics.FxType,
                Medium = samus.LiquidPhysics.LiquidPhysicsType,
                WaterSurfaceY = samus.LiquidPhysics.FxYPosition,
                LavaAcidSurfaceY = samus.LiquidPhysics.LavaAcidYPosition,
            }
            : null,
    };

    /// <summary>
    /// Changes the assists the session plays under, as when a stored game is restored under other assists. Game state, resource
    /// totals and the retained frame stay intact unless a granted loadout is newly enabled. An exact replay keeps the
    /// assists it was recorded with.
    /// </summary>
    /// <param name="assists">The assists to play under from now on.</param>
    public void ApplyAssists(GameplayAssists assists)
    {
        Ensure.NotNull(assists);
        Session.Runtime?.ApplyAssists(assists);
        Session.Assists = assists;
    }

    /// <summary>
    /// True when the next update continues a phase's step that waits out a frame of the display, instead of
    /// starting a new update of the phase. Such an update makes no per-update random advance.
    /// </summary>
    internal bool NextUpdateResumesFrameWait => Phases.Of(Session.GameState).ResumesFrameWait;

    /// <summary>
    /// Advances the random generator once per update through the title, file selection and
    /// loading. These phases run once per update and do not run gameplay, which advances it
    /// itself. Saves keep no copy of the generator.
    /// </summary>
    internal void AdvanceMenuRandom()
    {
        switch (Phases.Of(Session.GameState).RandomStep)
        {
            case DispatchRandomStep.Reseed:
                // Only power-on and soft reset reseed. A continued save must keep the value
                // produced by gameplay and the game-over and file-map waiting updates.
                Session.Random.SeedPowerOn();
                Session.frameClock.Reset();
                Session.menuCamera.Reset();
                // The reset phase itself runs after the first advance; the seed alone is
                // not what it sees.
                Session.Random.Generate();
                break;
            case DispatchRandomStep.NonGameplayLineEffectsThenAdvance:
                // The per-line effects still run on the update before the pause screen; lava swaps the live random bytes.
                Session.GameplayRuntime.DoorTransition.AdvanceEffectsAndRandom(Session.GameplayRuntime.Room.LoadedScope.RoomObjects, Session.GameplayRuntime.Room.LoadedScope);
                break;
            case DispatchRandomStep.AdvanceUnlessResumingWait:
                if (!NextUpdateResumesFrameWait)
                    Session.Random.Generate();
                break;
            case DispatchRandomStep.Advance:
                Session.Random.Generate();
                break;
            case DispatchRandomStep.RuntimeOwned:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DispatchRandomStep)} {Phases.Of(Session.GameState).RandomStep}.");
        }
        // Gameplay advances the generator itself after its per-line effects. Do not
        // advance here for gameplay updates or for an update that continues a waiting step.
    }
}
