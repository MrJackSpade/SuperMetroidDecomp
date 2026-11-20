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
    public bool IsActive => stage is OrdinaryDoorTransition;

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

    /// <summary>Captures the door Samus entered and starts the ordinary-room transition sequence.</summary>
    /// <param name="runtime">The runtime whose entered door, player, colour, and enemy state are prepared.</param>
    public void Begin(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        DoorDefinition door = runtime.Room.LoadedScope.RoomObjects.DoorContacts.EnteredDoor
            ?? throw new InvalidOperationException("A door transition requires a door Samus entered.");
        if (IsActive)
            throw new InvalidOperationException("A door transition is already active.");

        SamusState samus = runtime.Player.IntroducedSamus;
        runtime.Hud.EnableMinimapAfterDoorEntry();
        samus.Control.LockOutPlayer();
        runtime.DoorTransition.Passage.Start();
        var transition = new OrdinaryDoorTransition(
            door,
            sourceIsolated: runtime.Room.LoadedScope.Room.UsesIsolatedDoorTransition,
            destinationIsolated: runtime.RoomLoader.DestinationUsesIsolatedDoorTransition(door),
            samus.Position,
            new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, runtime.FadeStepCount));
        // Between two common rooms the fade keeps the HUD's item and minimap backgrounds lit as well.
        runtime.DoorFade.FadeOut(transition.FadeSteps, commonRooms: !transition.SourceIsolated && !transition.DestinationIsolated);
        stage = transition;
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
        if (stage is not OrdinaryDoorTransition transition)
            throw new InvalidOperationException("Door transition has not begun.");
        // A door transition carries the one Samus through both rooms.
        SamusState samus = runtime.Player.IntroducedSamus;

        // The haze steps before the stage's work: the two fades name their fade, and every other update steps it plainly.
        CeresHazeState? haze = runtime.Room.LoadedScope.CeresHaze;
        if (transition.Phase == DoorTransitionPhase.FadeInDestinationRoom)
            haze?.StepWhileRoomFadesIn();
        else if (transition.Phase == DoorTransitionPhase.FadeOutSourceRoom)
            haze?.StepWhileRoomFadesOut();
        else
            haze?.Step();
        switch (transition.Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, runtime.Room.LoadedScope, samus);
                if (!audio.HasQueuedSounds)
                    transition.Enter(DoorTransitionPhase.FadeOutSourceRoom);
                break;

            case DoorTransitionPhase.FadeOutSourceRoom:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                // Each colour step is one update, with its own effects and random draw.
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (transition.FadeSteps.Step())
                {
                    runtime.DoorFade.Darken();
                    runtime.Sprites.BeginFrame();
                    runtime.Sprites.FinalizeFrame();
                    transition.Enter(DoorTransitionPhase.BeginDoorDisplay);
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
                runtime.RoomLoader.BeginDoorTransitionDisplay(transition.SourceIsolated, transition.DestinationIsolated);
                transition.Enter(DoorTransitionPhase.AlignSourceCamera);
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (runtime.RoomLoader.AlignDoorCameraOnePixel(transition.Door))
                {
                    transition.DestinationSetupWait = UpdateCountdown.Lasting(DoorDestinationSetupWait.Updates);
                    transition.Enter(DoorTransitionPhase.DestinationSetupWait);
                }
                break;

            case DoorTransitionPhase.DestinationSetupWait:
                // The destination's setup happens at once in its load; its wait's updates are
                // still whole updates of the transition, each with its own random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (transition.DestinationSetupWait.Count())
                    transition.Enter(DoorTransitionPhase.MoveToDoorScrollStart);
                break;

            case DoorTransitionPhase.MoveToDoorScrollStart:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // The door's scroll starts from where Samus stands once the fade's updates have moved her, before the
                // scroll's first movement moves her again; that movement happens once, here.
                transition.SourceSamus = samus.Position;
                runtime.RoomLoader.ApplyDoorScrollingSetupMovement(transition.Door);
                transition.Enter(DoorTransitionPhase.PlaceSamusInDestination);
                break;

            case DoorTransitionPhase.PlaceSamusInDestination:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                runtime.RoomLoader.PlaceSamusForDestinationLoading(transition.Door);
                transition.Enter(DoorTransitionPhase.LoadDestinationRoom);
                break;

            case DoorTransitionPhase.LoadDestinationRoom:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // This update sets up the destination's enemy graphics, music, projectiles, block animations,
                // room objects, FX and backgrounds; each following update moves the door's scroll one step, from
                // where it stood when loading began, until it finishes.
                runtime.RoomLoader.LoadPendingDoorDestinationForTransition(transition.SourceSamus);
                runtime.LiquidWaveAndStatueDescentOrder.ResumeAfterRoomLoad();
                // The load re-asserted the door's gates over the destination's enemies:
                // elevator AI stays frozen through the final colour fade. The destination stays dark until its fade in
                // takes its first step.
                runtime.RoomLoader.BeginDoorOpeningScroll(
                    transition.Door,
                    transition.SourceSamus,
                    loaderProgress,
                    completedLoadingScrollUpdates: DoorOpeningScrollDefinitions.LoadingScrollUpdates);
                runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress);
                if (runtime.Options.IconCancelEnabled)
                {
                    // Resetting the projectiles selects the beam again when Icon Cancel is
                    // on. This is the only gameplay effect of the Icon Cancel option.
                    samus.SelectedHudItem = SuperMetroid.Core.Samus.HudItem.Beam;
                    samus.AutoCancelledItem = SuperMetroid.Core.Samus.HudItem.Beam;
                }
                transition.FadeSteps = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, runtime.FadeStepCount);
                runtime.DoorFade.FadeIn(transition.FadeSteps);
                transition.Enter(DoorTransitionPhase.WaitForDoorOpeningScroll);
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
                    transition.Enter(DoorTransitionPhase.AlignSamusWithDoorway);
                }
                break;

            case DoorTransitionPhase.AlignSamusWithDoorway:
                // The transition waits one update after the room objects run. This continues the
                // same update's work, so it aligns X without another random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.RoomLoader.AlignSamusAfterDoorLoading();
                transition.Enter(DoorTransitionPhase.StepDestinationBlockAnimations);
                break;

            case DoorTransitionPhase.StepDestinationBlockAnimations:
                // LoadRoom already initialized the destination's block animations; the
                // transition gives them one explicit step before polling the music queue.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                runtime.Room.LoadedScope.StepBlockAnimations(samus, runtime.System);
                transition.Enter(DoorTransitionPhase.WaitForMusicQueue);
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                if (!audio.HasQueuedMusic)
                {
                    transition.Enter(DoorTransitionPhase.FinishDoorOpeningScroll);
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
                transition.Enter(DoorTransitionPhase.DrawFirstDestinationFrame);
                break;

            case DoorTransitionPhase.DrawFirstDestinationFrame:
            case DoorTransitionPhase.FadeInDestinationRoom:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope, samus);
                // Every fade step runs the block animations, then the enemy and draw owners,
                // without Samus's movement and animation or the camera's scrolling.
                runtime.Room.LoadedScope.StepBlockAnimations(samus, runtime.System);
                runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Run, runtime.Room.LoadedScope, samus);
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
                    if (runtime.Room.LoadedScope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive &&
                        samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked)
                        samus.Control.Unlock();
                    stage = DoorTransitionStage.Complete;
                }
                break;

            case DoorTransitionPhase.Inactive:
            case DoorTransitionPhase.Complete:
            default:
                throw new InvalidOperationException($"Unknown door transition phase {transition.Phase}.");
        }
    }
}
