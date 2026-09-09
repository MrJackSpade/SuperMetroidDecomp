using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyDrainedPoseHistory(TestAddressSpace bus)
    {
        foreach (byte source in new[] { SamusPoseIds.CrouchingRightPose, SamusPoseIds.CrouchingLeftPose })
        {
            var samus = new SamusState { Pose = source, YPosition = 100 };
            samus.RefreshCollisionRadii(bus);
            var history = samus.PoseHistory;
            history.PreviousPose = source;
            history.PreviousDirectionAndMovement = (ushort)(samus.ReadPoseXDirection(bus) | ((byte)samus.ReadMovementType(bus) << 8));
            history.LastDifferentPose = SamusPoseIds.SpinJumpRightPose;
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
                ushort previousPose = history.PreviousPose;
                ushort previousMetadata = history.PreviousDirectionAndMovement;
                command.Apply();
                AssertEqual(previousPose, history.LastDifferentPose, $"drain {command.Name} shifts previous pose");
                AssertEqual(previousMetadata, history.LastDifferentDirectionAndMovement, $"drain {command.Name} shifts metadata");
                AssertEqual(samus.Pose, history.PreviousPose, $"drain {command.Name} publishes current pose");
                AssertEqual(samus.ReadPoseXDirection(bus) | ((byte)samus.ReadMovementType(bus) << 8),
                    history.PreviousDirectionAndMovement, $"drain {command.Name} publishes current metadata");
            }
            ushort olderPose = history.LastDifferentPose;
            ushort olderMetadata = history.LastDifferentDirectionAndMovement;
            ushort currentPose = history.PreviousPose;
            ushort currentMetadata = history.PreviousDirectionAndMovement;
            samus.Drained.EnableHyperBeam(samus);
            AssertEqual(olderPose, history.LastDifferentPose, "Hyper command does not shift older pose");
            AssertEqual(olderMetadata, history.LastDifferentDirectionAndMovement, "Hyper command retains older metadata");
            AssertEqual(currentPose, history.PreviousPose, "Hyper command retains previous pose");
            AssertEqual(currentMetadata, history.PreviousDirectionAndMovement, "Hyper command retains previous metadata");
        }
    }
}
