using SuperMetroid.Core.Input;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// An ordinary door transition, one stage per update: the sound drain, the source fade, the door's display and
/// camera alignment, the destination-setup wait, the destination's load and opening scroll, the music drain, the final
/// nudges and the destination fade-in.
/// </summary>
/// <remarks>
/// Room construction remains in <see cref="SuperMetroid.Core.World.RoomLoader.LoadPendingDoorDestination"/>;
/// this owner restores the observable stages that surround it. How the transition counts frames is
/// <see cref="DoorTransitionFrameCounting"/>'s, where its per-line effects stop is
/// <see cref="EffectPhaseRandomOrder"/>'s, how many updates it spends before the door's scroll starts is
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
    /// <param name="runtime">The runtime whose pending door, player, palette, and enemy state are prepared.</param>
    public void Begin(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        if (runtime.RoomLoader.PendingDoorTransition is null || runtime.Player.Samus is null)
            throw new InvalidOperationException("A door transition requires a pending door and Samus.");
        if (IsActive)
            throw new InvalidOperationException("A door transition is already active.");

        runtime.Hud.EnableMinimapAfterDoorEntry();
        runtime.Player.Samus.Control.LockHandlers();
        runtime.Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive = true;
        runtime.Room.LoadedScope.Enemies.RoomStatus.EnemyDoorTransitionActive = true;
        door = runtime.RoomLoader.PendingDoorTransition;
        sourceIsolated = runtime.Room.ActiveRoom?.UsesIsolatedDoorTransition
            ?? throw new InvalidOperationException("Door transition requires a source room.");
        destinationIsolated = runtime.RoomLoader.PendingDoorDestinationUsesIsolatedDoorTransition;
        sourceSamus = runtime.Player.Samus.Position;
        FadeSteps = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeDenominator, runtime.FadeStepCount);
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

        // Derive the door-owned enemy gate for resumed snapshots as well.
        runtime.Room.LoadedScope.Enemies.RoomStatus.EnemyDoorTransitionActive = true;
        runtime.Room.LoadedScope.CeresHaze.Step(Phase switch
        {
            DoorTransitionPhase.FadeInDestinationPalette => CeresHazeRoomFade.FadeIn,
            DoorTransitionPhase.FadeOutSourcePalette => CeresHazeRoomFade.FadeOut,
            DoorTransitionPhase.Inactive => CeresHazeRoomFade.None,
            DoorTransitionPhase.WaitForSoundQueues => CeresHazeRoomFade.None,
            DoorTransitionPhase.BeginDoorDisplay => CeresHazeRoomFade.None,
            DoorTransitionPhase.AlignSourceCamera => CeresHazeRoomFade.None,
            DoorTransitionPhase.DestinationSetupWait => CeresHazeRoomFade.None,
            DoorTransitionPhase.MoveToDoorScrollStart => CeresHazeRoomFade.None,
            DoorTransitionPhase.PlaceSamusInDestination => CeresHazeRoomFade.None,
            DoorTransitionPhase.LoadDestinationRoom => CeresHazeRoomFade.None,
            DoorTransitionPhase.WaitForDoorOpeningScroll => CeresHazeRoomFade.None,
            DoorTransitionPhase.AlignSamusWithDoorway => CeresHazeRoomFade.None,
            DoorTransitionPhase.StepDestinationAnimatedTiles => CeresHazeRoomFade.None,
            DoorTransitionPhase.WaitForMusicQueue => CeresHazeRoomFade.None,
            DoorTransitionPhase.FinishDoorOpeningScroll => CeresHazeRoomFade.None,
            DoorTransitionPhase.DrawFirstDestinationFrame => CeresHazeRoomFade.None,
            DoorTransitionPhase.Complete => CeresHazeRoomFade.None,
            _ => throw new InvalidOperationException($"Unknown door transition phase {Phase}."),
        });
        switch (Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, runtime.Room.LoadedScope);
                if (!audio.HasQueuedSounds)
                    Phase = DoorTransitionPhase.FadeOutSourcePalette;
                break;

            case DoorTransitionPhase.FadeOutSourcePalette:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                // Each palette step is one update, with its own effects and random draw.
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                if (FadeSteps.Step())
                {
                    runtime.DoorFade.Darken();
                    runtime.Sprites.BeginFrame();
                    runtime.Sprites.FinalizeFrame();
                    Phase = DoorTransitionPhase.BeginDoorDisplay;
                }
                else
                {
                    runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, runtime.Room.LoadedScope);
                }
                break;

            case DoorTransitionPhase.BeginDoorDisplay:
                // The door was captured by Begin; this update keeps its own frame, after
                // which the per-line effects stop and the door's display takes over the screen.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                runtime.EffectPhaseOrder.StopForDoorLoading();
                runtime.RoomLoader.BeginDoorTransitionDisplay(sourceIsolated, destinationIsolated);
                Phase = DoorTransitionPhase.AlignSourceCamera;
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
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
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                destinationSetupWait = destinationSetupWait.Spend();
                if (destinationSetupWait.IsOver)
                    Phase = DoorTransitionPhase.MoveToDoorScrollStart;
                break;

            case DoorTransitionPhase.MoveToDoorScrollStart:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                // Capture immediately before the setup call; source actors have run
                // during the fade. The atomic loader must not apply this step twice.
                var setupSamus = runtime.Player.Samus
                    ?? throw new InvalidOperationException("Door scrolling requires Samus.");
                sourceSamus = setupSamus.Position;
                runtime.RoomLoader.ApplyDoorScrollingSetupMovement();
                Phase = DoorTransitionPhase.PlaceSamusInDestination;
                break;

            case DoorTransitionPhase.PlaceSamusInDestination:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                runtime.RoomLoader.PlaceSamusForDoorTileLoading();
                Phase = DoorTransitionPhase.LoadDestinationRoom;
                break;

            case DoorTransitionPhase.LoadDestinationRoom:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                // Loading initializes enemy graphics, music, projectiles, block animations,
                // room objects, FX and backgrounds, and then waits one update at a time until the
                // door's scroll finishes. The room constructor performs that setup
                // atomically; the following updates run the scroll's real coordinate path.
                runtime.RoomLoader.LoadPendingDoorDestinationForTransition(sourceSamus);
                // The destination's load writes the entering beam's colours into the dark display's.
                runtime.DoorFade.WriteSamusColours(runtime.Player.Colours.Beams);
                runtime.EffectPhaseOrder.ResumeAfterRoomLoad();
                // The load re-asserted the door's gates over the destination's enemies:
                // elevator AI stays frozen through the final palette fade. The destination stays dark until its fade in
                // takes its first step.
                runtime.RoomLoader.BeginDoorOpeningScroll(
                    door ?? throw new InvalidOperationException("The transition's door was not captured."),
                    sourceSamus,
                    loaderProgress,
                    completedLoadingScrollUpdates: 1);
                runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress);
                if (runtime.Options.IconCancelEnabled && runtime.Player.Samus is { } samus)
                {
                    // Resetting the projectiles selects the beam again when Icon Cancel is
                    // on. This is the only gameplay effect of the Icon Cancel option.
                    samus.SelectedHudItem = SuperMetroid.Core.Samus.HudItem.Beam;
                    samus.AutoCancelHudItemIndex = SuperMetroid.Core.Samus.HudItem.Beam;
                }
                FadeSteps = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeDenominator, runtime.FadeStepCount);
                runtime.DoorFade.FadeIn(FadeSteps);
                Phase = DoorTransitionPhase.WaitForDoorOpeningScroll;
                break;

            case DoorTransitionPhase.WaitForDoorOpeningScroll:
                // Every update finishes an empty sprite frame even while the transition
                // only waits on the door's scroll. Publishing that empty frame prevents the
                // faded source enemies from becoming black ghosts over the moving
                // destination doorway.
                runtime.RunBlankGameplayFrame(controllerInput);
                if (runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress))
                {
                    // The room's objects run on the scroll's final update, before the frame
                    // that precedes the destination fade-in. Their setup can alter visible
                    // room state here; deferring them to the first ordinary gameplay frame
                    // exposes the room as it was before they ran.
                    runtime.Player.Colours.ShowVisor(SuperMetroid.Core.Samus.Palettes.SamusShades.DoorCompletionVisor);
                    runtime.DoorFade.WriteSamusColours(SuperMetroid.Core.Samus.Palettes.SamusShades.DoorCompletionVisor);
                    runtime.RoomObjectPhase.RunDoorTransitionObjectHandler(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope.BombProjectiles, runtime.Room.LoadedScope);
                    Phase = DoorTransitionPhase.AlignSamusWithDoorway;
                }
                break;

            case DoorTransitionPhase.AlignSamusWithDoorway:
                // The transition waits one update after the room objects run. This continues the
                // same update's work, so it aligns X without another random draw.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.RoomLoader.AlignSamusAfterDoorLoading();
                Phase = DoorTransitionPhase.StepDestinationAnimatedTiles;
                break;

            case DoorTransitionPhase.StepDestinationAnimatedTiles:
                // LoadRoom already initialized the destination's block animations; the
                // transition gives them one explicit step before polling the music queue.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                runtime.RoomMains.RunAnimatedTilesObjectHandler(runtime.Room.LoadedScope);
                Phase = DoorTransitionPhase.WaitForMusicQueue;
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                if (!audio.HasQueuedMusic)
                {
                    Phase = DoorTransitionPhase.FinishDoorOpeningScroll;
                    // The destination's music track is selected here, before the final
                    // Samus nudge and palette fade, not during room creation.
                    if (!runtime.AttractDemo.IsAttractDemo)
                    {
                        RoomStateSettings state = runtime.Room.ActiveRoom?.State
                            ?? throw new InvalidOperationException("Door music requires the destination room.");
                        audio.QueueRoomMusicTrack(state.Music);
                    }
                }
                break;

            case DoorTransitionPhase.FinishDoorOpeningScroll:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                // The narrow-door nudges apply only after the opening scroll and the music
                // queue are both complete. The atomic loader computed that exact endpoint,
                // which is restored here rather than during the visible scroll.
                runtime.RoomLoader.FinishDoorOpeningScroll();
                runtime.RoomLoader.EndDoorTransitionDisplay();
                // This update ends before the first destination fade. Falling through ran
                // Samus movement and counted three frames in one update.
                Phase = DoorTransitionPhase.DrawFirstDestinationFrame;
                break;

            case DoorTransitionPhase.DrawFirstDestinationFrame:
            case DoorTransitionPhase.FadeInDestinationPalette:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceEffectsAndRandom(runtime.Room.LoadedScope.RoomObjects, runtime.Room.LoadedScope);
                // Every fade step runs the block animations, then the enemy and draw owners,
                // without Samus's movement and animation or the camera's scrolling.
                runtime.RoomMains.RunAnimatedTilesObjectHandler(runtime.Room.LoadedScope);
                runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Run, runtime.Room.LoadedScope);
                Phase = DoorTransitionPhase.FadeInDestinationPalette;
                // The destination fades in toward the colours its owners show, which enemy scripts running
                // during the fade change as their typed colour state changes (#1512).
                if (FadeSteps.Step())
                {
                    runtime.DoorFade.End();
                    // The last fade step releases the door's enemy gate after the enemies run.
                    // Movement resumes on the following gameplay frame, not this one.
                    runtime.Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive = false;
                    runtime.Room.LoadedScope.Enemies.RoomStatus.EnemyDoorTransitionActive = false;
                    // Elevator arrival retains the elevator's lock until the platform
                    // reaches rest; its actor, not the room fade, restores Samus movement.
                    // The elevatube's door setup also installs that lock. The fade
                    // does not replace that handler: release only the temporary door
                    // input gate, never a stationary script's movement ownership.
                    if (runtime.Room.LoadedScope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Inactive &&
                        runtime.Player.Samus is { } arrivingSamus && arrivingSamus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked)
                        arrivingSamus.Control.Unlock();
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
