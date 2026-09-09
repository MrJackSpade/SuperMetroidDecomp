using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-driven crouch/unmorph turn timing compared with the cartridge CPU.</summary>
internal static class CrouchLockComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 19200 || rows.Any(row => row.Length != 16))
            throw new InvalidDataException("Unexpected crouch-lock capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            if (seed[0] is not ("0" or "1") || seed[1] is not ("0" or "1") || seed[3] is not ("0" or "1"))
                throw new InvalidDataException("Invalid crouch-lock case.");
            bool left = seed[0] == "1", ball = seed[1] == "1", repress = seed[3] == "1";
            int offset = int.Parse(seed[2], CultureInfo.InvariantCulture);
            if (offset is < -8 or > 16) throw new InvalidDataException("Invalid turn timing.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.Health = 99;
            samus.XPosition = 1024; samus.YPosition = ball ? (ushort)249 : (ushort)235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = ball ? (left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose) :
                (left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose);
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((byte)samus.ReadMovementType(bus) << 8) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4], CultureInfo.InvariantCulture) != frame)
                    throw new InvalidDataException("Reordered crouch-lock trace.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                int expected = frame >= 16 + offset && !(repress && frame == 48) ? (left ? 0x100 : 0x200) : 0;
                if (frame == 16) expected |= ball ? 0x800 : 0x400;
                if (input != expected) throw new InvalidDataException("Changed crouch-lock controller sequence.");
                runtime.StepFrame(input);
                VerifyLockWindow(samus, frame, left, ball, offset, repress);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && reports++ < 12)
                        Console.WriteLine($"CROUCH-LOCK {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 96) throw new InvalidDataException("Incomplete crouch-lock case.");
            cases++;
        }
        if (cases != 200) throw new InvalidDataException("Incomplete crouch-lock matrix.");
        Console.WriteLine($"Crouch lock: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

    private static void VerifyLockWindow(SamusState samus, int frame, bool left, bool ball, int offset, bool repress)
    {
        // Input is sampled before animation completion. The completion frame itself
        // still discards the turn edge: offset three for crouching, six for unmorphing.
        bool lockExpected = offset >= 0 && offset <= (ball ? 6 : 3);
        if (lockExpected && frame >= 22 && (!repress || frame <= 48))
        {
            byte crouchPose = left ? SamusPoseIds.CrouchingLeftPose : SamusPoseIds.CrouchingRightPose;
            if (samus.Pose != crouchPose || samus.Kinematics.XFixed != 0x04000000 ||
                samus.Kinematics.YFixed != 0x00f0ffff || samus.HorizontalSpeed.BaseFixed != 0)
                throw new InvalidDataException("Crouch lock must retain its original facing and fixed position despite opposite held input.");
        }
        if (lockExpected && repress && frame == 49 && samus.Pose !=
            (left ? SamusPoseIds.TurningLeftToRightCrouchingPose : SamusPoseIds.TurningRightToLeftCrouchingPose))
            throw new InvalidDataException("A fresh opposite-direction edge did not release crouch lock.");
        if (frame == 95 && (!lockExpected || repress))
        {
            bool movedOpposite = left ? samus.Kinematics.XFixed > 0x04000000 : samus.Kinematics.XFixed < 0x04000000;
            if (!movedOpposite)
                throw new InvalidDataException("Turning before/after the posture window or re-pressing direction must permit movement.");
        }
    }
}
