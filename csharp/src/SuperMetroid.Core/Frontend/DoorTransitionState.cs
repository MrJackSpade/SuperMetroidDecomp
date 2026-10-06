using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Ordinary non-elevator state-$0B coroutine from <c>$82:E29E-$82:E76A</c>.
/// </summary>
/// <remarks>
/// Room construction remains in <see cref="SuperMetroidRuntime.LoadPendingDoorDestination"/>;
/// this owner restores the observable stages that surround it: three-library drain, shared
/// palette fade, source-camera alignment, black-screen opening scroll, music drain, final
/// nudge ownership, and destination palette fade-in.
/// </remarks>
public sealed class DoorTransitionState
{
    private CartridgePaletteTransition? paletteTransition;
    private ushort[]? fadedSourcePalette;
    private CartridgeDoorHeader? door;
    private uint sourceSamusXFixed;
    private uint sourceSamusYFixed;
    private byte sourceCreBitset;
    private byte destinationCreBitset;

    public DoorTransitionPhase Phase { get; private set; } = DoorTransitionPhase.Inactive;

    public bool IsActive => Phase is not DoorTransitionPhase.Inactive and not DoorTransitionPhase.Complete;

    public void Begin(SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        if (runtime.PendingDoorTransition is null || runtime.Samus is null)
            throw new InvalidOperationException("A door transition requires a pending door and Samus.");
        if (IsActive)
            throw new InvalidOperationException("A door transition is already active.");

        runtime.Samus.InputLocked = true;
        runtime.Enemies.ElevatorDoorTransitionActive = true;
        door = runtime.PendingDoorTransition;
        sourceCreBitset = runtime.ActiveRoom?.CreBitset
            ?? throw new InvalidOperationException("Door transition requires a source room header.");
        destinationCreBitset = runtime.PendingDoorDestinationCreBitset;
        sourceSamusXFixed = runtime.Samus.Kinematics.XFixed;
        sourceSamusYFixed = runtime.Samus.Kinematics.YFixed;
        paletteTransition = new CartridgePaletteTransition(
            BuildSourceFadeTarget(runtime),
            denominator: 12);
        fadedSourcePalette = null;
        Phase = DoorTransitionPhase.WaitForSoundQueues;
    }

    /// <summary>Advances exactly one coroutine function selected by <see cref="Phase"/>.</summary>
    public void Step(
        SuperMetroidRuntime runtime,
        CartridgeAudioState audio,
        ushort controllerInput,
        Func<ushort>? queueEchoSound = null,
        Action? publishSoundWaitAudio = null)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(audio);
        if (!IsActive)
            throw new InvalidOperationException("Door transition has not begun.");

        runtime.CeresHaze.Step(
            roomFadeIn: Phase == DoorTransitionPhase.FadeInDestinationPalette,
            roomFadeOut: Phase == DoorTransitionPhase.FadeOutSourcePalette);
        switch (Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.RunDoorSoundWaitFrame(controllerInput);
                publishSoundWaitAudio?.Invoke();
                if (!audio.HasQueuedSounds)
                    Phase = DoorTransitionPhase.FadeOutSourcePalette;
                break;

            case DoorTransitionPhase.FadeOutSourcePalette:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                // A palette step returns to MainGameLoop. Hardware stalls while it
                // executes are not additional dispatches, but this step is one.
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: true);
                if (paletteTransition!.Step(runtime.Cgram))
                {
                    runtime.Oam.BeginFrame();
                    runtime.Oam.FinalizeFrame();
                    Phase = DoorTransitionPhase.LoadDoorHeader;
                }
                else
                {
                    runtime.DrawDoorTransitionActors();
                }
                break;

