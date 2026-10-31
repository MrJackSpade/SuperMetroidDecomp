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
    [field: NonSerialized]
    private MenuGamePhases MenuPhases => field ??= new MenuGamePhases(Session);

    /// <summary>The owner of LoadingGamePhases's moved members.</summary>
    [field: NonSerialized]
    private LoadingGamePhases LoadingPhases => field ??= new LoadingGamePhases(Session);

    /// <summary>The owner of GameplayGamePhases's moved members.</summary>
    [field: NonSerialized]
    private GameplayGamePhases GameplayPhases => field ??= new GameplayGamePhases(Session);

    /// <summary>The owner of PauseGamePhases's moved members.</summary>
    [field: NonSerialized]
    private PauseGamePhases PausePhases => field ??= new PauseGamePhases(Session);

    /// <summary>The owner of DeathGamePhases's moved members.</summary>
    [field: NonSerialized]
    private DeathGamePhases DeathPhases => field ??= new DeathGamePhases(Session);

    /// <summary>The owner of DoorGamePhases's moved members.</summary>
    [field: NonSerialized]
    private DoorGamePhases DoorPhases => field ??= new DoorGamePhases(Session);

    /// <summary>The owner of CinematicGamePhases's moved members.</summary>
    [field: NonSerialized]
    private CinematicGamePhases CinematicPhases => field ??= new CinematicGamePhases(Session);

    /// <summary>The owner of AttractGamePhases's moved members.</summary>
    [field: NonSerialized]
    private AttractGamePhases AttractPhases => field ??= new AttractGamePhases(Session);

    /// <summary>
    /// Binds installed artwork at startup and after debugger-state restoration (<see cref="GameSessionComposer"/>). Each
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


    /// <summary>Creates a reset-state session, reading the selected save slot and capturing the native boot carry before any dispatcher frame runs.</summary>
    /// <param name="saves">The battery-backed saves the session owns; the host loads them and persists them on <see cref="SaveRamChanged"/>.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    public SuperMetroidGame(
        SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions = null)
        : this(saves, gameOptions, SuperMetroidGameHostServices.Default, new SharedRandom())
    {
    }

    /// <summary>
    /// Creates the dispatcher with host-supplied timing and display policies, which verification hosts use to
    /// replay recorded evidence or to retain the display while they inspect cartridge state.
    /// </summary>
    /// <remarks>
    /// Frontend screens are always rendered because their state machines own their own compositors; only
    /// gameplay updates publish through <see cref="SuperMetroidGameHostServices.GameplayFrames"/>.
    /// </remarks>
    /// <param name="saves">The battery-backed saves the session owns.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    /// <param name="hostServices">The compatibility services and display policy; replay verification supplies recorded evidence here.</param>
    /// <param name="random">The session's random-number word, which the frontend and every gameplay runtime share.</param>
    internal SuperMetroidGame(
        SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions,
        SuperMetroidGameHostServices hostServices,
        SharedRandom random)
    {
        Session = new FrontendSession(saves, gameOptions, hostServices, random);
    }

    /// <summary>The session's battery-backed saves, which the host persists after <see cref="SaveRamChanged"/>.</summary>
    public SaveSlotStore Saves => Session.saves;

    /// <summary>
    /// Raised after translated code changes the battery-backed saves. The core owns them as
    /// <see cref="Saves"/>; a desktop/console host owns the choice of durable storage medium.
    /// </summary>
    public event Action? SaveRamChanged
    {
        add => Session.SaveRamChanged += value;
        remove => Session.SaveRamChanged -= value;
    }

    /// <summary>
    /// Binds the extracted audio catalog at startup and after debugger-state restoration, so
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

    /// <summary>Validated logical area/room pair, or null before cartridge gameplay.</summary>
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
        IReadOnlyList<AudioRequest> trailingAudioRequests = Session.audio.AdvanceFrame(Session.HostServices.SoundAcknowledgements.ForNextUpdate(Session.audioSession.Acknowledgements, Session.audio.SentSoundRequests),
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
    [field: NonSerialized]
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
                name: () => Session.ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator ? $"Ceres elevator departure ({Session.ceresDeparture.HoldFramesRemaining.RemainingFrames})" : Session.Runtime?.Room.CeresElevatorArrival is { IsComplete: false } ? "Ceres elevator arrival" : "Ceres controls unlocked"),
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
    /// Republishes the retained immutable display under a fresh host identity without
    /// advancing simulation, audio, or draw-owned state. Used after restoring a captured
    /// debugger state and when attaching a presenter to an already paused game.
    /// </summary>
    /// <remarks>
    /// Returns null when this game has only produced legacy pixels. The host must handle
    /// that explicitly; this method never fabricates a GPU packet from a CPU raster or
    /// calls a scene draw routine to reconstruct an earlier display.
    /// </remarks>
    public DisplayPacket GetRetainedDisplay(long hostSequence, long hostGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        return Session.display.Retained(new(hostSequence, hostGeneration, FrameNumber));
    }

    /// <summary>
    /// Advances one frame, capturing extracted scenes instead of rasterizing them.
    /// Unconverted scenes are explicitly reported as legacy pixel output. The host
    /// supplies sequence/generation outside the saved game graph, so restoring game
    /// state must not rewind presentation identity. This is a staged migration API.
    /// </summary>
    public CapturedFrontendFrame StepCaptured(ControllerButton controllerInput, long hostSequence, long hostGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        var identity = new RenderFrameIdentity(hostSequence, hostGeneration, WordWrap.Unsigned(FrameNumber + 1));
        Session.display.BeginCapture(identity);
        try
        {
            FrontendFrame frame = Step(controllerInput);
            return new(frame, Session.display.CompleteCapture(identity));
        }
        finally { Session.display.EndCapture(); }
    }

    /// <summary>
    /// Copies diagnostic state without reading cartridge data or invoking gameplay.
    /// Hosts capture this before a frame and again on failure to expose partial mutations.
    /// </summary>
    public string CaptureGameplayFailureContext()
    {
        string location = $"frame=${FrameNumber:X4}; state={Session.GameState}; " +
            $"room={GameplayActiveRoom?.Name}; roomState={GameplayActiveRoomState?.Name}; door={GameplayActiveDoor?.Name}";
        if (Session.Runtime?.Player.Samus is not { } samus)
            return location + "; Samus=not initialized";
        return location + $"; pose={samus.Pose}; animationFrame={samus.AnimationFrame}; animationTimer={samus.AnimationFrameTimer.RemainingFrames}; " +
            $"animatedPose={samus.AnimatedPose}; position={samus.Position.X.Whole},{samus.Position.Y.Whole}; " +
            $"input=${Session.Runtime.Controller1.Current:X}; newlyPressed=${Session.Runtime.Controller1.NewlyPressed:X}; inputLocked={samus.Control.AlphaLocked}; " +
            $"drainedPhase={samus.Drained.Phase}; drainedHandler={samus.Drained.GetUpHandler}; " +
            $"escapeTimer={Session.Runtime.EscapeTimer.Phase}; fx={samus.LiquidPhysics.FxType}; medium={samus.LiquidPhysics.LiquidPhysicsType}; " +
            $"waterY=${samus.LiquidPhysics.FxYPosition:X4}; acidY=${samus.LiquidPhysics.LavaAcidYPosition:X4}";
    }

    /// <summary>
    /// Rebinds host policy after an interactive debugger-state load. Cartridge state,
    /// resource totals, and the retained frame remain intact unless a tester grant is newly enabled. Exact replay must omit
    /// this override unless the recording explicitly supplies different host options.
    /// </summary>
    public void ApplyHostOptions(SuperMetroidGameOptions hostOptions)
    {
        Ensure.NotNull(hostOptions);
        Session.Runtime?.ApplyHostOptions(hostOptions.RuntimeOverrides(forAttractDemo: false));
        Session.ConfiguredOptions = hostOptions;
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
                Session.menuLayer1.Reset();
                // The reset phase itself runs after the first advance; the seed alone is
                // not what it sees.
                Session.Random.Generate();
                break;
            case DispatchRandomStep.NonGameplayLineEffectsThenAdvance:
                // The per-line effects still run on the update before the pause screen; lava swaps the live random bytes.
                Session.GameplayRuntime.DoorTransition.AdvanceEffectsAndRandom(Session.GameplayRuntime.Room.LoadedScope.Plms, Session.GameplayRuntime.Room.LoadedScope);
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
