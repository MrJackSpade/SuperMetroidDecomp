using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Selects the PPU compositor required by the room and door currently owned by a runtime.
/// </summary>
/// <remarks>
/// This decision used to live in the debug executable, which made the standalone game and
/// visual tests vulnerable to choosing a different rendering path. Keeping it beside the
/// renderers makes every caller honor the cartridge door setup in exactly the same way.
/// </remarks>
public static class SuperMetroidRuntimeFrameRenderer
{
    /// <summary>Renders the runtime's last NMI-published OAM and current room PPU state.</summary>
    public static Rgba32[] Render(SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        if (runtime.ActiveDoor is null)
            throw new InvalidOperationException("A cartridge room and door must be loaded before rendering gameplay.");

        if (runtime.ActiveDoor.UsesCeresElevatorMode7)
        {
            // DoorCode_CeresElevatorShaft at `$8F:E4E0` installs these literal Mode 7
            // registers below the HUD IRQ split. They are door behavior, not a visual
            // approximation inferred from the room's area or name.
            return SnesGameplayFrameRenderer.RenderHudMode7AndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                matrixA: 0x0100,
                matrixB: 0,
                matrixC: 0,
                matrixD: 0x0100,
                centerX: 0x0080,
                centerY: 0x03f0,
                horizontalOffset: 0,
                verticalOffset: 0);
        }

        return SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
            runtime.Vram,
            runtime.Cgram,
            runtime.DisplayedOam,
            runtime.BackgroundScroll.Bg1HorizontalScroll,
            runtime.BackgroundScroll.Bg1VerticalScroll,
            runtime.BackgroundScroll.Bg2HorizontalScroll,
            runtime.BackgroundScroll.Bg2VerticalScroll);
    }
}
