using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyStuckGrapple()
    {
        ReplayStuckGrapple(SnesButton.B);
        ReplayStuckGrapple(SnesButton.A);
    }

    /// <summary>Exercises the player's wedged rope state; only the kick button differs between runs.</summary>
    private static void ReplayStuckGrapple(SnesButton kickButton)
    {
        var loaded = DebuggerFixtureLoader.Load("issue-376-stuck-grapple", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        var grapple = samus.Grapple;
        Check(grapple.Phase == GrapplePhase.ConnectedSwinging && grapple.RopeLength == 12,
            "The preserved report must begin with the rope blocked before full retraction.");
        bool sawKick = false;
        Directory.CreateDirectory("csharp/test-temp/issue-376-stuck-grapple");
        for (int f = 0; f < 180; f++)
        {
            if (f is 0 or 179)
                PngWriter.WriteRgba($"csharp/test-temp/issue-376-stuck-grapple/frame-{f}.png",256,224,SuperMetroidRuntimeFrameRenderer.Render(runtime));
            loaded.Game.Step((ushort)(SnesButton.X | SnesButton.Up | (f < 45 ? SnesButton.Left : SnesButton.Right) |
                (f % 8 == 0 ? kickButton : SnesButton.None)));
            sawKick |= grapple.JumpImpulse != 0;
        }
        if (kickButton == SnesButton.B)
        {
            Check(!sawKick && grapple.Phase == GrapplePhase.ConnectedSwinging && grapple.RopeLength == 12,
                "Run must not impersonate the cartridge Jump binding or free the wedged rope.");
            Console.WriteLine("PASS Run produces no grapple kick; saved wedged state remains dangling.");
            return;
        }
        Check(sawKick, "Jump must produce the collision-assisted swing kick.");
        Check(grapple.Phase == GrapplePhase.WallGrab, "Reported dangling state never enters wall grab while retracting toward wall.");
        Check(grapple.RopeLength == 8 && samus.XPosition == 175 && samus.YPosition == 504 &&
            samus.TopSpritemapIndex == GrappleWallGrabGraphicsReference.TopSpritemap &&
            samus.BottomSpritemapIndex == GrappleWallGrabGraphicsReference.BottomSpritemap,
            "Freed rope must fully retract and visibly enter the ROM wall-grab sprite at the captured anchor.");
        VerifyGrappleWallGrabDma(loaded.AddressSpace, runtime.Vram,
            GrappleWallGrabGraphicsReference.TopDma, SamusRenderingRomData.TileTransfers.TopDestinations);
        VerifyGrappleWallGrabDma(loaded.AddressSpace, runtime.Vram,
            GrappleWallGrabGraphicsReference.BottomDma, SamusRenderingRomData.TileTransfers.BottomDestinations);
        loaded.Game.Step(0);
        Check(grapple.Phase == GrapplePhase.WallGrabRelease, "Releasing Shoot must open the wall-jump window.");
        loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
        Check(grapple.Phase == GrapplePhase.WallJumping, "Fresh Jump must accept wall contact.");
        loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
        Check(samus.Pose == SamusPoseIds.WallJumpLeftPose && grapple.Phase == GrapplePhase.Inactive,
            "Wall jump must detach and select the opposite-facing jump pose.");
        for (int f = 0; f < 13; f++) loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
        Check(samus.XPosition < 175 && samus.YPosition < 454,
            "Wall jump from the player's stuck setup must move away and rise by more than 50 pixels.");
        PngWriter.WriteRgba("csharp/test-temp/issue-376-stuck-grapple/jumped.png",256,224,SuperMetroidRuntimeFrameRenderer.Render(runtime));
        Console.WriteLine($"PASS Jump frees rope: length 12 -> 8, wall-grab sprite at (175,504), wall jump to ({samus.XPosition},{samus.YPosition}).");
    }
}
