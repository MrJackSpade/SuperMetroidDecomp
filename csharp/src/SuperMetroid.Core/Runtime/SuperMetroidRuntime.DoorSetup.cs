using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Bank-$8F door-setup dispatch kept separate from room asset construction.
/// </summary>
public sealed partial class SuperMetroidRuntime
{
    /// <summary>
    /// Runs the setup routine named by the active bank-$83 door header after PLM creation.
    /// </summary>
    /// <remarks>
    /// Native <c>LoadMoreThings</c> calls the door's bank-$8F routine between room-object
    /// construction and the first visible destination viewport. These routines are shared
    /// cartridge programs, not room-name patches: every door that points at a translated
    /// address receives the same writes. Unknown nonzero pointers throw so missing camera
    /// boundary behavior can never masquerade as a successful room load.
    /// </remarks>
    private void RunDoorSetupCode(CartridgeDoorHeader door)
    {
        if (Camera is null)
            throw new InvalidOperationException("Door setup requires the destination scroll grid.");

        DoorSetupCodeInterpreter.ApplyScrollWrites(
            door.SetupCodePointer,
            door.Pointer,
            Camera.Scrolls);

        switch (door.SetupCodePointer)
        {
            case 0:
                return;

            case DoorCodes.DoorASM_Scroll_0_Green_1_Blue:
            case DoorCodes.DoorCode_Scroll6_Green:
                return;

            case DoorCodes.DoorASM_ToCeresElevatorShaft:
                // `$8F:E4E0` owns the Ceres Mode-7 matrix and center registers. Those
                // values must exist before room graphics/actors initialize, so the shared
                // loader installs them at construction time and this native-order dispatch
                // point merely verifies that the recognized routine was not discarded.
                if (!door.UsesCeresElevatorMode7 || ActiveSamusMode7Transform is null)
                {
                    throw new InvalidDataException(
                        "Ceres elevator door setup reached dispatch without its Mode-7 transform.");
                }
                return;

            case DoorCodes.DoorASM_FromCeresElevatorShaft:
                // `$8F:E513` is the paired exit routine for `$8F:E4E0`. Native writes
                // fake BGMODE `$09` (Mode 1 with BG3 priority) and clears `$0783`, the
                // Mode-7 IRQ/transfer flag. The software renderer represents those two
                // words by selecting its ordinary room compositor and removing both the
                // main-loop and NMI-published transforms. Do this explicitly here rather
                // than relying on the destination loader's defensive state reset: this is
                // the cartridge routine that owns the transition back to normal rendering.
                ActiveSamusMode7Transform = null;
                DisplayedSamusMode7Transform = null;
                CeresElevatorShaft.Reset(active: false);
                return;

            default:
                throw new NotSupportedException(
                    $"Door $83:{door.Pointer:X4} setup AI $8F:{door.SetupCodePointer:X4} is not translated.");
        }
    }

    /// <summary>
    /// Test seam for applying the active cartridge door routine to a captured regression
    /// state. Gameplay calls the same dispatcher from the room loader.
    /// </summary>
    internal void RunActiveDoorSetupForVerification()
    {
        CartridgeDoorHeader door = ActiveDoor
            ?? throw new InvalidOperationException("No active door exists in the captured state.");
        RunDoorSetupCode(door);
    }
}

/// <summary>Pure scroll-byte portion of translated bank-$8F door setup programs.</summary>
internal static class DoorSetupCodeInterpreter
{
    public static void ApplyScrollWrites(
        ushort setupCodePointer,
        ushort doorPointer,
        RoomScrollGrid scrolls)
    {
        ArgumentNullException.ThrowIfNull(scrolls);
        switch (setupCodePointer)
        {
            case 0:
            case DoorCodes.DoorASM_ToCeresElevatorShaft:
            case DoorCodes.DoorASM_FromCeresElevatorShaft:
                // The two Ceres routines change PPU/Mode-7 state, not room scroll bytes.
                // Their non-scroll effects are applied by SuperMetroidRuntime above.
                return;

            case DoorCodes.DoorASM_Scroll_0_Green_1_Blue:
                // `$8F:BE25-$BE31` is `DoorASM_Scroll_0_Green_1_Blue`. The first byte
                // allows downward camera tracking through screen zero; the second retains
                // normal tracking in screen one. Leaving the room header's initial
                // `[blue, red]` pair intact makes a jump in lower Construction Zone drive
                // the camera upward until Samus exits the viewport and wraps around it.
                scrolls.SetStorage(0, (byte)RoomScrollState.Green);
                scrolls.SetStorage(1, (byte)RoomScrollState.Blue);
                return;

            case DoorCodes.DoorCode_Scroll6_Green:
                // `DoorCode_Scroll6_Green` is the complete `$8F:B981` routine. The early
                // return route reaches it through door $83:8B3E; retaining a red boundary
                // at storage cell six would stop the same generic camera tracker there.
                scrolls.SetStorage(6, (byte)RoomScrollState.Green);
                return;

            default:
                throw new NotSupportedException(
                    $"Door $83:{doorPointer:X4} setup AI $8F:{setupCodePointer:X4} is not translated.");
        }
    }
}
