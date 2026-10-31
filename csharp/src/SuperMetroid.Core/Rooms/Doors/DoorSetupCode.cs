namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The setup routine a door runs while Samus passes through it: the complete set the retail doors name, plus none.
/// The values are arbitrary; the import pipeline translates the cartridge's routine words (#1414).
/// </summary>
public enum DoorSetupCode
{
    /// <summary>The door header names no setup routine.</summary>
    None = 1,

    /// <summary><c>DoorASM_StartWreckedShipTreadmillWestEntrance</c>.</summary>
    DoorASM_StartWreckedShipTreadmillWestEntrance = 2,

    /// <summary>
    /// <c>DoorCode_Scroll6_Green</c>; changes room-scroll storage cell six
    /// to green.
    /// </summary>
    DoorCode_Scroll6_Green = 3,

    /// <summary><c>DoorASM_Scroll_0_Blue</c>.</summary>
    DoorASM_Scroll_0_Blue = 4,

    /// <summary><c>DoorASM_Scroll_13_Blue</c>.</summary>
    DoorASM_Scroll_13_Blue = 5,

    /// <summary><c>DoorASM_Scroll_4_Red_8_Green</c>.</summary>
    DoorASM_Scroll_4_Red_8_Green = 6,

    /// <summary><c>DoorASM_Scroll_8_9_A_B_Red</c>.</summary>
    DoorASM_Scroll_8_9_A_B_Red = 7,

    /// <summary><c>DoorASM_Scroll_2_3_4_5_B_C_D_11_Red</c>.</summary>
    DoorASM_Scroll_2_3_4_5_B_C_D_11_Red = 8,

    /// <summary><c>DoorASM_Scroll_1_4_Green</c>.</summary>
    DoorASM_Scroll_1_4_Green = 9,

    /// <summary><c>DoorASM_Scroll_2_Blue</c>.</summary>
    DoorASM_Scroll_2_Blue = 10,

    /// <summary><c>DoorASM_Scroll_17_Blue</c>.</summary>
    DoorASM_Scroll_17_Blue = 11,

    /// <summary><c>DoorASM_Scroll_4_Blue</c>.</summary>
    DoorASM_Scroll_4_Blue = 12,

    /// <summary><c>DoorASM_Scroll_6_Green_duplicate</c>.</summary>
    DoorASM_Scroll_6_Green_duplicate = 13,

    /// <summary><c>DoorASM_Scroll_3_Green</c>.</summary>
    DoorASM_Scroll_3_Green = 14,

    /// <summary><c>DoorASM_Scroll_18_1C_Green</c>.</summary>
    DoorASM_Scroll_18_1C_Green = 15,

    /// <summary><c>DoorASM_Scroll_5_6_Blue</c>.</summary>
    DoorASM_Scroll_5_6_Blue = 16,

    /// <summary><c>DoorASM_Scroll_1D_Blue</c>.</summary>
    DoorASM_Scroll_1D_Blue = 17,

    /// <summary><c>DoorASM_Scroll_2_3_Green</c>.</summary>
    DoorASM_Scroll_2_3_Green = 18,

    /// <summary><c>DoorASM_Scroll_0_Red_1_Green</c>.</summary>
    DoorASM_Scroll_0_Red_1_Green = 19,

    /// <summary><c>DoorASM_Scroll_B_Green</c>.</summary>
    DoorASM_Scroll_B_Green = 20,

    /// <summary><c>DoorASM_Scroll_Scroll_1C_Red_1D_Blue</c>.</summary>
    DoorASM_Scroll_Scroll_1C_Red_1D_Blue = 21,

    /// <summary><c>DoorASM_Scroll_4_Red</c>.</summary>
    DoorASM_Scroll_4_Red = 22,

    /// <summary><c>DoorASM_Scroll_20_24_25_Green</c>.</summary>
    DoorASM_Scroll_20_24_25_Green = 23,

    /// <summary><c>DoorASM_Scroll_2_Blue_duplicate</c>.</summary>
    DoorASM_Scroll_2_Blue_duplicate = 24,

    /// <summary><c>DoorASM_Scroll_0_Green</c>.</summary>
    DoorASM_Scroll_0_Green = 25,

    /// <summary><c>DoorASM_Scroll_6_7_Green</c>.</summary>
    DoorASM_Scroll_6_7_Green = 26,

    /// <summary><c>DoorASM_Scroll_1_Blue_2_Red</c>.</summary>
    DoorASM_Scroll_1_Blue_2_Red = 27,

    /// <summary><c>DoorASM_Scroll_1_Blue_3_Red</c>.</summary>
    DoorASM_Scroll_1_Blue_3_Red = 28,

