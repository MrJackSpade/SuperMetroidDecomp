using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Charged far-side platform walljump and soft-morph trajectory against the retail CPU.
/// Includes adjacent Jump/Down timings and a no-Forward-release negative control.
/// </summary>
internal static class ContinuousWalljumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "831BCA9D826CC63A4E329BAD672E54E45DDB06D2B98F3751345887F741920878")
            throw new InvalidDataException("Use the accepted continuous-walljump native v3 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 134400 || rows.Any(row => row.Length != 27))
            throw new InvalidDataException("Unexpected continuous-walljump trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[^2]},{row[^1]}"))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int downFrame = int.Parse(seed[1]);
            int jump = int.Parse(seed[^2]);
            bool skipRelease = seed[^1] == "1";
            bool expectedWalljump = !skipRelease && jump == 145;
            if (seed[0] is not ("0" or "1") || seed[^1] is not ("0" or "1") || downFrame < 230 || downFrame > 250 || jump < 143 || jump > 147)
                throw new InvalidDataException("Invalid continuous-walljump seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool solid = y <= 16 && (y == 16 || x == 1 || x == 142 || x == (left ? 56 : 87) && y >= 11);
                level.SetForegroundEntry(index, solid ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? 1281 : 1024); samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false, walljumped = false, bounced = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
                SnesButton expected = frame <= 70 ? SnesButton.X : 0;
                if (frame >= 30 && (skipRelease || frame != jump - 2)) expected |= forward;
                if (frame >= 30 && frame < 70) expected |= SnesButton.B;
                if (frame >= 70 && frame < jump - 1 || frame >= jump) expected |= SnesButton.A;
                if (frame >= downFrame) expected |= SnesButton.X | SnesButton.Down;
                if (frame >= 300) expected &= ~SnesButton.Down;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                walljumped |= samus.ReadMovementType(bus) == SamusMovementType.WallJumping;
                bounced |= samus.MorphBallBounceState != 0;
                if (expectedWalljump && frame == 145 &&
                    (samus.ReadMovementType(bus) != SamusMovementType.WallJumping ||
                     samus.HorizontalSpeed.ExtraRunSpeed != 2 || samus.HorizontalSpeed.ExtraRunSubspeed != 0 ||
                     runtime.Projectiles.FlareCounter != 71 || (left ? samus.XPosition >= 896 : samus.XPosition < 1408)))
                    throw new InvalidDataException("Failed to walljump from the platform's far side with carried speed and charge.");
                if (expectedWalljump && downFrame <= 245 && frame >= 145 && frame < 300 && runtime.Projectiles.FlareCounter != 71)
                    throw new InvalidDataException("Continuous walljump/morph lost the earned charge.");
                if (expectedWalljump && frame == 299)
                {
                    bool softMorph = downFrame is >= 240 and <= 245;
                    if (SamusState.IsStableBallPose(samus.Pose) != (downFrame <= 245) ||
                        samus.HorizontalSpeed.ExtraRunSpeed != (softMorph ? 2 : 0))
                        throw new InvalidDataException("Soft morph did not retain speed, or early/late control retained it incorrectly.");
                }
                if (expectedWalljump && downFrame <= 245 && frame == 300 &&
                    (runtime.BombProjectiles.BombCounter != 5 || runtime.Projectiles.FlareCounter != 0))
                    throw new InvalidDataException("Continuous-walljump carry did not release five bombs on Down release.");
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{bomb.Type:X4},{bomb.InstructionPointer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.BombSpreadXVelocity:X4},{bomb.BombSpreadYVelocity:X4},{samus.MorphBallBounceState:X4}";
                if (actual != string.Join(',', row[4..^2]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"CONTINUOUS WALLJUMP {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..^2])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 320) throw new InvalidDataException("Incomplete continuous-walljump case.");
            if (walljumped != expectedWalljump || expectedWalljump && bounced != (downFrame < 240))
                throw new InvalidDataException("Walljump or bounce window changed from the native controls.");
            cases++;
        }
        if (cases != 420) throw new InvalidDataException("Incomplete continuous-walljump matrix.");
        Console.WriteLine($"Continuous walljump: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
