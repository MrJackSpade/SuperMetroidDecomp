using static MakeSamusFaceForwardVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Checks diagonal shinespark crash alignment and its one-time previous-position adjustment.</summary>
internal static class SparkCrashAlignmentVerification
{
    /// <summary>Confirms the delayed standing pose shifts whole Y by two pixels, retains the fraction, and consumes its previous-position checkpoint once.</summary>
    internal static void VerifySparkCrashAlignment()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (SamusPoseId pose in new SamusPoseId[] { SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkDiagonalLeftPose })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(700, 35)) };
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x1234);
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Shinespark.FinishCrash(samus, (ushort)5, null);
            AssertEqual((ushort)35, samus.Position.Y.Whole, "Movement queues standing alignment until after animation");
            SamusShinesparkState.ApplyCrashFinishPose(bus, samus);
            AssertEqual((ushort)19, samus.Kinematics.Radius.Y, "Standing alignment does not publish next-frame radius early");
            AssertEqual((ushort)33, samus.Position.Y.Whole, "Native diagonal crash-to-standing feet alignment");
            AssertEqual((ushort)0x1234, samus.Kinematics.Position.Y.Fraction, "Crash alignment preserves current fraction");
            var previous = FixedPosition<RoomPixels>.FromWords(699, 0x5678, 40, 0xabcd);
            AssertEqual(previous.WithY(previous.Y.WithWhole(38)), samus.ApplyPreviousPositionWrites(previous),
                "Native alignment shifts previous whole Y, not replacing it with current Y");
            AssertEqual(previous, samus.ApplyPreviousPositionWrites(previous), "Alignment checkpoint consumed once");
        }
    }

    /// <summary>Runs crash alignment and related face-forward checks from the verification command line.</summary>
    [VerificationCommand("--spark-crash-alignment")]
    internal static int SparkCrashAlignmentCommand()
    {
        Suite(nameof(VerifySparkCrashAlignment), () => VerifySparkCrashAlignment());
        Suite(nameof(VerifyMakeSamusFaceForward), () => VerifyMakeSamusFaceForward());
        return 0;
    }
}
