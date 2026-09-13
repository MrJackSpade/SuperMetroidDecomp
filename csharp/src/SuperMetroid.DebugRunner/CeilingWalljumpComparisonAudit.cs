using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Ceiling-bonk far-side platform walljump against original cartridge CPU execution.
/// Includes both facings, neighboring positions/timings and a no-ceiling negative control.
/// </summary>
internal static class CeilingWalljumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "967C0F7E6B0BCEE3E48577E3BCC981701EDECB99F1BFA204B0F3EBF496067262")
            throw new InvalidDataException("Use the accepted ceiling-walljump native capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 46800 || rows.Any(row => row.Length != 27))
            throw new InvalidDataException("Unexpected ceiling-walljump trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[^2]},{row[^1]}"))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int shift = int.Parse(seed[1]);
            int jump = int.Parse(seed[^2]);
            bool ceiling = seed[^1] == "1";
            bool expectedWalljump = ceiling && jump == 134 && (left ? shift is -3 or -2 : shift is 4 or 5);
            if (seed[0] is not ("0" or "1") || seed[^1] is not ("0" or "1") || shift < -6 || shift > 6 || jump < 132 || jump > 136)
                throw new InvalidDataException("Invalid ceiling-walljump seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool solid = y <= 16 && (y == 16 || x == 1 || x == 142 || x == (left ? 58 : 85) && y >= 13 || ceiling && y == 7);
                level.SetForegroundEntry(index, solid ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)((left ? 1279 : 1024) + shift); samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0, minimumY = 235; bool reported = false, walljumped = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
                SnesButton expected = frame <= 70 ? SnesButton.X : 0;
                if (frame >= 30) expected |= forward;
                if (frame >= 30 && frame < 70) expected |= SnesButton.B;
                if (frame >= 70 && frame < jump - 1 || frame >= jump) expected |= SnesButton.A;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                if (frame < jump) minimumY = Math.Min(minimumY, samus.YPosition);
                walljumped |= samus.ReadMovementType(bus) == SamusMovementType.WallJumping;
                if (expectedWalljump && frame == 134 &&
                    (samus.ReadMovementType(bus) != SamusMovementType.WallJumping ||
                     samus.HorizontalSpeed.ExtraRunSpeed != 2 || samus.HorizontalSpeed.ExtraRunSubspeed != 0 ||
                     runtime.Projectiles.FlareCounter != 71 || (left ? samus.XPosition >= 928 : samus.XPosition < 1376)))
                    throw new InvalidDataException("Failed to walljump from the platform's far side with carried speed and charge.");
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
                    if (!reported) Console.WriteLine($"CEILING CWJ {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..^2])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 180) throw new InvalidDataException("Incomplete ceiling-walljump case.");
            if (walljumped != expectedWalljump || minimumY != (ceiling ? 140 : 124))
                throw new InvalidDataException("Walljump or ceiling contact changed from the native controls.");
            cases++;
        }
        if (cases != 260) throw new InvalidDataException("Incomplete ceiling-walljump matrix.");
        Console.WriteLine($"Ceiling CWJ: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
