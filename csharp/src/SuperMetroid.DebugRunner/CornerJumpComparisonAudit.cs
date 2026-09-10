using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete corner-jump trajectories against original cartridge execution.</summary>
internal static class CornerJumpComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "E2FD60B82E140AD17DE6C0962BC3C34995FB026EC1DD8EC2568A57804DE8DAB2")
            throw new InvalidDataException("Use the accepted native corner-jump v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 302400 || rows.Any(row => row.Length != 22))
            throw new InvalidDataException("Incomplete corner-jump matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..5])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int speed = int.Parse(seed[1]), reverse = int.Parse(seed[2]);
            int delay = int.Parse(seed[3]), jumpdelay = int.Parse(seed[4]);
            int caseIndex = ((((left ? 1 : 0) * 3 + speed) * 2 + reverse) * 25 + delay) * 9 + jumpdelay;
            if (caseIndex != cases)
                throw new InvalidDataException("Reordered corner-jump cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool gap = left ? x <= 61 : x >= 66;
                level.SetForegroundEntry(index, y == 48 || (y == 32 && !gap) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = 491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MovingLeftNormalPose : SamusPoseIds.MovingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.HorizontalSpeed.ExtraRunSpeed = (ushort)(speed * 2);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x0100 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch((ushort)(0x8000 | (left ? 0x200 : 0x100)));
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[5]) != frame) throw new InvalidDataException("Reordered corner-jump frames.");
                ushort input = ushort.Parse(row[6], NumberStyles.HexNumber);
                ushort direction = (ushort)(left ? 0x200 : 0x100);
                if (reverse != 0 && frame >= delay) direction = (ushort)(left ? 0x100 : 0x200);
                ushort expected = (ushort)(0x8000 | direction);
                if (frame >= delay + jumpdelay && frame < delay + jumpdelay + 40) expected |= 0x80;
                if (frame >= 80) expected = 0;
                if (input != expected) throw new InvalidDataException("Changed corner-jump input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                if (speed == 1 && delay == 6 && jumpdelay is 6 or 7)
                {
                    if (reverse == 1 && frame == 11)
                    {
                        bool clearOfEdge = left
                            ? samus.XPosition + samus.Kinematics.XRadius < CornerJumpFixtureData.SupportEdge(left)
                            : samus.XPosition - samus.Kinematics.XRadius >= CornerJumpFixtureData.SupportEdge(left);
                        if (!clearOfEdge || samus.ReadMovementType(bus) != SamusMovementType.TurningOnGround)
                            throw new InvalidDataException("Corner jump must retain ground-turn state with the entire body off the ledge.");
                    }
                    if (frame == 12 && reverse == 1 && jumpdelay == 6 &&
                        (samus.ReadMovementType(bus) != SamusMovementType.SpinJumping ||
                         samus.Kinematics.YDirection != 1 || samus.Kinematics.YSpeed != CornerJumpFixtureData.LaunchSpeed ||
                         samus.Kinematics.YSubspeed != CornerJumpFixtureData.LaunchSubspeed ||
                         samus.Kinematics.YFixed != (left ? CornerJumpFixtureData.LeftLaunchCenter : CornerJumpFixtureData.RightLaunchCenter)))
                        throw new InvalidDataException("Last-frame corner jump launch changed.");
                    if (frame == 13 && (reverse == 0 || jumpdelay == 7) &&
                        (samus.ReadMovementType(bus) != SamusMovementType.Falling || samus.Kinematics.YDirection != 2))
                        throw new InvalidDataException("Late/no-turn control incorrectly permits a midair jump.");
                }
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4}";
                if (actual != string.Join(',', row[7..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"CORNER {group.Key} frame={frame}: {actual} != {string.Join(',', row[7..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 112) throw new InvalidDataException("Incomplete corner-jump case.");
            cases++;
        }
        if (cases != 2700) throw new InvalidDataException("Incomplete corner-jump cases.");
        Console.WriteLine($"Corner jump: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

}
