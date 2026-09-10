using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Actual charge/run/jump/walljump/morph inputs against the cartridge CPU.</summary>
internal static class ChargedWalljumpComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "6B2C108DA71D6DAFCC3AB6C36C8D4C9D94EC6368E25DC0EB0E90524D1B382F67")
            throw new InvalidDataException("Use the accepted charged-walljump native v3 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 7020 || rows.Any(row => row.Length != 25))
            throw new InvalidDataException("Unexpected charged-walljump trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..2])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int downFrame = int.Parse(seed[1]);
            if (seed[0] is not ("0" or "1") || downFrame < 150 || downFrame > 210 || downFrame % 5 != 0)
                throw new InvalidDataException("Invalid charged-walljump seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool solid = y <= 16 && (y == 16 || x == (left ? 61 : 66));
                level.SetForegroundEntry(index, solid ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? 1047 : 1000); samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right, back = left ? SnesButton.Right : SnesButton.Left;
                SnesButton expected = frame < 80 ? SnesButton.X : 0;
                if (frame >= 60) expected |= frame < 92 ? forward : back;
                if (frame >= 60 && frame < 73) expected |= SnesButton.B;
                if (frame >= 70 && frame < 92 || frame >= 94) expected |= SnesButton.A;
                if (frame >= downFrame) expected |= SnesButton.X | SnesButton.Down;
                if (frame >= 240) expected &= ~SnesButton.Down;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                if (frame == 94 && (samus.ReadMovementType(bus) != SamusMovementType.WallJumping ||
                    runtime.Projectiles.FlareCounter != 71))
                    throw new InvalidDataException("The sequence did not enter a genuinely charged walljump.");
                if (downFrame <= 195 && frame >= downFrame && frame < 240 && runtime.Projectiles.FlareCounter != 71)
                    throw new InvalidDataException("Morph/roll failed to preserve the original charge.");
                if (downFrame == 195 && frame >= 195 && frame < 240 && samus.MorphBallBounceState != 0)
                    throw new InvalidDataException("Late soft morph unexpectedly bounced.");
                if (downFrame <= 195 && frame == 240 &&
                    (runtime.BombProjectiles.BombCounter != 5 || runtime.Projectiles.FlareCounter != 0))
                    throw new InvalidDataException("Releasing Down did not consume charge and produce the spread.");
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{bomb.Type:X4},{bomb.InstructionPointer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.BombSpreadXVelocity:X4},{bomb.BombSpreadYVelocity:X4},{samus.MorphBallBounceState:X4}";
                if (actual != string.Join(',', row[4..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"CHARGED WALLJUMP {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 270) throw new InvalidDataException("Incomplete charged-walljump case.");
            cases++;
        }
        if (cases != 26) throw new InvalidDataException("Incomplete charged-walljump matrix.");
        Console.WriteLine($"Charged walljump: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
