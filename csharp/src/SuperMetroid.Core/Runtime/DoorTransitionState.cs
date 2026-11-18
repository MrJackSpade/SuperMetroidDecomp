using SuperMetroid.Core.Input;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// An ordinary door transition, one stage per update: the sound drain, the source fade, the door's display and
/// camera alignment, the destination-setup wait, the destination's load and opening scroll, the music drain, the final
/// nudges and the destination fade-in.
/// </summary>
/// <remarks>
/// The destination room is set up on its loading update by
/// <see cref="SuperMetroid.Core.World.RoomLoader.LoadPendingDoorDestination"/>; the transition runs the updates before and
/// after it. How the transition counts frames is
/// <see cref="DoorTransitionFrameCounting"/>'s, where its liquid wave and statue descent stop is
/// <see cref="LiquidWaveAndStatueDescentOrder"/>'s, how many updates it spends before the door's scroll starts is
/// <see cref="DoorDestinationSetupWait"/>'s, and how far loading has progressed on each scroll update is
/// <see cref="IRoomLoadProgress"/>'s.
/// </remarks>
internal sealed class DoorTransitionState
{
    /// <summary>
    /// The steps of the door's current fade on the shared step counter, which exist from the start of the door fade: the
    /// source room's fade out, then the destination's fade in (#1512). What the fade shows is the runtime's
    /// <see cref="DoorFade"/>.
    /// </summary>
    [AllowNull]
    private GradualFadeSteps FadeSteps { get => field ?? throw new InvalidOperationException("The door fade has not started."); set; }
    /// <summary>Door definition captured before the destination room replaces the source.</summary>
    private DoorDefinition? door;
    /// <summary>Samus's source-room position used to compute the final door-scroll placement.</summary>
    private FixedPosition<RoomPixels> sourceSamus;
    /// <summary>Whether the source room selects the isolated door transition, for the transition's display.</summary>
    private bool sourceIsolated;
    /// <summary>Whether the destination room selects the isolated door transition, for the transition's display.</summary>
    private bool destinationIsolated;
    /// <summary>The destination-setup wait, started once the source camera has aligned.</summary>
    private DoorDestinationSetupWait destinationSetupWait;

    /// <summary>Gets the door transition's current stage.</summary>
    public DoorTransitionPhase Phase { get; private set; } = DoorTransitionPhase.Inactive;

    /// <summary>Gets whether the transition has begun and has not yet completed.</summary>
    public bool IsActive => Phase is not DoorTransitionPhase.Inactive and not DoorTransitionPhase.Complete;

    /// <summary>
    /// Whether the next update continues the destination's load from the update before, which began it on
    /// <see cref="DoorTransitionPhase.LoadDestinationRoom"/> and runs on through the door's scroll to Samus's doorway
    /// alignment as one step: such an update starts without the update's music processing while the transition's sounds
    /// are disabled.
    /// </summary>
    public bool ContinuesDestinationLoad =>
        Phase is DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.AlignSamusWithDoorway;

    /// <summary>
    /// Whether the next update leaves the destination's load still running when it ends, which it does from the load's
    /// first update through the door's scroll: such an update ends without the update's sound processing.
    /// </summary>
    public bool LeavesDestinationLoadRunning =>
        Phase is DoorTransitionPhase.LoadDestinationRoom or DoorTransitionPhase.WaitForDoorOpeningScroll;

