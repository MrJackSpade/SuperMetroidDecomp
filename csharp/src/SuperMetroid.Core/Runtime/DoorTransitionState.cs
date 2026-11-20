using SuperMetroid.Core.Input;
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
/// <see cref="SuperMetroid.Core.World.RoomLoader.LoadPendingDoorDestinationForTransition"/>; the transition runs the updates before and
/// after it. How the transition counts frames is
/// <see cref="DoorTransitionFrameCounting"/>'s, where its liquid wave and statue descent stop is
/// <see cref="LiquidWaveAndStatueDescentOrder"/>'s, how many updates it spends before the door's scroll starts is
/// <see cref="DoorDestinationSetupWait"/>'s, and how far loading has progressed on each scroll update is
/// <see cref="IRoomLoadProgress"/>'s.
/// </remarks>
internal sealed class DoorTransitionState
{
    /// <summary>
    /// The transition under way, which <see cref="Begin"/> creates with everything it holds and its last fade step
    /// completes, or the rest before the first door and after each.
    /// </summary>
    private DoorTransitionStage stage = DoorTransitionStage.Inactive;

    /// <summary>Gets the door transition's current stage.</summary>
    public DoorTransitionPhase Phase => stage.Phase;

    /// <summary>Gets whether the transition has begun and has not yet completed.</summary>
    public bool IsActive => stage is OrdinaryDoorTransition or OpeningDoorTransition;

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

    /// <summary>Samus has entered <paramref name="door"/>: the next transition to begin goes through it.</summary>
    /// <param name="door">The door the game decided the transition with.</param>
    /// <exception cref="InvalidOperationException">A door transition is already under way.</exception>
    public void Enter(DoorDefinition door)
    {
        Ensure.NotNull(door);
        if (IsActive)
            throw new InvalidOperationException("A door transition is already active.");
        stage = new DoorTransitionStage.DoorEntered(door, stage is DoorTransitionStage.DoorEntered previous ? previous.EnteredFrom : stage);
    }

    /// <summary>Starts the ordinary-room transition sequence through the door Samus entered.</summary>
    /// <param name="runtime">The runtime whose player, colour, and enemy state are prepared.</param>
    /// <param name="loaded">The runtime's loaded room, the transition's source.</param>
    /// <exception cref="InvalidOperationException">No door was entered since the last transition, or one is under way.</exception>
    public void Begin(SuperMetroidRuntime runtime, RoomOccupancy.Loaded loaded)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(loaded);
        if (stage is not DoorTransitionStage.DoorEntered entered)
            throw new InvalidOperationException(IsActive
                ? "A door transition is already active."
                : "A door transition goes through the door the game entered it with.");
        DoorDefinition door = entered.Door;

