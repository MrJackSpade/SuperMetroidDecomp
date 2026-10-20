using SuperMetroid.Core.Hardware;
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
            var samus = new SamusState { Pose = source, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var history = samus.PoseHistory;
            history.PreviousPose = source;
            history.PreviousDirectionAndMovement = SnesLittleEndian.Word(SamusPoseMetadata.ReadPoseXDirection(samus), (byte)SamusPoseMetadata.ReadMovementType(samus));
            history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
            history.LastDifferentDirectionAndMovement = 0x0308;
            (string Name, Action Apply)[] commands =
            [
                ("fall", () => samus.Drained.LetFall(samus)),
                ("stand", () => samus.Drained.PutStanding(samus)),
                ("release standing", () => samus.Drained.Release(samus)),
                ("crouch", () => samus.Drained.PutCrouchingOrFalling(samus)),
                ("release crouching", () => samus.Drained.Release(samus)),
                ("rainbow lock", () => samus.Drained.SetupForRainbowBeamAbleToStand(samus)),
            ];
            foreach (var command in commands)
            {
                SamusPoseId previousPose = history.PreviousPose;
                int previousMetadata = history.PreviousDirectionAndMovement;
                command.Apply();
                AssertEqual(previousPose, history.LastDifferentPose, $"drain {command.Name} shifts previous pose");
                AssertEqual(previousMetadata, history.LastDifferentDirectionAndMovement, $"drain {command.Name} shifts metadata");
                AssertEqual(samus.Pose, history.PreviousPose, $"drain {command.Name} publishes current pose");
                AssertEqual(SnesLittleEndian.Word(SamusPoseMetadata.ReadPoseXDirection(samus), (byte)SamusPoseMetadata.ReadMovementType(samus)),
                    history.PreviousDirectionAndMovement, $"drain {command.Name} publishes current metadata");
            }
            SamusPoseId olderPose = history.LastDifferentPose;
            int olderMetadata = history.LastDifferentDirectionAndMovement;
            SamusPoseId currentPose = history.PreviousPose;
            int currentMetadata = history.PreviousDirectionAndMovement;
            samus.Drained.EnableHyperBeam(samus);
            AssertEqual(olderPose, history.LastDifferentPose, "Hyper command does not shift older pose");
            AssertEqual(olderMetadata, history.LastDifferentDirectionAndMovement, "Hyper command retains older metadata");
            AssertEqual(currentPose, history.PreviousPose, "Hyper command retains previous pose");
            AssertEqual(currentMetadata, history.PreviousDirectionAndMovement, "Hyper command retains previous metadata");
        }
    }
}
