using SuperMetroid.Core.Game;

internal static partial class Program
{
    static void VerifySamusPoseHistory()
    {
        // Literal word-level fixture for $91:E719. Preserve direction bytes and
        // full pose words rather than silently narrowing the native history data.
        var history = new SamusPoseHistoryState
        {
            PreviousPose = 0x1234, PreviousDirectionAndMovement = 0x0308,
            LastDifferentPose = 0xabcd, LastDifferentDirectionAndMovement = 0x0204,
        };
        AssertTrue(!history.AllowsWallJumpProbe, "wall probe rejects normal-jump history");
        history.CommitTransition(0x001a, 0x0304);
        AssertEqual(0x1234, history.LastDifferentPose, "transition shifts previous pose word");
        AssertEqual(0x0308, history.LastDifferentDirectionAndMovement, "transition shifts packed direction/movement");
        AssertEqual(0x001a, history.PreviousPose, "transition records current pose");
        AssertEqual(0x0304, history.PreviousDirectionAndMovement, "transition records current packed metadata");
        AssertTrue(history.AllowsWallJumpProbe, "wall probe admits spin history");
        history.CommitTransition(0x001a, 0x0304);
        AssertEqual(0x001a, history.LastDifferentPose, "same-pose transition still shifts history");
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
