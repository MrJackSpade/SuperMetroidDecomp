namespace SuperMetroid.Core.Game;

/// <summary>
/// The next of the eight setup calls in the X-ray HDMA instruction list
/// (<c>InstList_HDMAObject_Xray_0</c>, <c>$91:D22E-$D26F</c>) to run, or
/// <see cref="Complete"/> once the list has installed <c>PreInstruction_Xray_Main</c>.
/// Native code keeps this as the instruction-list position; the port names each call.
/// </summary>
public enum XraySetupStage : byte
{
    /// <summary>All eight calls ran; the main pre-instruction <c>$91:D27F</c> successor owns the beam.</summary>
    Complete = 0,
    /// <summary><c>XraySetup_1_FreezeTime_BackupBG2Registers</c>, called at <c>$91:D22E</c>.</summary>
    FreezeTimeBackupBg2Registers = 1,
    /// <summary><c>XraySetup_2_ReadBG1Tilemap_2ndScreen</c> (<c>$91:CB1C</c>), called at <c>$91:D237</c>.</summary>
    ReadBg1SecondScreen = 2,
    /// <summary><c>XraySetup_3_ReadBG1Tilemap_1stScreen</c> (<c>$91:CB57</c>), called at <c>$91:D240</c>.</summary>
    ReadBg1FirstScreen = 3,
    /// <summary><c>XraySetup_4_BuildBG2Tilemap_ReadBG2Tilemap_1stScreen</c> (<c>$91:CB8E</c>), called at <c>$91:D249</c>.</summary>
    BuildRevealReadBg2FirstScreen = 4,
    /// <summary><c>XraySetup_5_ReadBG2Tilemap_2ndScreen</c>, called at <c>$91:D252</c>.</summary>
    ReadBg2SecondScreen = 5,
    /// <summary><c>XraySetup_6_TransferXrayTilemap_1stScreen</c>, called at <c>$91:D25B</c>.</summary>
    TransferRevealFirstScreen = 6,
    /// <summary><c>XraySetup_7_InitializeXray_TransferXrayTilemap_2ndScreen</c>, called at <c>$91:D264</c>.</summary>
    InitializeTransferRevealSecondScreen = 7,
    /// <summary><c>XraySetup_8_BackdropColor</c>, called at <c>$91:D26D</c>.</summary>
    BackdropColor = 8,
}

/// <summary>Bounded transitions and predicates over <see cref="XraySetupStage"/>.</summary>
public static class XraySetupStages
{
    /// <summary>The call that follows <paramref name="stage"/>; the eighth call completes setup.</summary>
    public static XraySetupStage Advance(this XraySetupStage stage) => stage switch
    {
        XraySetupStage.FreezeTimeBackupBg2Registers => XraySetupStage.ReadBg1SecondScreen,
        XraySetupStage.ReadBg1SecondScreen => XraySetupStage.ReadBg1FirstScreen,
        XraySetupStage.ReadBg1FirstScreen => XraySetupStage.BuildRevealReadBg2FirstScreen,
        XraySetupStage.BuildRevealReadBg2FirstScreen => XraySetupStage.ReadBg2SecondScreen,
        XraySetupStage.ReadBg2SecondScreen => XraySetupStage.TransferRevealFirstScreen,
        XraySetupStage.TransferRevealFirstScreen => XraySetupStage.InitializeTransferRevealSecondScreen,
        XraySetupStage.InitializeTransferRevealSecondScreen => XraySetupStage.BackdropColor,
        XraySetupStage.BackdropColor => XraySetupStage.Complete,
        XraySetupStage.Complete => throw new InvalidOperationException("X-ray setup has already completed."),
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined X-ray setup stage."),
    };

    /// <summary>
    /// Whether the room is drawn with X-ray blending while setup is still running. <c>$91:D223</c>
    /// installs the blending pre-instruction <c>$91:D27F</c> before call one; it first executes
    /// after call one, by which point call two has also run, so blending shows from the third call on.
    /// </summary>
    public static bool ShowsBlendedRoom(this XraySetupStage stage) => stage switch
    {
        XraySetupStage.FreezeTimeBackupBg2Registers or XraySetupStage.ReadBg1SecondScreen => false,
        XraySetupStage.ReadBg1FirstScreen or XraySetupStage.BuildRevealReadBg2FirstScreen or
            XraySetupStage.ReadBg2SecondScreen or XraySetupStage.TransferRevealFirstScreen or
            XraySetupStage.InitializeTransferRevealSecondScreen or XraySetupStage.BackdropColor or
            XraySetupStage.Complete => true,
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined X-ray setup stage."),
    };
}
