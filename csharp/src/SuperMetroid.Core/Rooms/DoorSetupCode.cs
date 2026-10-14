namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bank-$8F entry points used as setup programs by cartridge door headers: the complete
/// set of nonzero setup words in the retail bank-$83 door records, plus zero for none.
/// </summary>
public enum DoorSetupCode : ushort
{
    /// <summary>The door header names no setup routine.</summary>
    None = 0,

    /// <summary><c>DoorASM_StartWreckedShipTreadmillWestEntrance</c> at $8F:B971.</summary>
    DoorASM_StartWreckedShipTreadmillWestEntrance = 0xb971,

    /// <summary>
    /// <c>DoorCode_Scroll6_Green</c> at $8F:B981; changes room-scroll storage cell six
    /// to green.
    /// </summary>
    DoorCode_Scroll6_Green = 0xb981,

    /// <summary><c>DoorASM_Scroll_0_Blue</c> at $8F:B98C.</summary>
    DoorASM_Scroll_0_Blue = 0xb98c,

    /// <summary><c>DoorASM_Scroll_13_Blue</c> at $8F:B997.</summary>
    DoorASM_Scroll_13_Blue = 0xb997,

    /// <summary><c>DoorASM_Scroll_4_Red_8_Green</c> at $8F:B9A2.</summary>
    DoorASM_Scroll_4_Red_8_Green = 0xb9a2,

    /// <summary><c>DoorASM_Scroll_8_9_A_B_Red</c> at $8F:B9B3.</summary>
    DoorASM_Scroll_8_9_A_B_Red = 0xb9b3,

    /// <summary><c>DoorASM_Scroll_2_3_4_5_B_C_D_11_Red</c> at $8F:B9CA.</summary>
    DoorASM_Scroll_2_3_4_5_B_C_D_11_Red = 0xb9ca,

    /// <summary><c>DoorASM_Scroll_1_4_Green</c> at $8F:B9F1.</summary>
    DoorASM_Scroll_1_4_Green = 0xb9f1,

    /// <summary><c>DoorASM_Scroll_2_Blue</c> at $8F:BA00.</summary>
    DoorASM_Scroll_2_Blue = 0xba00,

    /// <summary><c>DoorASM_Scroll_17_Blue</c> at $8F:BA0B.</summary>
    DoorASM_Scroll_17_Blue = 0xba0b,

    /// <summary><c>DoorASM_Scroll_4_Blue</c> at $8F:BA16.</summary>
    DoorASM_Scroll_4_Blue = 0xba16,

    /// <summary><c>DoorASM_Scroll_6_Green_duplicate</c> at $8F:BA21.</summary>
    DoorASM_Scroll_6_Green_duplicate = 0xba21,

    /// <summary><c>DoorASM_Scroll_3_Green</c> at $8F:BA2C.</summary>
    DoorASM_Scroll_3_Green = 0xba2c,

    /// <summary><c>DoorASM_Scroll_18_1C_Green</c> at $8F:BD07.</summary>
    DoorASM_Scroll_18_1C_Green = 0xbd07,

    /// <summary><c>DoorASM_Scroll_5_6_Blue</c> at $8F:BD16.</summary>
    DoorASM_Scroll_5_6_Blue = 0xbd16,

    /// <summary><c>DoorASM_Scroll_1D_Blue</c> at $8F:BD25.</summary>
    DoorASM_Scroll_1D_Blue = 0xbd25,

    /// <summary><c>DoorASM_Scroll_2_3_Green</c> at $8F:BD30.</summary>
    DoorASM_Scroll_2_3_Green = 0xbd30,

    /// <summary><c>DoorASM_Scroll_0_Red_1_Green</c> at $8F:BD3F.</summary>
    DoorASM_Scroll_0_Red_1_Green = 0xbd3f,

    /// <summary><c>DoorASM_Scroll_B_Green</c> at $8F:BD50.</summary>
    DoorASM_Scroll_B_Green = 0xbd50,

    /// <summary><c>DoorASM_Scroll_Scroll_1C_Red_1D_Blue</c> at $8F:BD5B.</summary>
    DoorASM_Scroll_Scroll_1C_Red_1D_Blue = 0xbd5b,

