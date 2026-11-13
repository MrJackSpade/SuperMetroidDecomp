namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The setup a door runs while Samus passes through it: the complete set the retail doors name, plus none.
/// The values are arbitrary; the import pipeline maps the doors' setups to them (#1414).
/// </summary>
public enum DoorSetupCode
{
    /// <summary>The door definition names no setup.</summary>
    None = 1,

    /// <summary>Starts the Wrecked Ship treadmill for Samus entering from the west.</summary>
    DoorASM_StartWreckedShipTreadmillWestEntrance = 2,

    /// <summary>
    /// Turns room scroll cell 6 green.
    /// </summary>
    DoorCode_Scroll6_Green = 3,

    /// <summary>Turns room scroll cell 0 blue.</summary>
    DoorASM_Scroll_0_Blue = 4,

    /// <summary>Turns room scroll cell 19 blue.</summary>
    DoorASM_Scroll_13_Blue = 5,

    /// <summary>Turns room scroll cell 4 red and cell 8 green.</summary>
    DoorASM_Scroll_4_Red_8_Green = 6,

    /// <summary>Turns room scroll cells 8, 9, 10 and 11 red.</summary>
    DoorASM_Scroll_8_9_A_B_Red = 7,

    /// <summary>Turns room scroll cells 2, 3, 4, 5, 11, 12, 13 and 17 red.</summary>
    DoorASM_Scroll_2_3_4_5_B_C_D_11_Red = 8,

    /// <summary>Turns room scroll cells 1 and 4 green.</summary>
    DoorASM_Scroll_1_4_Green = 9,

    /// <summary>Turns room scroll cell 2 blue.</summary>
    DoorASM_Scroll_2_Blue = 10,

    /// <summary>Turns room scroll cell 23 blue.</summary>
    DoorASM_Scroll_17_Blue = 11,

    /// <summary>Turns room scroll cell 4 blue.</summary>
    DoorASM_Scroll_4_Blue = 12,

    /// <summary>Turns room scroll cell 6 green. A second setup making the same change.</summary>
    DoorASM_Scroll_6_Green_duplicate = 13,

    /// <summary>Turns room scroll cell 3 green.</summary>
    DoorASM_Scroll_3_Green = 14,

    /// <summary>Turns room scroll cells 24 and 28 green.</summary>
    DoorASM_Scroll_18_1C_Green = 15,

    /// <summary>Turns room scroll cells 5 and 6 blue.</summary>
    DoorASM_Scroll_5_6_Blue = 16,

    /// <summary>Turns room scroll cell 29 blue.</summary>
    DoorASM_Scroll_1D_Blue = 17,

    /// <summary>Turns room scroll cells 2 and 3 green.</summary>
    DoorASM_Scroll_2_3_Green = 18,

    /// <summary>Turns room scroll cell 0 red and cell 1 green.</summary>
    DoorASM_Scroll_0_Red_1_Green = 19,

    /// <summary>Turns room scroll cell 11 green.</summary>
    DoorASM_Scroll_B_Green = 20,

    /// <summary>Turns room scroll cell 28 red and cell 29 blue.</summary>
    DoorASM_Scroll_Scroll_1C_Red_1D_Blue = 21,

    /// <summary>Turns room scroll cell 4 red.</summary>
    DoorASM_Scroll_4_Red = 22,

    /// <summary>Turns room scroll cells 32, 36 and 37 green.</summary>
    DoorASM_Scroll_20_24_25_Green = 23,

    /// <summary>Turns room scroll cell 2 blue. A second setup making the same change.</summary>
    DoorASM_Scroll_2_Blue_duplicate = 24,

    /// <summary>Turns room scroll cell 0 green.</summary>
    DoorASM_Scroll_0_Green = 25,

    /// <summary>Turns room scroll cells 6 and 7 green.</summary>
    DoorASM_Scroll_6_7_Green = 26,

    /// <summary>Turns room scroll cell 1 blue and cell 2 red.</summary>
    DoorASM_Scroll_1_Blue_2_Red = 27,

    /// <summary>Turns room scroll cell 1 blue and cell 3 red.</summary>
    DoorASM_Scroll_1_Blue_3_Red = 28,

    /// <summary>Turns room scroll cell 0 red and cell 4 blue.</summary>
    DoorASM_Scroll_0_Red_4_Blue = 29,

