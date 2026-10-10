using SuperMetroid.Core.Game;

internal static partial class Program
{
    static void VerifySamusPoseHistory()
    {
        // Literal fixture for $91:E719. Preserve the packed direction/movement words
        // and distinct pose identities so every shifted field is observable.
        var history = new SamusPoseHistoryState
        {
            PreviousPose = SamusPoseId.UnusedKnockbackLeftPose, PreviousDirectionAndMovement = 0x0308,
            LastDifferentPose = SamusPoseId.ShinesparkDiagonalRightPose, LastDifferentDirectionAndMovement = 0x0204,
        };
        AssertTrue(!history.AllowsWallJumpProbe, "wall probe rejects normal-jump history");
        history.CommitTransition(SamusPoseId.SpinJumpLeftPose, 0x0304);
        AssertEqual(SamusPoseId.UnusedKnockbackLeftPose, history.LastDifferentPose, "transition shifts previous pose");
        AssertEqual(0x0308, history.LastDifferentDirectionAndMovement, "transition shifts packed direction/movement");
        AssertEqual(SamusPoseId.SpinJumpLeftPose, history.PreviousPose, "transition records current pose");
        AssertEqual(0x0304, history.PreviousDirectionAndMovement, "transition records current packed metadata");
        AssertTrue(history.AllowsWallJumpProbe, "wall probe admits spin history");
        history.CommitTransition(SamusPoseId.SpinJumpLeftPose, 0x0304);
        AssertEqual(SamusPoseId.SpinJumpLeftPose, history.LastDifferentPose, "same-pose transition still shifts history");
        AssertEqual(0x0304, history.LastDifferentDirectionAndMovement, "same-pose transition retains current direction");

        for (int movement = 0; movement <= byte.MaxValue; movement++)
        {
            history.LastDifferentDirectionAndMovement = (ushort)((movement << 8) | 0xa5);
            AssertEqual(movement is 3 or 20, history.AllowsWallJumpProbe,
                "native wall probe movement-byte gate ignores direction byte");
        }
        System.Console.WriteLine("  Pose-history primitive: exact word shifts, same-pose transitions, and all 256 movement-byte gates agree.");
    }
}
