using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete crouch-jump trajectories against original cartridge execution.</summary>
internal static class CrouchJumpComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "46E1D8EE864C518FEEC25B745AB864D66D1E374B148CE690985AB429186C4774")
            throw new InvalidDataException("Use the accepted native crouch-jump v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 115200 || rows.Any(row => row.Length != 23))
            throw new InvalidDataException("Incomplete crouch-jump matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..6])))
        {
            var seed = group.First();
            int medium = int.Parse(seed[0]), high = int.Parse(seed[1]);
            bool left = seed[2] == "1", crouch = seed[3] == "1";
            int angle = int.Parse(seed[4]), offset = int.Parse(seed[5]);
            if (((((medium * 2 + high) * 2 + (left ? 1 : 0)) * 2 + (crouch ? 1 : 0)) * 4 + angle) * 5 + offset + 2 != cases)
                throw new InvalidDataException("Reordered crouch-jump cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: medium != 0, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 48 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall |
                (high != 0 ? SamusEquipmentFlags.HiJumpBoots : SamusEquipmentFlags.None) |
                (medium == 2 ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.None));
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = 747;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            uint apex = uint.MaxValue;
            int heightAdjustment = crouch ? (angle != 0 && offset < 0 ? 2 : -8) : 0;
            foreach (var row in group)
            {
                if (int.Parse(row[6]) != frame) throw new InvalidDataException("Reordered crouch-jump frames.");
                ushort input = ushort.Parse(row[7], NumberStyles.HexNumber);
                ushort expected = (ushort)(crouch && frame == 2 ? 0x400 : 0);
                if (frame >= 24 && frame < 180) expected |= 0x80;
                if (frame >= 24 + offset && frame < 180) expected |= (ushort)(angle * 0x10);
                if (input != expected) throw new InvalidDataException("Changed crouch-jump input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                if (frame >= 24)
                    apex = Math.Min(apex, samus.Kinematics.YFixed);
                if (frame == 24 && samus.Kinematics.YFixed !=
                    CrouchJumpFixtureData.StandingLaunchY + (heightAdjustment << 16))
                    throw new InvalidDataException($"Crouch jump {group.Key}: launch-height exception changed.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4}";
                if (actual != string.Join(',', row[8..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"CROUCH {group.Key} frame={frame}: {actual} != {string.Join(',', row[8..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 240) throw new InvalidDataException("Incomplete crouch-jump case.");
            uint standingApex = CrouchJumpFixtureData.StandingApex(medium == 1, high != 0);
            if (apex != standingApex + (heightAdjustment << 16))
                throw new InvalidDataException($"Crouch jump {group.Key}: maximum height changed.");
            if (samus.Kinematics.XFixed != 1024u << 16 ||
                samus.Kinematics.YFixed != CrouchJumpFixtureData.StandingLaunchY ||
                samus.Kinematics.YRadius != 21 || samus.Kinematics.YDirection != 0)
                throw new InvalidDataException($"Crouch jump {group.Key}: complete landing changed.");
            cases++;
        }
        if (cases != 480) throw new InvalidDataException("Incomplete crouch-jump cases.");
        Console.WriteLine($"Crouch jump: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
