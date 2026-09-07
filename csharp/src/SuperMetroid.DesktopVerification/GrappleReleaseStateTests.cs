using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyGrappleReleaseState()
    {
        var loaded = DebuggerFixtureLoader.Load("issue-350-grounded-grapple-floor-clip", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        Directory.CreateDirectory("csharp/test-temp/issue-350-release");
        for (int y = (samus.YPosition >> 4) - 1; y <= (samus.YPosition >> 4) + 3; y++)
        {
            Console.Write($"row {y}: ");
            for (int x = (samus.XPosition >> 4) - 2; x <= (samus.XPosition >> 4) + 2; x++)
            {
                var block = runtime.LevelData!.GetCollisionBlock(x, y);
                Console.Write($"{x}:{block.LevelWord:X4}/{block.Behavior:X2} ");
            }
            Console.WriteLine();
        }
        for (int frame = 0; frame <= 30; frame++)
        {
            var feet = runtime.LevelData!.GetCollisionBlockAtPixel(samus.XPosition,
                (ushort)(samus.YPosition + samus.Kinematics.YRadius - 1));
            Console.WriteLine($"f{frame}: ({samus.XPosition:X4},{samus.YPosition:X4}) radius={samus.Kinematics.YRadius} " +
                $"pose={samus.Pose:X2} grapple={samus.Grapple.Phase} feet={feet.LevelWord:X4} " +
                $"Yspeed={samus.Kinematics.YSpeed:X4}.{samus.Kinematics.YSubspeed:X4}");
            if (frame is 0 or 3 or 12)
                PngWriter.WriteRgba($"csharp/test-temp/issue-350-release/frame-{frame}.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
            if (frame >= 2 && samus.Grapple.Phase == GrapplePhase.Inactive)
            {
                var body = samus.Kinematics;
                for (int y = (body.YPosition - body.YRadius) >> 4;
                    y <= (body.YPosition + body.YRadius - 1) >> 4; y++)
                for (int x = (body.XPosition - body.XRadius) >> 4;
                    x <= (body.XPosition + body.XRadius - 1) >> 4; x++)
                {
                    var block = runtime.LevelData!.GetCollisionBlock(x, y);
                    if (block.CollisionType == RoomCollisionType.SolidBlock)
                        throw new InvalidDataException($"Released Samus overlaps solid ledge block ({x},{y}) at frame {frame}.");
                }
            }
            loaded.Game.Step(0);
        }
    }
}
