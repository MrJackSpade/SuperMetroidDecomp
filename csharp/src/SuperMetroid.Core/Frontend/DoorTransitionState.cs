using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
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
    private int openingFramesRemaining;

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
        paletteTransition = new CartridgePaletteTransition(
            BuildSourceFadeTarget(runtime),
            denominator: 12);
        fadedSourcePalette = null;
        openingFramesRemaining = 0;
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
                    Phase = DoorTransitionPhase.AlignSourceCamera;
                break;

            case DoorTransitionPhase.AlignSourceCamera:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (runtime.AlignPendingDoorCameraOnePixel())
                    Phase = DoorTransitionPhase.LoadDestination;
                break;

            case DoorTransitionPhase.LoadDestination:
                // SetupScrolling fixes this count before PlaceSamusLoadTiles consumes the
                // pending header. Preserve it now, then let the existing cartridge loader
                // perform room/state/FX/PLM/enemy/background setup atomically under black.
                openingFramesRemaining = runtime.PendingDoorOpeningFrameCount;
                fadedSourcePalette = runtime.Cgram.Colors.ToArray();
                runtime.LoadPendingDoorDestination();

                // `$82:E737` will later animate enemies while fading in. Build one coherent
                // destination OAM/VRAM publication now; it remains hidden by the fully dark
                // palette during the opening-scroll wait and prevents source-room OAM from
                // appearing against destination graphics on the first visible frame.
                runtime.StepFrame(controller1Input: 0, advanceGameTime: false);
                ushort[] destinationTarget = runtime.Cgram.Colors.ToArray();
                RestorePalette(runtime.Cgram, fadedSourcePalette);
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
                Phase = DoorTransitionPhase.OpenDoorAndScroll;
                break;

            case DoorTransitionPhase.OpenDoorAndScroll:
                runtime.RunNmi(controllerInput, mainLoopRequestedNmi: true);
                if (--openingFramesRemaining <= 0)
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

        byte sourceCre = runtime.ActiveRoom?.CreBitset ?? 0;
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
    AlignSourceCamera,
    LoadDestination,
    OpenDoorAndScroll,
    HandleAnimatedTiles,
    WaitForMusicQueue,
    FadeInDestinationPalette,
    Complete,
}
