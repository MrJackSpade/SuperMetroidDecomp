using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Full-dispatcher comparison of impact thresholds and controller-driven fall-to-morph timing.</summary>
internal static class MorphBounceComparisonAudit
{
    public static int Run(string rom, string trace, MorphBounceAuditScenario scenario = MorphBounceAuditScenario.Impact)
    {
        bool morphTiming = scenario == MorphBounceAuditScenario.FallingMorphTiming;
        bool runJump = scenario == MorphBounceAuditScenario.RunJumpMorph;
        bool wide = morphTiming || runJump;
        int frameCount = runJump ? 180 : 96;
        uint[] speeds = [0, 0x1ffff, 0x2c7ff, 0x2e3ff, 0x2e400, 0x2ffff, 0x30000, 0x50000];
        uint[] carries = [0, 0x14000, 0x30000, 0x50000, 0x4000, 0xc000, 0x14000, 0x20000];
        uint[] timingCarries = [0, 0xc000, 0x14000, 0x20000];
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        int expectedCases = morphTiming ? 144 : 256;
        if (rows.Length != expectedCases * frameCount || rows.Any(row => row.Length != 17))
            throw new InvalidDataException("Unexpected Morph Ball bounce capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            if (seed[0] is not ("0" or "1") || seed[3] is not ("0" or "1"))
                throw new InvalidDataException("Invalid bounce direction/input mode.");
            bool left = seed[0] == "1", held = seed[3] == "1";
            int speed = int.Parse(seed[1]), carry = int.Parse(seed[2]);
            if ((uint)speed >= (runJump ? 16 : morphTiming ? 9 : speeds.Length) || (uint)carry >= (wide ? 4 : carries.Length))
                throw new InvalidDataException("Invalid bounce speed seed.");
            bool groundedRoll = morphTiming && speed == 8;
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: wide);
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
            samus.XPosition = runJump ? (ushort)1024 : wide ? (ushort)512 : (ushort)128;
            samus.YPosition = runJump ? (ushort)235 : morphTiming && !groundedRoll ? (ushort)180 : (ushort)249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            if (morphTiming)
                samus.Pose = groundedRoll ? (left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose) :
                    (left ? SamusPoseIds.FallingLeftPose : SamusPoseIds.FallingRightPose);
            if (runJump) samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            SamusMovementType movementType = morphTiming
                ? (groundedRoll ? SamusMovementType.MorphBallGround : SamusMovementType.Falling)
                : runJump ? SamusMovementType.Standing : SamusMovementType.MorphBallFalling;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((byte)movementType << 8) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            uint verticalSpeed = runJump ? 0 : morphTiming ? (groundedRoll ? 0u : 0x18000u) : speeds[speed];
            samus.Kinematics.YSpeed = (ushort)(verticalSpeed >> 16);
            samus.Kinematics.YSubspeed = (ushort)verticalSpeed;
            samus.Kinematics.YDirection = groundedRoll || runJump ? (ushort)0 : (ushort)2;
            uint baseSpeed = runJump ? 0 : morphTiming ? 0x14000u : carry < 4 ? carries[carry] : 0x14000;
            samus.HorizontalSpeed.BaseSpeed = (ushort)(baseSpeed >> 16);
            samus.HorizontalSpeed.BaseSubspeed = (ushort)baseSpeed;
            if (morphTiming) samus.HorizontalSpeed.HasRunningMomentum = carry != 0;
            if (morphTiming || carry >= 4)
            {
                uint extra = morphTiming ? timingCarries[carry] : carries[carry];
                samus.HorizontalSpeed.ExtraRunSpeed = (ushort)(extra >> 16);
                samus.HorizontalSpeed.ExtraRunSubspeed = (ushort)extra;
            }
            runtime.Controller1.Latch(0);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered bounce trace.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                int expectedInput = morphTiming ? (groundedRoll || frame >= speed * 2 + 8 ? (left ? 0x200 : 0x100) : 0) | (held ? 0x80 : 0) |
                    (!groundedRoll && (frame == speed * 2 || (frame >= speed * 2 + 2 && frame < speed * 2 + 8)) ? 0x400 : 0) :
                    held ? 0x80 | (left ? 0x200 : 0x100) : 0;
                if (runJump) expectedInput = RunJumpInput(frame, speed, carry, left, held);
                if (input != expectedInput)
                    throw new InvalidDataException("Changed bounce input.");
                runtime.StepFrame(input);
                if (runJump) VerifyRunJumpSample(samus, frame, carry, held);
                // The input-timing suite is deliberately not only an equality check:
                // early morphs rebound twice, the adjacent late morph misses both,
                // and grounded rolling never manufactures a landing bounce.
                if (morphTiming && frame is 22 or 43 or 46)
                {
                    int expectedBounce = speed < 7 ? (frame == 22 ? 1 : frame == 43 ? 2 : 0) : 0;
                    if (samus.MorphBallBounceState != expectedBounce)
                        throw new InvalidDataException($"Incorrect morph timing rebound boundary: {group.Key}, frame {frame}.");
                }
                if (morphTiming && speed == 0 && carry != 0 && frame == 0 &&
                    samus.HorizontalSpeed.AccelerationMode != SamusHorizontalAccelerationModes.Decelerating)
                    throw new InvalidDataException($"Falling aim did not select carried-momentum acceleration: {group.Key}.");
                if (morphTiming && !groundedRoll && frame == 1 &&
                    (samus.HorizontalSpeed.ExtraRunSpeed != 0 || samus.HorizontalSpeed.ExtraRunSubspeed != 0 ||
                     samus.HorizontalSpeed.HasRunningMomentum))
                    throw new InvalidDataException($"Falling lookup failure retained dash momentum: {group.Key}.");
                // Gravity updates the stored magnitude before collision command
                // selection: $0002.E3FF stays below three, $0002.E400 reaches it.
                if (!wide && frame == 0 && speed != 0 && samus.MorphBallBounceState != (speed >= 4 ? 1 : 0))
                    throw new InvalidDataException($"Incorrect first-rebound speed boundary: {group.Key}.");
                if (!wide && speed >= 4 && frame == 21 && samus.MorphBallBounceState != 2)
                    throw new InvalidDataException($"Missing second rebound: {group.Key}.");
                if (!wide && speed >= 4 && frame == 24 &&
                    (samus.MorphBallBounceState != 0 || samus.Kinematics.YDirection != 0 ||
                     samus.HorizontalSpeed.BaseFixed != 0))
                    throw new InvalidDataException($"Final rebound did not ground and clear base momentum: {group.Key}.");
                if (!wide && speed >= 4 && held && carry >= 4 && frame <= 24 &&
                    (((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed) != carries[carry])
                    throw new InvalidDataException($"Rebound lost carried dash momentum: {group.Key}.");
                if (!wide && speed == 0 && held && carry < 4 && frame == 54 && samus.AnimationFrameTimer != 15)
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
            if (frame != frameCount) throw new InvalidDataException("Incomplete bounce case.");
            cases++;
        }
        if (cases != expectedCases) throw new InvalidDataException("Incomplete bounce matrix.");
        Console.WriteLine($"Morph bounce: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

    private static ushort RunJumpInput(int frame, int timing, int runway, bool left, bool held)
    {
        int[] runFrames = [8, 16, 24, 40];
        int launch = runFrames[runway], morph = launch + 8 + timing * 2;
        int forward = left ? 0x200 : 0x100;
        if (frame < launch) return (ushort)(0x8000 | forward);
        int jump = held || frame < launch + 8 ? 0x80 : 0;
        if (frame == launch) return (ushort)(jump | 0x800);
        bool morphWindow = frame >= morph && frame < morph + 8;
        int down = frame == morph || (frame >= morph + 2 && frame < morph + 8) ? 0x400 : 0;
        return (ushort)(jump | down | (morphWindow ? 0 : forward));
    }

    private static void VerifyRunJumpSample(SamusState samus, int frame, int runway, bool held)
    {
        int[] runFrames = [8, 16, 24, 40];
        uint[] acquiredExtra = [0x7000, 0xf000, 0x17000, 0x20000];
        int launch = runFrames[runway], firstBounce = launch + 92;
        uint extra = ((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed;
        if (frame == launch && extra != acquiredExtra[runway])
            throw new InvalidDataException("Run-up did not acquire the native extra speed through controller input.");
        if (!held && samus.MorphBallBounceState != 0)
            throw new InvalidDataException("Short released-Jump control unexpectedly bounced.");
        if (!held) return;
        if (frame >= launch && frame <= firstBounce + 24 && extra != acquiredExtra[runway])
            throw new InvalidDataException("Held-Jump sequence lost acquired speed before completing both rebounds.");
        if (frame == firstBounce && samus.MorphBallBounceState != 1 ||
            frame == firstBounce + 21 && samus.MorphBallBounceState != 2)
            throw new InvalidDataException("Run/jump/morph sequence missed a native rebound.");
        if (frame == firstBounce + 24 && (samus.MorphBallBounceState != 0 ||
            samus.Kinematics.YDirection != 0 || samus.HorizontalSpeed.BaseFixed != 0))
            throw new InvalidDataException("Final landing did not clear base speed and ground the ball.");
        if (frame == firstBounce + 25 && extra != 0)
            throw new InvalidDataException("Grounded rolling retained the completed jump's extra speed.");
    }
}
