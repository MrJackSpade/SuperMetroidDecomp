using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

/// <summary>Checks the closed X-Ray setup-stage order, display rule, and debugger decoding boundary.</summary>
internal static class XraySetupStageDomainVerification
{
    /// <summary>
    /// #627: the X-ray setup stage is a closed domain. Every call advances through a bounded
    /// transition, the display rule is defined for every member, and the debugger boundary
    /// decodes older byte payloads while rejecting undefined values.
    /// </summary>
    internal static void VerifyXraySetupStageDomain()
    {
        XraySetupStage[] order =
        [
            XraySetupStage.FreezeTimeBackupBg2Registers,
            XraySetupStage.ReadBg1SecondScreen,
            XraySetupStage.ReadBg1FirstScreen,
            XraySetupStage.BuildRevealReadBg2FirstScreen,
            XraySetupStage.ReadBg2SecondScreen,
            XraySetupStage.TransferRevealFirstScreen,
            XraySetupStage.InitializeTransferRevealSecondScreen,
            XraySetupStage.BackdropColor,
            XraySetupStage.Complete,
        ];
        AssertEqual(Enum.GetValues<XraySetupStage>().Length, order.Length, "every X-ray setup stage is in the call order");
        for (int index = 0; index + 1 < order.Length; index++)
            AssertEqual(order[index + 1], order[index].Advance(), $"X-ray setup call {order[index]} advances in order");
        AssertThrows<InvalidOperationException>(() => XraySetupStage.Complete.Advance(), "completed setup cannot advance");
        AssertThrows<ArgumentOutOfRangeException>(() => ((XraySetupStage)9).Advance(), "undefined stage cannot advance");

        // $91:D27F first runs after call one, together with call two: blending shows from the third call.
        foreach (XraySetupStage stage in order)
        {
            bool expected = stage is not (XraySetupStage.FreezeTimeBackupBg2Registers or XraySetupStage.ReadBg1SecondScreen);
            AssertEqual(expected, stage.ShowsBlendedRoom(), $"X-ray stage {stage} blending");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => ((XraySetupStage)0xff).ShowsBlendedRoom(), "undefined stage has no display rule");

        // Production stepping walks the same order.
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)) };
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        AssertTrue(samus.Xray.TryBegin(bus, samus, previousMovementType: SamusMovementType.Standing, conditions: new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }),
            "X-ray setup accepted");
        foreach (XraySetupStage expected in order)
        {
            AssertEqual(expected, samus.Xray.SetupStage, $"production X-ray setup reaches {expected}");
            if (expected != XraySetupStage.Complete)
                samus.Xray.StepBeam(bus, samus, 0);
        }

        // Debugger-state boundary: undefined saved enum values are rejected.
        AssertThrows<InvalidDataException>(() => DebuggerEnumValues.Decode(typeof(XraySuspendedSubsystems), (byte)0x80),
            "undefined flags bits are rejected at the boundary");
        Console.WriteLine("X-ray setup stage domain: bounded transitions, display rule, production order and debugger boundary agree.");
    }
}
