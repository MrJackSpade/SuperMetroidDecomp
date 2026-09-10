using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Input-driven release/aim stopping against native CPU output.</summary>
internal static class StopOnDimeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "32EF0272528F859608285FDC9E8987CC21A9396EF8230B780350F26B99DD78FD")
            throw new InvalidDataException("Use the accepted native stop-on-dime v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 15360 || rows.Any(row => row.Length != 19))
            throw new InvalidDataException("Incomplete stop-on-dime matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", dash = seed[1] == "1";
            int medium = int.Parse(seed[2]), pattern = int.Parse(seed[3]);
            if (int.Parse(seed[0]) * 48 + int.Parse(seed[1]) * 24 + medium * 8 + pattern != cases)
                throw new InvalidDataException("Reordered stop-on-dime cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: medium != 0, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 16 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | (medium == 2 ? SamusEquipmentFlags.GravitySuit : 0));
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            uint stoppedX = 0;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered stop-on-dime frames.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort expected = (ushort)(dash && frame < 80 ? 0x8000 : 0);
                if (frame < 80 || pattern == 5) expected |= (ushort)(left ? 0x200 : 0x100);
                if (frame >= 80 && pattern >= 1 && pattern <= 5)
                    expected |= (ushort)(pattern == 1 ? 0x800 : pattern is 2 or 5 ? 0x10 : pattern == 3 ? 0x20 : 0x30);
                if (pattern == 6 && frame >= 81 || pattern == 7 && frame >= 79) expected |= 0x10;
                if (input != expected) throw new InvalidDataException("Changed stop-on-dime input timeline.");
                runtime.StepFrame(input);
                // Aim stopping is a pose-driven cancellation, not a host input clamp.
                // Movement on the input frame and the following standing frame must
                // remain observable; after frame 82 these controls should be stationary.
                if (pattern is >= 1 and <= 4)
                {
                    if (frame == 82) stoppedX = samus.Kinematics.XFixed;
                    if (frame > 82 && (samus.Kinematics.XFixed != stoppedX ||
                        samus.HorizontalSpeed.BaseFixed != 0 || samus.HorizontalSpeed.ExtraRunSpeed != 0 ||
                        samus.HorizontalSpeed.ExtraRunSubspeed != 0))
                        throw new InvalidDataException("Aim stopping continued to drift after the native cancellation window.");
                }
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"STOP {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 160) throw new InvalidDataException("Incomplete stop-on-dime case.");
            if (pattern == 5 && samus.HorizontalSpeed.BaseFixed == 0)
                throw new InvalidDataException("Holding direction while aiming incorrectly triggered a stop.");
            cases++;
        }
        if (cases != 96) throw new InvalidDataException("Incomplete stop-on-dime cases.");
        Console.WriteLine($"Stop on a dime: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