            case DoorTransitionPhase.LoadDoorHeader:
                // `$82:E2F7` parses the already selected bank-$83 header, disables HDMA,
                // and points the IRQ dispatcher at the door-scrolling handler. The typed
                // header was captured by Begin; retaining this separate call preserves the
                // coroutine boundary and its one accepted NMI.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: true);
                runtime.BeginDoorTransitionIrqDisplay(sourceCreBitset, destinationCreBitset);
                Phase = DoorTransitionPhase.AlignSourceCamera;
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: false);
                if (runtime.AlignPendingDoorCameraOnePixel())
                    Phase = DoorTransitionPhase.FixDoorsMovingUp;
                break;

            case DoorTransitionPhase.FixDoorsMovingUp:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: false);
                runtime.FixPendingDoorTilesMovingUp();
                Phase = DoorTransitionPhase.SetupNewRoom;
                break;

            case DoorTransitionPhase.SetupNewRoom:
                // Room/state/FX/level setup is atomic in LoadPendingDoorDestination, but
                // native exposes this function separately from scrolling and tile upload.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: false);
                Phase = DoorTransitionPhase.SetupScrolling;
                break;

            case DoorTransitionPhase.SetupScrolling:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: false);
                // Capture immediately before the setup call; source actors have run
                // during the fade. The atomic loader must not apply this step twice.
                var setupSamus = runtime.Samus
                    ?? throw new InvalidOperationException("Door scrolling requires Samus.");
                sourceSamusXFixed = setupSamus.Kinematics.XFixed;
                sourceSamusYFixed = setupSamus.Kinematics.YFixed;
                runtime.ApplyDoorScrollingSetupMovement();
                Phase = DoorTransitionPhase.PlaceSamusAndLoadTiles;
                break;

            case DoorTransitionPhase.PlaceSamusAndLoadTiles:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: false);
                runtime.PlaceSamusForDoorTileLoading();
                Phase = DoorTransitionPhase.LoadMoreThingsAndOpenDoor;
                break;

            case DoorTransitionPhase.LoadMoreThingsAndOpenDoor:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: false);
                // LoadMoreThings initializes enemy graphics/music/projectiles/animtiles,
                // PLMs, FX, backgrounds, and then yields once per NMI until the IRQ raises
                // door_transition_flag bit $8000. The host room constructor performs that
                // setup atomically; its following calls now run the real coordinate path.
                fadedSourcePalette = runtime.Cgram.Colors.ToArray();
                runtime.LoadPendingDoorDestinationForTransition(sourceSamusXFixed, sourceSamusYFixed);
                // Enemy loading resets room-private host gates. Restore the native
                // $0795 transition ownership before any destination EnemyMain call;
                // elevator AI must remain frozen through the final palette fade.
                runtime.Enemies.ElevatorDoorTransitionActive = true;
                ushort[] destinationTarget = runtime.Cgram.Colors.ToArray();
                RestorePalette(runtime.Cgram, fadedSourcePalette);
                runtime.BeginDoorOpeningScroll(
                    door ?? throw new InvalidOperationException("Door header was not captured."),
                    sourceSamusXFixed,
                    sourceSamusYFixed,
                    completedLoadingIrqSteps: 1);
                runtime.StepDoorOpeningScroll();
                if (runtime.IconCancelEnabled && runtime.Samus is { } samus)
                {
                    // ResetProjectileData checks `$09EA` after clearing all projectile
                    // slots. This is the only gameplay effect of the Icon Cancel option.
                    samus.SelectedHudItem = 0;
                    samus.AutoCancelHudItemIndex = 0;
                }
                paletteTransition = new CartridgePaletteTransition(
                    destinationTarget,
                    denominator: 12);
                Phase = DoorTransitionPhase.WaitForDoorOpeningScroll;
                break;

            case DoorTransitionPhase.WaitForDoorOpeningScroll:
                // RunOneFrameOfGameInner always clears/finalizes the OAM build even when
                // this coroutine is only waiting on the room-loading IRQ. Publishing that
                // empty build prevents the faded source enemies from becoming black ghosts
                // over the incrementally moving destination doorway.
                runtime.RunBlankGameplayFrame(controllerInput);
                if (runtime.StepDoorOpeningScroll())
                {
                    // The cartridge calls PLM_Handler at $82:E53C on the final IRQ
                    // scrolling update, before the NMI that precedes destination fade-in.
                    // Setup lists can alter visible room state here; deferring them to the
                    // first ordinary gameplay frame exposes their pre-PLM state.
                    runtime.RunDoorTransitionPlmHandler();
                    Phase = DoorTransitionPhase.FinishDoorLoading;
                }
                break;

            case DoorTransitionPhase.FinishDoorLoading:
                // $82:E540 yields once after PLM processing. This resumes the
                // same outer dispatch, so it aligns X without another RNG call.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AlignSamusAfterDoorLoading();
                Phase = DoorTransitionPhase.HandleAnimatedTiles;
                break;

            case DoorTransitionPhase.HandleAnimatedTiles:
                // LoadCartridgeRoom already initialized the destination animtile owner.
                // Native gives it one explicit call before polling the global music queue.
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: true);
                Phase = DoorTransitionPhase.WaitForMusicQueue;
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunBlankGameplayFrame(controllerInput);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: true);
                if (!audio.HasQueuedMusic)
                {
                    Phase = DoorTransitionPhase.HandleTransition;
                    // $82:E670 calls LoadNewMusicTrackIfChanged here, before the
                    // final Samus nudge and palette fade, not during room creation.
                    if (!runtime.IsAttractDemo)
                    {
                        CartridgeRoomState state = runtime.ActiveRoom?.State
                            ?? throw new InvalidOperationException("Door music requires the destination room.");
                        audio.QueueRoomMusicTrack(state.MusicDataIndex, state.MusicTrackIndex);
                    }
                }
                break;

            case DoorTransitionPhase.HandleTransition:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: true);
                // `$82:E6A2` applies the narrow-door X/Y nudges only after the opening IRQ
                // and music queue are both complete. The atomic loader computed that exact
                // endpoint, which is restored here rather than during the visible scroll.
                runtime.FinishDoorOpeningScroll();
                runtime.EndDoorTransitionIrqDisplay();
                // E6A2 returns before the next E737 actor/fade dispatch. Falling
                // through ran Samus movement and accepted three NMIs in one update.
                Phase = DoorTransitionPhase.BuildDestinationOam;
                break;

            case DoorTransitionPhase.BuildDestinationOam:
            case DoorTransitionPhase.FadeInDestinationPalette:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                runtime.AdvanceDoorMainLoopRandom(hdmaObjectsEnabled: true);
                // E737 runs enemy/draw owners on every fade step, without Samus's
                // movement/animation handler or the ordinary camera streamer.
                runtime.DrawDoorTransitionActors();
                Phase = DoorTransitionPhase.FadeInDestinationPalette;
                // Enemy instruction lists run after the initial destination palette copy.
                // Their target writes belong to this same fade, not a private dead buffer.
                runtime.Enemies.ConsumeTargetPaletteWrites(paletteTransition!.SetTargetColor);
                if (paletteTransition!.Step(runtime.Cgram))
                {
                    // $82:E737 releases $0795 after EnemyMain and the last fade step.
                    // Movement resumes on the following gameplay frame, not this one.
                    runtime.Enemies.ElevatorDoorTransitionActive = false;
                    // Elevator arrival retains command zero's lock until the platform
                    // reaches rest; its actor, not the room fade, restores Samus movement.
                    // The elevatube door ASM also installs command zero. Native E737
                    // does not replace that handler: release only our temporary door
                    // input gate, never a stationary script's movement ownership.
                    if (runtime.Enemies.ElevatorStatus == ElevatorActorStatus.Inactive &&
                        runtime.Samus is { StationaryScriptControlLocked: false } arrivingSamus)
                        arrivingSamus.InputLocked = false;
                    Phase = DoorTransitionPhase.Complete;
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown door transition phase {Phase}.");
        }
    }

    private static ushort[] BuildSourceFadeTarget(SuperMetroidRuntime runtime)
    {
        var target = new ushort[SnesCgram.ColorCount];
        ReadOnlySpan<ushort> current = runtime.Cgram.Colors;

        // Keep HUD colors legible while the source room palette disappears.
        DoorTransitionPaletteDefinitions.PreserveHud(current, target);

        CartridgeRoomHeader sourceRoom = runtime.ActiveRoom ??
            throw new InvalidOperationException(
                "Door palette fade started without an active source room header.");
        byte sourceCre = sourceRoom.CreBitset;
        byte destinationCre = runtime.PendingDoorDestinationCreBitset;
        if (((sourceCre | destinationCre) & 1) == 0)
        {
            DoorTransitionPaletteDefinitions.PreserveCommonCre(current, target);
            if (runtime.EscapeTimer.IsActive)
                DoorTransitionPaletteDefinitions.PreserveEscapeTimer(current, target);
        }
        return target;
    }

    private static void RestorePalette(SnesCgram cgram, ReadOnlySpan<ushort> colors)
    {
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            cgram.SetColor(color, colors[color]);
    }
}

public enum DoorTransitionPhase
{
    Inactive,
    WaitForSoundQueues,
    FadeOutSourcePalette,
    LoadDoorHeader,
    AlignSourceCamera,
    FixDoorsMovingUp,
    SetupNewRoom,
    SetupScrolling,
    PlaceSamusAndLoadTiles,
    LoadMoreThingsAndOpenDoor,
    WaitForDoorOpeningScroll,
    HandleAnimatedTiles,
    WaitForMusicQueue,
    HandleTransition,
    FadeInDestinationPalette,
    Complete,
    // Append to preserve numeric identities already stored in debugger states.
    BuildDestinationOam,
    FinishDoorLoading,
}
