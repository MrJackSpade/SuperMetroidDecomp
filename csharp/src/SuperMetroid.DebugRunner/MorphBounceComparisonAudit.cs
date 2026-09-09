using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Full-dispatcher comparison of both Morph Ball rebounds at the impact boundary.</summary>
internal static class MorphBounceComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        uint[] speeds = [0, 0x1ffff, 0x2c7ff, 0x2e3ff, 0x2e400, 0x2ffff, 0x30000, 0x50000];
        uint[] carries = [0, 0x14000, 0x30000, 0x50000, 0x4000, 0xc000, 0x14000, 0x20000];
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 24576 || rows.Any(row => row.Length != 17))
            throw new InvalidDataException("Unexpected Morph Ball bounce capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            if (seed[0] is not ("0" or "1") || seed[3] is not ("0" or "1"))
                throw new InvalidDataException("Invalid bounce direction/input mode.");
            bool left = seed[0] == "1", held = seed[3] == "1";
            int speed = int.Parse(seed[1]), carry = int.Parse(seed[2]);
            if ((uint)speed >= speeds.Length || (uint)carry >= carries.Length)
                throw new InvalidDataException("Invalid bounce speed seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int y = 0; y <= 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + level.WidthInBlocks - 1, 0x8000);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.Health = 99;
            samus.XPosition = 128; samus.YPosition = 249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((byte)SamusMovementType.MorphBallFalling << 8) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            samus.Kinematics.YSpeed = (ushort)(speeds[speed] >> 16);
            samus.Kinematics.YSubspeed = (ushort)speeds[speed];
            samus.Kinematics.YDirection = 2;
            uint baseSpeed = carry < 4 ? carries[carry] : 0x14000;
            samus.HorizontalSpeed.BaseSpeed = (ushort)(baseSpeed >> 16);
            samus.HorizontalSpeed.BaseSubspeed = (ushort)baseSpeed;
            if (carry >= 4)
            {
                samus.HorizontalSpeed.ExtraRunSpeed = (ushort)(carries[carry] >> 16);
                samus.HorizontalSpeed.ExtraRunSubspeed = (ushort)carries[carry];
            }
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered bounce trace.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                if (input != (held ? 0x80 | (left ? 0x200 : 0x100) : 0))
                    throw new InvalidDataException("Changed bounce input.");
                runtime.StepFrame(input);
                // Gravity updates the stored magnitude before collision command
                // selection: $0002.E3FF stays below three, $0002.E400 reaches it.
                if (frame == 0 && speed != 0 && samus.MorphBallBounceState != (speed >= 4 ? 1 : 0))
                    throw new InvalidDataException($"Incorrect first-rebound speed boundary: {group.Key}.");
                if (speed >= 4 && frame == 21 && samus.MorphBallBounceState != 2)
                    throw new InvalidDataException($"Missing second rebound: {group.Key}.");
                if (speed >= 4 && frame == 24 &&
                    (samus.MorphBallBounceState != 0 || samus.Kinematics.YDirection != 0 ||
                     samus.HorizontalSpeed.BaseFixed != 0))
                    throw new InvalidDataException($"Final rebound did not ground and clear base momentum: {group.Key}.");
                if (speed >= 4 && held && carry >= 4 && frame <= 24 &&
                    (((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed) != carries[carry])
                    throw new InvalidDataException($"Rebound lost carried dash momentum: {group.Key}.");
                if (speed == 0 && held && carry < 4 && frame == 54 && samus.AnimationFrameTimer != 15)
                    throw new InvalidDataException($"Unchanged wall-stop pose restarted animation: {group.Key}.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.MorphBallBounceState:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && reports++ < 10)
                        Console.WriteLine($"MORPH-BOUNCE {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 96) throw new InvalidDataException("Incomplete bounce case.");
            cases++;
        }
        if (cases != 256) throw new InvalidDataException("Incomplete bounce matrix.");
        Console.WriteLine($"Morph bounce: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
