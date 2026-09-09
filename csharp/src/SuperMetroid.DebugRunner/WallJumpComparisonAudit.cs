using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>#473 ordinary wall input windows; no grapple owner or artificial trigger.</summary>
internal static class WallJumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        bool hasHistory = rows.Length == 1560 && rows[0].Length == 9;
        if (!hasHistory && (rows.Length != 780 || rows[0].Length != 8))
            throw new InvalidDataException("Expected 26 or 52 cases of 30 frames.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int sample = 0, mismatches = 0;
        for (int history = 0; history < (hasHistory ? 2 : 1); history++)
        for (int left = 0; left < 2; left++)
        for (int delay = 0; delay <= 12; delay++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int y = 0; y <= 16; y++)
                level.SetForegroundEntry(y * level.WidthInBlocks + (left != 0 ? 8 : 7), 0x8000);
            var samus = runtime.Samus!;
            samus.XPosition = (ushort)(left != 0 ? 122 : 134);
            samus.YPosition = 160;
            samus.Kinematics.YSubposition = 0;
            samus.Pose = left != 0 ? SamusPoseIds.SpinJumpRightPose : SamusPoseIds.SpinJumpLeftPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.Kinematics.YDirection = 2;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            int firstWallJump = -1;
            for (int frame = 0; frame < 30; frame++)
            {
                var row = rows[sample++];
                if (hasHistory)
                {
                    if (int.Parse(row[0]) != history)
                        throw new InvalidDataException("Reordered walljump history trace.");
                    row = row[1..];
                }
                if (int.Parse(row[0]) != left || int.Parse(row[1]) != delay || int.Parse(row[2]) != frame)
                    throw new InvalidDataException("Reordered walljump trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.AnimationFrame:X4}";
                string expected = string.Join(',', row[4..]);
                if (actual != expected && mismatches++ < 16)
                    Console.WriteLine($"WALL history={history} left={left} delay={delay} frame={frame}: {actual} != {expected}");
                if (firstWallJump < 0 && samus.ReadMovementType(bus) == SamusMovementType.WallJumping) firstWallJump = frame;
            }
            Console.WriteLine($"WINDOW history={history} left={left} delay={delay} firstWallJump={firstWallJump}");
        }
        Console.WriteLine($"Walljump: {sample} samples, {mismatches} position/pose/animation mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
