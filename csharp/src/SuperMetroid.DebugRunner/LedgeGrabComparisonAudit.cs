using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete ledge-grab trajectories against original cartridge execution.</summary>
internal static class LedgeGrabComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "27BF3ACB883C55E2D29A788BBB5AB95DA027CC4CF064EF90CDE43B8A28512148")
            throw new InvalidDataException("Use the accepted native ledge-grab v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 221184 || rows.Any(row => row.Length != 23))
            throw new InvalidDataException("Incomplete ledge-grab matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..6])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int family = int.Parse(seed[1]), scenario = int.Parse(seed[2]), speed = int.Parse(seed[3]);
            int gap = int.Parse(seed[4]), delay = int.Parse(seed[5]);
            if ((((((left ? 1 : 0) * 2 + family) * 2 + scenario) * 3 + speed) * 9 + gap) * 8 + delay + 1 != cases)
                throw new InvalidDataException("Reordered ledge-grab cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 48 || (y == 32 && x == (left ? 63 : 64)) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? (scenario != 0 ? 1029 : 1028) : (scenario != 0 ? 1019 : 1020));
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = family != 0
                ? (left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose)
                : scenario != 0
                    ? (left ? SamusPoseIds.NormalJumpAimDownLeftPose : SamusPoseIds.NormalJumpAimDownRightPose)
                    : (left ? SamusPoseIds.FallingAimDownLeftPose : SamusPoseIds.FallingAimDownRightPose);
            samus.RefreshCollisionRadii(bus);
            samus.YPosition = (ushort)(512 - samus.Kinematics.YRadius + (scenario != 0 ? gap - 4 : -gap));
            samus.Kinematics.YSpeed = (ushort)(speed == 2 ? 3 : speed);
            samus.Kinematics.YDirection = (ushort)(scenario != 0 ? 1 : 2);
            samus.HorizontalSpeed.BaseSpeed = (ushort)(scenario != 0 ? 2 : 0);
            samus.HorizontalSpeed.AccelerationMode = (ushort)(scenario != 0 ? 2 : 0);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((family != 0 ? 0x0300 : (scenario != 0 ? 0x0200 : 0x0600)) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            // Down is already held in the down-aim seed, not newly pressed to morph.
            runtime.Controller1.Latch((ushort)(family != 0 ? 0x80 : (scenario != 0 ? 0x480 : 0x400)));
            int frame = 0; bool reported = false;
            int? firstUpperLanding = null;
            foreach (var row in group)
            {
                if (int.Parse(row[6]) != frame) throw new InvalidDataException("Reordered ledge-grab frames.");
                ushort input = ushort.Parse(row[7], NumberStyles.HexNumber);
                ushort expected = (ushort)(family != 0 ? 0x80 : (scenario != 0 ? 0x480 : 0x400));
                if (delay >= 0 && frame >= delay)
                    expected = (ushort)(family != 0 ? 0x90 : (scenario != 0 ? 0x80 : 0) | (frame < delay + 4 ? (left ? 0x200 : 0x100) : 0));
                if (frame >= 24) expected = 0;
                if (input != expected) throw new InvalidDataException("Changed ledge-grab input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                if (samus.Kinematics.YDirection == 0 &&
                    samus.YPosition + samus.Kinematics.YRadius == LedgeGrabFixtureData.PlatformTop)
                {
                    if (firstUpperLanding is null &&
                        (samus.Kinematics.YFixed != LedgeGrabFixtureData.LandingCenter || samus.Kinematics.YRadius != 21))
                        throw new InvalidDataException($"Ledge grab {group.Key}: landing center/radius changed.");
                    firstUpperLanding ??= frame;
                }
                if (samus.YPosition + samus.Kinematics.YRadius > LedgeGrabFixtureData.FloorTop)
                    throw new InvalidDataException($"Ledge grab {group.Key}: fell through the floor.");
                if (family == 0 && scenario == 1 && speed == 0 && gap == 0 && delay == 0 && frame == 0 &&
                    (samus.HorizontalSpeed.AccelerationMode != SamusHorizontalAccelerationModes.Accelerating ||
                     samus.HorizontalSpeed.BaseFixed != LedgeGrabFixtureData.ExpansionBaseSpeed))
                    throw new InvalidDataException("Jump expansion must reset acceleration mode without clearing base speed.");
                if (family == 0 && scenario == 1 && speed == 0 && gap == 2 && delay == -1 && frame == 9 &&
                    (firstUpperLanding != 8 || samus.Pose != (left ? SamusPoseIds.NeutralJumpTransitionLeftPose : SamusPoseIds.NeutralJumpTransitionRightPose) ||
                     samus.Kinematics.YDirection != 1))
                    throw new InvalidDataException("Compact landing omitted the next-frame held-Jump restart.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4}";
                if (actual != string.Join(',', row[8..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"GRAB {group.Key} frame={frame}: {actual} != {string.Join(',', row[8..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 128) throw new InvalidDataException("Incomplete ledge-grab case.");
            if (scenario == 0 && speed == 2 && gap == 8 && delay is -1 or 0 or 1 &&
                firstUpperLanding != (delay == 0 ? 1 : 2))
                throw new InvalidDataException("Expansion no longer produces the measured one-frame early landing.");
            if (family == 0 && scenario == 1 && speed == 2 && gap == 8 && delay is 1 or 2 &&
                firstUpperLanding != (delay == 2 ? (int?)49 : null))
                throw new InvalidDataException("Ascending downgrab success / one-frame-too-early failure changed.");
            cases++;
        }
        if (cases != 1728) throw new InvalidDataException("Incomplete ledge-grab cases.");
        Console.WriteLine($"Ledge grab: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

}
