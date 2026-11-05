namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The next of the eight setup updates of the X-ray cone effect to run, or
/// <see cref="Complete"/> once setup has handed the cone to its per-update beam step.
/// Each member names one setup update.
/// </summary>
public enum XraySetupStage
{
    /// <summary>All eight calls ran; the main frame behaviour's successor owns the beam.</summary>
    Complete = 0,
    /// <summary>
    /// Call one: time freezes and the background's view is kept to restore when the beam ends
    /// (<c>XraySetup_1_FreezeTime_BackupBG2Registers</c>, called).
    /// </summary>
    FreezeTimeSaveBackground = 1,
    /// <summary><c>XraySetup_2_ReadBG1Tilemap_2ndScreen</c>, called.</summary>
    ReadForegroundSecondScreen = 2,
    /// <summary><c>XraySetup_3_ReadBG1Tilemap_1stScreen</c>, called.</summary>
    ReadForegroundFirstScreen = 3,
    /// <summary>
    /// Call four, where the game builds the reveal (<c>XraySetup_4_BuildBG2Tilemap_ReadBG2Tilemap_1stScreen</c>, called);
    /// the port shows the reveal from the room as each frame shows it, so the call only spends its update.
    /// </summary>
    BuildRevealFirstScreen = 4,
    /// <summary>
    /// Call five, where the game builds the reveal's second screen (<c>XraySetup_5_ReadBG2Tilemap_2ndScreen</c>,
    /// called); the call only spends its update.
    /// </summary>
    BuildRevealSecondScreen = 5,
    /// <summary><c>XraySetup_6_TransferXrayTilemap_1stScreen</c>, called.</summary>
    TransferRevealFirstScreen = 6,
    /// <summary><c>XraySetup_7_InitializeXray_TransferXrayTilemap_2ndScreen</c>, called.</summary>
    InitializeTransferRevealSecondScreen = 7,
    /// <summary><c>XraySetup_8_BackdropColor</c>, called.</summary>
    BackdropColor = 8,
}
