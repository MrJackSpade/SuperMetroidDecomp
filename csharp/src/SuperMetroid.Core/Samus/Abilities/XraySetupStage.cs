namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The next of the eight setup calls in the X-ray HDMA instruction list
/// (<c>InstList_HDMAObject_Xray_0</c>, <c>$91:D22E-$D26F</c>) to run, or
/// <see cref="Complete"/> once the list has installed <c>PreInstruction_Xray_Main</c>.
/// Native code keeps this as the instruction-list position; the port names each call.
/// </summary>
public enum XraySetupStage
{
    /// <summary>All eight calls ran; the main pre-instruction <c>$91:D27F</c> successor owns the beam.</summary>
    Complete = 0,
    /// <summary>
    /// Call one: time freezes and the background's view is kept to restore when the beam ends
    /// (<c>XraySetup_1_FreezeTime_BackupBG2Registers</c>, called at <c>$91:D22E</c>).
    /// </summary>
    FreezeTimeSaveBackground = 1,
    /// <summary><c>XraySetup_2_ReadBG1Tilemap_2ndScreen</c> (<c>$91:CB1C</c>), called at <c>$91:D237</c>.</summary>
    ReadBg1SecondScreen = 2,
    /// <summary><c>XraySetup_3_ReadBG1Tilemap_1stScreen</c> (<c>$91:CB57</c>), called at <c>$91:D240</c>.</summary>
    ReadBg1FirstScreen = 3,
    /// <summary>
    /// Call four, where the reveal is built: <c>XraySetup_4_BuildBG2Tilemap_ReadBG2Tilemap_1stScreen</c>
    /// (<c>$91:CB8E</c>), called at <c>$91:D249</c>. The port builds the whole reveal here.
    /// </summary>
    BuildRevealFirstScreen = 4,
    /// <summary>
    /// Call five, where the cartridge builds the reveal's second screen (<c>XraySetup_5_ReadBG2Tilemap_2ndScreen</c>,
    /// called at <c>$91:D252</c>); the port has built it already, so the call only spends its update.
    /// </summary>
    BuildRevealSecondScreen = 5,
    /// <summary><c>XraySetup_6_TransferXrayTilemap_1stScreen</c>, called at <c>$91:D25B</c>.</summary>
    TransferRevealFirstScreen = 6,
    /// <summary><c>XraySetup_7_InitializeXray_TransferXrayTilemap_2ndScreen</c>, called at <c>$91:D264</c>.</summary>
    InitializeTransferRevealSecondScreen = 7,
    /// <summary><c>XraySetup_8_BackdropColor</c>, called at <c>$91:D26D</c>.</summary>
    BackdropColor = 8,
}
