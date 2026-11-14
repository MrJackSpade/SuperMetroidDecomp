using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Samus.Poses.MakeSamusFaceForwardVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks diagonal shinespark crash alignment and its one-time previous-position adjustment.</summary>
internal static class SparkCrashAlignmentVerification
{
    /// <summary>Confirms the delayed standing pose shifts whole Y by two pixels, retains the fraction, and consumes its previous-position checkpoint once.</summary>
    internal static void VerifySparkCrashAlignment()
    {
        foreach (SamusPoseId pose in new SamusPoseId[] { SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkDiagonalLeftPose })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(700, 35)) };
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x1234);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var fullShots = new SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem(new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.SlotCount));
            fullShots.SlotTable.ProjectileCounter = 5;
            samus.Shinespark.FinishCrash(samus, fullShots);
            AssertEqual((ushort)35, samus.Position.Y.Whole, "Movement queues standing alignment until after animation");
            SamusShinesparkState.ApplyCrashFinishPose(samus);
            AssertEqual((ushort)19, samus.Kinematics.Radius.Y, "Standing alignment does not publish next-frame radius early");
            AssertEqual((ushort)33, samus.Position.Y.Whole, "Native diagonal crash-to-standing feet alignment");
            AssertEqual((ushort)0x1234, samus.Kinematics.Position.Y.Fraction, "Crash alignment preserves current fraction");
            var previous = FixedPosition<RoomPixels>.FromStoredParts(699, 0x5678, 40, 0xabcd);
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
