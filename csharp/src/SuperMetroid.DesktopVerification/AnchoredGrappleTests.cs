using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Room-local controller reproduction from the player's Halfie Climb slot.</summary>
    private static void VerifyAnchoredGrapple()
    {
        var loaded = DebuggerFixtureLoader.Load("issue-376-anchored-grapple", 0);
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        Console.WriteLine($"room={game.GameplayActiveRoomPointer:X4} pos={samus.XPosition},{samus.YPosition} pose={samus.Pose:X2} grapple={samus.Grapple.Phase}");
        Directory.CreateDirectory("csharp/test-temp/issue-376-anchored-grapple");
        PngWriter.WriteRgba("csharp/test-temp/issue-376-anchored-grapple/start.png", 256, 224,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
        int grabbedFrames = 0;
        for (int frame = 0; frame < 160; frame++)
        {
            SnesButton input = frame switch
            {
                0 => SnesButton.Select,
                < 20 => SnesButton.Right,
                < 40 => SnesButton.Right | SnesButton.A,
                _ => SnesButton.Right | SnesButton.A | SnesButton.X,
            };
            if (samus.Grapple.Phase == GrapplePhase.ConnectedSwinging)
                input = SnesButton.X | SnesButton.Up | SnesButton.Right;
            if (samus.Grapple.Phase == GrapplePhase.WallGrab || grabbedFrames > 0)
            {
                grabbedFrames++;
                // Jump while still holding Shoot must NOT detach: native C814 waits for
                // Shoot release. A fresh Jump on the following frame enters C832's check.
                input = grabbedFrames <= 5 ? SnesButton.X | SnesButton.A
                    : grabbedFrames == 6 ? SnesButton.None : SnesButton.A | SnesButton.Left;
            }
            game.Step((ushort)input);
            if (grabbedFrames is >= 1 and <= 5)
                Check(samus.Grapple.Phase == GrapplePhase.WallGrab && samus.XPosition == 175 && samus.YPosition == 552,
                    "Holding Shoot+Jump must retain the native wall-grab pose and position.");
            if (grabbedFrames == 6)
                Check(samus.Grapple.Phase == GrapplePhase.WallGrabRelease && samus.Grapple.WallJumpTimer == 30,
                    "Shoot release must open the native 30-frame grace window.");
            if (grabbedFrames == 7)
                Check(runtime.LastGrappleMovement is { WallJumpQueued: true, WallProbeCollided: true },
                    "Fresh Jump must accept the real wall contact in the player's room.");
            if (grabbedFrames == 8)
                Check(runtime.LastGrappleMovement is { WallJumpStarted: true } &&
                    samus.Pose == SamusPoseIds.WallJumpLeftPose && samus.Grapple.Phase == GrapplePhase.Inactive,
                    "Accepted grapple wall jump must detach and select the leftward wall-jump pose.");
            if (grabbedFrames is 1 or 8 or 21)
                PngWriter.WriteRgba($"csharp/test-temp/issue-376-anchored-grapple/grab-step-{grabbedFrames}.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
            if (grabbedFrames == 21)
            {
                Check(samus.XPosition == 158 && samus.YPosition == 489,
                    $"Wall jump did not propel Samus upward and away: ({samus.XPosition},{samus.YPosition}).");
                Console.WriteLine("PASS slot 0: retract into wall grab (175,552), retain while Shoot held, release then Jump, detach and rise to (158,489).");
                return;
            }
        }
        throw new InvalidDataException("Player-room sequence never reached a completed grapple wall jump.");
    }
}
