using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

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
            var level = new RoomLevelData(new Extent16<RoomBlocks>(12, 8), words, bts, new ushort[words.Length], new byte[8]);
            var body = new SamusKinematicsState { Position = new(new((ushort)v[2], 0), new((ushort)v[3], 0)), Radius = new(5, (ushort)v[4])};
            SamusBlockCollision.EjectAfterGrapple(loaded.AddressSpace, level, body);
            if (body.Position.Y.Whole != v[6])
                throw new InvalidDataException($"Post-grapple cartridge vector mismatch: {line}; actualY={body.Position.Y.Whole}.");
            vectorCount++;
        }
        Console.WriteLine($"Post-grapple cartridge vectors: {vectorCount} passed.");
        // Independently executed ROM bytes (GrapplePoseAudit) establish the ejection
        // before pose replacement. Restore the starting center so the playback below
        // still reproduces the player's exact unmodified input/state sequence.
        ushort originalY = samus.Position.Y.Whole;
        SamusBlockCollision.EjectAfterGrapple(loaded.AddressSpace, runtime.LevelData!, samus.Kinematics);
        if (samus.Position.Y.Whole != 0x048f)
            throw new InvalidDataException("Post-grapple ejection differs from the cartridge reference position $048F.");
        samus.Position = samus.Position.WithWholeY(originalY);
        Directory.CreateDirectory("csharp/test-temp/issue-350-release");
        var overlaps = new List<string>();
        for (int y = (samus.Position.Y.Whole >> 4) - 1; y <= (samus.Position.Y.Whole >> 4) + 3; y++)
        {
            Console.Write($"row {y}: ");
            for (int x = (samus.Position.X.Whole >> 4) - 2; x <= (samus.Position.X.Whole >> 4) + 2; x++)
            {
                var block = runtime.LevelData!.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
                Console.Write($"{x}:{block.LevelWord:X4}/{block.Behavior:X2} ");
            }
            Console.WriteLine();
        }
        for (int frame = 0; frame <= 30; frame++)
        {
            var feet = runtime.LevelData!.GetCollisionBlockAtPixel(new(samus.Position.X.Whole, (ushort)(samus.Position.Y.Whole + samus.Kinematics.Radius.Y - 1)));
            Console.WriteLine($"f{frame}: ({samus.Position.X.Whole:X4},{samus.Position.Y.Whole:X4}) radius={samus.Kinematics.Radius.Y} " +
                $"pose={(int)samus.Pose:X2} grapple={samus.Grapple.Phase} feet={feet.LevelWord:X4} " +
                $"Yspeed={samus.Kinematics.YSpeed:X4}.{samus.Kinematics.YSubspeed:X4}");
            if (frame is 0 or 3 or 12)
                PngWriterTooling.WriteRgba($"csharp/test-temp/issue-350-release/frame-{frame}.png", new Extent16<ScreenPixels>(256, 224),
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
            if (frame >= 2 && samus.Grapple.Phase == GrapplePhase.Inactive)
            {
                var body = samus.Kinematics;
                // Native ROM ejection + prospective release/landing dispatch leaves a
                // two-pixel overlap, too. Assert its exact center and full-body depth,
                // not an impossible zero-overlap property or a center-only feet probe.
                ushort expectedY = frame == 2 ? (ushort)0x048f : (ushort)0x048d;
                if (body.Position.X.Whole != 0x0542 || body.Position.Y.Whole != expectedY)
                    overlaps.Add($"Frame {frame} differs from native ledge position: {body.Position.X.Whole:X4},{body.Position.Y.Whole:X4}.");
                for (int y = (body.Position.Y.Whole - body.Radius.Y) >> 4;
                    y <= (body.Position.Y.Whole + body.Radius.Y - 1) >> 4; y++)
                for (int x = (body.Position.X.Whole - body.Radius.X) >> 4;
                    x <= (body.Position.X.Whole + body.Radius.X - 1) >> 4; x++)
                {
                    var block = runtime.LevelData!.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
                    if (block.CollisionType == RoomCollisionType.SolidBlock && body.Position.Y.Whole + body.Radius.Y - y * 16 > 2)
                        overlaps.Add($"Released Samus overlaps solid ledge block ({x},{y}) at frame {frame}.");
                }
            }
            loaded.Game.Step(0);
        }
        if (overlaps.Count != 0) throw new InvalidDataException(string.Join(Environment.NewLine, overlaps));
    }
}