        SamusState samus = runtime.Player.Samus;
        runtime.Hud.EnableMinimapAfterDoorEntry();
        samus.Control.LockOutPlayer();
        runtime.DoorTransition.Passage.Start();
        var transition = new OrdinaryDoorTransition(
            door,
            sourceIsolated: loaded.Scope.Room.UsesIsolatedDoorTransition,
            destinationIsolated: runtime.RoomLoader.DestinationUsesIsolatedDoorTransition(door),
            samus.Position,
            new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, runtime.FadeStepCount));
        // Between two common rooms the fade keeps the HUD's item and minimap backgrounds lit as well.
        runtime.DoorFade.FadeOut(transition.FadeSteps, commonRooms: !transition.SourceIsolated && !transition.DestinationIsolated);
        stage = transition;
    }

    /// <summary>Advances exactly one update of the stage selected by <see cref="Phase"/>.</summary>
    /// <param name="runtime">The gameplay runtime.</param>
    /// <param name="loaded">The runtime's loaded room: the source room until the destination loads, then the destination.</param>
    /// <param name="loaderProgress">The room load's progress.</param>
    /// <param name="controllerInput">The controller input.</param>
    /// <param name="audio">The audio queues.</param>
    public void Step(
        SuperMetroidRuntime runtime,
        RoomOccupancy.Loaded loaded,
        AudioQueues audio,
        ControllerButton controllerInput,
        IRoomLoadProgress loaderProgress)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(audio);
        Ensure.NotNull(loaderProgress);
        if (stage is not (OrdinaryDoorTransition or OpeningDoorTransition))
            throw new InvalidOperationException("Door transition has not begun.");
        // A door transition carries the one Samus through both rooms.
        SamusState samus = runtime.Player.Samus;

        // The haze steps before the stage's work: the two fades name their fade, and every other update steps it plainly.
        CeresHazeState? haze = loaded.Scope.CeresHaze;
        if (stage.Phase == DoorTransitionPhase.FadeInDestinationRoom)
            haze?.StepWhileRoomFadesIn();
        else if (stage.Phase == DoorTransitionPhase.FadeOutSourceRoom)
            haze?.StepWhileRoomFadesOut();
        else
            haze?.Step();
        switch (stage)
        {
            case OpeningDoorTransition opening:
                StepScrollingIn(opening, runtime, loaded, audio, controllerInput, loaderProgress, samus);
                return;
            case OrdinaryDoorTransition ordinary:
                StepToDestination(ordinary, runtime, loaded, audio, controllerInput, loaderProgress, samus);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DoorTransitionStage)} {stage}.");
        }
    }

    /// <summary>Runs one update of the transition before the destination has loaded, up to its load.</summary>
    private void StepToDestination(OrdinaryDoorTransition transition, SuperMetroidRuntime runtime, RoomOccupancy.Loaded loaded, AudioQueues audio,
        ControllerButton controllerInput, IRoomLoadProgress loaderProgress, SamusState samus)
    {
        switch (transition.Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, loaded.Scope, samus);
                if (!audio.HasQueuedSounds)
                    transition.Enter(DoorTransitionPhase.FadeOutSourceRoom);
                break;

            case DoorTransitionPhase.FadeOutSourceRoom:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                // Each colour step is one update, with its own effects and random draw.
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                if (transition.FadeSteps.Step())
                {
                    runtime.DoorFade.Darken();
                    runtime.Sprites.BeginFrame();
                    runtime.Sprites.FinalizeFrame();
                    transition.Enter(DoorTransitionPhase.BeginDoorDisplay);
                }
                else
                {
                    runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, loaded.Scope, samus);
                }
                break;

            case DoorTransitionPhase.BeginDoorDisplay:
                // The door was captured by Begin; this update keeps its own frame, after
                // which the liquid wave and statue descent stop and the door's display takes over the screen.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                runtime.LiquidWaveAndStatueDescentOrder.StopForDoorLoading();
                runtime.RoomLoader.BeginDoorTransitionDisplay(transition.SourceIsolated, transition.DestinationIsolated);
                transition.Enter(DoorTransitionPhase.AlignSourceCamera);
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                if (runtime.RoomLoader.AlignDoorCameraOnePixel(transition.Door, loaded))
                {
                    transition.DestinationSetupWait = UpdateCountdown.Lasting(DoorDestinationSetupWait.Updates);
                    transition.Enter(DoorTransitionPhase.DestinationSetupWait);
                }
                break;

            case DoorTransitionPhase.DestinationSetupWait:
                // The destination's setup happens at once in its load; its wait's updates are
                // still whole updates of the transition, each with its own random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                if (transition.DestinationSetupWait.Count())
                    transition.Enter(DoorTransitionPhase.MoveToDoorScrollStart);
                break;

            case DoorTransitionPhase.MoveToDoorScrollStart:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                // The door's scroll starts from where Samus stands once the fade's updates have moved her, before the
                // scroll's first movement moves her again; that movement happens once, here.
                transition.SourceSamus = samus.Position;
                runtime.RoomLoader.ApplyDoorScrollingSetupMovement(transition.Door, loaded);
                transition.Enter(DoorTransitionPhase.PlaceSamusInDestination);
                break;

            case DoorTransitionPhase.PlaceSamusInDestination:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                runtime.RoomLoader.PlaceSamusForDestinationLoading(transition.Door, loaded);
                transition.Enter(DoorTransitionPhase.LoadDestinationRoom);
                break;

            case DoorTransitionPhase.LoadDestinationRoom:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                // This update sets up the destination's enemy graphics, music, projectiles, block animations,
                // room objects, FX and backgrounds; each following update moves the door's scroll one step, from
                // where it stood when loading began, until it finishes.
                DoorOpeningSourceView sourceView =
                    runtime.RoomLoader.LoadPendingDoorDestinationForTransition(transition.Door, transition.SourceSamus, loaded);
                runtime.LiquidWaveAndStatueDescentOrder.ResumeAfterRoomLoad();
                // The load re-asserted the door's gates over the destination's enemies:
                // elevator AI stays frozen through the final colour fade. The destination stays dark until its fade in
                // takes its first step.
                DoorOpeningScrollState scroll = runtime.RoomLoader.BeginDoorOpeningScroll(
                    transition.Door,
                    transition.SourceSamus,
                    sourceView,
                    loaderProgress,
                    completedLoadingScrollUpdates: DoorOpeningScrollDefinitions.LoadingScrollUpdates,
                    loaded);
                runtime.RoomLoader.StepDoorOpeningScroll(scroll, loaderProgress, loaded);
                if (runtime.Options.IconCancelEnabled)
                {
                    // Resetting the projectiles selects the beam again when Icon Cancel is
                    // on. This is the only gameplay effect of the Icon Cancel option.
                    samus.SelectedHudItem = SuperMetroid.Core.Samus.HudItem.Beam;
                    samus.AutoCancelledItem = SuperMetroid.Core.Samus.HudItem.Beam;
                }
                var fadeIn = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, runtime.FadeStepCount);
                runtime.DoorFade.FadeIn(fadeIn);
                // The transition goes on with the destination's opening scroll, at its wait for the scroll.
                stage = new OpeningDoorTransition(scroll, fadeIn);
                break;

            default:
                throw new InvalidOperationException($"Door transition phase {transition.Phase} does not run before the destination loads.");
        }
    }

    /// <summary>Runs one update of the transition once the destination has loaded: its opening scroll and fade in.</summary>
    private void StepScrollingIn(OpeningDoorTransition transition, SuperMetroidRuntime runtime, RoomOccupancy.Loaded loaded, AudioQueues audio,
        ControllerButton controllerInput, IRoomLoadProgress loaderProgress, SamusState samus)
    {
        switch (transition.Phase)
        {
            case DoorTransitionPhase.WaitForDoorOpeningScroll:
                // Every update finishes an empty sprite frame even while the transition
                // only waits on the door's scroll, so no faded enemy of the room left behind shows over the moving
                // destination doorway.
                runtime.RunBlankGameplayFrame(controllerInput);
                if (runtime.RoomLoader.StepDoorOpeningScroll(transition.Scroll, loaderProgress, loaded))
                {
                    // The room's objects run on the scroll's final update, before the frame
                    // that precedes the destination fade-in. Their setup can alter visible
                    // room state here; deferring them to the first ordinary gameplay frame
                    // exposes the room as it was before they ran.
                    runtime.Player.Colours.ShowVisor(SuperMetroid.Core.Samus.Colours.SamusShades.DoorCompletionVisor);
                    runtime.DoorFade.DoorOpened();
                    RoomObjectFramePhase.RunRoomObjectsForDoor(loaded.Scope, samus);
                    transition.Enter(DoorTransitionPhase.AlignSamusWithDoorway);
                }
                break;

            case DoorTransitionPhase.AlignSamusWithDoorway:
                // The transition waits one update after the room objects run. This continues the
                // same update's work, so it aligns X without another random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.RoomLoader.AlignSamusAfterDoorLoading(transition.Scroll);
                transition.Enter(DoorTransitionPhase.StepDestinationBlockAnimations);
                break;

            case DoorTransitionPhase.StepDestinationBlockAnimations:
                // LoadRoom already initialized the destination's block animations; the
                // transition gives them one explicit step before polling the music queue.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                loaded.Scope.StepBlockAnimations(samus, runtime.System);
                transition.Enter(DoorTransitionPhase.WaitForMusicQueue);
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                if (!audio.HasQueuedMusic)
                {
                    transition.Enter(DoorTransitionPhase.FinishDoorOpeningScroll);
                    // The destination's music track is selected here, before the final
                    // Samus nudge and colour fade, not during room creation.
                    if (!runtime.AttractDemo.IsAttractDemo)
                    {
                        audio.QueueRoomMusicTrack(loaded.Scope.Room.State.Music);
                    }
                }
                break;

            case DoorTransitionPhase.FinishDoorOpeningScroll:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                // Samus's nudges off the screen's edges and an elevator door's control setup apply only once the
                // opening scroll has finished and the music is queued, not during the visible scroll.
                runtime.RoomLoader.FinishDoorOpeningScroll(transition.Scroll, loaded);
                runtime.RoomLoader.EndDoorTransitionDisplay();
                // This update ends here; the first destination fade starts on the next.
                transition.Enter(DoorTransitionPhase.DrawFirstDestinationFrame);
                break;

            case DoorTransitionPhase.DrawFirstDestinationFrame:
            case DoorTransitionPhase.FadeInDestinationRoom:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(loaded.Scope, samus);
                // Every fade step runs the block animations, then the enemy and draw owners,
                // without Samus's movement and animation or the camera's scrolling.
                loaded.Scope.StepBlockAnimations(samus, runtime.System);
                runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Run, loaded.Scope, samus);
                transition.Enter(DoorTransitionPhase.FadeInDestinationRoom);
                // The destination fades in toward the colours its owners show, which enemy scripts running
                // during the fade change as their typed colour state changes (#1512).
                if (transition.FadeSteps.Step())
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
                    if (loaded.Scope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive &&
                        samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked)
                        samus.Control.Unlock();
                    stage = DoorTransitionStage.Complete;
                }
                break;

            default:
                throw new InvalidOperationException($"Door transition phase {transition.Phase} does not run once the destination has loaded.");
        }
    }
}
