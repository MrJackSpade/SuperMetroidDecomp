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

        if (runtime.Enemies.CeresRidley is { Mode7Active: true } getaway)
        {
            // Ceres room main $A6:AAAF temporarily replaces ordinary mode-nine BG1/BG2
            // below the HUD with the rotating Ridley/Baby Mode-7 map. OAM stays live, so
            // Samus and the timer retain the usual gameplay compositor and priority rules.
            return SnesGameplayFrameRenderer.RenderHudMode7AndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                matrixA: unchecked((short)getaway.Mode7MatrixA),
                matrixB: unchecked((short)getaway.Mode7MatrixB),
                matrixC: unchecked((short)getaway.Mode7MatrixC),
                matrixD: unchecked((short)getaway.Mode7MatrixD),
                centerX: unchecked((short)getaway.Mode7CenterX),
                centerY: unchecked((short)getaway.Mode7CenterY),
                horizontalOffset: unchecked((short)getaway.Mode7HorizontalOffset),
                verticalOffset: unchecked((short)getaway.Mode7VerticalOffset));
        }

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
                // Mode 7 reuses BG1HOFS/BG1VOFS as M7HOFS/M7VOFS. MainScrollingRoutine
                // continues publishing those live camera words even though its ordinary
                // tilemap row/column producers return early while `$0783` is nonzero.
                // Leaving these at zero pins the shaft art to the window while Samus,
                // enemies, and projectiles correctly move relative to the camera.
                horizontalOffset: unchecked((short)runtime.BackgroundScroll.Bg1HorizontalScroll),
                verticalOffset: unchecked((short)runtime.BackgroundScroll.Bg1VerticalScroll));
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
