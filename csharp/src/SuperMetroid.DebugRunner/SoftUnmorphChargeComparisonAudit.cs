using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Exact-height Up-input unmorph, earned charge, selected-X-ray Run carry and soft morph
/// against the retail CPU. Adjacent ordinary landings stop before scope activation.
/// </summary>
internal static class SoftUnmorphChargeComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "30B110B1866FCC94A05B3A6661BD387B4F710198FE3E601979EEB38A6FC3EC60")
            throw new InvalidDataException("Use the accepted soft-unmorph-charge native v2 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 47808 || rows.Any(row => row.Length != 30))
            throw new InvalidDataException("Unexpected soft-unmorph-charge trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[^2]},{row[^1]}"))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int downFrame = int.Parse(seed[1]);
            int height = int.Parse(seed[^2]), up = int.Parse(seed[^1]);
            bool prepared = height == 202 && up == 20;
            if (seed[0] is not ("0" or "1") || downFrame < 224 || downFrame > 241 || height < 201 || height > 203 || up < 19 || up > 21)
                throw new InvalidDataException("Invalid soft-unmorph-charge seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                bool solid = y <= 16 && (y == 16 || x == 1 || x == 142);
                level.SetForegroundEntry(index, solid ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.XrayScope);
            samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Charge;
            samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = (ushort)height;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            samus.Kinematics.YDirection = 2;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 0x0804 : 0x0808);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false, bounced = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
                SnesButton expected = frame == up ? SnesButton.Up : 0;
                if (frame == 60) expected |= SnesButton.Select;
                if (frame >= 61 && frame <= 125 || frame >= downFrame + 1) expected |= SnesButton.X;
                if (frame >= 126 && frame <= downFrame + 1) expected |= SnesButton.B;
                if (frame >= 130 && (frame < downFrame - 2 || frame > downFrame)) expected |= forward;
                if (frame >= 145) expected |= SnesButton.A;
                if (frame == downFrame - 2 || frame >= downFrame && frame < 300) expected |= SnesButton.Down;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                bounced |= samus.MorphBallBounceState != 0;
                if (runtime.TimeIsFrozen || frame >= 60 && samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
                    throw new InvalidDataException("The intended preparation/carry unexpectedly activated X-ray or missed Select.");
                if (frame == 125)
                {
                    uint verticalSpeed = (uint)samus.Kinematics.YSpeed << 16 | samus.Kinematics.YSubspeed;
                    if (verticalSpeed != (prepared ? 0x2f400u : 0) || runtime.Projectiles.FlareCounter != 65)
                        throw new InvalidDataException("Soft-unmorph preparation failed to distinguish residual speed from adjacent landings.");
                }
                if (prepared && downFrame <= 237 && frame >= 125 && frame < 300 && runtime.Projectiles.FlareCounter != 65)
                    throw new InvalidDataException("Soft-unmorph Run/jump/morph sequence lost its earned charge.");
                if (prepared && frame == 299 && SamusState.IsStableBallPose(samus.Pose) != (downFrame <= 237))
                    throw new InvalidDataException("Soft-morph timing no longer distinguishes the late missed-morph controls.");
                if (prepared && downFrame <= 237 && frame == 300 &&
                    (runtime.BombProjectiles.BombCounter != 5 || runtime.Projectiles.FlareCounter != 0))
                    throw new InvalidDataException("Soft-unmorph charge carry did not release five bombs on Down release.");
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{bomb.Type:X4},{bomb.InstructionPointer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.BombSpreadXVelocity:X4},{bomb.BombSpreadYVelocity:X4},{samus.MorphBallBounceState:X4},{samus.SelectedHudItem:X4},{(runtime.TimeIsFrozen ? 1 : 0):X4},{runtime.Projectiles.ProjectileCounter:X4}";
                if (actual != string.Join(',', row[4..^2]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"SOFT UNMORPH {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..^2])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != (prepared ? 320 : 126)) throw new InvalidDataException("Incomplete soft-unmorph-charge case.");
            if (prepared && bounced != (downFrame <= 230))
                throw new InvalidDataException("Soft-morph bounce window differs from the native timing controls.");
            cases++;
        }
        if (cases != 324) throw new InvalidDataException("Incomplete soft-unmorph-charge matrix.");
        Console.WriteLine($"Soft-unmorph charge: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
