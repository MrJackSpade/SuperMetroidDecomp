using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The game: each update runs the current <see cref="SuperMetroidGameState"/> sequence and the audio around it.
/// </summary>
/// <remarks>
/// Every sequence owns its own steps, logic and display, and a transition always names the sequence and step it enters,
/// so an undefined step stops the game with its name instead of continuing somewhere arbitrary.
/// </remarks>
public sealed class SuperMetroidGame
{
    /// <summary>The state and owners every phase runs against (<see cref="FrontendSession"/>).</summary>
    internal FrontendSession Session { get; }

    /// <summary>
    /// Binds installed artwork whenever a host binds the game to its content (<see cref="GameSessionComposer.Bind"/>). Each
    /// catalog whose reference changed reaches the scenes that hold it; unchanged catalogs are left alone. The live runtime
    /// binds through <see cref="SuperMetroidRuntime.BindArtwork"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">A screen is shown in a session no host bound artwork to before.</exception>
    internal void BindArtwork(GameArtwork artwork)
    {
        Ensure.NotNull(artwork);
        GameArtwork? previous = Session.ArtworkBound ? Session.Artwork : null;
        Session.Artwork = artwork;
        Session.Runtime?.BindArtwork(artwork.Gameplay);
        if (previous is null || !ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
            Session.saves.BindMaps(artwork.ScreenContent.Maps);
        // Every screen is built over the session's bound artwork, so a shown screen always has artwork to rebind from.
        if (Session.Screen is { } screen)
            screen.BindArtwork(previous ?? throw new InvalidOperationException(
                "A frontend screen is shown in a session whose installed artwork was never bound."), artwork);
    }


    /// <summary>
    /// Creates a reset-state session, reading the selected save slot and fixing the power-on save check's outcome
    /// (<see cref="Compatibility.PowerOnEarthquakeSoundDelay"/>) before any update runs.
    /// </summary>
    /// <param name="saves">The saves the session owns; the host loads them and persists them on <see cref="SavesChanged"/>.</param>
    /// <param name="assists">The assists the session plays under, or <see langword="null"/> for the unassisted game.</param>
    public SuperMetroidGame(
        SaveSlotStore saves,
        GameplayAssists? assists = null)
        : this(saves, assists, SuperMetroidGameHostServices.Default, new SharedRandom())
    {
    }

    /// <summary>
    /// Creates the game-state loop with host-supplied timing and display policies: a host may supply its own timing
    /// or keep the display it shows while it reads the game's state.
    /// </summary>
    /// <remarks>
    /// Frontend screens are always rendered because their state machines own their own compositors; only
    /// gameplay updates publish through <see cref="SuperMetroidGameHostServices.GameplayFrames"/>.
    /// </remarks>
    /// <param name="saves">The saves the session owns.</param>
    /// <param name="assists">The assists the session plays under, or <see langword="null"/> for the unassisted game.</param>
    /// <param name="hostServices">The compatibility services and display policy.</param>
    /// <param name="random">The session's random number generator, which the frontend and every gameplay runtime share.</param>
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
    /// Binds the installed sound clips whenever a host binds the game to its content, so every following update also
    /// mixes its sound into the samples the host copies (<see cref="CopyAudioFrame"/>).
    /// </summary>
    /// <remarks>
    /// Binding affects presentation only. Each update advances the session's sound acceptance clock and
    /// its sound-effect acknowledgements whether or not audio is bound, so gameplay is identical
    /// with audio enabled, disabled, or without an output device.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="clips"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The session already advanced without synthesis and has no player to continue.</exception>
    internal void BindAudio(ClipAudioLibrary clips) => Session.audioSession.BindClips(clips);

    /// <summary>Copies the last update's interleaved stereo samples (<see cref="AudioSession.PcmSamplesPerFrame"/>) into host-owned storage.</summary>
    /// <exception cref="InvalidOperationException">No audio catalog is bound.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> holds fewer than 1,600 samples.</exception>
    public void CopyAudioFrame(Span<short> destination) => Session.audioSession.CopyFrame(destination);


    /// <summary>The sequence the game is running.</summary>
    public SuperMetroidGameState GameState => Session.GameState;

    /// <summary>
    /// The number of updates the game has run. Hosts and diagnostics name frames by it; no gameplay reads it, so it counts
    /// on without the game's frame counter wrap.
    /// </summary>
    public int FrameNumber { get; private set; }

    /// <summary>The active room, exposed for door-transition watches, or null before gameplay.</summary>
    public RoomDefinition? GameplayActiveRoom => Session.Runtime?.Room.ActiveRoom?.Definition;

    /// <summary>The active room's selected state, exposed beside the room, or null before gameplay.</summary>
    public RoomStateSettings? GameplayActiveRoomState => Session.Runtime?.Room.ActiveRoom?.State;

    /// <summary>The door the active room was entered through, exposed for door-transition watches.</summary>
    public DoorDefinition? GameplayActiveDoor => Session.Runtime?.Room.ActiveDoor;


    /// <summary>Runs one update and returns its status.</summary>
    public FrontendFrame Step(ControllerButton controllerInput)
    {
        // The sound queues read whether the door transition has disabled sound effects from the door's phase itself as each
        // request is made (#2499); the phase's audio here chooses only the update's own audio processing.
        GamePhaseAudio phaseAudio = Phases.Of(Session.GameState).Audio;
        bool pauseInputEpilogue = phaseAudio == GamePhaseAudio.PauseInputEpilogue;
        bool messageWasActive = Session.Runtime?.MessageBox.IsActive == true;
        bool doorAudioUpdate = phaseAudio is GamePhaseAudio.DoorTransition or GamePhaseAudio.DoorTransitionSoundsDisabled;
        // Whether this update continues the door's destination load, and whether it leaves the load running, are read
        // before the phase steps.
        bool continuesDoorLoad = Session.doorTransition.ContinuesDestinationLoad;
        bool leavesDoorLoadRunning = Session.doorTransition.LeavesDestinationLoadRunning;
        // An update that finishes a phase's step waiting out a frame of the display runs
        // neither the update's music processing nor its sound processing.
        bool runsAudioProcessing = DisplayFrameWaitContinuation.RunsAudioProcessing(PhaseIsWaitingOutDisplayFrame);
        // Music processing runs before any phase, so a phase that waits on HasQueuedMusic
        // sees this update's queue; sound processing runs after the phase returns. A
        // message box's updates run only the box's own audio processing (below).
        bool prologueMusicDispatch = runsAudioProcessing && !messageWasActive &&
            (!doorAudioUpdate || phaseAudio != GamePhaseAudio.DoorTransitionSoundsDisabled || !continuesDoorLoad);
        IReadOnlyList<AudioRequest> prologueMusicRequests = prologueMusicDispatch
            ? Session.audio.AdvanceMusicDispatch() : Array.Empty<AudioRequest>();
        // Frames pass on the frame counters while a door loads a new music data set
        // (IDoorMusicLoadFrames); nothing between this update's frame boundary and the load reads them.
        if (doorAudioUpdate && prologueMusicRequests.Any(request => request is AudioRequest.ChangeMusicDataSet))
            Session.GameplayRuntime.Clock.CountDoorMusicLoad(Session.HostServices.DoorMusicLoadFrames);
        FrameNumber = checked(FrameNumber + 1);
        Session.display.BeginUpdate(FrameNumber);
        if (Session.Runtime is null)
            Session.frameClock.AdvanceAcceptedFrame();
        AdvanceMenuRandom();
        if (Session.Runtime is not null)
            Session.Runtime.RunningGameState = Session.GameState;
        Phases.Of(Session.GameState).Step(controllerInput);

        Session.CollectFrontendAudioRequests();
        // Opening a message box cancels sound effects before the box's first update.
        // Publish earlier gameplay requests first, then cancel once at this entry
        // boundary; repeating it while the box is shown would cancel the message's
        // own cursor and saving sounds. A loaded active message is not an entry and
        // must not queue a new cancellation merely because the host resumed.
        if (!messageWasActive && Session.Runtime?.MessageBox.IsActive == true)
            Session.audio.QueueCancelSoundEffects(SoundRequestPriority.Normal);
        bool runsMusicQueue = false;
        int soundQueuePasses = runsAudioProcessing && (!doorAudioUpdate || !leavesDoorLoadRunning) ? 1 : 0;
        if (messageWasActive)
        {
            // A message-box update runs exactly the box's own audio processing.
            MessageBoxFrameAudio boxAudio = Session.GameplayRuntime.MessageBox.LastFrameAudio;
            runsMusicQueue = boxAudio.MusicQueuePasses != 0;
            soundQueuePasses = boxAudio.SoundQueuePasses;
        }
        else if (Session.Runtime?.MessageBox.IsActive == true)
        {
            // The opening update's sound processing runs only once the box closes.
            soundQueuePasses = 0;
        }
        IReadOnlyList<AudioRequest> trailingAudioRequests = Session.audio.AdvanceFrame(Session.HostServices.SoundAcceptance.ForNextUpdate(Session.audioSession.Acknowledgements, Session.audio.SentSoundRequests),
            advanceMusicQueue: runsMusicQueue,
            soundQueuePasses: soundQueuePasses);
        IReadOnlyList<AudioRequest> audioRequests = prologueMusicRequests.Count == 0 ? trailingAudioRequests
            : [.. prologueMusicRequests, .. trailingAudioRequests];
        // The sound acceptance clock acts on this update's requests during the update's own
        // samples; its acknowledgements become visible to the next update's sound queues.
        AudioFrameEvents audio = new(Session.audioSession.Advance(audioRequests));
        // The pause phases publish held input after they return, even though Samus's
        // drawing step, which ends by publishing it, did not run. Publish only the held input;
        // updating the press edge or auto-jump timer here would invent gameplay.
        if (pauseInputEpilogue)
            Session.GameplayRuntime.Player.IntroducedSamus.InputSnapshot.PublishHeldInput(Session.GameplayRuntime.PlayerActions.Current);
        return new(Session.GameState, PhaseName, FrameNumber, audio);
    }

    /// <summary>The gameplay phases, which the pause's brightening and the time-up's white-out also run (<see cref="GameplayGamePhases"/>).</summary>
    private GameplayGamePhases GameplayPhases => field ??= new GameplayGamePhases(Session);

    /// <summary>One phase per sequence; each runs that sequence's update and owns its steps.</summary>
    private GamePhaseTable Phases => field ??= new GamePhaseTable(
        [
            new TitleGameSequence(Session),
            new FileSelectGameSequence(Session),
            new LoadingGameSequence(Session),
            new GamePhase(SuperMetroidGameState.GameplayFadeIn,
                PhaseRandomStep.RuntimeOwned,
                step: controllerInput => GameplayPhases.RunGameplayFadeIn(controllerInput),
                name: () => Session.GameplayFadeLeadsToCeresArrival ? $"Ceres elevator fade-in (brightness {Session.GameplayFadeIn.Level})" : $"Landing Site fade-in (brightness {Session.GameplayFadeIn.Level})"),
            new GamePhase(SuperMetroidGameState.MainGameplay,
                PhaseRandomStep.RuntimeOwned,
                step: controllerInput => GameplayPhases.RunMainGameplay(controllerInput),
                name: () => "Gameplay"),
            new DoorTransitionGameSequence(Session),
            new PauseGameSequence(Session, GameplayPhases),
            new GamePhase(SuperMetroidGameState.ReserveTankRefill,
                PhaseRandomStep.RuntimeOwned,
                step: controllerInput => GameplayPhases.RunReserveTankRefill(controllerInput),
                name: () => "Reserve tank refill"),
            new DeathGameSequence(Session),
            new GameOverMenuGamePhase(Session),
            new IntroCinematicGamePhase(Session),
            new CeresDepartureGameSequence(Session),
            new CeresDestructionGameSequence(Session),
            new TimeUpGameSequence(Session, GameplayPhases),
            new EndingGameSequence(Session),
            new AttractDemoGameSequence(Session),
        ]);

    /// <summary>
    /// Reads the current status without advancing the game-state loop. No update ran, so it publishes no audio; an update's
    /// audio events come only with the frame <see cref="Step"/> returns.
    /// </summary>
    public FrontendFrame CurrentFrame => new(Session.GameState, PhaseName, FrameNumber, new AudioFrameEvents([]));

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
        // A host also takes a snapshot when an update fails, which may be the update that creates the gameplay runtime before
        // it gives the runtime its Samus, so a missing Samus is kept here (#2503).
        Samus = Session.Runtime is { Player.Samus: { } samus } runtime
            ? new SamusSnapshot
            {
                Pose = samus.Pose,
                AnimationFrame = samus.AnimationFrameName,
                AnimationFramesRemaining = samus.AnimationFrameTimer.RemainingFrames,
                AnimatedPose = samus.AnimatedPose,
                Position = samus.Position.Whole,
                HeldActions = runtime.PlayerActions.Current,
                NewlyPressedActions = runtime.PlayerActions.NewlyPressed,
                InputLocked = samus.Control.InputLocked,
                DrainedPhase = samus.Drained.Phase,
                DrainedGetUp = samus.Drained.GetUpCheck,
                EscapeTimer = runtime.EscapeTimer.Phase,
                Fx = samus.LiquidPhysics.FxType,
                Medium = samus.LiquidPhysics.LiquidPhysicsType,
                WaterSurfaceY = samus.LiquidPhysics.WaterSurfaceY,
                LavaAcidSurfaceY = samus.LiquidPhysics.LavaAcidSurfaceY,
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
    /// True when the current phase's step is waiting out a frame of the display, so the next update finishes it instead
    /// of starting a new update of the phase (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool PhaseIsWaitingOutDisplayFrame => Phases.Of(Session.GameState).IsWaitingOutDisplayFrame;

    /// <summary>
    /// Advances the random generator once per update through the title, file selection and
    /// loading. These phases run once per update and do not run gameplay, which advances it
    /// itself. Saves keep no copy of the generator.
    /// </summary>
    internal void AdvanceMenuRandom()
    {
        switch (Phases.Of(Session.GameState).RandomStep)
        {
            case PhaseRandomStep.Reseed:
                // Only power-on and soft reset reseed. A continued save must keep the value
                // produced by gameplay and the game-over and file-map waiting updates.
                Session.Random.SeedPowerOn();
                Session.frameClock.Reset();
                Session.cameraFractions.Clear();
                // The reset phase itself runs after the first advance; the seed alone is
                // not what it sees.
                Session.Random.Generate();
                break;
            case PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance:
                // The liquid wave and the statues' descent still advance on the update before the pause screen; a lava or
                // acid wave swaps the halves of the random number.
                Session.GameplayRuntime.DoorTransition.AdvanceEffectsAndRandom(Session.GameplayRuntime.Room.LoadedScope, Session.GameplayRuntime.Player.IntroducedSamus);
                break;
            case PhaseRandomStep.Advance:
                if (DisplayFrameWaitContinuation.DrawsRandom(PhaseIsWaitingOutDisplayFrame))
                    Session.Random.Generate();
                break;
            case PhaseRandomStep.RuntimeOwned:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PhaseRandomStep)} {Phases.Of(Session.GameState).RandomStep}.");
        }
        // Gameplay advances the generator itself after its liquid wave and statue descent. Do not
        // advance here for gameplay updates or for an update that finishes a waiting step.
    }
}
