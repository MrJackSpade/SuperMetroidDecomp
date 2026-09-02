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

            case DoorCodes.DoorASM_StartWreckedShipTreadmillWestEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Rightwards);
                return;

            case DoorCodes.DoorASM_StartWreckedShipTreadmillEastEntrance:
                StartWreckedShipTreadmill(WreckedShipTreadmillDirection.Leftwards);
                return;

            case DoorCodes.DoorASM_SetupElevatubeFromSouth:
                SetUpMaridiaElevatube(fromSouth: true);
                return;

            case DoorCodes.DoorASM_SetupElevatubeFromNorth:
                SetUpMaridiaElevatube(fromSouth: false);
                return;

            case DoorCodes.DoorASM_ResetElevatubeOnNorthExit:
            case DoorCodes.DoorASM_ResetElevatubeOnSouthExit:
                MaridiaElevatube.ResetOnExit(Samus ?? throw new InvalidOperationException(
                    "Maridia elevatube exit setup requires an active Samus state."));
                return;

            default:
                if (DoorScrollPrograms.Contains(door.SetupCodePointer))
                    return;
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

    /// <summary>
    /// Test seam for invoking a real cartridge header after directly loading its destination
    /// room. Production reaches the identical private dispatcher through door transition.
    /// </summary>
    internal void RunDoorSetupForVerification(CartridgeDoorHeader door) =>
        RunDoorSetupCode(door);

    private void StartWreckedShipTreadmill(WreckedShipTreadmillDirection direction)
    {
        if (LevelData is null || BackgroundStreamer is null || ActiveRoom is null)
        {
            throw new InvalidOperationException(
                "Wrecked Ship treadmill door setup requires an active destination room.");
        }

        WreckedShipTreadmill.Start(direction);
        // Both native spawn helpers return carry set when their fixed arrays are full, and
        // the door routine deliberately ignores that result. The dedicated animated owner
        // above cannot exhaust; preserve the PLM allocator's native false result here.
        _ = Plms.TrySpawnWreckedShipEntranceTreadmill(
            LevelData,
            BackgroundStreamer,
            direction,
            System.HasAnyBossBits(ActiveRoom.AreaIndex, BossBits.AreaBoss));
    }

    private void SetUpMaridiaElevatube(bool fromSouth)
    {
        if (LevelData is null)
            throw new InvalidOperationException("Maridia elevatube setup requires active level data.");
        SamusState samus = Samus ?? throw new InvalidOperationException(
            "Maridia elevatube setup requires an active Samus state.");

        if (fromSouth)
            MaridiaElevatube.SetUpFromSouth(samus);
        else
            MaridiaElevatube.SetUpFromNorth(samus);

        // The door routine ignores SpawnHardcodedPLM's carry result when all forty IDs are
        // occupied. Retaining the bool-returning allocator preserves that cartridge edge.
        _ = Plms.TrySpawnMaridiaElevatube(LevelData);
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
            case DoorCodes.DoorASM_StartWreckedShipTreadmillWestEntrance:
            case DoorCodes.DoorASM_StartWreckedShipTreadmillEastEntrance:
            case DoorCodes.DoorASM_SetupElevatubeFromSouth:
            case DoorCodes.DoorASM_SetupElevatubeFromNorth:
            case DoorCodes.DoorASM_ResetElevatubeOnNorthExit:
                // The two Ceres routines change PPU/Mode-7 state, not room scroll bytes.
                // The treadmill and first three elevatube routines likewise own only
                // object/Samus state. Their non-scroll effects are applied by the runtime.
                return;

            case DoorCodes.DoorASM_ResetElevatubeOnSouthExit:
                // $8F:E309 performs one 16-bit $0202 store at $7E:CD20. The typed grid
                // expresses the same two green storage bytes before Samus is unlocked.
                scrolls.SetStorage(0, (byte)RoomScrollState.Green);
                scrolls.SetStorage(1, (byte)RoomScrollState.Green);
                return;

            default:
                if (DoorScrollPrograms.TryApply(setupCodePointer, scrolls))
                    return;
                throw new NotSupportedException(
                    $"Door $83:{doorPointer:X4} setup AI $8F:{setupCodePointer:X4} is not translated.");
        }
    }
}
