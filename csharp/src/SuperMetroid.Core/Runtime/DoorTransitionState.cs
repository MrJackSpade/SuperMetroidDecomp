using SuperMetroid.Core.Assets;
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
/// Ordinary non-elevator state-$0B coroutine from <c>$82:E29E-$82:E76A</c>.
/// </summary>
/// <remarks>
/// Room construction remains in <see cref="SuperMetroid.Core.World.RoomLoader.LoadPendingDoorDestination"/>;
/// this owner restores the observable stages that surround it: three-library drain, shared
/// palette fade, source-camera alignment, black-screen opening scroll, music drain, final
/// nudge ownership, and destination palette fade-in.
/// </remarks>
internal sealed class DoorTransitionState
{
    /// <summary>The door's palette transition, which exists from the start of the door fade.</summary>
    [AllowNull]
    private GradualPaletteTransition PaletteTransition { get => field ?? throw new InvalidOperationException("The door palette transition has not started."); set; }
    /// <summary>The faded source colours, restored after atomic destination room construction.</summary>
    private ColourImage? fadedSource;
    /// <summary>Door header captured before the destination room replaces the source.</summary>
    private DoorDefinition? door;
    /// <summary>Samus's source-room position used to compute the final door-scroll placement.</summary>
    private FixedPosition<RoomPixels> sourceSamus;
    /// <summary>Whether the source room selects the isolated door transition, for the transition IRQ setup.</summary>
    private bool sourceIsolated;
    /// <summary>Whether the destination room selects the isolated door transition, for the transition IRQ setup.</summary>
    private bool destinationIsolated;

    /// <summary>Gets the currently selected native door-transition coroutine phase.</summary>
    public DoorTransitionPhase Phase { get; private set; } = DoorTransitionPhase.Inactive;

