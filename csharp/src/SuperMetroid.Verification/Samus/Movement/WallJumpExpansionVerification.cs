using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks wall-jump collision expansion against ceiling contact and preserved spin radius.</summary>
internal static class WallJumpExpansionVerification
{
    /// <summary>Exercises both facings with and without a ceiling, including center correction and fractional Y.</summary>
    internal static void VerifyWallJumpExpansion()
    {
        foreach (bool ceiling in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            var entries = new ushort[16 * 32];
            if (ceiling) entries[6 * 16 + (left ? 7 : 8)] = 0x8000;
            var room = CreateRoom(new(16, 32), entries, new byte[entries.Length]);
            var samus = new SamusState
            {
                Pose = left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(left ? (ushort)119 : (ushort)137, 127)),
            };
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x1000);
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation();
            ushort beforeX = samus.Position.X.Whole;
            samus.PoseTransitions.ApplyWallJumpTrigger(room);
            AssertEqual(left ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose,
                samus.Pose, "walljump expansion selects facing-preserving pose");
            AssertEqual(beforeX, samus.Position.X.Whole, "vertical expansion preserves X");
            AssertEqual(ceiling ? 131 : 127, samus.Position.Y.Whole, "walljump ceiling expansion center correction");
            AssertEqual(ceiling ? 0 : 0x1000, samus.Kinematics.Position.Y.Fraction,
                "walljump collision clamps fractional Y; air preserves it");
            // $91:FDAE keeps the spin radius until alpha's SetSamusRadius next frame.
            AssertEqual(SamusPoseMetadata.ReadPoseYRadius(left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose),
                samus.Kinematics.Radius.Y, "walljump keeps the spin radius through its frame");
            AssertEqual(19, SamusPoseMetadata.ReadPoseYRadius(samus.Pose), "walljump expanded collision radius");
        }
    }
}
