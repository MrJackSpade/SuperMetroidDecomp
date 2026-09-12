using System.Globalization;
using SuperMetroid.Core.Input;

/// <summary>Replays fixed recorded input without running the search feedback policy.</summary>
internal static class BombTraversalComparison
{
    public static int Run(string rom, string inputPath, string tracePath, bool expectTraversal = true)
    {
        var inputs = File.ReadLines(inputPath).Skip(1).Select((line, frame) =>
        {
            var row = line.Split(',');
            if (row.Length != 2 || int.Parse(row[0]) != frame) throw new InvalidDataException("Reordered traversal inputs.");
            return ushort.Parse(row[1], NumberStyles.HexNumber);
        }).ToArray();
        var trace = File.ReadLines(tracePath).Skip(1).Select(line => line.Split(',')).ToArray();
        if (inputs.Length != 600 || trace.Length != 1200 || trace.Any(row => row.Length != 49))
            throw new InvalidDataException("Incomplete traversal input/native capture.");
        int mismatches = 0;
        for (int left = 0; left < 2; left++)
        {
            var runtime = BombTraversalFixture.Create(rom, ceiling: true, left: left != 0);
            var samus = runtime.Samus!;
            int launches = 0, floorContacts = 0, ceilingContacts = 0, firstFloorContact = -1;
            for (int frame = 0; frame < inputs.Length; frame++)
            {
                ushort input = inputs[frame];
                if (left != 0) input = (ushort)((input & ~(ushort)(SnesButton.Left | SnesButton.Right)) |
                    ((input & (ushort)SnesButton.Right) << 1) | ((input & (ushort)SnesButton.Left) >> 1));
                var expected = trace[left * 600 + frame];
                if (int.Parse(expected[0]) != left || int.Parse(expected[1]) != frame ||
                    ushort.Parse(expected[2], NumberStyles.HexNumber) != input)
                    throw new InvalidDataException("Native input differs from the fixed recording.");
                runtime.StepFrame(input);
                if (runtime.LastBombJumpMovement is { Started: true }) launches++;
                if (frame > 170 && samus.YPosition >= 249)
                {
                    floorContacts++;
                    if (firstFloorContact < 0) firstFloorContact = frame;
                }
                if (frame > 170 && (runtime.LastMorphBallMovement is { HitCeiling: true } ||
                    runtime.LastBombJumpMovement is { Vertical.Collided: true } && samus.YPosition == 215)) ceilingContacts++;
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.BombJumpDirection:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.HorizontalSpeed.BaseFixed:X8},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{(samus.BombJumpPoseInputLocked ? 1 : 0)},{(samus.BombJumpStarting ? 1 : 0)},{(samus.BombJumpActive ? 1 : 0)}";
                foreach (var bomb in runtime.BombProjectiles.Slots)
                    actual += $",{bomb.Type:X4},{bomb.BombTimer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.InstructionPointer:X4},{bomb.InstructionTimer:X4},{bomb.SpritemapPointer:X4}";
                if (actual != string.Join(',', expected[3..]))
                {
                    if (mismatches++ < 8) Console.WriteLine($"Traversal left={left} frame={frame}:\n{actual}\n{string.Join(',', expected[3..])}");
                }
            }
            Console.WriteLine($"Traversal left={left}: x={samus.XPosition}, y={samus.YPosition}, launches={launches}, floorContacts={floorContacts}.");
            // A long list of matching frames alone would also accept a chain that
            // never leaves the floor. Require the specific sustained traversal.
            if (expectTraversal && (launches != 22 || floorContacts != 0 || ceilingContacts != 18 ||
                samus.Kinematics.XFixed != (left == 0 ? 0x00c03000u : 0x003fd000u) ||
                samus.Kinematics.YFixed != 0x00da1000u))
                throw new InvalidDataException("Fixed ceiling chain no longer sustains its captured traversal.");
            // Removing Right on frame 171 breaks only the rightward chain. The
            // mirrored one-frame Left pulse still succeeds on the cartridge;
            // integer/subpixel asymmetry must not be normalized away by the test.
            if (!expectTraversal && left == 0 && (launches != 11 || floorContacts != 207 ||
                firstFloorContact != 282 || samus.Kinematics.XFixed != 0x00074000u ||
                samus.Kinematics.YFixed != 0x00dc3bffu))
                throw new InvalidDataException("Adjacent rightward steering miss lost its native failure boundary.");
            if (!expectTraversal && left != 0 && (launches != 22 || floorContacts != 0 ||
                samus.Kinematics.XFixed != 0x00415000u || samus.Kinematics.YFixed != 0x00da1000u))
                throw new InvalidDataException("Mirrored one-frame steering pulse must retain its native success.");
        }
        Console.WriteLine($"Compared 1200 traversal frames; mismatches={mismatches}.");
        return mismatches == 0 ? 0 : 1;
    }
}
