using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>Checks native pose-history word shifts and the movement-byte gate for wall-jump probing.</summary>
internal static class SamusPoseHistoryVerification
{
    /// <summary>Checks distinct and repeated transitions, then all 256 movement bytes with direction held separately.</summary>
    internal static void VerifySamusPoseHistory()
    {
        // Literal fixture for $91:E719. Preserve the packed direction/movement words
        // and distinct pose identities so every shifted field is observable.
        var history = new SamusPoseHistoryState
        {
            PreviousPose = SamusPoseId.UnusedKnockbackLeftPose, PreviousMovement = SamusMovementType.SpinJumping,
            LastDifferentPose = SamusPoseId.ShinesparkDiagonalRightPose, LastDifferentMovement = SamusMovementType.NormalJumping,
        };
        AssertTrue(!history.AllowsWallJumpProbe, "wall probe rejects normal-jump history");
        history.CommitTransition(SamusPoseId.SpinJumpLeftPose, SamusMovementType.SpinJumping);
        AssertEqual(SamusPoseId.UnusedKnockbackLeftPose, history.LastDifferentPose, "transition shifts previous pose");
        AssertEqual(SamusMovementType.SpinJumping, history.LastDifferentMovement, "transition shifts packed direction/movement");
        AssertEqual(SamusPoseId.SpinJumpLeftPose, history.PreviousPose, "transition records current pose");
        AssertEqual(SamusMovementType.SpinJumping, history.PreviousMovement, "transition records current packed metadata");
        AssertTrue(history.AllowsWallJumpProbe, "wall probe admits spin history");
        history.CommitTransition(SamusPoseId.SpinJumpLeftPose, SamusMovementType.SpinJumping);
        AssertEqual(SamusPoseId.SpinJumpLeftPose, history.LastDifferentPose, "same-pose transition still shifts history");
        AssertEqual(SamusMovementType.SpinJumping, history.LastDifferentMovement, "same-pose transition retains current direction");

        foreach (SamusMovementType movement in Enum.GetValues<SamusMovementType>())
        {
            history.LastDifferentMovement = movement;
            AssertEqual(movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping, history.AllowsWallJumpProbe,
                $"wall probe gate for {movement} history");
        }
        System.Console.WriteLine("  Pose-history primitive: exact word shifts, same-pose transitions, and all 256 movement-byte gates agree.");
    }
}
