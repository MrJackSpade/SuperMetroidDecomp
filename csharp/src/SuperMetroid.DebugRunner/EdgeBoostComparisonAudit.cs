using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete edge-boost trajectories against original cartridge execution.</summary>
internal static class EdgeBoostComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "83C89AFB4D0A7CC4521E4904D9F2909642104C3E8C72414FA3977697FB8FBFA7")
            throw new InvalidDataException("Use the accepted native edge-boost v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 124416 || rows.Any(row => row.Length != 23))
            throw new InvalidDataException("Incomplete edge-boost matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..6])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int family = int.Parse(seed[1]), speed = int.Parse(seed[2]), ledge = int.Parse(seed[3]);
            int gap = int.Parse(seed[4]), delay = int.Parse(seed[5]);
            if ((((((left ? 1 : 0) * 2 + family) * 3 + speed) * 2 + ledge) * 9 + gap) * 6 + delay + 1 != cases)
                throw new InvalidDataException("Reordered edge-boost cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 48 || (ledge != 0 && y == 31 && x == (left ? 63 : 64)) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? 1028 : 1020);
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = family != 0
                ? (left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose)
                : (left ? SamusPoseIds.FallingAimDownLeftPose : SamusPoseIds.FallingAimDownRightPose);
            samus.RefreshCollisionRadii(bus);
            samus.YPosition = (ushort)(512 + samus.Kinematics.YRadius + gap);
            samus.Kinematics.YSpeed = (ushort)(speed == 2 ? 5 : speed);
            samus.Kinematics.YDirection = 2;
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((family != 0 ? 0x0300 : 0x0600) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            // Down is already held in the down-aim seed, not newly pressed to morph.
            runtime.Controller1.Latch((ushort)(family != 0 ? 0x80 : 0x400));
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[6]) != frame) throw new InvalidDataException("Reordered edge-boost frames.");
                ushort input = ushort.Parse(row[7], NumberStyles.HexNumber);
                ushort expected = (ushort)(family != 0 ? 0x80 : 0x400);
                if (delay >= 0 && frame >= delay)
                    expected = (ushort)(family != 0 ? 0x90 : (frame < delay + 4 ? (left ? 0x200 : 0x100) : 0));
                if (input != expected) throw new InvalidDataException("Changed edge-boost input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                VerifyBoundaryWitness(samus, family, speed, ledge, gap, delay, frame);
                if (samus.YPosition + samus.Kinematics.YRadius > EdgeBoostFixtureData.FloorTop)
                    throw new InvalidDataException($"Edge boost {group.Key}, frame {frame}: floor penetration.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4}";
                if (actual != string.Join(',', row[8..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"EDGE {group.Key} frame={frame}: {actual} != {string.Join(',', row[8..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 96) throw new InvalidDataException("Incomplete edge-boost case.");
            if (samus.YPosition + samus.Kinematics.YRadius != EdgeBoostFixtureData.FloorTop ||
                samus.Kinematics.YDirection != 0)
                throw new InvalidDataException($"Edge boost {group.Key}: did not settle on the floor.");
            cases++;
        }
        if (cases != 1296) throw new InvalidDataException("Incomplete edge-boost cases.");
        Console.WriteLine($"Edge boost: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

    private static void VerifyBoundaryWitness(
        SamusState samus, int family, int speed, int ledge, int gap, int delay, int frame)
    {
        if (gap != 0)
            return;
        if (delay == 0 && frame == 0)
        {
            // Measured original-CPU centers: immediate expansion pushes down to
            // the underside; the empty-space twin retains its uncorrected center.
            uint expected = ledge != 0 ? EdgeBoostFixtureData.ExpandedUndersideY :
                EdgeBoostFixtureData.UnobstructedFirstFrameY(family != 0, speed);
            if (samus.Kinematics.YFixed != expected)
                throw new InvalidDataException("Immediate edge boost / empty-space control changed.");
        }
        if (speed == 2 && delay == 1 && frame == 1)
        {
            // At terminal speed, waiting just one additional frame places the
            // expanded body below the edge. Both geometry variants now agree.
            uint expected = family != 0 ? EdgeBoostFixtureData.LateSpinY : EdgeBoostFixtureData.LateDownAimY;
            if (samus.Kinematics.YFixed != expected)
                throw new InvalidDataException("One-frame-late failed edge-boost window changed.");
        }
    }
}