    /// <summary>Turns room scroll cells 2 and 3 blue.</summary>
    DoorASM_Scroll_2_3_Blue = 30,

    /// <summary>Turns room scroll cells 0 and 1 green.</summary>
    DoorASM_Scroll_0_1_Green = 31,

    /// <summary>Turns room scroll cell 1 green.</summary>
    DoorASM_Scroll_1_Green = 32,

    /// <summary>Turns room scroll cells 15 and 18 green.</summary>
    DoorASM_Scroll_F_12_Green = 33,

    /// <summary>Turns room scroll cell 6 green. A third setup making the same change.</summary>
    DoorASM_Scroll_6_Green_duplicate_again = 34,

    /// <summary>
    /// Turns room scroll cell 0 green and cell 1 blue, opening Construction Zone's
    /// two-screen vertical camera route after returning from First Missile.
    /// </summary>
    DoorASM_Scroll_0_Green_1_Blue = 35,

    /// <summary>Turns room scroll cell 2 green.</summary>
    DoorASM_Scroll_2_Green = 36,

    /// <summary>Turns room scroll cells 3 and 4 red and cells 6, 7 and 8 blue.</summary>
    DoorASM_Scroll_3_4_Red_6_7_8_Blue = 37,

    /// <summary>Turns room scroll cells 1, 2 and 3 blue, cell 4 green and cell 6 red.</summary>
    DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red = 38,

    /// <summary>Turns room scroll cells 0 and 1 blue.</summary>
    DoorASM_Scroll_0_1_Blue = 39,

    /// <summary>Turns room scroll cell 0 blue and cell 1 red.</summary>
    DoorASM_Scroll_0_Blue_1_Red = 40,

    /// <summary>Turns room scroll cell 10 green.</summary>
    DoorASM_Scroll_A_Green = 41,

    /// <summary>Turns room scroll cells 0 and 2 green.</summary>
    DoorASM_Scroll_0_2_Green = 42,

    /// <summary>Turns room scroll cells 6 and 7 blue and cell 8 red.</summary>
    DoorASM_Scroll_6_7_Blue_8_Red = 43,

    /// <summary>Turns room scroll cell 2 red and cell 3 blue.</summary>
    DoorASM_Scroll_2_Red_3_Blue = 44,

    /// <summary>Turns room scroll cell 7 green.</summary>
    DoorASM_Scroll_7_Green = 45,

    /// <summary>Turns room scroll cell 1 red and cell 2 blue.</summary>
    DoorASM_Scroll_1_Red_2_Blue = 46,

    /// <summary>Turns room scroll cell 0 blue and cell 3 red.</summary>
    DoorASM_Scroll_0_Blue_3_Red = 47,

    /// <summary>Turns room scroll cell 1 blue and cell 4 red.</summary>
    DoorASM_Scroll_1_Blue_4_Red = 48,

    /// <summary>Turns room scroll cell 0 blue and cells 1, 2 and 3 red.</summary>
    DoorASM_Scroll_0_Blue_1_2_3_Red = 49,

    /// <summary>Turns room scroll cell 0 green. A second setup making the same change.</summary>
    DoorASM_Scroll_0_Green_duplicate = 50,

    /// <summary>Turns room scroll cells 0 and 1 blue and cell 4 red.</summary>
    DoorASM_Scroll_0_1_Blue_4_Red = 51,

    /// <summary>Turns room scroll cell 0 blue and cell 3 red. A second setup making the same change.</summary>
    DoorASM_Scroll_0_Blue_3_Red_duplicate = 52,

    /// <summary>Turns room scroll cell 0 blue. A second setup making the same change.</summary>
    DoorASM_Scroll_0_Blue_duplicate = 53,

    /// <summary>Turns room scroll cell 0 blue and cell 1 red. A second setup making the same change.</summary>
    DoorASM_Scroll_0_Blue_1_Red_duplicate = 54,

    /// <summary>Turns room scroll cell 24 blue.</summary>
    DoorASM_Scroll_18_Blue = 55,

    /// <summary>Turns room scroll cell 2 blue and cell 3 red.</summary>
    DoorASM_Scroll_2_Blue_3_Red = 56,

    /// <summary>Turns room scroll cell 14 red.</summary>
    DoorASM_Scroll_E_Red = 57,