    /// <summary><c>DoorASM_Scroll_0_Red_4_Blue</c>.</summary>
    DoorASM_Scroll_0_Red_4_Blue = 29,

    /// <summary><c>DoorASM_Scroll_2_3_Blue</c>.</summary>
    DoorASM_Scroll_2_3_Blue = 30,

    /// <summary><c>DoorASM_Scroll_0_1_Green</c>.</summary>
    DoorASM_Scroll_0_1_Green = 31,

    /// <summary><c>DoorASM_Scroll_1_Green</c>.</summary>
    DoorASM_Scroll_1_Green = 32,

    /// <summary><c>DoorASM_Scroll_F_12_Green</c>.</summary>
    DoorASM_Scroll_F_12_Green = 33,

    /// <summary><c>DoorASM_Scroll_6_Green_duplicate_again</c>.</summary>
    DoorASM_Scroll_6_Green_duplicate_again = 34,

    /// <summary>
    /// <c>DoorASM_Scroll_0_Green_1_Blue</c>; opens Construction Zone's
    /// two-screen vertical camera route after returning from First Missile.
    /// </summary>
    DoorASM_Scroll_0_Green_1_Blue = 35,

    /// <summary><c>DoorASM_Scroll_2_Green</c>.</summary>
    DoorASM_Scroll_2_Green = 36,

    /// <summary><c>DoorASM_Scroll_3_4_Red_6_7_8_Blue</c>.</summary>
    DoorASM_Scroll_3_4_Red_6_7_8_Blue = 37,

    /// <summary><c>DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red</c>.</summary>
    DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red = 38,

    /// <summary><c>DoorASM_Scroll_0_1_Blue</c>.</summary>
    DoorASM_Scroll_0_1_Blue = 39,

    /// <summary><c>DoorASM_Scroll_0_Blue_1_Red</c>.</summary>
    DoorASM_Scroll_0_Blue_1_Red = 40,

    /// <summary><c>DoorASM_Scroll_A_Green</c>.</summary>
    DoorASM_Scroll_A_Green = 41,

    /// <summary><c>DoorASM_Scroll_0_2_Green</c>.</summary>
    DoorASM_Scroll_0_2_Green = 42,

    /// <summary><c>DoorASM_Scroll_6_7_Blue_8_Red</c>.</summary>
    DoorASM_Scroll_6_7_Blue_8_Red = 43,

    /// <summary><c>DoorASM_Scroll_2_Red_3_Blue</c>.</summary>
    DoorASM_Scroll_2_Red_3_Blue = 44,

    /// <summary><c>DoorASM_Scroll_7_Green</c>.</summary>
    DoorASM_Scroll_7_Green = 45,

    /// <summary><c>DoorASM_Scroll_1_Red_2_Blue</c>.</summary>
    DoorASM_Scroll_1_Red_2_Blue = 46,

    /// <summary><c>DoorASM_Scroll_0_Blue_3_Red</c>.</summary>
    DoorASM_Scroll_0_Blue_3_Red = 47,

    /// <summary><c>DoorASM_Scroll_1_Blue_4_Red</c>.</summary>
    DoorASM_Scroll_1_Blue_4_Red = 48,

    /// <summary><c>DoorASM_Scroll_0_Blue_1_2_3_Red</c>.</summary>
    DoorASM_Scroll_0_Blue_1_2_3_Red = 49,

    /// <summary><c>DoorASM_Scroll_0_Green_duplicate</c>.</summary>
    DoorASM_Scroll_0_Green_duplicate = 50,

    /// <summary><c>DoorASM_Scroll_0_1_Blue_4_Red</c>.</summary>
    DoorASM_Scroll_0_1_Blue_4_Red = 51,

    /// <summary><c>DoorASM_Scroll_0_Blue_3_Red_duplicate</c>.</summary>
    DoorASM_Scroll_0_Blue_3_Red_duplicate = 52,

    /// <summary><c>DoorASM_Scroll_0_Blue_duplicate</c>.</summary>
    DoorASM_Scroll_0_Blue_duplicate = 53,

    /// <summary><c>DoorASM_Scroll_0_Blue_1_Red_duplicate</c>.</summary>
    DoorASM_Scroll_0_Blue_1_Red_duplicate = 54,

    /// <summary><c>DoorASM_Scroll_18_Blue</c>.</summary>
    DoorASM_Scroll_18_Blue = 55,

    /// <summary><c>DoorASM_Scroll_2_Blue_3_Red</c>.</summary>
    DoorASM_Scroll_2_Blue_3_Red = 56,

