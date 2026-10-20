using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the recorded moonwalk-to-wall transition that chooses the proposed run's facing.</summary>
internal static class MoonwalkRanIntoWallVerification
{
    /// <summary>
    /// $91:EADE probes a proposed run one pixel along the current pose's facing but picks the
    /// wall pose from the proposed run's direction ($91:EB4E). A right-facing moonwalk ($75)
    /// that proposes a left run therefore stops in left-facing wall pose $8A. In the 13% movie
    /// Samus moonwalks against Mother Brain's arena wall and takes exactly that route.
    /// </summary>
    internal static void VerifyMoonwalkRanIntoWall()
    {
        var samus = new SamusState { Pose = SamusPoseId.MoonwalkAimUpLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x00eb, 0x00c3)) };
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        AssertTrue(!samus.PoseMetadata.IsFacing(SamusFacingDirection.Left), "moonwalk pose $75 faces right");

        samus.PoseTransitions.ApplyRanIntoWallPoseChange(SamusPoseId.RanIntoWallLeftPose);
        AssertEqual(SamusPoseId.RanIntoWallLeftPose, samus.Pose, "the proposed left run stops in wall pose $8A");
        Console.WriteLine("  Moonwalk ran into wall: a right-facing moonwalk can stop in the left-facing wall pose.");
    }

    /// <summary>Runs the focused retail-ROM wall-pose check from the verification command line.</summary>
    [VerificationCommand("--moonwalk-ran-into-wall")]
    internal static int MoonwalkRanIntoWallCommand()
    {
        Suite(nameof(VerifyMoonwalkRanIntoWall), () => VerifyMoonwalkRanIntoWall());
        return 0;
    }
}
