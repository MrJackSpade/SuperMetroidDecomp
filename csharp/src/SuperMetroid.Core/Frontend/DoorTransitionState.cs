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
        door = runtime.PendingDoorTransition;
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
        ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(audio);
        if (!IsActive)
            throw new InvalidOperationException("Door transition has not begun.");

        switch (Phase)
        {
            case DoorTransitionPhase.WaitForSoundQueues:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (!audio.HasQueuedSounds)
                    Phase = DoorTransitionPhase.FadeOutSourcePalette;
                break;

            case DoorTransitionPhase.FadeOutSourcePalette:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (paletteTransition!.Step(runtime.Cgram))
                    Phase = DoorTransitionPhase.LoadDoorHeader;
                break;

            case DoorTransitionPhase.LoadDoorHeader:
                // `$82:E2F7` parses the already selected bank-$83 header, disables HDMA,
                // and points the IRQ dispatcher at the door-scrolling handler. The typed
                // header was captured by Begin; retaining this separate call preserves the
                // coroutine boundary and its one accepted NMI.
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                Phase = DoorTransitionPhase.AlignSourceCamera;
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (runtime.AlignPendingDoorCameraOnePixel())
                    Phase = DoorTransitionPhase.FixDoorsMovingUp;
                break;

            case DoorTransitionPhase.FixDoorsMovingUp:
                // The upward-only staging correction changes previous block coordinates,
                // not the final host framebuffer. Its coordinate consequence is included
                // by BeginDoorOpeningScroll's +$FF origin.
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                Phase = DoorTransitionPhase.SetupNewRoom;
                break;

            case DoorTransitionPhase.SetupNewRoom:
                // Room/state/FX/level setup is atomic in LoadPendingDoorDestination, but
                // native exposes this function separately from scrolling and tile upload.
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                Phase = DoorTransitionPhase.SetupScrolling;
                break;

            case DoorTransitionPhase.SetupScrolling:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                Phase = DoorTransitionPhase.PlaceSamusAndLoadTiles;
                break;

            case DoorTransitionPhase.PlaceSamusAndLoadTiles:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                Phase = DoorTransitionPhase.LoadMoreThingsAndOpenDoor;
                break;

            case DoorTransitionPhase.LoadMoreThingsAndOpenDoor:
                // LoadMoreThings initializes enemy graphics/music/projectiles/animtiles,
                // PLMs, FX, backgrounds, and then yields once per NMI until the IRQ raises
                // door_transition_flag bit $8000. The host room constructor performs that
                // setup atomically; its following calls now run the real coordinate path.
                fadedSourcePalette = runtime.Cgram.Colors.ToArray();
                runtime.LoadPendingDoorDestination();

                // `$82:E737` will later animate enemies while fading in. Build one coherent
                // destination OAM/VRAM publication now; it remains hidden by the fully dark
                // palette during the opening-scroll wait and prevents source-room OAM from
                // appearing against destination graphics on the first visible frame.
                runtime.StepFrame(controller1Input: 0, advanceGameTime: false);
                ushort[] destinationTarget = runtime.Cgram.Colors.ToArray();
                RestorePalette(runtime.Cgram, fadedSourcePalette);
                runtime.BeginDoorOpeningScroll(
                    door ?? throw new InvalidOperationException("Door header was not captured."),
                    sourceSamusXFixed,
                    sourceSamusYFixed);
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
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (runtime.StepDoorOpeningScroll())
                    Phase = DoorTransitionPhase.HandleAnimatedTiles;
                break;

            case DoorTransitionPhase.HandleAnimatedTiles:
                // LoadCartridgeRoom already initialized the destination animtile owner.
                // Native gives it one explicit call before polling the global music queue.
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                Phase = DoorTransitionPhase.WaitForMusicQueue;
                break;

            case DoorTransitionPhase.WaitForMusicQueue:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (!audio.HasQueuedMusic)
                    Phase = DoorTransitionPhase.HandleTransition;
                break;

            case DoorTransitionPhase.HandleTransition:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                // `$82:E6A2` applies the narrow-door X/Y nudges only after the opening IRQ
                // and music queue are both complete. The atomic loader computed that exact
                // endpoint, which is restored here rather than during the visible scroll.
                runtime.FinishDoorOpeningScroll();
                Phase = DoorTransitionPhase.FadeInDestinationPalette;
                break;

            case DoorTransitionPhase.FadeInDestinationPalette:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (paletteTransition!.Step(runtime.Cgram))
                {
                    if (runtime.Samus is { } arrivingSamus)
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

        // GameState_10 initially blacks every target entry, then restores the seven fixed
        // HUD/door colors below. They stay legible while the room palette disappears.
        int[] alwaysPreserved = [9, 10, 13, 14, 17, 18, 19, 29];
        foreach (int color in alwaysPreserved)
            target[color] = current[color];

        CartridgeRoomHeader sourceRoom = runtime.ActiveRoom ??
            throw new InvalidOperationException(
                "Door palette fade started without an active source room header.");
        byte sourceCre = sourceRoom.CreBitset;
        byte destinationCre = runtime.PendingDoorDestinationCreBitset;
        if (((sourceCre | destinationCre) & 1) == 0)
        {
            int[] commonCreColors = [20, 21, 22, 23, 28];
            foreach (int color in commonCreColors)
                target[color] = current[color];
            if (runtime.EscapeTimer.IsActive)
            {
                int[] timerColors = [209, 210, 212, 221];
                foreach (int color in timerColors)
                    target[color] = current[color];
            }
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
}
