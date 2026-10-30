namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Bounded transitions and predicates over <see cref="XraySetupStage"/>.</summary>
public static class XraySetupStages
{
    /// <summary>
    /// The call that follows <paramref name="stage"/>, in the stages' order; the eighth call completes setup, whose stage
    /// is the order's first value.
    /// </summary>
    public static XraySetupStage Advance(this XraySetupStage stage) => stage switch
    {
        XraySetupStage.Complete => throw new InvalidOperationException("X-ray setup has already completed."),
        XraySetupStage.FreezeTimeSaveBackground or XraySetupStage.ReadBg1SecondScreen or XraySetupStage.ReadBg1FirstScreen or
            XraySetupStage.BuildRevealFirstScreen or XraySetupStage.BuildRevealSecondScreen or
            XraySetupStage.TransferRevealFirstScreen or XraySetupStage.InitializeTransferRevealSecondScreen or
            XraySetupStage.BackdropColor => EnumOrder.NextWrapping(stage),
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined X-ray setup stage."),
    };

    /// <summary>
    /// Whether the room is drawn with X-ray blending while setup is still running. <c>$91:D223</c>
    /// installs the blending pre-instruction <c>$91:D27F</c> before call one; it first executes
    /// after call one, by which point call two has also run, so blending shows from the third call on.
    /// </summary>
    public static bool ShowsBlendedRoom(this XraySetupStage stage) => stage switch
    {
        XraySetupStage.FreezeTimeSaveBackground or XraySetupStage.ReadBg1SecondScreen => false,
        XraySetupStage.ReadBg1FirstScreen or XraySetupStage.BuildRevealFirstScreen or
            XraySetupStage.BuildRevealSecondScreen or XraySetupStage.TransferRevealFirstScreen or
            XraySetupStage.InitializeTransferRevealSecondScreen or XraySetupStage.BackdropColor or
            XraySetupStage.Complete => true,
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined X-ray setup stage."),
    };
}
