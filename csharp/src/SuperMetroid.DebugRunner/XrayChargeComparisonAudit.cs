using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>
/// Actual Select/charge/Run/soft-morph inputs against the cartridge CPU, including
/// undercharge and missing-Run controls. This does not inject charge or HUD selection.
/// </summary>
internal static class XrayChargeComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "4893532C55D1162C79764520CEE8F4674F1FF9F1AACBBEC4856136215862B288")
            throw new InvalidDataException("Use the accepted xray-charge native v3 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 22500 || rows.Any(row => row.Length != 29))
            throw new InvalidDataException("Unexpected xray-charge trace dimensions.");
        int cases = 0, mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[^1]}"))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int downFrame = int.Parse(seed[1]);
            int mode = int.Parse(seed[^1]);
            int chargeEnd = mode == 1 ? 135 : 137;
            if (mode is < 0 or > 2 || seed[0] is not ("0" or "1") || downFrame < 150 || downFrame > 164)
                throw new InvalidDataException("Invalid xray-charge seed.");
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
            samus.XPosition = 1024; samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false, bounced = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered charge trace.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
                SnesButton expected = 0;
                if (frame >= 30 && (frame < downFrame - 2 || frame > downFrame)) expected |= forward;
                if (frame >= 30 && frame < 70 || mode != 2 && frame > chargeEnd && frame <= downFrame + 1) expected |= SnesButton.B;
                if (frame >= 70) expected |= SnesButton.A;
                if (frame == 75) expected |= SnesButton.Select;
                if (frame >= 76 && frame <= chargeEnd || frame >= downFrame + 1) expected |= SnesButton.X;
                if (frame == downFrame - 2 || frame >= downFrame && frame < 220) expected |= SnesButton.Down;
                if (input != (ushort)expected) throw new InvalidDataException("Changed charge inputs.");
                runtime.StepFrame(input);
                bounced |= samus.MorphBallBounceState != 0;
                if (runtime.TimeIsFrozen || frame >= 75 && samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
                    throw new InvalidDataException("The airborne Select path failed or unexpectedly activated the scope.");
                if (frame == chargeEnd && runtime.Projectiles.FlareCounter != (mode == 1 ? 58 : 60))
                    throw new InvalidDataException("Inputs failed to earn the expected charge before Run took over.");
                if (mode == 0 && downFrame <= 162 && frame > chargeEnd && frame < 220 && runtime.Projectiles.FlareCounter != 60)
                    throw new InvalidDataException("Run or morph failed to preserve the earned full charge.");
                if (mode == 2 && frame == 138 && runtime.Projectiles.FlareCounter != 0)
                    throw new InvalidDataException("Missing-Run control unexpectedly retained charge after Shoot release.");
                if (frame == 219 && downFrame <= 162 &&
                    (runtime.Projectiles.FlareCounter != (mode == 0 ? 60 : 0) || runtime.BombProjectiles.BombCounter != 0))
                    throw new InvalidDataException("Carried/undercharged control state changed before release.");
                if (frame == 220 && downFrame <= 162 &&
                    (runtime.Projectiles.FlareCounter != 0 || runtime.BombProjectiles.BombCounter != (mode == 0 ? 5 : 0)))
                    throw new InvalidDataException("Bomb Spread release ignored the retained-charge threshold.");
                var bomb = runtime.BombProjectiles.Slots[0];
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{runtime.Projectiles.FlareCounter:X4},{samus.BombSpreadChargeTimeoutCounter:X4},{runtime.BombProjectiles.BombCounter:X4}," +
                    $"{bomb.Type:X4},{bomb.InstructionPointer:X4},{bomb.XPosition:X4},{bomb.YPosition:X4},{bomb.BombSpreadXVelocity:X4},{bomb.BombSpreadYVelocity:X4},{samus.MorphBallBounceState:X4},{samus.SelectedHudItem:X4},{(runtime.TimeIsFrozen ? 1 : 0):X4},{runtime.Projectiles.ProjectileCounter:X4}";
                if (actual != string.Join(',', row[4..^1]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"XRAY CHARGE {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..^1])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 250) throw new InvalidDataException("Incomplete xray-charge case.");
            if (bounced != (downFrame <= 155))
                throw new InvalidDataException("Soft-morph timing no longer distinguishes early bouncing controls.");
            cases++;
        }
        if (cases != 90) throw new InvalidDataException("Incomplete xray-charge matrix.");
        Console.WriteLine($"X-ray charge: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
