using SuperMetroid.Core.Input;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyGrappleRetry()
    {
        ReplayGrappleRetry(repressUp: false);
        ReplayGrappleRetry(repressUp: true);
    }

    /// <summary>Reproduces stopped retraction, a missed jump, and reacquisition without resetting the game.</summary>
    private static void ReplayGrappleRetry(bool repressUp)
    {
        var loaded = DebuggerFixtureLoader.Load("issue-376-grapple-retry", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        var g = samus.Grapple;
        Check(g.Phase == GrapplePhase.ConnectedSwinging && g.RopeLength == 9 && g.RopeLengthDelta == 0,
            "Retry fixture must begin with stopped retraction one pixel above minimum.");
        for (int frame = 0; frame < 180; frame++)
        {
            // Native BB64 samples a new Up edge; holding the captured Up input cannot
            // restart a length delta which AC4D has cleared after a clamped collision.
            loaded.Game.Step((ushort)(SnesButton.X | (!repressUp || frame % 12 != 0 ? SnesButton.Up : SnesButton.None) | (frame < 45 ? SnesButton.Left : SnesButton.Right) |
                (frame % 8 == 0 ? SnesButton.A : SnesButton.None)));
        }
        if (!repressUp)
        {
            Check(g.Phase == GrapplePhase.ConnectedSwinging && g.RopeLength == 9 && g.RopeLengthDelta == 0,
                "Held Up unexpectedly restarted the cartridge edge-triggered retraction.");
            Console.WriteLine("PASS reported retry stall: holding Up leaves length 9/delta 0 unchanged.");
            return;
        }
        Check(g.Phase == GrapplePhase.WallGrab, "Repeated report never reaches the wall-grab pose.");
        Check(samus.TopSpritemapIndex == GrappleWallGrabGraphicsReference.TopSpritemap &&
            samus.BottomSpritemapIndex == GrappleWallGrabGraphicsReference.BottomSpritemap,
            "Reacquired ready state must actually select the cartridge wall-grab sprite.");
        loaded.Game.Step((ushort)SnesButton.A);
        Check(g.Phase == GrapplePhase.WallGrabRelease && g.WallJumpTimer == 30,
            "Same-frame release/Jump must open the grace window rather than dispatch its following-frame check.");
        for (int f = 0; f < 33; f++) loaded.Game.Step((ushort)SnesButton.A);
        Check(g.Phase == GrapplePhase.Inactive && samus.Pose != SamusPoseIds.WallJumpLeftPose,
            "Holding the already-consumed Jump edge should reproduce the missed jump and expired window.");
        for (int frame = 0; frame < 180; frame++)
        {
            var input = SnesButton.X | SnesButton.Right |
                (frame % 12 != 0 ? SnesButton.Up : SnesButton.None) |
                (frame % 8 == 0 ? SnesButton.A : SnesButton.None);
            if (frame == 0) input = SnesButton.Right;
            loaded.Game.Step((ushort)input);
            if (g.Phase == GrapplePhase.WallGrab) break;
        }
        Check(g.Phase == GrapplePhase.WallGrab, "Could not reacquire wall grab after the missed-jump window expired.");
        loaded.Game.Step(0);
        loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
        loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
        Check(runtime.LastGrappleMovement is { WallJumpStarted: true }, "Retry did not perform the wall jump.");
        ushort startX = samus.XPosition;
        ushort startY = samus.YPosition;
        for (int frame = 0; frame < 13; frame++) loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
        Check(samus.XPosition < startX && samus.YPosition < startY - 50,
            "Successful retry must move away from the wall and upward, not merely queue a pose.");
        Console.WriteLine("PASS re-press Up -> ready sprite -> missed simultaneous-release jump -> reattach -> ready -> delayed Jump -> upward/away movement.");
    }
}
