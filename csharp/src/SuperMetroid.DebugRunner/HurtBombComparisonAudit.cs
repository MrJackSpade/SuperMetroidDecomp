using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Compares #413's constructed collision seam against untouched-CPU frame traces.</summary>
internal static class HurtBombComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 12800 || rows.Any(row => row.Length != 15))
            throw new InvalidDataException("Unexpected hurt/bomb trace dimensions.");
        int mismatches = 0, samples = 0, cases = 0, reportedCases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int bombFrame = int.Parse(seed[1]), jumpFrame = int.Parse(seed[2]);
            if (seed[0] is not ("0" or "1") || bombFrame is < 0 or > 9 || jumpFrame is < 0 or > 15)
                throw new InvalidDataException("Invalid hurt/bomb case.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int x = 0; x < level.WidthInBlocks; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y <= 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + level.WidthInBlocks - 1, 0x8000);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.Bombs | SamusEquipmentFlags.MorphBall);
            samus.Health = 79; // The oracle admits one twenty-damage projectile before tracing.
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = 128; samus.YPosition = 160;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            SamusKnockbackMovement.Start(bus, samus, 0, (ushort)(left ? 1 : 0), 5);
            samus.CommitPoseHistory(bus);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered hurt/bomb trace.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expectedInput = frame >= jumpFrame ? (ushort)((left ? 0x100 : 0x200) | 0x80) : (ushort)0;
                if (input != expectedInput) throw new InvalidDataException("Changed hurt/bomb input.");
                var bomb = runtime.BombProjectiles.Slots[0];
                if (frame == bombFrame)
                {
                    // Match the oracle's collision-only stimulus: no animation/fuse
                    // update, but the actual production overlap pass publishes it.
                    bomb.Type = (ushort)SamusProjectileFamily.Bomb;
                    bomb.Damage = 30; bomb.BombTimer = 8;
                    bomb.XPosition = samus.XPosition; bomb.YPosition = samus.YPosition;
                    bomb.XRadius = bomb.YRadius = 8;
                }
                runtime.StepFrame(input);
                bomb.ClearFields();
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                    $"{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4},{samus.BombJumpDirection:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.HorizontalSpeed.BaseFixed:X8}";
                // Native handler pointers are retained in the trace for diagnosis;
                // compare observable state rather than inventing managed pointer IDs.
                string expected = string.Join(',', row[5..11].Concat(row[12..15]));
                if (actual != expected)
                {
                    mismatches++;
                    if (!reported && reportedCases++ < 16)
                        Console.WriteLine($"HURT-BOMB {group.Key} frame={frame}: {actual} != {expected}");
                    reported = true;
                }
                samples++; frame++;
            }
            if (frame != 40) throw new InvalidDataException("Incomplete hurt/bomb sequence.");
            cases++;
        }
        if (cases != 320) throw new InvalidDataException("Incomplete hurt/bomb matrix.");
        Console.WriteLine($"Hurt/bomb: {cases} cases, {samples} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