    /// <summary>Captures the pending door and starts the ordinary-room transition sequence.</summary>
    /// <param name="runtime">The runtime whose pending door, player, colour, and enemy state are prepared.</param>
    public void Begin(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        if (runtime.RoomLoader.PendingDoorTransition is null)
            throw new InvalidOperationException("A door transition requires a pending door.");
        if (IsActive)
            throw new InvalidOperationException("A door transition is already active.");

        SamusState samus = runtime.Player.IntroducedSamus;
        runtime.Hud.EnableMinimapAfterDoorEntry();
        samus.Control.LockOutPlayer();
        runtime.DoorTransition.Passage.Start();
        door =runtime.RoomLoader.PendingDoorTransition;
        sourceIsolated = runtime.Room.LoadedScope.Room.UsesIsolatedDoorTransition;
        destinationIsolated = runtime.RoomLoader.PendingDoorDestinationUsesIsolatedDoorTransition;
        sourceSamus = samus.Position;
        FadeSteps = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, runtime.FadeStepCount);
        // Between two common rooms the fade keeps the HUD's item and minimap backgrounds lit as well.
        runtime.DoorFade.FadeOut(FadeSteps, commonRooms: !sourceIsolated && !destinationIsolated);
        Phase = DoorTransitionPhase.WaitForSoundQueues;
    }

    /// <summary>Advances exactly one update of the stage selected by <see cref="Phase"/>.</summary>
    public void Step(
        SuperMetroidRuntime runtime,
        AudioQueues audio,
        ControllerButton controllerInput,
        IRoomLoadProgress loaderProgress)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(audio);
        Ensure.NotNull(loaderProgress);
        if (!IsActive)
            throw new InvalidOperationException("Door transition has not begun.");
        // A door transition carries the one Samus through both rooms.
        SamusState samus = runtime.Player.IntroducedSamus;

        // The haze steps before the stage's work: the two fades name their fade, and every other update steps it plainly.
        CeresHazeState? haze = runtime.Room.LoadedScope.CeresHaze;
        if (Phase == DoorTransitionPhase.FadeInDestinationRoom)
            haze?.StepWhileRoomFadesIn();
        else if (Phase == DoorTransitionPhase.FadeOutSourceRoom)
            haze?.StepWhileRoomFadesOut();
        else
            haze?.Step();
        switch (Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, runtime.Room.LoadedScope, samus);
                if (!audio.HasQueuedSounds)
                    Phase = DoorTransitionPhase.FadeOutSourceRoom;
                break;

            case DoorTransitionPhase.FadeOutSourceRoom:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                // Each colour step is one update, with its own effects and random draw.
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (FadeSteps.Step())
                {
                    runtime.DoorFade.Darken();
                    runtime.Sprites.BeginFrame();
                    runtime.Sprites.FinalizeFrame();
                    Phase = DoorTransitionPhase.BeginDoorDisplay;
                }
                else
                {
                    runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, runtime.Room.LoadedScope, samus);
                }
                break;

            case DoorTransitionPhase.BeginDoorDisplay:
                // The door was captured by Begin; this update keeps its own frame, after
                // which the liquid wave and statue descent stop and the door's display takes over the screen.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                runtime.LiquidWaveAndStatueDescentOrder.StopForDoorLoading();
                runtime.RoomLoader.BeginDoorTransitionDisplay(sourceIsolated, destinationIsolated);
                Phase = DoorTransitionPhase.AlignSourceCamera;
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (runtime.RoomLoader.AlignPendingDoorCameraOnePixel())
                {
                    destinationSetupWait = DoorDestinationSetupWait.Started;
                    Phase = DoorTransitionPhase.DestinationSetupWait;
                }
                break;

            case DoorTransitionPhase.DestinationSetupWait:
                // The destination's setup happens at once in its load; its wait's updates are
                // still whole updates of the transition, each with its own random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                destinationSetupWait = destinationSetupWait.Spend();
                if (destinationSetupWait.IsOver)
                    Phase = DoorTransitionPhase.MoveToDoorScrollStart;
                break;

            case DoorTransitionPhase.MoveToDoorScrollStart:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // The door's scroll starts from where Samus stands once the fade's updates have moved her, before the
                // scroll's first movement moves her again; that movement happens once, here.
                sourceSamus = samus.Position;
                runtime.RoomLoader.ApplyDoorScrollingSetupMovement();
                Phase = DoorTransitionPhase.PlaceSamusInDestination;
                break;

            case DoorTransitionPhase.PlaceSamusInDestination:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                runtime.RoomLoader.PlaceSamusForDestinationLoading();
                Phase = DoorTransitionPhase.LoadDestinationRoom;
                break;

            case DoorTransitionPhase.LoadDestinationRoom:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // This update sets up the destination's enemy graphics, music, projectiles, block animations,
                // room objects, FX and backgrounds; each following update moves the door's scroll one step, from
                // where it stood when loading began, until it finishes.
                runtime.RoomLoader.LoadPendingDoorDestinationForTransition(sourceSamus);
                runtime.LiquidWaveAndStatueDescentOrder.ResumeAfterRoomLoad();
                // The load re-asserted the door's gates over the destination's enemies:
                // elevator AI stays frozen through the final colour fade. The destination stays dark until its fade in
                // takes its first step.
                runtime.RoomLoader.BeginDoorOpeningScroll(
                    door ?? throw new InvalidOperationException("The transition's door was not captured."),
                    sourceSamus,
                    loaderProgress,
                    completedLoadingScrollUpdates: 1);
                runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress);
                if (runtime.Options.IconCancelEnabled)
                {
                    // Resetting the projectiles selects the beam again when Icon Cancel is
                    // on. This is the only gameplay effect of the Icon Cancel option.
                    samus.SelectedHudItem = SuperMetroid.Core.Samus.HudItem.Beam;
                    samus.AutoCancelledItem = SuperMetroid.Core.Samus.HudItem.Beam;
                }
                FadeSteps = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, runtime.FadeStepCount);
                runtime.DoorFade.FadeIn(FadeSteps);
                Phase = DoorTransitionPhase.WaitForDoorOpeningScroll;
                break;

            case DoorTransitionPhase.WaitForDoorOpeningScroll:
                // Every update finishes an empty sprite frame even while the transition
                // only waits on the door's scroll, so no faded enemy of the room left behind shows over the moving
                // destination doorway.
                runtime.RunBlankGameplayFrame(controllerInput);
                if (runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress))
                {
                    // The room's objects run on the scroll's final update, before the frame
                    // that precedes the destination fade-in. Their setup can alter visible
                    // room state here; deferring them to the first ordinary gameplay frame
                    // exposes the room as it was before they ran.
                    runtime.Player.Colours.ShowVisor(SuperMetroid.Core.Samus.Colours.SamusShades.DoorCompletionVisor);
                    runtime.DoorFade.DoorOpened();
                    RoomObjectFramePhase.RunRoomObjectsForDoor(runtime.Room.LoadedScope, samus);
                    Phase = DoorTransitionPhase.AlignSamusWithDoorway;
                }
                break;

            case DoorTransitionPhase.AlignSamusWithDoorway:
                // The transition waits one update after the room objects run. This continues the
                // same update's work, so it aligns X without another random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.RoomLoader.AlignSamusAfterDoorLoading();
                Phase = DoorTransitionPhase.StepDestinationBlockAnimations;
                break;

            case DoorTransitionPhase.StepDestinationBlockAnimations:
                // LoadRoom already initialized the destination's block animations; the
                // transition gives them one explicit step before polling the music queue.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                runtime.RoomMains.RunBlockAnimations(runtime.Room.LoadedScope, samus);
                Phase = DoorTransitionPhase.WaitForMusicQueue;
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (!audio.HasQueuedMusic)
                {
                    Phase = DoorTransitionPhase.FinishDoorOpeningScroll;
                    // The destination's music track is selected here, before the final
                    // Samus nudge and colour fade, not during room creation.
                    if (!runtime.AttractDemo.IsAttractDemo)
                    {
                        audio.QueueRoomMusicTrack(runtime.Room.LoadedScope.Room.State.Music);
                    }
                }
                break;

            case DoorTransitionPhase.FinishDoorOpeningScroll:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // Samus's nudges off the screen's edges and an elevator door's control setup apply only once the
                // opening scroll has finished and the music is queued, not during the visible scroll.
                runtime.RoomLoader.FinishDoorOpeningScroll();
                runtime.RoomLoader.EndDoorTransitionDisplay();
                // This update ends here; the first destination fade starts on the next.
                Phase = DoorTransitionPhase.DrawFirstDestinationFrame;
                break;

            case DoorTransitionPhase.DrawFirstDestinationFrame:
            case DoorTransitionPhase.FadeInDestinationRoom:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // Every fade step runs the block animations, then the enemy and draw owners,
                // without Samus's movement and animation or the camera's scrolling.
                runtime.RoomMains.RunBlockAnimations(runtime.Room.LoadedScope, samus);
                runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Run, runtime.Room.LoadedScope, samus);
                Phase = DoorTransitionPhase.FadeInDestinationRoom;
                // The destination fades in toward the colours its owners show, which enemy scripts running
                // during the fade change as their typed colour state changes (#1512).
                if (FadeSteps.Step())
                {
                    runtime.DoorFade.End();
                    // The last fade step finishes Samus's passage after the enemies run.
                    // Movement resumes on the following gameplay frame, not this one.
                    runtime.DoorTransition.Passage.Finish();
                    // Elevator arrival retains the elevator's lock until the platform
                    // reaches rest; its actor, not the room fade, restores Samus movement.
                    // The elevatube's door setup also installs that lock. The fade
                    // does not replace that lock: release only the temporary door
                    // input gate, never a stationary script's movement ownership.
                    if (runtime.Room.LoadedScope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive &&
                        samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked)
                        samus.Control.Unlock();
                    Phase = DoorTransitionPhase.Complete;
                }
                break;

            case DoorTransitionPhase.Inactive:
            case DoorTransitionPhase.Complete:
            default:
                throw new InvalidOperationException($"Unknown door transition phase {Phase}.");
        }
    }
}