    /// <summary>Starts the Wrecked Ship treadmill for Samus entering from the east.</summary>
    DoorASM_StartWreckedShipTreadmillEastEntrance = 58,

    /// <summary>Turns room scroll cell 1 blue.</summary>
    DoorASM_Scroll_1_Blue = 59,

    /// <summary>Turns room scroll cell 0 green. A third setup making the same change.</summary>
    DoorASM_Scroll_0_Green_duplicate_again = 60,

    /// <summary>Turns room scroll cell 3 red and cell 4 blue.</summary>
    DoorASM_Scroll_3_Red_4_Blue = 61,

    /// <summary>Turns room scroll cell 41 blue.</summary>
    DoorASM_Scroll_29_Blue = 62,

    /// <summary>Turns room scroll cells 40 and 46 green.</summary>
    DoorASM_Scroll_28_2E_Green = 63,

    /// <summary>Turns room scroll cells 6, 7, 8, 9, 10 and 11 red.</summary>
    DoorASM_Scroll_6_7_8_9_A_B_Red = 64,

    /// <summary>Sets up the Maridia elevatube for Samus arriving from the south.</summary>
    DoorASM_SetupElevatubeFromSouth = 65,

    /// <summary>Sets up the Maridia elevatube for Samus arriving from the north.</summary>
    DoorASM_SetupElevatubeFromNorth = 66,

    /// <summary>Resets the Maridia elevatube when Samus leaves by its north door.</summary>
    DoorASM_ResetElevatubeOnNorthExit = 67,

    /// <summary>Resets the Maridia elevatube when Samus leaves by its south door.</summary>
    DoorASM_ResetElevatubeOnSouthExit = 68,

    /// <summary>Turns room scroll cell 10 red and cell 11 blue.</summary>
    DoorASM_Scroll_A_Red_B_Blue = 69,

    /// <summary>Turns room scroll cell 0 red and cell 4 blue. A second setup making the same change.</summary>
    DoorASM_Scroll_0_Red_4_Blue_duplicate = 70,

    /// <summary>Turns room scroll cell 0 red and cell 1 blue.</summary>
    DoorASM_Scroll_0_Red_1_Blue = 71,

    /// <summary>Turns room scroll cell 9 red and cell 10 blue.</summary>
    DoorASM_Scroll_9_Red_A_Blue = 72,

    /// <summary>Turns room scroll cells 0 and 2 red and cell 1 blue.</summary>
    DoorASM_Scroll_0_2_Red_1_Blue = 73,

    /// <summary>Turns room scroll cell 1 blue. A second setup making the same change.</summary>
    DoorASM_Scroll_1_Blue_duplicate = 74,

    /// <summary>Turns room scroll cell 6 blue.</summary>
    DoorASM_Scroll_6_Blue = 75,

    /// <summary>Turns room scroll cell 4 red. A second setup making the same change.</summary>
    DoorASM_Scroll_4_Red_duplicate = 76,

    /// <summary>Turns room scroll cells 4 and 7 red.</summary>
    DoorASM_Scroll_4_7_Red = 77,

    /// <summary>Turns room scroll cell 1 blue and cell 2 red. A second setup making the same change.</summary>
    DoorASM_Scroll_1_Blue_2_Red_duplicate = 78,

    /// <summary>Turns room scroll cells 0 and 2 green. A second setup making the same change.</summary>
    DoorASM_Scroll_0_2_Green_duplicate = 79,

    /// <summary>Turns room scroll cells 0 and 1 green. A second setup making the same change.</summary>
    DoorASM_Scroll_0_1_Green_duplicate = 80,

    /// <summary>
    /// Door callback. Its traditional name says cells 8/9, but the
    /// door's own operands are cells 24 and 25.
    /// </summary>
    DoorASM_Scroll_18_Blue_19_Red = 81,

    /// <summary>
    /// Entering the Ceres elevator shaft: selects the turning floor and installs
    /// the shaft's initial rotation and center.
    /// </summary>
    DoorASM_ToCeresElevatorShaft = 82,

    /// <summary>
    /// The Ceres elevator shaft's exit program; returns the screen from the turning
    /// floor to the ordinary room display.
    /// </summary>
    DoorASM_FromCeresElevatorShaft = 83,
}
