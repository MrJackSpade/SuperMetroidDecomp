using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Matched real-input shot/no-shot bomb-jump timelines, including a morph tunnel.</summary>
internal static class ShotBombComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "04951D7C788EF33D63D71BB64F0AA1129A32DACBB4963FB92189DD6624B32F66")
            throw new InvalidDataException("Expected the reviewed room-size-correct shot/bomb capture.");
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 20800 || rows.Any(row => row.Length != 19))
            throw new InvalidDataException("Unexpected shot/bomb capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", tunnel = seed[1] == "1", shot = seed[2] == "1";
            int shotFrame = int.Parse(seed[3]);
            if (seed.Take(3).Any(value => value is not ("0" or "1")) || shotFrame is < 35 or > 60)
                throw new InvalidDataException("Invalid shot/bomb seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int index = 0; index < level.WidthInBlocks * level.HeightInBlocks; index++)
                level.SetBehavior(index, (byte)0);
            for (int x = 0; x < 16; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y <= 16; y++)
            {
                level.SetForegroundEntry(y * 16, 0x8000);
                level.SetForegroundEntry(y * 16 + 15, 0x8000);
            }
            if (tunnel)
                for (int x = 9; x < 15; x++) level.SetForegroundEntry(14 * 16 + (left ? 15 - x : x), 0x8000);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = 0;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 128; samus.YPosition = 249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x400 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered shot/bomb capture.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort expectedInput = frame == 0 || shot && frame == shotFrame ? (ushort)SnesButton.X : (ushort)0;
                if (frame is >= 10 and <= 20 or 24) expectedInput |= (ushort)SnesButton.Up;
                if (tunnel && (frame == shotFrame - 9 || frame == shotFrame + 1)) expectedInput |= (ushort)SnesButton.Down;
                if (tunnel && frame >= shotFrame + 8 && frame < shotFrame + 21)
                    expectedInput |= (ushort)(left ? SnesButton.Left : SnesButton.Right);
                if (input != expectedInput) throw new InvalidDataException("Changed shot/bomb input.");
                runtime.StepFrame(input);
                // Assert the technique itself, not merely agreement with a recording.
                // The explosion overlaps on frame 51; a shot on 42..51 suppresses it.
                if (!tunnel && frame == 51)
                {
                    ushort expectedJump = shot && shotFrame is >= 42 and <= 51 ? (ushort)0 : (ushort)0x0802;
                    if (samus.BombJumpDirection != expectedJump)
                        throw new InvalidDataException("Shot suppression window or adjacent launch timing changed.");
                }
                // These tunnel cases distinguish shooting from morph timing alone.
                if (tunnel && frame == 51 && shotFrame is 42 or 43)
                {
                    if (samus.BombJumpDirection != (shot ? 0 : 0x0802))
                        throw new InvalidDataException("Matched tunnel shot/no-shot launch differs.");
                }
                if (shot && frame >= shotFrame && frame <= shotFrame + 10 &&
                    runtime.Projectiles.ProjectileInvincibilityTimer != Math.Max(0, 9 - (frame - shotFrame)))
                    throw new InvalidDataException("Projectile interaction timer did not expire at the native rate.");
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.BombJumpDirection:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.ProjectileInvincibilityTimer:X4},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{runtime.Projectiles.ProjectileCounter:X4},{bomb.BombTimer:X4},{bomb.Type:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && reports++ < 12)
                        Console.WriteLine($"SHOT-BOMB {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}; beam={runtime.Projectiles.Slots[0].XPosition:X4}/{runtime.Projectiles.Slots[0].Type:X4}/{runtime.Projectiles.Slots[0].InstructionPointer:X4}/{runtime.Projectiles.Slots[0].InstructionTimer:X4}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 100) throw new InvalidDataException("Incomplete shot/bomb case.");
            cases++;
        }
        if (cases != 208) throw new InvalidDataException("Incomplete shot/bomb matrix.");
        Console.WriteLine($"Shot/bomb: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
