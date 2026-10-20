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

/// <summary>
/// Top-level C# counterpart of bank $82's <c>RunOneFrameOfGameInner</c> dispatcher.
/// </summary>
/// <remarks>
/// The dispatcher is intentionally narrow while the front end is translated incrementally:
/// every supported state owns its cartridge-derived logic and framebuffer, and unsupported
/// transitions stop on a named native state instead of silently jumping to a debug room.
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
        Session.runtime?.BindArtwork(artwork, previous);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
        {
            Session.saves.BindMapPresentation(artwork.MapPresentation);
            Session.ceresDestruction?.BindFixedColors(artwork.MapPresentation?.PowerBombFixedColors);
            Session.ceresDestruction?.BindPaletteFxColors(artwork.MapPresentation?.RoomPaletteFx);
            Session.pauseMenu?.BindMapPresentation(artwork.MapPresentation);
            Session.fileSelectMap?.BindMapPresentation(artwork.MapPresentation);
            Session.gameOver?.BindMapPresentation(artwork.MapPresentation);
            Session.options?.BindMapPresentation(artwork.MapPresentation);
            Session.fileSelect?.BindMapPresentation(artwork.MapPresentation);
            Session.title?.BindTitleGradient(artwork.MapPresentation?.TitleGradient);
            Session.title?.BindTitleGraphics(artwork.MapPresentation?.TitleGraphics);
            Session.title?.BindTitlePalette(artwork.MapPresentation?.TitlePalette);
            if (Session.intro is not null) Session.intro.NarrationPresentation = artwork.MapPresentation?.IntroNarration;
            Session.intro?.BindIntroFont(artwork.MapPresentation?.IntroFont);
            Session.intro?.BindSamusHurtColors(artwork.MapPresentation?.SamusHurtColors);
            Session.endingCredits?.BindEndingText(artwork.MapPresentation?.EndingText);
            Session.endingCredits?.BindPaletteFxColors(artwork.MapPresentation?.RoomPaletteFx);
            Session.endingCredits?.BindEndingFont(artwork.MapPresentation?.EndingFont);
            Session.endingCredits?.BindStaffCredits(artwork.MapPresentation?.StaffCredits);
        }
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
        {
            Session.intro?.BindCharacterArtwork(artwork.IntroCinematicArt);
            Session.ceresDestruction?.BindArtwork(artwork.IntroCinematicArt);
            Session.endingCredits?.BindFlightArtwork(artwork.IntroCinematicArt?.CeresFlight);
        }
        if (!ReferenceEquals(previous.SamusBodyArt, artwork.SamusBodyArt))
        {
            Session.intro?.BindSamusBodyArtwork(artwork.SamusBodyArt);
        }
        if (!ReferenceEquals(previous.EndingMode7Art, artwork.EndingMode7Art))
        {
            Session.endingCredits?.BindMode7Artwork(artwork.EndingMode7Art);
        }
        if (!ReferenceEquals(previous.EndingObjectArt, artwork.EndingObjectArt))
        {
            Session.endingCredits?.BindObjectArtwork(artwork.EndingObjectArt);
        }
        if (!ReferenceEquals(previous.EndingPaletteArt, artwork.EndingPaletteArt))
        {
            Session.endingCredits?.BindPaletteArtwork(artwork.EndingPaletteArt);
        }
        if (!ReferenceEquals(previous.ProjectileCompositions, artwork.ProjectileCompositions))
        {
            if (Session.intro is not null) Session.intro.ProjectileCompositions = artwork.ProjectileCompositions;
        }
        if (!ReferenceEquals(previous.ProjectileFrameBindings, artwork.ProjectileFrameBindings))
        {
            if (Session.intro is not null) Session.intro.ProjectileFrameBindings = artwork.ProjectileFrameBindings;
        }
        if (!ReferenceEquals(previous.BeamArtwork, artwork.BeamArtwork))
        {
            // A completed intro remains in a restored object graph, but its VRAM
            // and beam DMA are no longer displayed or advanced during gameplay.
            // Rebinding that dormant scene would replay an unrelated cartridge
            // read while loading a gameplay debugger state.
            if (Session.GameState == SuperMetroidGameState.IntroCinematic)
                Session.intro?.BindBeamArtwork(artwork.BeamArtwork);
        }
        if (!ReferenceEquals(previous.TrailArtwork, artwork.TrailArtwork))
        {
            if (Session.intro is not null) Session.intro.TrailArtwork = artwork.TrailArtwork;
        }
    }

    /// <summary>Current host options; interactive state restoration rebinds these to the active session.</summary>
    public SuperMetroidGameOptions ConfiguredOptions { get => Session.ConfiguredOptions; }

    /// <summary>Creates a reset-state session, reading the selected save slot and capturing the native boot carry before any dispatcher frame runs.</summary>
    /// <param name="saves">The battery-backed saves the session owns; the host loads them and persists them on <see cref="SaveRamChanged"/>.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    public SuperMetroidGame(
        SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions = null)
        : this(saves, gameOptions, renderGameplayFrames: true, SuperMetroidGameHostServices.Default, new CartridgeRandom())
    {
    }

    /// <summary>
    /// Creates the dispatcher with an optional gameplay-software-PPU bypass for friend
    /// verification hosts that inspect cartridge state rather than frame pixels.
    /// </summary>
    /// <remarks>
    /// Frontend screens remain rendered because their state machines own their own
    /// compositors. The expensive room compositor is skipped only after a runtime exists;
    /// production callers always enter through the public rendering constructor above.
    /// </remarks>
    /// <param name="saves">The battery-backed saves the session owns.</param>
    /// <param name="gameOptions">Retained host options, or <see langword="null"/> to use a new default options record.</param>
    /// <param name="renderGameplayFrames">Whether gameplay frames are composited.</param>
    /// <param name="hostServices">The hardware-timing policies; replay verification supplies recorded evidence here.</param>
    /// <param name="random">The session's random-number word, which the frontend and every gameplay runtime share.</param>
    internal SuperMetroidGame(
        SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions,
        bool renderGameplayFrames,
        SuperMetroidGameHostServices hostServices,
        CartridgeRandom random)
    {
        Session = new FrontendSession(saves, gameOptions, renderGameplayFrames, hostServices, random);
    }

    /// <summary>The session's battery-backed saves, which the host persists after <see cref="SaveRamChanged"/>.</summary>
    public SaveSlotStore Saves => Session.saves;

    /// <summary>
    /// Raised after translated code changes the battery-backed saves. The core owns them as
    /// <see cref="Saves"/>; a desktop/console host owns the choice of durable storage medium.
    /// </summary>
    public event Action? SaveRamChanged
    {
        add => Session.saveRamChanged += value;
        remove => Session.saveRamChanged -= value;
    }

    /// <summary>
    /// Binds the extracted audio catalog at startup and after debugger-state restoration, so
    /// every following update also synthesizes host PCM (<see cref="CopyAudioFrame"/>).
    /// </summary>
    /// <remarks>
    /// Binding affects presentation only. Each update advances the session's SPC port model and
    /// its sound-effect acknowledgements whether or not audio is bound, so gameplay is identical
    /// with audio enabled, disabled, or without an output device.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="clips"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session already advanced without synthesis and has no player to continue.</exception>
    public void BindAudio(ClipAudioCatalog clips) => Session.audioSession.BindClips(clips);

    /// <summary>Copies the last update's 1,600 interleaved 48-kHz stereo samples into host-owned storage.</summary>
    /// <exception cref="InvalidOperationException">No audio catalog is bound.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> holds fewer than 1,600 samples.</exception>
    public void CopyAudioFrame(Span<short> destination) => Session.audioSession.CopyFrame(destination);


    /// <summary>Current value of the native game-state word at WRAM $0998.</summary>
    public SuperMetroidGameState GameState { get => Session.GameState; internal set => Session.GameState = value; }

    /// <summary>Wrapping frame counter corresponding to the game's 16-bit NMI counter, 0 through 65535.</summary>
    public int FrameNumber
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The active room, exposed for door-transition watches, or null before gameplay.</summary>
    public RoomHeaderDefinition? GameplayActiveRoom => Session.runtime?.Room.ActiveRoom?.Definition;

    /// <summary>The active room's selected state, exposed beside the room identity, or null before gameplay.</summary>
    public CartridgeRoomState? GameplayActiveRoomState => Session.runtime?.Room.ActiveRoom?.State;

    /// <summary>Validated logical area/room pair, or null before cartridge gameplay.</summary>
    public RoomIdentity? GameplayActiveRoomIdentity => Session.runtime?.Room.ActiveRoom?.Identity;

    /// <summary>The door the active room was entered through, exposed for door-transition watches.</summary>
    public CartridgeDoorHeader? GameplayActiveDoor => Session.runtime?.Room.ActiveDoor;


    /// <summary>Runs one dispatcher frame and returns the PPU-visible result.</summary>
    public FrontendFrame Step(SnesButton controllerInput)
    {
        // Derive the door-owned native disable flag before any producer can queue
        // audio. This also restores its meaning for legacy snapshots without adding
        // another serialized flag that could disagree with the coroutine phase.
        GamePhaseAudio phaseAudio = Phases.Of(Session.GameState).Audio;
        Session.audio.DoorTransitionSoundsDisabled = phaseAudio.DoorSoundsDisabled;
        bool pauseInputEpilogue = phaseAudio.PauseInputEpilogue;
        bool messageWasActive = Session.runtime?.MessageBox.IsActive == true;
        bool doorAudioDispatch = phaseAudio.DoorAudioDispatch;
        DoorTransitionPhase startingDoorPhase = Session.doorTransition.Phase;
        // An update that resumes a dispatch after its NMI wait never reaches the main loop's
        // music prologue ($88:84B9) or its sound handler ($82:89EF).
        bool resumesNmiWait = NextUpdateResumesNmiWait;
        // MainGameLoop runs the music queue ($82:894B) before dispatching any game state, so
        // a dispatch that waits on HasQueuedMusic sees this update's queue. Sound handlers
        // run after its coroutine returns. An accepted IRQ/NMI alone runs neither handler,
        // and a message box's lag frames run only the box's own calls (below).
        bool prologueMusicDispatch = !resumesNmiWait && !messageWasActive &&
            (!doorAudioDispatch || !phaseAudio.DoorSoundsDisabled ||
             startingDoorPhase is not (DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading));
        IReadOnlyList<CartridgeAudioCommand> prologueMusicCommands = prologueMusicDispatch
            ? Session.audio.AdvanceMusicDispatch() : Array.Empty<CartridgeAudioCommand>();
        // SendAPUData stalls a door dispatch while door IRQs keep NMIs accepted; nothing
        // between this update's own NMI and the upload reads the counters.
        if (doorAudioDispatch && prologueMusicCommands.Any(command => command.Kind == CartridgeAudioCommandKind.Upload))
            Session.GameplayRuntime.Clock.AdvanceHardwareWaitFrames(Session.HostServices.DoorMusicUploadNmis.AcceptedNmisDuringUpload(Session.audio.MusicDataIndex));
        var gameplayAudio = new GameplayAudioFramePublication(Session.audio);
        FrameNumber = NativeWordCounter.IncrementWord(FrameNumber);
        if (Session.runtime is null)
            Session.frameClock.AdvanceAcceptedFrame();
        AdvanceMenuRandom();
        if (Session.runtime is not null)
            Session.runtime.DispatchGameState = Session.GameState;
        Phases.Of(Session.GameState).Step(controllerInput, gameplayAudio);

        Session.CollectTranslatedAudioRequests(gameplayAudio);
        // DisplayMessageBox ($85:8080) calls CancelSoundEffects before its first
        // NMI wait. Publish earlier gameplay requests first, then cancel once at
        // this entry boundary; repeating it during the wait would cancel the
        // message's own cursor/saving sounds. A loaded active message is not an
        // entry and must not inject a new command merely because the host resumed.
        if (!messageWasActive && Session.runtime?.MessageBox.IsActive == true)
            Session.audio.QueueCancelSoundEffects();
        bool musicHandler = false;
        int soundHandlers = !resumesNmiWait && (!doorAudioDispatch ||
            startingDoorPhase is not (DoorTransitionPhase.LoadMoreThingsAndOpenDoor or DoorTransitionPhase.WaitForDoorOpeningScroll))
            ? 1 : 0;
        if (messageWasActive)
        {
            // A message-box lag frame runs exactly the routine's own audio calls.
            MessageBoxFrameAudio boxAudio = Session.GameplayRuntime.MessageBox.LastFrameAudio;
            musicHandler = boxAudio.MusicHandlerCalls != 0;
            soundHandlers = boxAudio.SoundHandlerCalls;
        }
        else if (Session.runtime?.MessageBox.IsActive == true)
        {
            // The opening dispatch's HandleSounds runs only once the routine returns.
            soundHandlers = 0;
        }
        IReadOnlyList<CartridgeAudioCommand> trailingAudioCommands = Session.audio.AdvanceFrame(Session.HostServices.SoundAcknowledgements.ForNextFrame(Session.audioSession.Acknowledgements, Session.audio.SoundPortWrites),
            advanceMusicQueue: musicHandler,
            soundEffectHandlerCalls: soundHandlers);
        IReadOnlyList<CartridgeAudioCommand> audioCommands = prologueMusicCommands.Count == 0 ? trailingAudioCommands
            : [.. prologueMusicCommands, .. trailingAudioCommands];
        // The SPC acts on this update's port writes and uploads during the update's own DSP
        // cycles; its echoes become visible to the next update's sound handlers.
        Session.lastAudioEvents = Session.audioSession.Advance(audioCommands);
        // $82:8AB0 runs after the pause dispatcher returns, even though Samus's
        // draw-time epilogue did not run. Preserve its held-only publication;
        // updating the press edge or auto-jump timer here would invent gameplay.
        if (pauseInputEpilogue)
            Session.GameplayRuntime.Player.IntroducedSamus.InputSnapshot.PublishMainLoopHeldInput(Session.GameplayRuntime.Controller1.Current);
        return CurrentFrame;
    }

    /// <summary>One phase per game state, in native state order; each runs that state's dispatch.</summary>
    [field: NonSerialized]
    private GamePhaseTable Phases => field ??= new GamePhaseTable(
        [
            new GamePhase(SuperMetroidGameState.Reset,
                MainLoopRandomCall.Reseed,
                step: (_, _) => MenuPhases.DispatchReset(),
                name: () => "Reset"),
            new GamePhase(SuperMetroidGameState.OpeningCinematic,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => MenuPhases.DispatchOpeningCinematic(controllerInput),
                name: () => Session.ActiveTitle.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.GameOptionsMenu,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => MenuPhases.DispatchGameOptionsMenu(controllerInput),
                name: () => Session.ActiveOptions.Phase.ToString()),
            new UnreachableGamePhase(SuperMetroidGameState.Unused03),
            new GamePhase(SuperMetroidGameState.FileSelectMenus,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchFileSelectMenus(controllerInput),
                name: () => Session.ActiveFileSelect.Phase.ToString(),
                resumesNmiWait: () => Session.fileSelect?.ResumesAfterNmiWait == true),
            // The map's final dispatch resumes after its forced-blank NMI wait.
            // The final map dispatch latched its load request, then waited for this NMI ($81:AF8C).
            new GamePhase(SuperMetroidGameState.FileSelectMap,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchFileSelectMap(controllerInput),
                name: () => Session.fileSelectMap?.Phase.ToString() ?? "Preparing saved-game map",
                resumesNmiWait: () => Session.fileSelectMap?.LoadRequested == true),
            new GamePhase(SuperMetroidGameState.LoadingGameData,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => LoadingPhases.DispatchLoadingGameData(controllerInput),
                name: () => $"Loading Landing Site ({Session.gameLoadingWaitsRemaining} NMI waits)",
                resumesNmiWait: () => Session.ResumesGameLoadingWait),
            new GamePhase(SuperMetroidGameState.MainGameplayFadeIn,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => LoadingPhases.DispatchMainGameplayFadeIn(controllerInput, gameplayAudio),
                name: () => Session.gameplayFadeLeadsToCeresArrival ? $"Ceres elevator fade-in (brightness {Session.postCeresFadeBrightness})" : $"Landing Site fade-in (brightness {Session.postCeresFadeBrightness})"),
            new GamePhase(SuperMetroidGameState.MainGameplay,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchMainGameplay(controllerInput, gameplayAudio),
                name: () => "Gameplay"),
            new GamePhase(SuperMetroidGameState.HitDoorBlock,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchDoorCollisionAndLoading(controllerInput),
                name: () => "Door collision",
                audio: new GamePhaseAudio(DoorAudioDispatch: true, DoorSoundsDisabled: false, PauseInputEpilogue: false)),
            new GamePhase(SuperMetroidGameState.LoadingNextRoomA,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchDoorCollisionAndLoading(controllerInput),
                name: () => "Loading destination room",
                audio: new GamePhaseAudio(DoorAudioDispatch: true, DoorSoundsDisabled: false, PauseInputEpilogue: false)),
            new GamePhase(SuperMetroidGameState.LoadingNextRoomB,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DoorPhases.DispatchLoadingNextRoomB(controllerInput),
                name: () => "Destination room ready",
                audio: new GamePhaseAudio(DoorAudioDispatch: true, DoorSoundsDisabled: true, PauseInputEpilogue: false)),
            new GamePhase(SuperMetroidGameState.PausingDarkening,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchPausingDarkening(controllerInput, gameplayAudio),
                name: () => $"Pausing: gameplay darken ({Session.pauseBrightness})"),
            new GamePhase(SuperMetroidGameState.Pausing,
                MainLoopRandomCall.NonGameplayHdmaThenAdvance,
                step: (controllerInput, _) => PausePhases.DispatchPausing(controllerInput),
                name: () => "Pausing: load pause assets",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            // $82:894F runs once before each pause-only dispatcher as well.
            // Fade states $0C/$12 run StepFrame and retain its own RNG call.
            new GamePhase(SuperMetroidGameState.PausedA,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchPausedA(controllerInput),
                name: () => $"Paused: fade in ({Session.pauseBrightness})",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.PausedB,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchPausedB(controllerInput),
                name: () => Session.pauseMenu?.ScreenMode == 1 ? "Paused: equipment" : "Paused: map",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.UnpausingA,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchUnpausingA(controllerInput),
                name: () => $"Unpausing: fade out ({Session.pauseBrightness})",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.UnpausingB,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => PausePhases.DispatchUnpausingB(controllerInput),
                name: () => "Unpausing: restore gameplay",
                audio: new GamePhaseAudio(DoorAudioDispatch: false, DoorSoundsDisabled: false, PauseInputEpilogue: true)),
            new GamePhase(SuperMetroidGameState.Unpausing,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchUnpausing(controllerInput, gameplayAudio),
                name: () => $"Unpausing: gameplay brighten ({Session.pauseBrightness})"),
            new GamePhase(SuperMetroidGameState.DeathSequenceStart,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => DeathPhases.DispatchDeathSequenceStart(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.DeathSequenceStart)),
            new GamePhase(SuperMetroidGameState.DeathBlackOutSurroundings,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => DeathPhases.DispatchDeathBlackOutSurroundings(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.DeathBlackOutSurroundings)),
            new GamePhase(SuperMetroidGameState.DeathWaitForMusic,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathWaitForMusic(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathWaitForMusic)),
            new GamePhase(SuperMetroidGameState.DeathPreFlashing,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathPreFlashing)),
            new GamePhase(SuperMetroidGameState.DeathFlashing,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathFlashing)),
            new GamePhase(SuperMetroidGameState.DeathExplosionWhiteOut,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFlashing(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathExplosionWhiteOut)),
            // State $19 only fades; MainGameLoop's RNG call precedes it as any dispatch.
            new GamePhase(SuperMetroidGameState.DeathFinalBlackOut,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => DeathPhases.DispatchDeathFinalBlackOut(controllerInput),
                name: () => nameof(SuperMetroidGameState.DeathFinalBlackOut)),
            // Game-over indexes zero and one resume inside their own NMI waits.
            new GamePhase(SuperMetroidGameState.GameOverMenu,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => MenuPhases.DispatchGameOverMenu(controllerInput),
                name: () => nameof(SuperMetroidGameState.GameOverMenu),
                resumesNmiWait: () => Session.gameOver?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.ReserveTanksAuto,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchReserveTanksAuto(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.ReserveTanksAuto)),
            new UnreachableGamePhase(SuperMetroidGameState.Unused1c),
            new UnreachableGamePhase(SuperMetroidGameState.DebugGameOverMenu),
            new GamePhase(SuperMetroidGameState.IntroCinematic,
                MainLoopRandomCall.Advance,
                step: (controllerInput, _) => CinematicPhases.DispatchIntroCinematic(controllerInput),
                name: () => Session.ActiveIntro.Phase.ToString()),
            new GamePhase(SuperMetroidGameState.SetUpNewGame,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => LoadingPhases.DispatchSetUpNewGame(controllerInput),
                name: () => Session.loadingExistingSave ? "Loading saved game" : "Loading fresh Ceres game",
                resumesNmiWait: () => Session.ResumesGameLoadingWait),
            new GamePhase(SuperMetroidGameState.MadeItToCeresElevator,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchMadeItToCeresElevator(controllerInput, gameplayAudio),
                name: () => Session.ceresDeparture.Phase == CeresDeparturePhase.HoldingOnElevator ? $"Ceres elevator departure ({Session.ceresDeparture.HoldFramesRemaining.RawValue})" : Session.runtime?.Room.CeresElevatorArrival is { IsComplete: false } ? "Ceres elevator arrival" : "Ceres controls unlocked"),
            new GamePhase(SuperMetroidGameState.BlackoutFromCeres,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchBlackoutFromCeres(controllerInput, gameplayAudio),
                name: () => $"Ceres blackout (brightness {Session.ceresDeparture.Brightness})"),
            // $82:8B0E shares the intro's state-$1E/$22/$25 handler and its prologue call.
            new GamePhase(SuperMetroidGameState.CeresGoesBoom,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => CinematicPhases.DispatchCeresGoesBoom(controllerInput),
                name: () => $"Ceres destruction: {Session.ceresDestruction?.Phase}",
                resumesNmiWait: () => Session.ceresDestruction?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.TimeUp,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchTimeUp(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TimeUp)),
            // $24 fades without a gameplay call; its HDMA objects still run until it clears them.
            new GamePhase(SuperMetroidGameState.WhitingOutFromTimeUp,
                MainLoopRandomCall.NonGameplayHdmaThenAdvance,
                step: (controllerInput, _) => CinematicPhases.DispatchWhitingOutFromTimeUp(controllerInput),
                name: () => nameof(SuperMetroidGameState.WhitingOutFromTimeUp)),
            new GamePhase(SuperMetroidGameState.CeresGoesBoomWithSamus,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (controllerInput, _) => CinematicPhases.DispatchCeresGoesBoomWithSamus(controllerInput),
                name: () => nameof(SuperMetroidGameState.CeresGoesBoomWithSamus),
                resumesNmiWait: () => Session.ceresDestruction?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.SamusEscapesFromZebes,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => GameplayPhases.DispatchSamusEscapesFromZebes(controllerInput, gameplayAudio),
                name: () => $"Zebes escape fade-out ({Session.endingFadeBrightness})"),
            // State $27 is dispatched by MainGameLoop after its RNG call, except while the
            // ending setup resumes inside its own NMI waits.
            new GamePhase(SuperMetroidGameState.EndingAndCredits,
                MainLoopRandomCall.AdvanceUnlessResumingWait,
                step: (_, _) => CinematicPhases.DispatchEndingAndCredits(),
                name: () => $"Ending: {Session.endingCredits?.Phase}",
                resumesNmiWait: () => Session.endingCredits?.ResumesAfterNmiWait == true),
            new GamePhase(SuperMetroidGameState.TransitionToDemoA,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionToDemoA)),
            new GamePhase(SuperMetroidGameState.TransitionToDemoB,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionToDemoB)),
            new GamePhase(SuperMetroidGameState.PlayingDemo,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.PlayingDemo)),
            new GamePhase(SuperMetroidGameState.TransitionFromDemoA,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionFromDemoA)),
            new GamePhase(SuperMetroidGameState.TransitionFromDemoB,
                MainLoopRandomCall.RuntimeOwned,
                step: (controllerInput, gameplayAudio) => AttractPhases.DispatchAttractDemo(controllerInput, gameplayAudio),
                name: () => nameof(SuperMetroidGameState.TransitionFromDemoB)),
        ]);

    /// <summary>Reads the current status without advancing the dispatcher.</summary>
    public FrontendFrame CurrentFrame => new(Session.GameState, PhaseName, FrameNumber);

    /// <summary>Human-readable active subphase for diagnostics without changing native state.</summary>
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
    public RenderFrameSnapshot GetRetainedDisplay(long hostSequence, long hostGeneration)
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
    public CapturedFrontendFrame StepCaptured(SnesButton controllerInput, long hostSequence, long hostGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostSequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hostGeneration);
        var identity = new RenderFrameIdentity(hostSequence, hostGeneration, unchecked((ushort)(FrameNumber + 1)));
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
        string location = $"frame=${FrameNumber:X4}; state=${(ushort)Session.GameState:X2} {Session.GameState}; " +
            $"room={GameplayActiveRoom?.Name}; roomState={GameplayActiveRoomState?.Name}; door={GameplayActiveDoor?.Name}";
        if (Session.runtime?.Player.Samus is not { } samus)
            return location + "; Samus=not initialized";
        return location + $"; pose=${(int)samus.Pose:X2}; animationFrame={samus.AnimationFrame}; animationTimer={samus.AnimationFrameTimer.RawValue}; " +
            $"animationList=${samus.AnimationDelayListAddress:X6}; position={samus.Position.X.Whole},{samus.Position.Y.Whole}; " +
            $"input=${Session.runtime.Controller1.Current:X}; newlyPressed=${Session.runtime.Controller1.NewlyPressed:X}; inputLocked={samus.Control.AlphaLocked}; " +
            $"drainedPhase={samus.Drained.Phase}; drainedHandler={samus.Drained.GetUpHandler}; " +
            $"escapeTimer={Session.runtime.EscapeTimer.State}; fx={samus.LiquidPhysics.FxType}; medium={samus.LiquidPhysics.LiquidPhysicsType}; " +
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
        Session.runtime?.ApplyHostOptions(hostOptions.RuntimeOverrides(forAttractDemo: false));
        Session.ConfiguredOptions = hostOptions;
    }

    /// <summary>
    /// True when the next update resumes a frontend dispatch after its own NMI wait instead
    /// of entering MainGameLoop. Such an update makes no main-loop RNG call.
    /// </summary>
    internal bool NextUpdateResumesNmiWait => Phases.Of(Session.GameState).ResumesNmiWait;

    /// <summary>
    /// Preserves the bank-$82 main-loop RNG call through title/file selection and
    /// loading. These menu dispatchers return once per frame; they do not run the
    /// gameplay runtime's prologue. SRAM contains no copy of the live RNG word.
    /// </summary>
    internal void AdvanceMenuRandom()
    {
        switch (Phases.Of(Session.GameState).RandomCall)
        {
            case MainLoopRandomCall.Reseed:
                // Only the reset vector reseeds. A continued save must keep the word
                // produced by gameplay and the game-over/file-map waiting frames.
                Session.Random.SeedPowerOn();
                Session.frameClock.Reset();
                Session.menuLayer1.Reset();
                // State zero is itself dispatched by MainGameLoop after its first
                // GenerateRandomNumber call; it is not the reset vector's seed store.
                Session.Random.Generate();
                break;
            case MainLoopRandomCall.NonGameplayHdmaThenAdvance:
                // HDMA runs before $0D disables it; lava swaps the live RNG bytes.
                Session.GameplayRuntime.DoorTransition.AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true, Session.GameplayRuntime.Room.LoadedScope.Plms, Session.GameplayRuntime.Room.LoadedScope);
                break;
            case MainLoopRandomCall.AdvanceUnlessResumingWait:
                if (!NextUpdateResumesNmiWait)
                    Session.Random.Generate();
                break;
            case MainLoopRandomCall.Advance:
                Session.Random.Generate();
                break;
            case MainLoopRandomCall.RuntimeOwned:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MainLoopRandomCall)} {Phases.Of(Session.GameState).RandomCall}.");
        }
        // Gameplay retains its existing call after accepted NMI/HDMA processing.
        // Do not advance here for runtime frames or nested NMI-waiting coroutines.
    }
}
