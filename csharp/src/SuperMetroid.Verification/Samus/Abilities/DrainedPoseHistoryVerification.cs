using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Abilities;
/// <summary>Checks how drained Samus poses advance prior-pose metadata.</summary>
internal static class DrainedPoseHistoryVerification
{
    /// <summary>Exercises both crouch facings and confirms Hyper Beam enablement preserves pose history.</summary>
    internal static void VerifyDrainedPoseHistory()
    {
        foreach (SamusPoseId source in new[] { SamusPoseId.CrouchingRightPose, SamusPoseId.CrouchingLeftPose })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = source, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var history = samus.PoseHistory;
            history.PreviousPose = source;
            history.PreviousMovement = SamusPoseMetadata.ReadMovementType(samus);
            history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
            history.LastDifferentMovement = SamusMovementType.SpinJumping;
            DrainedPoseHistoryCommand[] commands =
            [
                new DrainedPoseHistoryCommand("fall", () => samus.Drained.LetFall(samus)),
                new DrainedPoseHistoryCommand("stand", () => samus.Drained.PutStanding(samus)),
                new DrainedPoseHistoryCommand("release standing", () => samus.Drained.Release(samus)),
                new DrainedPoseHistoryCommand("crouch", () => samus.Drained.PutCrouchingOrFalling(samus)),
                new DrainedPoseHistoryCommand("release crouching", () => samus.Drained.Release(samus)),
                new DrainedPoseHistoryCommand("rainbow lock", () => samus.Drained.SetupForRainbowBeamAbleToStand(samus)),
            ];
            foreach (var command in commands)
            {
                SamusPoseId previousPose = history.PreviousPose;
                SamusMovementType previousMetadata = history.PreviousMovement;
                command.Apply();
                AssertEqual(previousPose, history.LastDifferentPose, $"drain {command.Name} shifts previous pose");
                AssertEqual(previousMetadata, history.LastDifferentMovement, $"drain {command.Name} shifts metadata");
                AssertEqual(samus.Pose, history.PreviousPose, $"drain {command.Name} publishes current pose");
                AssertEqual(SamusPoseMetadata.ReadMovementType(samus),
                    history.PreviousMovement, $"drain {command.Name} publishes current metadata");
            }
            SamusPoseId olderPose = history.LastDifferentPose;
            SamusMovementType olderMetadata = history.LastDifferentMovement;
            SamusPoseId currentPose = history.PreviousPose;
            SamusMovementType currentMetadata = history.PreviousMovement;
            samus.Drained.EnableHyperBeam(samus);
            AssertEqual(olderPose, history.LastDifferentPose, "Hyper command does not shift older pose");
            AssertEqual(olderMetadata, history.LastDifferentMovement, "Hyper command retains older metadata");
            AssertEqual(currentPose, history.PreviousPose, "Hyper command retains previous pose");
            AssertEqual(currentMetadata, history.PreviousMovement, "Hyper command retains previous metadata");
        }
    }
}
