using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>Pure scroll-byte portion of translated bank-$8F door setup programs.</summary>
internal static class DoorSetupCodeInterpreter
{
    /// <summary>Applies scroll writes.</summary>
    public static void ApplyScrollWrites(
        DoorSetupCode setupCodePointer,
        ushort doorPointer,
        RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        switch (setupCodePointer)
        {
            case DoorSetupCode.None:
            case DoorSetupCode.DoorASM_ToCeresElevatorShaft:
            case DoorSetupCode.DoorASM_FromCeresElevatorShaft:
            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillWestEntrance:
            case DoorSetupCode.DoorASM_StartWreckedShipTreadmillEastEntrance:
            case DoorSetupCode.DoorASM_SetupElevatubeFromSouth:
            case DoorSetupCode.DoorASM_SetupElevatubeFromNorth:
            case DoorSetupCode.DoorASM_ResetElevatubeOnNorthExit:
                // The two Ceres routines change PPU/Mode-7 state, not room scroll bytes.
                // The treadmill and first three elevatube routines likewise own only
                // object/Samus state. Their non-scroll effects are applied by the runtime.
                return;

            case DoorSetupCode.DoorASM_ResetElevatubeOnSouthExit:
                // $8F:E309 performs one 16-bit $0202 store at $7E:CD20. The typed grid
                // expresses the same two green storage bytes before Samus is unlocked.
                scrolls.SetStorage(0, RoomScrollState.Green);
                scrolls.SetStorage(1, RoomScrollState.Green);
                return;

            default:
                if (DoorScrollPrograms.TryApply(setupCodePointer, scrolls))
                    return;
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorSetupCode)} {(int)setupCodePointer:X4} in door $83:{doorPointer:X4}.");
        }
    }
}
