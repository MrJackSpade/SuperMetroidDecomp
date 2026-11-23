using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Runtime;

/// <summary>
/// The statue room's background row while the camera lies above the room's top (#2631): the camera's wrapped position is
/// taken signed, so the row is a small negative one the background's view holds, rather than one past the signed range
/// that overflowed the view as the door transition after the statue room began.
/// </summary>
internal static class StatueDescentBackgroundRowVerification
{
    /// <summary>The background row for a camera above, at and below the room's top, with and without a descent.</summary>
    internal static void VerifyStatueDescentRowTakesTheCameraSigned()
    {
        // Eight rows above the room's top, the camera's position has wrapped to the far end of the room's coordinates.
        Position16<RoomPixels> aboveTop = new(0, 0x10000 - 8);
        AssertEqual(-24, GameplayDisplayCapture.StatueDescentBackgroundRow(aboveTop, -16), "camera eight rows above the room, descended sixteen");
        AssertEqual(-24, (int)Offset16<BackgroundPixels>.Of(0, 0).WithY(GameplayDisplayCapture.StatueDescentBackgroundRow(aboveTop, -16)).Y,
            "the background's view holds the row");
        AssertEqual(0, GameplayDisplayCapture.StatueDescentBackgroundRow(new(0, 0), 0), "camera at the room's top, no descent");
        AssertEqual(84, GameplayDisplayCapture.StatueDescentBackgroundRow(new(0, 100), -16), "camera inside the room, descended sixteen");
    }

    /// <summary>Runs the statue room's background row check.</summary>
    [VerificationCommand("--statue-descent-background-row")]
    internal static int StatueDescentBackgroundRowCommand()
    {
        Suite(nameof(VerifyStatueDescentRowTakesTheCameraSigned), VerifyStatueDescentRowTakesTheCameraSigned);
        return 0;
    }
}
