using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>#473 ordinary wall input windows; no grapple owner or artificial trigger.</summary>
internal static class WallJumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        bool checksSpeed = rows.Length is 6240 or 7800 or 9360 && rows[0].Length == 18;
        bool hasPostInput = checksSpeed || rows.Length == 6240 && rows[0].Length == 14;
        int inputModes = hasPostInput ? rows.Length / 1560 : 1;
        bool checksHistoryWords = hasPostInput || rows.Length == 1560 && rows[0].Length == 13;
        bool hasHistory = hasPostInput || rows.Length == 1560 && (rows[0].Length == 9 || checksHistoryWords);
        if (!hasHistory && (rows.Length != 780 || rows[0].Length != 8))
            throw new InvalidDataException("Expected 26, 52, 208, 260, or 312 cases of 30 frames.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        VerifyGrappleLaunchHistory(bus);
        int sample = 0, mismatches = 0, historyMismatches = 0;
        int speedMismatches = 0;
        int[] mismatchesByPostInput = new int[inputModes];
        for (int postInput = 0; postInput < inputModes; postInput++)
        for (int history = 0; history < (hasHistory ? 2 : 1); history++)
        for (int left = 0; left < 2; left++)
        for (int delay = 0; delay <= 12; delay++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var level = runtime.LevelData!;
            for (int y = 0; y <= 16; y++)
                level.SetForegroundEntry(y * level.WidthInBlocks + (left != 0 ? 8 : 7), 0x8000);
            if (postInput == 5)
                level.SetForegroundEntry(7 * level.WidthInBlocks + (left != 0 ? 7 : 8), 0x8000);
            var samus = runtime.Samus!;
            if (hasPostInput) samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.XPosition = (ushort)(left != 0 ? 122 : 134);
            samus.YPosition = 160;
            samus.Kinematics.YSubposition = 0;
            samus.Pose = left != 0 ? SamusPoseIds.SpinJumpRightPose : SamusPoseIds.SpinJumpLeftPose;
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left != 0 ? 0x0308 : 0x0304);
            samus.PoseHistory.LastDifferentPose = 0;
            samus.PoseHistory.LastDifferentDirectionAndMovement = (ushort)(history != 0 ? 0x0300 : 0);
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.Kinematics.YDirection = 2;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            int firstWallJump = -1;
            int managedLaunches = 0, nativeLaunches = 0;
            bool managedWasWall = false, nativeWasWall = false;
            ushort nativeMinimumY = ushort.MaxValue;
            for (int frame = 0; frame < 30; frame++)
            {
                var row = rows[sample++];
                if (hasPostInput)
                {
                    if (int.Parse(row[0]) != postInput)
                        throw new InvalidDataException("Reordered post-walljump input trace.");
                    row = row[1..];
                }
                if (hasHistory)
                {
                    if (int.Parse(row[0]) != history)
                        throw new InvalidDataException("Reordered walljump history trace.");
                    row = row[1..];
                }
                if (int.Parse(row[0]) != left || int.Parse(row[1]) != delay || int.Parse(row[2]) != frame)
                    throw new InvalidDataException("Reordered walljump trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{samus.AnimationFrame:X4}";
                string expected = string.Join(',', row[4..8]);
                if (actual != expected) mismatchesByPostInput[postInput]++;
                if (actual != expected && mismatches++ < 16)
                    Console.WriteLine($"WALL postInput={postInput} history={history} left={left} delay={delay} frame={frame}: {actual} != {expected}");
                if (checksHistoryWords)
                {
                    var h = samus.PoseHistory;
                    string actualHistory = $"{h.PreviousPose:X4},{h.PreviousDirectionAndMovement:X4},{h.LastDifferentPose:X4},{h.LastDifferentDirectionAndMovement:X4}";
                    string expectedHistory = string.Join(',', row[8..12]);
                    if (actualHistory != expectedHistory && historyMismatches++ < 16)
                        Console.WriteLine($"HISTORY history={history} left={left} delay={delay} frame={frame}: {actualHistory} != {expectedHistory}");
                }
                if (checksSpeed)
                {
                    var speed = samus.HorizontalSpeed;
                    string actualSpeed = $"{speed.BaseFixed:X8},{speed.ExtraRunSpeed:X4}{speed.ExtraRunSubspeed:X4},{speed.AccelerationMode:X4},{speed.SpeedDivisor:X4}";
                    string expectedSpeed = string.Join(',', row[12..]);
                    if (actualSpeed != expectedSpeed && speedMismatches++ < 16)
                        Console.WriteLine($"SPEED postInput={postInput} history={history} left={left} delay={delay} frame={frame}: {actualSpeed} != {expectedSpeed}");
                }
                bool managedIsWall = samus.ReadMovementType(bus) == SamusMovementType.WallJumping;
                bool nativeIsWall = byte.Parse(row[6], NumberStyles.HexNumber) is
                    SamusPoseIds.WallJumpRightPose or SamusPoseIds.WallJumpLeftPose;
                nativeMinimumY = Math.Min(nativeMinimumY, (ushort)(uint.Parse(row[5], NumberStyles.HexNumber) >> 16));
                if (managedIsWall && !managedWasWall) managedLaunches++;
                if (nativeIsWall && !nativeWasWall) nativeLaunches++;
                managedWasWall = managedIsWall;
                nativeWasWall = nativeIsWall;
                if (firstWallJump < 0 && managedIsWall) firstWallJump = frame;
            }
            Console.WriteLine($"WINDOW history={history} left={left} delay={delay} firstWallJump={firstWallJump}");
            if (postInput == 4)
            {
                Console.WriteLine($"REPEAT history={history} left={left} delay={delay}: managed={managedLaunches} native={nativeLaunches}");
                // Native capture confirms both launches for delays 2..8. The
                // adjacent failed-first-launch cases still execute the second jump.
                int expectedLaunches = delay is >= 2 and <= 8 ? 2 : 1;
                if (nativeLaunches != expectedLaunches || managedLaunches != nativeLaunches)
                    throw new InvalidDataException("Same-wall fixture did not execute the verified launch sequence.");
            }
            if (postInput == 5)
                Console.WriteLine($"OVERHANG history={history} left={left} delay={delay}: nativeMinY={nativeMinimumY} launches={nativeLaunches}");
        }
        Console.WriteLine($"Walljump: {sample} samples, {mismatches} position/pose/animation mismatches.");
        Console.WriteLine($"History-word mismatches: {historyMismatches} (checked={checksHistoryWords}).");
        for (int mode = 0; mode < inputModes; mode++)
            Console.WriteLine($"Post-input mode {mode}: {mismatchesByPostInput[mode]} motion/pose/animation mismatches.");
        Console.WriteLine($"Speed-word mismatches: {speedMismatches} (checked={checksSpeed}).");
        return mismatches == 0 && historyMismatches == 0 && speedMismatches == 0 ? 0 : 1;
    }

    private static void VerifyGrappleLaunchHistory(SuperMetroidAddressSpace bus)
    {
        foreach (byte source in new[] { SamusPoseIds.GrappleWallContactLeftPose, SamusPoseIds.GrappleWallContactRightPose })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            samus.Pose = source;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Grapple.Phase = GrapplePhase.WallJumping;
            ushort previousMetadata = (ushort)(samus.ReadPoseXDirection(bus) | ((byte)samus.ReadMovementType(bus) << 8));
            samus.PoseHistory.PreviousPose = source;
            samus.PoseHistory.PreviousDirectionAndMovement = previousMetadata;
            samus.PoseHistory.LastDifferentPose = SamusPoseIds.FacingRightNormalPose;
            samus.PoseHistory.LastDifferentDirectionAndMovement = 8;
            runtime.StepFrame(0);
            var history = samus.PoseHistory;
            ushort currentMetadata = (ushort)(samus.ReadPoseXDirection(bus) | ((byte)samus.ReadMovementType(bus) << 8));
            if (runtime.LastGrappleMovement is not { WallJumpStarted: true } ||
                history.PreviousPose != samus.Pose || history.PreviousDirectionAndMovement != currentMetadata ||
                history.LastDifferentPose != source || history.LastDifferentDirectionAndMovement != previousMetadata)
                throw new InvalidDataException($"Grapple launch ${source:X2} -> ${samus.Pose:X2} failed to commit its native transitional-slot history: " +
                    $"{history.PreviousPose:X4},{history.PreviousDirectionAndMovement:X4},{history.LastDifferentPose:X4},{history.LastDifferentDirectionAndMovement:X4}.");
        }
        Console.WriteLine("Grapple launch history: both forced launch directions commit exactly once.");
    }
}
