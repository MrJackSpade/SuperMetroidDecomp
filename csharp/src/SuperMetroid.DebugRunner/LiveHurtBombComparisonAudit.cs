using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Normal bomb placement and full fuse, with a bounded timed damage stimulus.</summary>
internal static class LiveHurtBombComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 32000 || rows.Any(row => row.Length != 24))
            throw new InvalidDataException("Unexpected live hurt/bomb capture dimensions.");
        int cases = 0, mismatches = 0, reportedCases = 0, retainedLaunches = 0, postLaunchBoosts = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int contactFrame = int.Parse(seed[1]), jumpFrame = int.Parse(seed[2]);
            if (seed[0] is not ("0" or "1") || contactFrame is < 44 or > 53 || jumpFrame is < 52 or > 67)
                throw new InvalidDataException("Invalid live hurt/bomb case.");
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
            samus.Health = 99;
            samus.XPosition = 128; samus.YPosition = 249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose;
            samus.RefreshCollisionRadii(bus);
            runtime.BombProjectiles.StepFrame(bus, level, samus, (ushort)SnesButton.X, (ushort)SnesButton.X, runtime.Plms);
            // Match the oracle's single setup boundary: unmorphing itself is outside
            // this test. No pose, position, fuse, or bomb state is forced after here.
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.YPosition = 235;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false, retained = false, boostedAfterLaunch = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered live hurt/bomb trace.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expectedInput = frame >= jumpFrame ? (ushort)((left ? 0x100 : 0x200) | 0x80) : (ushort)0;
                if (input != expectedInput) throw new InvalidDataException("Changed live hurt/bomb input.");
                if (frame == contactFrame)
                {
                    var projectile = runtime.Enemies.EnemyProjectiles[^1];
                    projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
                    projectile.PreInstruction = EnemyProjectileCodePointers.RTS_8684FB;
                    projectile.InstructionTimer = 2;
                    projectile.XPosition = (ushort)(samus.XPosition + (left ? -8 : 8));
                    projectile.YPosition = samus.YPosition;
                    projectile.XRadius = projectile.YRadius = 8;
                    projectile.Damage = 20;
                    projectile.InvincibilityFrames = 96;
                    projectile.CanDamageSamus = true;
                }
                runtime.StepFrame(input);
                if (frame == contactFrame) runtime.Enemies.EnemyProjectiles[^1].Clear();
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}," +
                    $"{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4},{samus.BombJumpDirection:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.Health:X4},{runtime.BombProjectiles.BombCounter:X4},{bomb.Type:X4},{bomb.BombTimer:X4}," +
                    $"{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.InstructionPointer:X4},{bomb.InstructionTimer:X4},{bomb.SpritemapPointer:X4},{bomb.XRadius:X2}{bomb.YRadius:X2}";
                if (actual != string.Join(',', row[5..]))
                {
                    mismatches++;
                    if (!reported && reportedCases++ < 16)
                        Console.WriteLine($"LIVE-HURT-BOMB {group.Key} frame={frame}: {actual} != {string.Join(',', row[5..])}");
                    reported = true;
                }
                retained |= samus.BombJumpActive && samus.Pose is SamusPoseIds.KnockbackRightPose or SamusPoseIds.KnockbackLeftPose;
                boostedAfterLaunch |= retained && samus.Pose is SamusPoseIds.DamageBoostRightPose or SamusPoseIds.DamageBoostLeftPose;
                frame++;
            }
            if (frame != 100) throw new InvalidDataException("Incomplete live hurt/bomb sequence.");
            if (retained) retainedLaunches++;
            if (boostedAfterLaunch) postLaunchBoosts++;
            cases++;
        }
        if (cases != 320) throw new InvalidDataException("Incomplete live hurt/bomb matrix.");
        Console.WriteLine($"Live hurt/bomb: {cases} cases, {rows.Length} frames, {mismatches} mismatches; retained-pose launches={retainedLaunches}, subsequent boosts={postLaunchBoosts}.");
        return mismatches == 0 && retainedLaunches == 84 && postLaunchBoosts == 84 ? 0 : 1;
    }
}