    /// <summary><c>DoorASM_Scroll_4_Red</c> at $8F:BD6C.</summary>
    DoorASM_Scroll_4_Red = 0xbd6c,

    /// <summary><c>DoorASM_Scroll_20_24_25_Green</c> at $8F:BD77.</summary>
    DoorASM_Scroll_20_24_25_Green = 0xbd77,

    /// <summary><c>DoorASM_Scroll_2_Blue_duplicate</c> at $8F:BD8A.</summary>
    DoorASM_Scroll_2_Blue_duplicate = 0xbd8a,

    /// <summary><c>DoorASM_Scroll_0_Green</c> at $8F:BD95.</summary>
    DoorASM_Scroll_0_Green = 0xbd95,

    /// <summary><c>DoorASM_Scroll_6_7_Green</c> at $8F:BDA0.</summary>
    DoorASM_Scroll_6_7_Green = 0xbda0,

    /// <summary><c>DoorASM_Scroll_1_Blue_2_Red</c> at $8F:BDAF.</summary>
    DoorASM_Scroll_1_Blue_2_Red = 0xbdaf,

    /// <summary><c>DoorASM_Scroll_1_Blue_3_Red</c> at $8F:BDC0.</summary>
    DoorASM_Scroll_1_Blue_3_Red = 0xbdc0,

    /// <summary><c>DoorASM_Scroll_0_Red_4_Blue</c> at $8F:BDD1.</summary>
    DoorASM_Scroll_0_Red_4_Blue = 0xbdd1,

    /// <summary><c>DoorASM_Scroll_2_3_Blue</c> at $8F:BDE2.</summary>
    DoorASM_Scroll_2_3_Blue = 0xbde2,

    /// <summary><c>DoorASM_Scroll_0_1_Green</c> at $8F:BDF1.</summary>
    DoorASM_Scroll_0_1_Green = 0xbdf1,

    /// <summary><c>DoorASM_Scroll_1_Green</c> at $8F:BE00.</summary>
    DoorASM_Scroll_1_Green = 0xbe00,

    /// <summary><c>DoorASM_Scroll_F_12_Green</c> at $8F:BE0B.</summary>
    DoorASM_Scroll_F_12_Green = 0xbe0b,

    /// <summary><c>DoorASM_Scroll_6_Green_duplicate_again</c> at $8F:BE1A.</summary>
    DoorASM_Scroll_6_Green_duplicate_again = 0xbe1a,

    /// <summary>
    /// <c>DoorASM_Scroll_0_Green_1_Blue</c> at $8F:BE25; opens Construction Zone's
    /// two-screen vertical camera route after returning from First Missile.
    /// </summary>
    DoorASM_Scroll_0_Green_1_Blue = 0xbe25,

    /// <summary><c>DoorASM_Scroll_2_Green</c> at $8F:BE36.</summary>
    DoorASM_Scroll_2_Green = 0xbe36,

    /// <summary><c>DoorASM_Scroll_3_4_Red_6_7_8_Blue</c> at $8F:BF9E.</summary>
    DoorASM_Scroll_3_4_Red_6_7_8_Blue = 0xbf9e,

    /// <summary><c>DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red</c> at $8F:BFBB.</summary>
    DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red = 0xbfbb,

    /// <summary><c>DoorASM_Scroll_0_1_Blue</c> at $8F:BFDA.</summary>
    DoorASM_Scroll_0_1_Blue = 0xbfda,

    /// <summary><c>DoorASM_Scroll_0_Blue_1_Red</c> at $8F:BFE9.</summary>
    DoorASM_Scroll_0_Blue_1_Red = 0xbfe9,

    /// <summary><c>DoorASM_Scroll_A_Green</c> at $8F:BFFA.</summary>
    DoorASM_Scroll_A_Green = 0xbffa,

    /// <summary><c>DoorASM_Scroll_0_2_Green</c> at $8F:C016.</summary>
    DoorASM_Scroll_0_2_Green = 0xc016,

