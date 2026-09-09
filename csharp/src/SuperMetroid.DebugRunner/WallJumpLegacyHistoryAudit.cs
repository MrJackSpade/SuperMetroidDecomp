using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static class WallJumpLegacyHistoryAudit
{
    public static void Run(SuperMetroidAddressSpace bus)
    {
        var legacy = FlatFloorMovementFixture.Create(bus, water: false);
        var current = FlatFloorMovementFixture.Create(bus, water: false);
        foreach (var runtime in new[] { legacy, current })
        {
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = 128;
            samus.YPosition = 235;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = 8;
            samus.PoseHistory.LastDifferentPose = SamusPoseIds.SpinJumpRightPose;
            samus.PoseHistory.LastDifferentDirectionAndMovement = 0x0308;
        }
        // Model the exact migration boundary: all older fields exist, but the newly
        // added owner was not serialized. The serializer layout itself is separately
        // exercised by DiagnosticsVerification's legacy-options-migration gate.
        typeof(SamusState).GetField("_poseHistory", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(legacy.Samus, null);
        if (legacy.Samus!.PoseHistory.AllowsWallJumpProbe)
            throw new InvalidDataException("Missing legacy history invented walljump eligibility.");
        int convergenceFrame = -1;
        for (int frame = 0; frame < 90; frame++)
        {
            ushort input = frame < 30 ? (ushort)SnesButton.Right : (ushort)0;
            if (frame >= 2 && frame < 25) input |= (ushort)SnesButton.A;
            legacy.StepFrame(input);
            current.StepFrame(input);
            var a = legacy.Samus!;
            var b = current.Samus!;
            if (a.XPosition != b.XPosition || a.YPosition != b.YPosition || a.Pose != b.Pose ||
                a.Kinematics.XSubposition != b.Kinematics.XSubposition || a.Kinematics.YSubposition != b.Kinematics.YSubposition)
                throw new InvalidDataException($"Legacy history changed unobstructed movement at frame {frame}.");
            bool equal = a.PoseHistory.PreviousPose == b.PoseHistory.PreviousPose &&
                a.PoseHistory.PreviousDirectionAndMovement == b.PoseHistory.PreviousDirectionAndMovement &&
                a.PoseHistory.LastDifferentPose == b.PoseHistory.LastDifferentPose &&
                a.PoseHistory.LastDifferentDirectionAndMovement == b.PoseHistory.LastDifferentDirectionAndMovement;
            if (equal && convergenceFrame < 0) convergenceFrame = frame;
            if (!equal && convergenceFrame >= 0)
                throw new InvalidDataException("Legacy history diverged after transition-owned convergence.");
        }
        if (convergenceFrame < 0 || convergenceFrame > 3)
            throw new InvalidDataException($"Legacy history did not repopulate after running/jump transitions: {convergenceFrame}.");
        Console.WriteLine($"Legacy history entry: 90 production frames agree; four-word history converges at frame {convergenceFrame}.");
    }
}
