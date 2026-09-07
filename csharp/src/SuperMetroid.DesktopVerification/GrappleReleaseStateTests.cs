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
        int vectorCount = 0;
        foreach (string line in File.ReadLines("csharp/test-fixtures/issue-350-grounded-grapple-floor-clip/post-grapple-vectors.csv").Skip(1))
        {
            int[] v = line.Split(',').Select(int.Parse).ToArray();
            var words = new ushort[12 * 8];
            var bts = new byte[words.Length];
            for (int row = 2; row <= 5; row++)
            for (int column = 3; column <= 7; column++)
            {
                if (row < 4 && v[5] == 0) continue;
                words[row * 12 + column] = (ushort)(v[0] << 12);
                bts[row * 12 + column] = (byte)v[1];
            }
            var level = new RoomLevelData(12, 8, words, bts, new ushort[words.Length], new byte[8]);
            var body = new SamusKinematicsState { XPosition = (ushort)v[2], YPosition = (ushort)v[3], XRadius = 5, YRadius = (ushort)v[4] };
            SamusBlockCollision.EjectAfterGrapple(loaded.AddressSpace, level, body);
            if (body.YPosition != v[6])
                throw new InvalidDataException($"Post-grapple cartridge vector mismatch: {line}; actualY={body.YPosition}.");
            vectorCount++;
        }
        Console.WriteLine($"Post-grapple cartridge vectors: {vectorCount} passed.");
        // Independently executed ROM bytes (GrapplePoseAudit) establish the ejection
        // before pose replacement. Restore the starting center so the playback below
        // still reproduces the player's exact unmodified input/state sequence.
        ushort originalY = samus.YPosition;
        SamusBlockCollision.EjectAfterGrapple(loaded.AddressSpace, runtime.LevelData!, samus.Kinematics);
        if (samus.YPosition != 0x048f)
            throw new InvalidDataException("Post-grapple ejection differs from the cartridge reference position $048F.");
        samus.YPosition = originalY;
        Directory.CreateDirectory("csharp/test-temp/issue-350-release");
        var overlaps = new List<string>();
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
                // Native ROM ejection + prospective release/landing dispatch leaves a
                // two-pixel overlap, too. Assert its exact center and full-body depth,
                // not an impossible zero-overlap property or a center-only feet probe.
                ushort expectedY = frame == 2 ? (ushort)0x048f : (ushort)0x048d;
                if (body.XPosition != 0x0542 || body.YPosition != expectedY)
                    overlaps.Add($"Frame {frame} differs from native ledge position: {body.XPosition:X4},{body.YPosition:X4}.");
                for (int y = (body.YPosition - body.YRadius) >> 4;
                    y <= (body.YPosition + body.YRadius - 1) >> 4; y++)
                for (int x = (body.XPosition - body.XRadius) >> 4;
                    x <= (body.XPosition + body.XRadius - 1) >> 4; x++)
                {
                    var block = runtime.LevelData!.GetCollisionBlock(x, y);
                    if (block.CollisionType == RoomCollisionType.SolidBlock && body.YPosition + body.YRadius - y * 16 > 2)
                        overlaps.Add($"Released Samus overlaps solid ledge block ({x},{y}) at frame {frame}.");
                }
            }
            loaded.Game.Step(0);
        }
        if (overlaps.Count != 0) throw new InvalidDataException(string.Join(Environment.NewLine, overlaps));
    }
}