    /// <summary>Gets whether the transition has begun and has not yet completed.</summary>
    public bool IsActive => Phase is not DoorTransitionPhase.Inactive and not DoorTransitionPhase.Complete;

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
            ?? throw new InvalidOperationException("Door transition requires a source room header.");
        destinationIsolated = runtime.RoomLoader.PendingDoorDestinationUsesIsolatedDoorTransition;
        sourceSamus = runtime.Player.Samus.Position;
        PaletteTransition = new GradualPaletteTransition(
            BuildSourceFadeTarget(runtime),
            denominator: 12,
            runtime.EnemySession.GradualColorChange);
        fadedSource = null;
        Phase = DoorTransitionPhase.WaitForSoundQueues;
    }

    /// <summary>Advances exactly one coroutine function selected by <see cref="Phase"/>.</summary>
    public void Step(
        SuperMetroidRuntime runtime,
        AudioQueues audio,
        ControllerButton controllerInput,
        IRoomLoadProgress loaderProgress,
        Action? publishSoundWaitAudio = null)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(audio);
        Ensure.NotNull(loaderProgress);
        if (!IsActive)
            throw new InvalidOperationException("Door transition has not begun.");

        // Derive the native door-owned gate for resumed legacy snapshots as well.
        runtime.Room.LoadedScope.Enemies.RoomStatus.EnemyDoorTransitionActive = true;
        runtime.Room.LoadedScope.CeresHaze.Step(Phase switch
        {
            DoorTransitionPhase.FadeInDestinationPalette => CeresHazeRoomFade.FadeIn,
            DoorTransitionPhase.FadeOutSourcePalette => CeresHazeRoomFade.FadeOut,
            DoorTransitionPhase.Inactive => CeresHazeRoomFade.None,
            DoorTransitionPhase.WaitForSoundQueues => CeresHazeRoomFade.None,
            DoorTransitionPhase.LoadDoorHeader => CeresHazeRoomFade.None,
            DoorTransitionPhase.AlignSourceCamera => CeresHazeRoomFade.None,
            DoorTransitionPhase.FixDoorsMovingUp => CeresHazeRoomFade.None,
            DoorTransitionPhase.SetupNewRoom => CeresHazeRoomFade.None,
            DoorTransitionPhase.SetupScrolling => CeresHazeRoomFade.None,
            DoorTransitionPhase.PlaceSamusAndLoadTiles => CeresHazeRoomFade.None,
            DoorTransitionPhase.LoadMoreThingsAndOpenDoor => CeresHazeRoomFade.None,
            DoorTransitionPhase.WaitForDoorOpeningScroll => CeresHazeRoomFade.None,
            DoorTransitionPhase.HandleAnimatedTiles => CeresHazeRoomFade.None,
            DoorTransitionPhase.WaitForMusicQueue => CeresHazeRoomFade.None,
            DoorTransitionPhase.HandleTransition => CeresHazeRoomFade.None,
            DoorTransitionPhase.Complete => CeresHazeRoomFade.None,
            DoorTransitionPhase.DrawFirstDestinationFrame => CeresHazeRoomFade.None,
            DoorTransitionPhase.FinishDoorLoading => CeresHazeRoomFade.None,
            _ => throw new InvalidOperationException($"Unknown door transition phase {Phase}."),
        });
        switch (Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.DoorTransition.RunDoorSoundWaitFrame(controllerInput, runtime.Room.LoadedScope);
                publishSoundWaitAudio?.Invoke();
                if (!audio.HasQueuedSounds)
                    Phase = DoorTransitionPhase.FadeOutSourcePalette;
                break;

            case DoorTransitionPhase.FadeOutSourcePalette:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                // A palette step returns to MainGameLoop. Hardware stalls while it
                // executes are not additional dispatches, but this step is one.
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Run, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                if (PaletteTransition.Step())
                {
                    runtime.Sprites.BeginFrame();
                    runtime.Sprites.FinalizeFrame();
                    Phase = DoorTransitionPhase.LoadDoorHeader;
                }
                else
                {
                    runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Held, runtime.Room.LoadedScope);
                }
                break;

            case DoorTransitionPhase.LoadDoorHeader:
                // `$82:E2F7` parses the already selected bank-$83 header, disables HDMA,
                // and points the IRQ dispatcher at the door-scrolling handler. The typed
                // header was captured by Begin; retaining this separate call preserves the
                // coroutine boundary and its one accepted NMI.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Run, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                runtime.RoomLoader.BeginDoorTransitionIrqDisplay(sourceIsolated, destinationIsolated);
                Phase = DoorTransitionPhase.AlignSourceCamera;
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Skipped, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                if (runtime.RoomLoader.AlignPendingDoorCameraOnePixel())
                    Phase = DoorTransitionPhase.FixDoorsMovingUp;
                break;

            case DoorTransitionPhase.FixDoorsMovingUp:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Skipped, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                // `$80:AD1D` repairs the source room's tilemap ring for an upward door; with
                // backgrounds drawn from level data there is no ring, only this frame.
                Phase = DoorTransitionPhase.SetupNewRoom;
                break;

            case DoorTransitionPhase.SetupNewRoom:
                // Room/state/FX/level setup is atomic in LoadPendingDoorDestination, but
                // native exposes this function separately from scrolling and tile upload.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Skipped, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                runtime.RoomLoader.SelectDoorDestinationHudCharacterBase();
                Phase = DoorTransitionPhase.SetupScrolling;
                break;

            case DoorTransitionPhase.SetupScrolling:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Skipped, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                // Capture immediately before the setup call; source actors have run
                // during the fade. The atomic loader must not apply this step twice.
                var setupSamus = runtime.Player.Samus
                    ?? throw new InvalidOperationException("Door scrolling requires Samus.");
                sourceSamus = setupSamus.Position;
                runtime.RoomLoader.ApplyDoorScrollingSetupMovement();
                Phase = DoorTransitionPhase.PlaceSamusAndLoadTiles;
                break;

            case DoorTransitionPhase.PlaceSamusAndLoadTiles:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Skipped, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                runtime.RoomLoader.PlaceSamusForDoorTileLoading();
                Phase = DoorTransitionPhase.LoadMoreThingsAndOpenDoor;
                break;

            case DoorTransitionPhase.LoadMoreThingsAndOpenDoor:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Skipped, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                // LoadMoreThings initializes enemy graphics/music/projectiles/animtiles,
                // PLMs, FX, backgrounds, and then yields once per NMI until the IRQ raises
                // door_transition_flag bit $8000. The host room constructor performs that
                // setup atomically; its following calls now run the real coordinate path.
                fadedSource = runtime.Colours.Capture();
                runtime.RoomLoader.LoadPendingDoorDestinationForTransition(sourceSamus);
                // The load re-asserted the door's gates ($0795/$0797) over the destination's
                // enemies: elevator AI stays frozen through the final palette fade.
                ColourImage destinationTarget = runtime.Colours.Capture();
                fadedSource.Restore();
                // UpdateBeamTilesAndPalette ($90:ACCD) writes the loaded beam colours live during loading,
                // unlike the destination room, enemy and suit colours, which wait for the fade.
                ColourRun beam = runtime.Player.Colours.Beam;
                beam.CopyFrom(destinationTarget.Of(beam));
                runtime.RoomLoader.BeginDoorOpeningScroll(
                    door ?? throw new InvalidOperationException("Door header was not captured."),
                    sourceSamus,
                    loaderProgress,
                    completedLoadingIrqSteps: 1);
                runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress);
                if (runtime.Options.IconCancelEnabled && runtime.Player.Samus is { } samus)
                {
                    // ResetProjectileData checks `$09EA` after clearing all projectile
                    // slots. This is the only gameplay effect of the Icon Cancel option.
                    samus.SelectedHudItem = SuperMetroid.Core.Samus.HudItem.Beam;
                    samus.AutoCancelHudItemIndex = SuperMetroid.Core.Samus.HudItem.Beam;
                }
                PaletteTransition = new GradualPaletteTransition(
                    destinationTarget,
                    denominator: 12,
                    runtime.EnemySession.GradualColorChange);
                Phase = DoorTransitionPhase.WaitForDoorOpeningScroll;
                break;

            case DoorTransitionPhase.WaitForDoorOpeningScroll:
                // RunOneFrameOfGameInner always clears/finalizes the OAM build even when
                // this coroutine is only waiting on the room-loading IRQ. Publishing that
                // empty build prevents the faded source enemies from becoming black ghosts
                // over the incrementally moving destination doorway.
                runtime.RunBlankGameplayFrame(controllerInput);
                if (runtime.RoomLoader.StepDoorOpeningScroll(loaderProgress))
                {
                    // The cartridge calls PLM_Handler at $82:E53C on the final IRQ
                    // scrolling update, before the NMI that precedes destination fade-in.
                    // Setup lists can alter visible room state here; deferring them to the
                    // first ordinary gameplay frame exposes their pre-PLM state.
                    runtime.Player.Colours.Visor.Write(0, DoorTransitionColourDefinitions.DoorCompletionVisorGreen);
                    runtime.PlmPhase.RunDoorTransitionPlmHandler(runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope.BombProjectiles, runtime.Room.LoadedScope);
                    Phase = DoorTransitionPhase.FinishDoorLoading;
                }
                break;

            case DoorTransitionPhase.FinishDoorLoading:
                // $82:E540 yields once after PLM processing. This resumes the
                // same outer dispatch, so it aligns X without another RNG call.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.RoomLoader.AlignSamusAfterDoorLoading();
                Phase = DoorTransitionPhase.HandleAnimatedTiles;
                break;

            case DoorTransitionPhase.HandleAnimatedTiles:
                // LoadCartridgeRoom already initialized the destination animtile owner.
                // Native gives it one explicit call before polling the global music queue.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Run, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                // $82:E659 runs the animated-tile objects once in the destination room.
                runtime.RoomMains.RunAnimatedTilesObjectHandler(runtime.Room.LoadedScope);
                Phase = DoorTransitionPhase.WaitForMusicQueue;
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Run, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                if (!audio.HasQueuedMusic)
                {
                    Phase = DoorTransitionPhase.HandleTransition;
                    // $82:E670 calls LoadNewMusicTrackIfChanged here, before the
                    // final Samus nudge and palette fade, not during room creation.
                    if (!runtime.AttractDemo.IsAttractDemo)
                    {
                        CartridgeRoomState state = runtime.Room.ActiveRoom?.State
                            ?? throw new InvalidOperationException("Door music requires the destination room.");
                        audio.QueueRoomMusicTrack(state.Music);
                    }
                }
                break;

            case DoorTransitionPhase.HandleTransition:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Run, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                // `$82:E6A2` applies the narrow-door X/Y nudges only after the opening IRQ
                // and music queue are both complete. The atomic loader computed that exact
                // endpoint, which is restored here rather than during the visible scroll.
                runtime.RoomLoader.FinishDoorOpeningScroll();
                runtime.RoomLoader.EndDoorTransitionIrqDisplay();
                // E6A2 returns before the next E737 actor/fade dispatch. Falling
                // through ran Samus movement and accepted three NMIs in one update.
                Phase = DoorTransitionPhase.DrawFirstDestinationFrame;
                break;

            case DoorTransitionPhase.DrawFirstDestinationFrame:
            case DoorTransitionPhase.FadeInDestinationPalette:
                runtime.FrameBoundary.BeginAcceptedFrame(controllerInput);
                runtime.DoorTransition.AdvanceNonGameplayMainLoopRandom(perLineEffects: PerLineEffectProcessing.Run, runtime.Room.LoadedScope.Plms, runtime.Room.LoadedScope);
                // E737 runs the animated-tile objects, then enemy/draw owners, on every
                // fade step, without Samus's movement/animation handler or the camera streamer.
                runtime.RoomMains.RunAnimatedTilesObjectHandler(runtime.Room.LoadedScope);
                runtime.DoorTransition.DrawDoorTransitionActors(enemyProjectiles: DoorTransitionEnemyProjectiles.Run, runtime.Room.LoadedScope);
                Phase = DoorTransitionPhase.FadeInDestinationPalette;
                // Enemy instruction lists run after the initial destination palette copy.
                // Their target writes belong to this same fade, not a private dead buffer.
                runtime.Room.LoadedScope.Enemies.ConsumeTargetPaletteWrites(staged =>
                    PaletteTransition.Target.Of(staged.Run.In(runtime.Colours))[staged.Colour] = staged.Value);
                if (PaletteTransition.Step())
                {
                    // $82:E737 releases $0795 after EnemyMain and the last fade step.
                    // Movement resumes on the following gameplay frame, not this one.
                    runtime.Room.LoadedScope.Enemies.Elevator.ElevatorDoorTransitionActive = false;
                    runtime.Room.LoadedScope.Enemies.RoomStatus.EnemyDoorTransitionActive = false;
                    // Elevator arrival retains command zero's lock until the platform
                    // reaches rest; its actor, not the room fade, restores Samus movement.
                    // The elevatube door ASM also installs command zero. Native E737
                    // does not replace that handler: release only our temporary door
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

    /// <summary>Fades room colors while preserving HUD and applicable common CRE or escape-timer colors.</summary>
    private static ColourImage BuildSourceFadeTarget(SuperMetroidRuntime runtime)
    {
        ColourImage target = runtime.Colours.Capture();
        target.Fill(Bgr555.Black);

        // Keep HUD colors legible while the source room palette disappears.
        Keep(runtime.Colours, target, DoorTransitionColourDefinitions.Hud);

        CartridgeRoomHeader sourceRoom = runtime.Room.ActiveRoom ??
            throw new InvalidOperationException(
                "Door palette fade started without an active source room header.");
        if (!sourceRoom.UsesIsolatedDoorTransition && !runtime.RoomLoader.PendingDoorDestinationUsesIsolatedDoorTransition)
        {
            Keep(runtime.Colours, target, DoorTransitionColourDefinitions.CommonRoom);
            if (runtime.EscapeTimer.IsActive)
                Keep(runtime.Colours, target, DoorTransitionColourDefinitions.EscapeTimer);
        }
        return target;
    }

    /// <summary>Puts <paramref name="kept"/>' current colours into <paramref name="target"/>.</summary>
    private static void Keep(GameplayColours colours, ColourImage target, IReadOnlyList<IColourSetRun> kept)
    {
        foreach (IColourSetRun run in kept)
        {
            ColourRun live = run.In(colours);
            live.Colors.CopyTo(target.Of(live));
        }
    }

}
