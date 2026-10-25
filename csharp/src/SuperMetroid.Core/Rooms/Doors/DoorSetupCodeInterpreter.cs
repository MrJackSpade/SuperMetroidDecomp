using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Pure scroll-byte portion of translated bank-$8F door setup programs.</summary>
internal static class DoorSetupCodeInterpreter
{
    /// <summary>Applies only scroll-byte effects of a door setup program; non-scroll setup remains with the runtime.</summary>
    public static void ApplyScrollWrites(
        DoorSetupCode setupCode,
        string doorName,
        RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        switch (setupCode)
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

            case DoorSetupCode.DoorCode_Scroll6_Green:
            case DoorSetupCode.DoorASM_Scroll_0_Blue:
            case DoorSetupCode.DoorASM_Scroll_13_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Red_8_Green:
            case DoorSetupCode.DoorASM_Scroll_8_9_A_B_Red:
            case DoorSetupCode.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red:
            case DoorSetupCode.DoorASM_Scroll_1_4_Green:
            case DoorSetupCode.DoorASM_Scroll_2_Blue:
            case DoorSetupCode.DoorASM_Scroll_17_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Blue:
            case DoorSetupCode.DoorASM_Scroll_6_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_3_Green:
            case DoorSetupCode.DoorASM_Scroll_18_1C_Green:
            case DoorSetupCode.DoorASM_Scroll_5_6_Blue:
            case DoorSetupCode.DoorASM_Scroll_1D_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_3_Green:
            case DoorSetupCode.DoorASM_Scroll_0_Red_1_Green:
            case DoorSetupCode.DoorASM_Scroll_B_Green:
            case DoorSetupCode.DoorASM_Scroll_Scroll_1C_Red_1D_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Red:
            case DoorSetupCode.DoorASM_Scroll_20_24_25_Green:
            case DoorSetupCode.DoorASM_Scroll_2_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Green:
            case DoorSetupCode.DoorASM_Scroll_6_7_Green:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_3_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_3_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_1_Green:
            case DoorSetupCode.DoorASM_Scroll_1_Green:
            case DoorSetupCode.DoorASM_Scroll_F_12_Green:
            case DoorSetupCode.DoorASM_Scroll_6_Green_duplicate_again:
            case DoorSetupCode.DoorASM_Scroll_0_Green_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_Green:
            case DoorSetupCode.DoorASM_Scroll_3_4_Red_6_7_8_Blue:
            case DoorSetupCode.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red:
            case DoorSetupCode.DoorASM_Scroll_0_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red:
            case DoorSetupCode.DoorASM_Scroll_A_Green:
            case DoorSetupCode.DoorASM_Scroll_0_2_Green:
            case DoorSetupCode.DoorASM_Scroll_6_7_Blue_8_Red:
            case DoorSetupCode.DoorASM_Scroll_2_Red_3_Blue:
            case DoorSetupCode.DoorASM_Scroll_7_Green:
            case DoorSetupCode.DoorASM_Scroll_1_Red_2_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_4_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_1_2_3_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_1_Blue_4_Red:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_18_Blue:
            case DoorSetupCode.DoorASM_Scroll_2_Blue_3_Red:
            case DoorSetupCode.DoorASM_Scroll_E_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Green_duplicate_again:
            case DoorSetupCode.DoorASM_Scroll_3_Red_4_Blue:
            case DoorSetupCode.DoorASM_Scroll_29_Blue:
            case DoorSetupCode.DoorASM_Scroll_28_2E_Green:
            case DoorSetupCode.DoorASM_Scroll_6_7_8_9_A_B_Red:
            case DoorSetupCode.DoorASM_Scroll_A_Red_B_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_Red_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_9_Red_A_Blue:
            case DoorSetupCode.DoorASM_Scroll_0_2_Red_1_Blue:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_duplicate:
            case DoorSetupCode.DoorASM_Scroll_6_Blue:
            case DoorSetupCode.DoorASM_Scroll_4_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_4_7_Red:
            case DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_2_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_0_1_Green_duplicate:
            case DoorSetupCode.DoorASM_Scroll_18_Blue_19_Red:
            default:
                if (DoorScrollPrograms.TryApply(setupCode, scrolls))
                    return;
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorSetupCode)} {setupCode} in door {doorName}.");
        }
    }
}
