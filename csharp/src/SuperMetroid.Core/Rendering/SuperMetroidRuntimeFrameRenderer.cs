using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
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

        RoomShakeFrameResult shake = runtime.Enemies.LastRoomShake;
        ushort bg1HorizontalScroll = AddShake(
            runtime.BackgroundScroll.Bg1HorizontalScroll,
            shake.Bg1X);
        ushort bg1VerticalScroll = AddShake(
            runtime.BackgroundScroll.Bg1VerticalScroll,
            shake.Bg1Y);
        ushort bg2HorizontalScroll = AddShake(
            runtime.BackgroundScroll.Bg2HorizontalScroll,
            shake.Bg2X);
        ushort bg2VerticalScroll = AddShake(
            runtime.BackgroundScroll.Bg2VerticalScroll,
            shake.Bg2Y);

        Rgba32[] frame;
        if (runtime.Enemies.CeresRidley is { Mode7Active: true } getaway)
        {
            // Ceres room main $A6:AAAF temporarily replaces ordinary mode-nine BG1/BG2
            // below the HUD with the rotating Ridley/Baby Mode-7 map. OAM stays live, so
            // Samus and the timer retain the usual gameplay compositor and priority rules.
            frame = SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                matrixA: unchecked((short)getaway.Mode7MatrixA),
                matrixB: unchecked((short)getaway.Mode7MatrixB),
                matrixC: unchecked((short)getaway.Mode7MatrixC),
                matrixD: unchecked((short)getaway.Mode7MatrixD),
                centerX: unchecked((short)getaway.Mode7CenterX),
                centerY: unchecked((short)getaway.Mode7CenterY),
                horizontalOffset: unchecked((short)(getaway.Mode7HorizontalOffset + shake.Bg1X)),
                verticalOffset: unchecked((short)(getaway.Mode7VerticalOffset + shake.Bg1Y)),
                bg2HorizontalScroll: bg2HorizontalScroll,
                bg2VerticalScroll: bg2VerticalScroll,
                // Setup ASM $8F:C97B writes BG12NBA=$66; the floor slice returns to
                // Mode 1 and therefore consumes the same $6000 character base as BG2 did
                // before the getaway HDMA tables changed BGMODE.
                bg2CharacterBaseWord: 0x6000);
        }
        else if (runtime.ActiveDoor.UsesCeresElevatorMode7)
        {
            // DoorCode_CeresElevatorShaft at `$8F:E4E0` installs these literal Mode 7
            // registers below the HUD IRQ split. They are door behavior, not a visual
            // approximation inferred from the room's area or name.
            frame = SnesGameplayFrameRenderer.RenderHudMode7AndObjs(
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
                horizontalOffset: unchecked((short)bg1HorizontalScroll),
                verticalOffset: unchecked((short)bg1VerticalScroll));
        }
        else
        {
            // Setup ASM $8F:C97B writes BG12NBA=$66 for the Ceres Ridley arena. Each
            // register nibble is a $1000-word character-base selector, so both BG1 and
            // BG2 deliberately reuse VRAM $6000. This is not a Ridley-art special case in
            // the compositor: it is the literal PPU register value selected by the room.
            // Other translated ordinary-room setup routines retain power-on base zero.
            bool usesCeresRidleyCharacterBase =
                runtime.ActiveRoom?.State.SetupCodePointer == 0xc97b;
            ushort bgCharacterBaseWord = usesCeresRidleyCharacterBase
                ? (ushort)0x6000
                : (ushort)0;
            bool crocomireOwnsBg2 = runtime.Enemies.Crocomire is not null;
            frame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                bg1HorizontalScroll,
                bg1VerticalScroll,
                crocomireOwnsBg2
                    ? AddShake(runtime.Enemies.CrocomireBg2HorizontalScroll, shake.Bg2X)
                    : bg2HorizontalScroll,
                crocomireOwnsBg2
                    ? AddShake(runtime.Enemies.CrocomireBg2VerticalScroll, shake.Bg2Y)
                    : bg2VerticalScroll,
                bg2VerticalScrollByLine: crocomireOwnsBg2
                    ? runtime.Enemies.CrocomireDeath?.Bg2ScrollByScanline
                    : null,
                bg1CharacterBaseWord: bgCharacterBaseWord,
                bg2CharacterBaseWord: bgCharacterBaseWord);
        }

        // These are the three setup routines that explicitly call FXType_2C_CeresHaze.
        // Keying the effect from cartridge state avoids applying a guessed “Ceres tint” to
        // scenes that do not spawn the HDMA object.
        if (runtime.ActiveRoom?.State.SetupCodePointer is 0xc96e or 0xc976 or 0xc97b)
            SnesGameplayFrameRenderer.ApplyCeresHaze(frame, ridleyIsDead: false);

        return frame;
    }

    private static ushort AddShake(ushort scroll, short displacement) =>
        unchecked((ushort)(scroll + displacement));
}
