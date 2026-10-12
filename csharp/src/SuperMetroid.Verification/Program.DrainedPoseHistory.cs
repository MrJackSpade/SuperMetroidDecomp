using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using static AssertionsVerification;

/// <summary>Represents drained pose history verification.</summary>
internal static class DrainedPoseHistoryVerification
{
    /// <summary>Verifies drained pose history.</summary>
    internal static void VerifyDrainedPoseHistory(TestAddressSpace bus)
    {
        foreach (SamusPoseId source in new[] { SamusPoseId.CrouchingRightPose, SamusPoseId.CrouchingLeftPose })
        {
            var samus = new SamusState { Pose = source, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            var history = samus.PoseHistory;
            history.PreviousPose = source;
            history.PreviousDirectionAndMovement = (ushort)(SnesLittleEndian.Word(samus.PoseMetadata.ReadPoseXDirection(bus), (byte)samus.PoseMetadata.ReadMovementType(bus)));
            history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
            history.LastDifferentDirectionAndMovement = 0x0308;
            (string Name, Action Apply)[] commands =
            [
                ("fall", () => samus.Drained.LetFall(bus, samus)),
                ("stand", () => samus.Drained.PutStanding(bus, samus)),
                ("release standing", () => samus.Drained.Release(bus, samus)),
                ("crouch", () => samus.Drained.PutCrouchingOrFalling(bus, samus)),
                ("release crouching", () => samus.Drained.Release(bus, samus)),
                ("rainbow lock", () => samus.Drained.SetupForRainbowBeamAbleToStand(bus, samus)),
            ];
            foreach (var command in commands)
            {
                SamusPoseId previousPose = history.PreviousPose;
                ushort previousMetadata = history.PreviousDirectionAndMovement;
                command.Apply();
                AssertEqual(previousPose, history.LastDifferentPose, $"drain {command.Name} shifts previous pose");
                AssertEqual(previousMetadata, history.LastDifferentDirectionAndMovement, $"drain {command.Name} shifts metadata");
                AssertEqual(samus.Pose, history.PreviousPose, $"drain {command.Name} publishes current pose");
                AssertEqual(SnesLittleEndian.Word(samus.PoseMetadata.ReadPoseXDirection(bus), (byte)samus.PoseMetadata.ReadMovementType(bus)),
                    history.PreviousDirectionAndMovement, $"drain {command.Name} publishes current metadata");
            }
            SamusPoseId olderPose = history.LastDifferentPose;
            ushort olderMetadata = history.LastDifferentDirectionAndMovement;
            SamusPoseId currentPose = history.PreviousPose;
            ushort currentMetadata = history.PreviousDirectionAndMovement;
            samus.Drained.EnableHyperBeam(samus);
            AssertEqual(olderPose, history.LastDifferentPose, "Hyper command does not shift older pose");
            AssertEqual(olderMetadata, history.LastDifferentDirectionAndMovement, "Hyper command retains older metadata");
            AssertEqual(currentPose, history.PreviousPose, "Hyper command retains previous pose");
            AssertEqual(currentMetadata, history.PreviousDirectionAndMovement, "Hyper command retains previous metadata");
        }
    }
}