    /// <summary><c>DoorASM_Scroll_6_7_Blue_8_Red</c> at $8F:C025.</summary>
    DoorASM_Scroll_6_7_Blue_8_Red = 0xc025,

    /// <summary><c>DoorASM_Scroll_2_Red_3_Blue</c> at $8F:C03A.</summary>
    DoorASM_Scroll_2_Red_3_Blue = 0xc03a,

    /// <summary><c>DoorASM_Scroll_7_Green</c> at $8F:C04B.</summary>
    DoorASM_Scroll_7_Green = 0xc04b,

    /// <summary><c>DoorASM_Scroll_1_Red_2_Blue</c> at $8F:C056.</summary>
    DoorASM_Scroll_1_Red_2_Blue = 0xc056,

    /// <summary><c>DoorASM_Scroll_0_Blue_3_Red</c> at $8F:C067.</summary>
    DoorASM_Scroll_0_Blue_3_Red = 0xc067,

    /// <summary><c>DoorASM_Scroll_1_Blue_4_Red</c> at $8F:C078.</summary>
    DoorASM_Scroll_1_Blue_4_Red = 0xc078,

    /// <summary><c>DoorASM_Scroll_0_Blue_1_2_3_Red</c> at $8F:C089.</summary>
    DoorASM_Scroll_0_Blue_1_2_3_Red = 0xc089,

    /// <summary><c>DoorASM_Scroll_0_Green_duplicate</c> at $8F:C0A2.</summary>
    DoorASM_Scroll_0_Green_duplicate = 0xc0a2,

    /// <summary><c>DoorASM_Scroll_0_1_Blue_4_Red</c> at $8F:C0AD.</summary>
    DoorASM_Scroll_0_1_Blue_4_Red = 0xc0ad,

    /// <summary><c>DoorASM_Scroll_0_Blue_3_Red_duplicate</c> at $8F:C0C2.</summary>
    DoorASM_Scroll_0_Blue_3_Red_duplicate = 0xc0c2,

    /// <summary><c>DoorASM_Scroll_0_Blue_duplicate</c> at $8F:C0D3.</summary>
    DoorASM_Scroll_0_Blue_duplicate = 0xc0d3,

    /// <summary><c>DoorASM_Scroll_0_Blue_1_Red_duplicate</c> at $8F:C0DE.</summary>
    DoorASM_Scroll_0_Blue_1_Red_duplicate = 0xc0de,

    /// <summary><c>DoorASM_Scroll_18_Blue</c> at $8F:C0EF.</summary>
    DoorASM_Scroll_18_Blue = 0xc0ef,

    /// <summary><c>DoorASM_Scroll_2_Blue_3_Red</c> at $8F:C0FA.</summary>
    DoorASM_Scroll_2_Blue_3_Red = 0xc0fa,

    /// <summary><c>DoorASM_Scroll_E_Red</c> at $8F:C10B.</summary>
    DoorASM_Scroll_E_Red = 0xc10b,

    /// <summary><c>DoorASM_StartWreckedShipTreadmillEastEntrance</c> at $8F:E1D8.</summary>
    DoorASM_StartWreckedShipTreadmillEastEntrance = 0xe1d8,

    /// <summary><c>DoorASM_Scroll_1_Blue</c> at $8F:E1E8.</summary>
    DoorASM_Scroll_1_Blue = 0xe1e8,

    /// <summary><c>DoorASM_Scroll_0_Green_duplicate_again</c> at $8F:E1F3.</summary>
    DoorASM_Scroll_0_Green_duplicate_again = 0xe1f3,

    /// <summary><c>DoorASM_Scroll_3_Red_4_Blue</c> at $8F:E1FE.</summary>
    DoorASM_Scroll_3_Red_4_Blue = 0xe1fe,

    /// <summary><c>DoorASM_Scroll_29_Blue</c> at $8F:E20F.</summary>
    DoorASM_Scroll_29_Blue = 0xe20f,

    /// <summary><c>DoorASM_Scroll_28_2E_Green</c> at $8F:E21A.</summary>
    DoorASM_Scroll_28_2E_Green = 0xe21a,

    /// <summary><c>DoorASM_Scroll_6_7_8_9_A_B_Red</c> at $8F:E229.</summary>
    DoorASM_Scroll_6_7_8_9_A_B_Red = 0xe229,