    /// <summary><c>DoorASM_Scroll_E_Red</c>.</summary>
    DoorASM_Scroll_E_Red = 57,

    /// <summary><c>DoorASM_StartWreckedShipTreadmillEastEntrance</c>.</summary>
    DoorASM_StartWreckedShipTreadmillEastEntrance = 58,

    /// <summary><c>DoorASM_Scroll_1_Blue</c>.</summary>
    DoorASM_Scroll_1_Blue = 59,

    /// <summary><c>DoorASM_Scroll_0_Green_duplicate_again</c>.</summary>
    DoorASM_Scroll_0_Green_duplicate_again = 60,

    /// <summary><c>DoorASM_Scroll_3_Red_4_Blue</c>.</summary>
    DoorASM_Scroll_3_Red_4_Blue = 61,

    /// <summary><c>DoorASM_Scroll_29_Blue</c>.</summary>
    DoorASM_Scroll_29_Blue = 62,

    /// <summary><c>DoorASM_Scroll_28_2E_Green</c>.</summary>
    DoorASM_Scroll_28_2E_Green = 63,

    /// <summary><c>DoorASM_Scroll_6_7_8_9_A_B_Red</c>.</summary>
    DoorASM_Scroll_6_7_8_9_A_B_Red = 64,

    /// <summary><c>DoorASM_SetupElevatubeFromSouth</c>.</summary>
    DoorASM_SetupElevatubeFromSouth = 65,

    /// <summary><c>DoorASM_SetupElevatubeFromNorth</c>.</summary>
    DoorASM_SetupElevatubeFromNorth = 66,

    /// <summary><c>DoorASM_ResetElevatubeOnNorthExit</c>.</summary>
    DoorASM_ResetElevatubeOnNorthExit = 67,

    /// <summary><c>DoorASM_ResetElevatubeOnSouthExit</c>.</summary>
    DoorASM_ResetElevatubeOnSouthExit = 68,

    /// <summary><c>DoorASM_Scroll_A_Red_B_Blue</c>.</summary>
    DoorASM_Scroll_A_Red_B_Blue = 69,

    /// <summary><c>DoorASM_Scroll_0_Red_4_Blue_duplicate</c>.</summary>
    DoorASM_Scroll_0_Red_4_Blue_duplicate = 70,

    /// <summary><c>DoorASM_Scroll_0_Red_1_Blue</c>.</summary>
    DoorASM_Scroll_0_Red_1_Blue = 71,

    /// <summary><c>DoorASM_Scroll_9_Red_A_Blue</c>.</summary>
    DoorASM_Scroll_9_Red_A_Blue = 72,

    /// <summary><c>DoorASM_Scroll_0_2_Red_1_Blue</c>.</summary>
    DoorASM_Scroll_0_2_Red_1_Blue = 73,

    /// <summary><c>DoorASM_Scroll_1_Blue_duplicate</c>.</summary>
    DoorASM_Scroll_1_Blue_duplicate = 74,

    /// <summary><c>DoorASM_Scroll_6_Blue</c>.</summary>
    DoorASM_Scroll_6_Blue = 75,

    /// <summary><c>DoorASM_Scroll_4_Red_duplicate</c>.</summary>
    DoorASM_Scroll_4_Red_duplicate = 76,

    /// <summary><c>DoorASM_Scroll_4_7_Red</c>.</summary>
    DoorASM_Scroll_4_7_Red = 77,

    /// <summary><c>DoorASM_Scroll_1_Blue_2_Red_duplicate</c>.</summary>
    DoorASM_Scroll_1_Blue_2_Red_duplicate = 78,

    /// <summary><c>DoorASM_Scroll_0_2_Green_duplicate</c>.</summary>
    DoorASM_Scroll_0_2_Green_duplicate = 79,

    /// <summary><c>DoorASM_Scroll_0_1_Green_duplicate</c>.</summary>
    DoorASM_Scroll_0_1_Green_duplicate = 80,

    /// <summary>
    /// Door callback. The inherited disassembly label says cells 8/9, but the
    /// cartridge operands are <c>Scrolls+$18</c> and <c>Scrolls+$19</c>.
    /// </summary>
    DoorASM_Scroll_18_Blue_19_Red = 81,

    /// <summary>
    /// <c>DoorASM_ToCeresElevatorShaft</c>; selects Mode 7 and installs
    /// the shaft's initial matrix and center.
    /// </summary>
    DoorASM_ToCeresElevatorShaft = 82,

    /// <summary>
    /// The Ceres elevator shaft's exit program; returns the screen from the turning
    /// floor to the ordinary room display.
    /// </summary>
    DoorASM_FromCeresElevatorShaft = 83,
}
