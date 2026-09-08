using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>Player's wall approach, without manually commanding rope retraction.</summary>
    private static void VerifyGrappleAutomaticRetraction()
    {
        var loaded = DebuggerFixtureLoader.Load("issue-376-anchored-grapple", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        var grapple = samus.Grapple;
        int connectedFrame = -1;
        bool automaticRetractionStarted = false;
        for (int frame = 0; frame < 160; frame++)
        {
            SnesButton input = connectedFrame >= 0 ? SnesButton.X | SnesButton.Right : frame switch
            {
                0 => SnesButton.Select,
                < 20 => SnesButton.Right,
                < 40 => SnesButton.Right | SnesButton.A,
                _ => SnesButton.Right | SnesButton.A | SnesButton.X,
            };
            loaded.Game.Step((ushort)input);
            if (connectedFrame < 0 && grapple.Phase == GrapplePhase.ConnectedSwinging)
            {
                connectedFrame = frame;
                Console.WriteLine($"Attached at frame {frame}: rope={grapple.RopeLength}, delta={grapple.RopeLengthDelta}");
                // Independent expected word: executing ROM $9B:C745..C758 leaves FFF8.
                automaticRetractionStarted = grapple.RopeLengthDelta == -8;
            }
            if (grapple.Phase == GrapplePhase.WallGrab)
            {
                Check(automaticRetractionStarted, "Successful attachment omitted the cartridge's automatic eight-pixel retraction.");
                Console.WriteLine($"Ready at frame {frame}, {frame - connectedFrame} frames after attachment, without Up input.");
                Check(samus.Pose == SamusPoseIds.GrappleWallContactLeftPose && grapple.RopeLength == 8,
                    "Automatic retraction must produce cartridge pose B8 at minimum length.");
                Check(samus.TopSpritemapIndex == GrappleWallGrabGraphicsReference.TopSpritemap &&
                    samus.BottomSpritemapIndex == GrappleWallGrabGraphicsReference.BottomSpritemap,
                    "Ready body must draw the wall-grab sprite, not dangling art.");
                loaded.Game.Step(0);
                loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
                loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
                Check(runtime.LastGrappleMovement is { WallJumpStarted: true }, "Automatic wall attachment did not permit the wall jump.");
                ushort startX = samus.XPosition, startY = samus.YPosition;
                for (int i = 0; i < 13; i++) loaded.Game.Step((ushort)(SnesButton.A | SnesButton.Left));
                Check(samus.XPosition < startX && samus.YPosition < startY - 50,
                    "Wall jump must actually carry Samus up and away from the wall.");
                Console.WriteLine("PASS automatic attachment -> ready sprite -> wall jump, no manual rope input.");
                return;
            }
        }
        throw new InvalidOperationException("Wall approach stayed dangling instead of automatically reaching the ready pose.");
    }
}