    /// <summary><c>DoorASM_SetupElevatubeFromSouth</c> at $8F:E26C.</summary>
    DoorASM_SetupElevatubeFromSouth = 0xe26c,

    /// <summary><c>DoorASM_SetupElevatubeFromNorth</c> at $8F:E291.</summary>
    DoorASM_SetupElevatubeFromNorth = 0xe291,

    /// <summary><c>DoorASM_ResetElevatubeOnNorthExit</c> at $8F:E301.</summary>
    DoorASM_ResetElevatubeOnNorthExit = 0xe301,

    /// <summary><c>DoorASM_ResetElevatubeOnSouthExit</c> at $8F:E309.</summary>
    DoorASM_ResetElevatubeOnSouthExit = 0xe309,

    /// <summary><c>DoorASM_Scroll_A_Red_B_Blue</c> at $8F:E318.</summary>
    DoorASM_Scroll_A_Red_B_Blue = 0xe318,

    /// <summary><c>DoorASM_Scroll_0_Red_4_Blue_duplicate</c> at $8F:E345.</summary>
    DoorASM_Scroll_0_Red_4_Blue_duplicate = 0xe345,

    /// <summary><c>DoorASM_Scroll_0_Red_1_Blue</c> at $8F:E356.</summary>
    DoorASM_Scroll_0_Red_1_Blue = 0xe356,

    /// <summary><c>DoorASM_Scroll_9_Red_A_Blue</c> at $8F:E367.</summary>
    DoorASM_Scroll_9_Red_A_Blue = 0xe367,

    /// <summary><c>DoorASM_Scroll_0_2_Red_1_Blue</c> at $8F:E378.</summary>
    DoorASM_Scroll_0_2_Red_1_Blue = 0xe378,

    /// <summary><c>DoorASM_Scroll_1_Blue_duplicate</c> at $8F:E38D.</summary>
    DoorASM_Scroll_1_Blue_duplicate = 0xe38d,

    /// <summary><c>DoorASM_Scroll_6_Blue</c> at $8F:E398.</summary>
    DoorASM_Scroll_6_Blue = 0xe398,

    /// <summary><c>DoorASM_Scroll_4_Red_duplicate</c> at $8F:E3A3.</summary>
    DoorASM_Scroll_4_Red_duplicate = 0xe3a3,

    /// <summary><c>DoorASM_Scroll_4_7_Red</c> at $8F:E3B9.</summary>
    DoorASM_Scroll_4_7_Red = 0xe3b9,

    /// <summary><c>DoorASM_Scroll_1_Blue_2_Red_duplicate</c> at $8F:E3C8.</summary>
    DoorASM_Scroll_1_Blue_2_Red_duplicate = 0xe3c8,

    /// <summary><c>DoorASM_Scroll_0_2_Green_duplicate</c> at $8F:E3D9.</summary>
    DoorASM_Scroll_0_2_Green_duplicate = 0xe3d9,

    /// <summary><c>DoorASM_Scroll_0_1_Green_duplicate</c> at $8F:E4C0.</summary>
    DoorASM_Scroll_0_1_Green_duplicate = 0xe4c0,

    /// <summary>
    /// Door callback at $8F:E4CF. The inherited disassembly label says cells 8/9, but the
    /// cartridge operands are <c>Scrolls+$18</c> and <c>Scrolls+$19</c>.
    /// </summary>
    DoorASM_Scroll_18_Blue_19_Red = 0xe4cf,

    /// <summary>
    /// <c>DoorASM_ToCeresElevatorShaft</c> at $8F:E4E0; selects Mode 7 and installs
    /// the shaft's initial matrix and center.
    /// </summary>
    DoorASM_ToCeresElevatorShaft = 0xe4e0,

    /// <summary>
    /// <c>DoorASM_FromCeresElevatorShaft</c> at $8F:E513; restores ordinary Mode 1
    /// rendering and disables Mode-7 IRQ/transfer handling.
    /// </summary>
    DoorASM_FromCeresElevatorShaft = 0xe513,
}
