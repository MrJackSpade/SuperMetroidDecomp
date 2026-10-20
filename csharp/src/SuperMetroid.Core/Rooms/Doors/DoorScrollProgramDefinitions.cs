using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The straight-line bank-$8F door callbacks that only rewrite room scroll cells (#641). Each callback is
/// its ordered list of writes; a scroll index is the row-major cell number in the room's scroll grid.
/// The setup-code identities name the callbacks in the cartridge's hexadecimal cell notation.
/// </summary>
internal static class DoorScrollProgramDefinitions
{
    /// <summary>
    /// The ordered scroll writes of <paramref name="code"/>, or <see langword="null"/> for a setup routine that
    /// writes no scroll cells. An undefined setup code throws.
    /// </summary>
    internal static IReadOnlyList<DoorScrollWrite>? Writes(DoorSetupCode code) => code switch
    {
        DoorSetupCode.DoorCode_Scroll6_Green => [new(6, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_0_Blue => [new(0, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_13_Blue => [new(19, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_4_Red_8_Green => [new(4, RoomScrollState.RedBoundary), new(8, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_8_9_A_B_Red => [new(8, RoomScrollState.RedBoundary), new(9, RoomScrollState.RedBoundary), new(10, RoomScrollState.RedBoundary), new(11, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red => [new(2, RoomScrollState.RedBoundary), new(3, RoomScrollState.RedBoundary), new(4, RoomScrollState.RedBoundary), new(5, RoomScrollState.RedBoundary), new(11, RoomScrollState.RedBoundary), new(12, RoomScrollState.RedBoundary), new(13, RoomScrollState.RedBoundary), new(17, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_1_4_Green => [new(1, RoomScrollState.Green), new(4, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_2_Blue => [new(2, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_17_Blue => [new(23, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_4_Blue => [new(4, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_6_Green_duplicate => [new(6, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_3_Green => [new(3, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_18_1C_Green => [new(24, RoomScrollState.Green), new(28, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_5_6_Blue => [new(5, RoomScrollState.Blue), new(6, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_1D_Blue => [new(29, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_2_3_Green => [new(2, RoomScrollState.Green), new(3, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_0_Red_1_Green => [new(0, RoomScrollState.RedBoundary), new(1, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_B_Green => [new(11, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_Scroll_1C_Red_1D_Blue => [new(28, RoomScrollState.RedBoundary), new(29, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_4_Red => [new(4, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_20_24_25_Green => [new(32, RoomScrollState.Green), new(36, RoomScrollState.Green), new(37, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_2_Blue_duplicate => [new(2, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Green => [new(0, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_6_7_Green => [new(6, RoomScrollState.Green), new(7, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red => [new(1, RoomScrollState.Blue), new(2, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_1_Blue_3_Red => [new(1, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue => [new(0, RoomScrollState.RedBoundary), new(4, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_2_3_Blue => [new(2, RoomScrollState.Blue), new(3, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_1_Green => [new(0, RoomScrollState.Green), new(1, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_1_Green => [new(1, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_F_12_Green => [new(15, RoomScrollState.Green), new(18, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_6_Green_duplicate_again => [new(6, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_0_Green_1_Blue => [new(0, RoomScrollState.Green), new(1, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_2_Green => [new(2, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_3_4_Red_6_7_8_Blue => [new(3, RoomScrollState.RedBoundary), new(4, RoomScrollState.RedBoundary), new(6, RoomScrollState.Blue), new(7, RoomScrollState.Blue), new(8, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red => [new(1, RoomScrollState.Blue), new(2, RoomScrollState.Blue), new(3, RoomScrollState.Blue), new(4, RoomScrollState.Green), new(6, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_1_Blue => [new(0, RoomScrollState.Blue), new(1, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red => [new(0, RoomScrollState.Blue), new(1, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_A_Green => [new(10, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_0_2_Green => [new(0, RoomScrollState.Green), new(2, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_6_7_Blue_8_Red => [new(6, RoomScrollState.Blue), new(7, RoomScrollState.Blue), new(8, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_2_Red_3_Blue => [new(2, RoomScrollState.RedBoundary), new(3, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_7_Green => [new(7, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_1_Red_2_Blue => [new(1, RoomScrollState.RedBoundary), new(2, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red => [new(0, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_1_Blue_4_Red => [new(1, RoomScrollState.Blue), new(4, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_Blue_1_2_3_Red => [new(0, RoomScrollState.Blue), new(1, RoomScrollState.RedBoundary), new(2, RoomScrollState.RedBoundary), new(3, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_Green_duplicate => [new(0, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_0_1_Blue_4_Red => [new(0, RoomScrollState.Blue), new(1, RoomScrollState.Blue), new(4, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red_duplicate => [new(0, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_Blue_duplicate => [new(0, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red_duplicate => [new(0, RoomScrollState.Blue), new(1, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_18_Blue => [new(24, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_2_Blue_3_Red => [new(2, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_E_Red => [new(14, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_1_Blue => [new(1, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Green_duplicate_again => [new(0, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_3_Red_4_Blue => [new(3, RoomScrollState.RedBoundary), new(4, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_29_Blue => [new(41, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_28_2E_Green => [new(40, RoomScrollState.Green), new(46, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_6_7_8_9_A_B_Red => [new(6, RoomScrollState.RedBoundary), new(7, RoomScrollState.RedBoundary), new(8, RoomScrollState.RedBoundary), new(9, RoomScrollState.RedBoundary), new(10, RoomScrollState.RedBoundary), new(11, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_A_Red_B_Blue => [new(10, RoomScrollState.RedBoundary), new(11, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue_duplicate => [new(0, RoomScrollState.RedBoundary), new(4, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_Red_1_Blue => [new(0, RoomScrollState.RedBoundary), new(1, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_9_Red_A_Blue => [new(9, RoomScrollState.RedBoundary), new(10, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_0_2_Red_1_Blue => [new(0, RoomScrollState.RedBoundary), new(2, RoomScrollState.RedBoundary), new(1, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_1_Blue_duplicate => [new(1, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_6_Blue => [new(6, RoomScrollState.Blue)],
        DoorSetupCode.DoorASM_Scroll_4_Red_duplicate => [new(4, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_4_7_Red => [new(4, RoomScrollState.RedBoundary), new(7, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red_duplicate => [new(1, RoomScrollState.Blue), new(2, RoomScrollState.RedBoundary)],
        DoorSetupCode.DoorASM_Scroll_0_2_Green_duplicate => [new(0, RoomScrollState.Green), new(2, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_0_1_Green_duplicate => [new(0, RoomScrollState.Green), new(1, RoomScrollState.Green)],
        DoorSetupCode.DoorASM_Scroll_18_Blue_19_Red => [new(24, RoomScrollState.Blue), new(25, RoomScrollState.RedBoundary)],
        DoorSetupCode.None or
        DoorSetupCode.DoorASM_StartWreckedShipTreadmillWestEntrance or
        DoorSetupCode.DoorASM_StartWreckedShipTreadmillEastEntrance or
        DoorSetupCode.DoorASM_SetupElevatubeFromSouth or
        DoorSetupCode.DoorASM_SetupElevatubeFromNorth or
        DoorSetupCode.DoorASM_ResetElevatubeOnNorthExit or
        DoorSetupCode.DoorASM_ResetElevatubeOnSouthExit or
        DoorSetupCode.DoorASM_ToCeresElevatorShaft or
        DoorSetupCode.DoorASM_FromCeresElevatorShaft => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorSetupCode)} {(int)code:X4}."),
    };
}