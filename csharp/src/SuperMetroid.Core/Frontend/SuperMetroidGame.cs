using SuperMetroid.Core.World;
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
    /// Binds installed artwork in place of the catalogs the game holds, as a host does when it binds a game it brings back to
    /// its content (<see cref="GameSessionComposer.Bind"/>). Each catalog whose reference changed reaches the scenes that
    /// hold it; unchanged catalogs are left alone. The live runtime binds through <see cref="SuperMetroidRuntime.BindArtwork"/>.
    /// </summary>
    internal void BindArtwork(GameArtwork artwork)
    {
        Ensure.NotNull(artwork);
        GameArtwork previous = Session.Artwork;
        Session.Artwork = artwork;
        Session.Runtime?.BindArtwork(artwork.Gameplay);
        if (!ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
            Session.saves.BindMaps(artwork.ScreenContent.Maps);
        Session.Screen?.BindArtwork(previous, artwork);
    }


    /// <summary>
    /// Creates a reset-state session, reading the selected save slot and fixing the power-on save check's outcome
    /// (<see cref="Compatibility.PowerOnEarthquakeSoundDelay"/>) before any update runs.
    /// </summary>
    /// <param name="saves">The saves the session owns; the host loads them and persists them on <see cref="SavesChanged"/>.</param>
    /// <param name="audioOutput">What the session's audio produces for its host.</param>
    /// <param name="artwork">The installed catalogs the game draws from.</param>
    /// <param name="assists">The assists the session plays under, or <see langword="null"/> for the unassisted game.</param>
    public SuperMetroidGame(
        SaveSlotStore saves,
        GameAudioOutput audioOutput,
        GameArtwork artwork,
        GameplayAssists? assists = null)
        : this(saves, assists, SuperMetroidGameHostServices.Default, new SharedRandom(), audioOutput, artwork)
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
    /// <param name="audioOutput">What the session's audio produces for its host.</param>
    /// <param name="artwork">The installed catalogs the game draws from.</param>
    internal SuperMetroidGame(
        SaveSlotStore saves,
        GameplayAssists? assists,
        SuperMetroidGameHostServices hostServices,
        SharedRandom random,
        GameAudioOutput audioOutput,
        GameArtwork artwork)
    {
        Session = new FrontendSession(saves, assists, hostServices, random, audioOutput, artwork);
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
    /// Copies the last update's interleaved stereo samples (<see cref="AudioSession.PcmSamplesPerFrame"/>) into host-owned
    /// storage: the game's sound mixed from the clips it was built with, or silence from a muted game.
    /// </summary>
    /// <remarks>
    /// The output is presentation only. Each update advances the session's sound acceptance clock and its sound-effect
    /// acknowledgements whatever the output, so gameplay is identical with audio enabled, disabled, or without an output
    /// device.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="destination"/> holds fewer than one update's samples.</exception>
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


    /// <summary>
    /// What <paramref name="phase"/>'s current step runs on, matched once from its need (#2554): a step that runs gameplay
    /// runs only on the session's runtime, and one in a room only once that runtime has loaded one.
    /// </summary>
    /// <exception cref="InvalidOperationException">The session has not loaded what the step runs on.</exception>
    internal PhaseGameplay MatchGameplay(IGamePhase phase)
    {
        Ensure.NotNull(phase);
        if (phase.Need == PhaseGameplayNeed.Nothing)
            return PhaseGameplay.Frontend.Instance;
        if (phase.Need == PhaseGameplayNeed.Runtime && Session.Runtime is { } runtime)
            return new PhaseGameplay.OfRuntime(runtime);
        if (phase.Need == PhaseGameplayNeed.LoadedRoom && Session.Runtime is { Room.Scope: { } room } roomRuntime)
            return new PhaseGameplay.InRoom(new LoadedGameplay(roomRuntime, room));
        throw new InvalidOperationException($"The {phase.Name} step runs on {phase.Need}, which the session has not loaded.");
    }

    /// <summary>Runs one update and returns its status.</summary>
    private FrontendFrame Step(ControllerButton controllerInput)
    {
        // The sound queues read whether the door transition has disabled sound effects from the door's phase itself as each
        // request is made (#2499); the phase's audio here chooses only the update's own audio processing.
        IGamePhase phase = Session.RunningPhase;
        PhaseGameplay gameplay = MatchGameplay(phase);
        GamePhaseAudio phaseAudio = phase.Audio;
        bool publishesHeldInputAfterPause = phaseAudio == GamePhaseAudio.PublishesHeldInputAfterPause;
        // The runtime whose message box was open as the update began, or none when no box was open.
        SuperMetroidRuntime? boxRuntime = Session.Runtime?.MessageBox.IsActive == true ? Session.Runtime : null;
        bool messageWasActive = boxRuntime is not null;
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
        bool musicRunsBeforePhase = runsAudioProcessing && !messageWasActive &&
            (!doorAudioUpdate || phaseAudio != GamePhaseAudio.DoorTransitionSoundsDisabled || !continuesDoorLoad);
        IReadOnlyList<AudioRequest> musicRequestsBeforePhase = musicRunsBeforePhase
            ? Session.audio.AdvanceMusicDispatch() : Array.Empty<AudioRequest>();
        // Frames pass on the frame counters while a door loads a new music data set
        // (IDoorMusicLoadFrames); nothing between this update's frame boundary and the load reads them.
        if (doorAudioUpdate && gameplay is PhaseGameplay.InRoom { Live.Runtime: var doorRuntime } &&
            musicRequestsBeforePhase.Any(request => request is AudioRequest.ChangeMusicDataSet))
            doorRuntime.Clock.CountDoorMusicLoad(Session.HostServices.DoorMusicLoadFrames);
        FrameNumber = checked(FrameNumber + 1);
        Session.display.BeginUpdate(FrameNumber);
        if (Session.Runtime is null)
            Session.frameClock.AdvanceAcceptedFrame();
        AdvanceMenuRandom(phase, gameplay);
        if (Session.Runtime is not null)
            Session.Runtime.RunningGameState = Session.GameState;
        if (phase is IFrontendPhase frontend && gameplay is PhaseGameplay.Frontend)
            frontend.Step(controllerInput);
        else if (phase is IUnattendedPhase unattended && gameplay is PhaseGameplay.Frontend)
            unattended.Step();
        else if (phase is IRuntimePhase runtimePhase && gameplay is PhaseGameplay.OfRuntime ofRuntime)
            runtimePhase.Step(controllerInput, ofRuntime.Runtime);
        else if (phase is IGameplayPhase gameplayPhase && gameplay is PhaseGameplay.InRoom inRoom)
            gameplayPhase.Step(controllerInput, inRoom.Live);
        else
            throw new InvalidOperationException($"The {phase.Name} step has no step on {phase.Need}.");

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
        if (boxRuntime is not null)
        {
            // A message-box update runs exactly the box's own audio processing.
            MessageBoxFrameAudio boxAudio = boxRuntime.MessageBox.LastFrameAudio;
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
        IReadOnlyList<AudioRequest> audioRequests = musicRequestsBeforePhase.Count == 0 ? trailingAudioRequests
            : [.. musicRequestsBeforePhase, .. trailingAudioRequests];
        // The sound acceptance clock acts on this update's requests during the update's own
        // samples; its acknowledgements become visible to the next update's sound queues.
        AudioFrameEvents audio = new(Session.audioSession.Advance(audioRequests));
        // The pause steps publish held input after they return, though Samus's drawing step, which ends by publishing
        // it, did not run; only the held input is published, and the press edge and the auto-jump timer stay as they
        // are.
        if (publishesHeldInputAfterPause && gameplay is PhaseGameplay.InRoom { Live.Runtime: var pausedRuntime })
            pausedRuntime.Player.Samus.InputSnapshot.PublishHeldInput(pausedRuntime.PlayerActions.Current);
        return new(Session.GameState, PhaseName, FrameNumber, audio);
    }


    /// <summary>
    /// Reads the current status without advancing the game-state loop. No update ran, so it publishes no audio; an update's
    /// audio events come only with the frame <see cref="Step"/> returns.
    /// </summary>
    public FrontendFrame CurrentFrame => new(Session.GameState, PhaseName, FrameNumber, new AudioFrameEvents([]));

    /// <summary>Human-readable active subphase for diagnostics, without changing state.</summary>
    private string PhaseName => Session.RunningPhase.Name;

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
        Samus = Session.Runtime is { Player.Samus: var samus } runtime
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
                Medium = samus.LiquidPhysics.MediumLastEntered,
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
    private bool PhaseIsWaitingOutDisplayFrame => Session.RunningPhase.IsWaitingOutDisplayFrame;

    /// <summary>
    /// Advances the random generator once per update through the title, file selection and
    /// loading. These phases run once per update and do not run gameplay, which advances it
    /// itself. Saves keep no copy of the generator.
    /// </summary>
    private void AdvanceMenuRandom(IGamePhase phase, PhaseGameplay gameplay)
    {
        Ensure.NotNull(phase);
        switch (phase.RandomStep)
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
            case PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance when gameplay is PhaseGameplay.InRoom { Live: var paused }:
                // The liquid wave and the statues' descent still advance on the update before the pause screen; a lava
                // or acid wave stirs the random number (SharedRandom.StirForLiquidWave).
                paused.Runtime.DoorTransition.AdvanceEffectsAndRandom(paused.Room, paused.Runtime.Player.Samus);
                break;
            case PhaseRandomStep.Advance:
                if (DisplayFrameWaitContinuation.DrawsRandom(PhaseIsWaitingOutDisplayFrame))
                    Session.Random.Generate();
                break;
            case PhaseRandomStep.RuntimeOwned:
                break;
            case PhaseRandomStep.LiquidWaveAndStatueDescentThenAdvance:
            default:
                throw new InvalidOperationException($"Undefined {nameof(PhaseRandomStep)} {phase.RandomStep} on {phase.Need}.");
        }
        // Gameplay advances the generator itself after its liquid wave and statue descent, so neither a gameplay update
        // nor an update that finishes a waiting step advances it here.
    }
}
