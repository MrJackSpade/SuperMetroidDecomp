using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Room-local Moonwalk entry, interruption and reversal timelines against native CPU output.</summary>
internal static class MoonwalkComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "DCAB63C8B61CCC2C78504F4CC0D65ABBA6F889D35B41870CAC45B41E7197F438")
            throw new InvalidDataException("Unaccepted Moonwalk capture; use the archived native v6 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var rows = File.ReadLines(trace).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 20160 || rows.Any(row => row.Length != 22))
            throw new InvalidDataException("Unexpected Moonwalk capture dimensions.");
        int cases = 0, mismatches = 0, reports = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", enabled = seed[1] == "1";
            int medium = int.Parse(seed[2]), scenario = int.Parse(seed[3]);
            if (seed.Take(2).Any(value => value is not ("0" or "1")) || medium is < 0 or > 2 || scenario is < 0 or > 13)
                throw new InvalidDataException("Invalid Moonwalk seed.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: medium != 0, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                bool floor = y == 16 && (scenario is not (6 or 10 or 11) || (left ? x <= 64 : x >= 63));
                bool wall = scenario == 3 && y < 16 && x == (left ? 62 : 65);
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, floor || wall ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, (byte)0);
            }
            if (scenario == 9) level.SetForegroundEntry(16 * level.WidthInBlocks + (left ? 65 : 62), 0xa000);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            runtime.MoonwalkEnabled = enabled;
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | (medium == 2 ? SamusEquipmentFlags.GravitySuit : 0));
            samus.EquippedBeams = 0x1000; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = 235;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            if (runtime.NmiFrameCounter != 1)
                throw new InvalidDataException($"Unexpected fixture NMI seed {runtime.NmiFrameCounter}.");
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered Moonwalk capture.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                SnesButton forward = left ? SnesButton.Left : SnesButton.Right, back = left ? SnesButton.Right : SnesButton.Left;
                SnesButton expected = SnesButton.X;
                if (frame >= (scenario == 13 ? 70 : 40)) expected |= back;
                if (scenario is 1 or 3 && frame < 40) expected |= forward;
                if (scenario == 2 && frame == 10) expected |= SnesButton.Down;
                if (scenario == 4 && frame >= 41) expected = SnesButton.X | forward;
                if (scenario == 5 && frame >= 50) expected |= SnesButton.A;
                if (scenario == 8 && frame >= 80) expected |= SnesButton.A;
                if (scenario == 12 && frame >= 40 || scenario == 13 && frame >= 70) expected |= SnesButton.A;
                if (scenario is 7 or 10) expected |= SnesButton.R;
                if (scenario == 11) expected |= SnesButton.L;
                if (frame >= 90) expected = 0;
                if (input != (ushort)expected) throw new InvalidDataException("Changed Moonwalk inputs.");
                runtime.StepFrame(input);
                if (frame == 40 && scenario < 12)
                {
                    bool shouldMoonwalk = enabled && scenario is not (1 or 2 or 3);
                    if ((samus.ReadMovementType(bus) == SamusMovementType.Moonwalking) != shouldMoonwalk)
                        throw new InvalidDataException("Moonwalk entry ignored its setting or running/crouching/wall restriction.");
                }
                if (enabled && scenario == 4 && frame == 41 &&
                    (samus.HorizontalSpeed.BaseFixed != 0x8000 || samus.Pose !=
                        (left ? SamusPoseIds.MovingLeftNormalPose : SamusPoseIds.MovingRightNormalPose)))
                    throw new InvalidDataException("Immediate reversal lost native half-pixel starting speed.");
                if (enabled && (scenario == 5 && frame == 51 || scenario == 8 && frame == 81) &&
                    (runtime.Projectiles.FlareCounter != 0 || runtime.Projectiles.LastFiredProjectileSnapshot is null))
                    throw new InvalidDataException("Moonwalk jump did not actually release the held beam.");
                if (enabled && scenario == 6 && samus.ReadMovementType(bus) == SamusMovementType.Falling &&
                    samus.ReadPoseXDirection(bus) != (left ? 8 : 4))
                    throw new InvalidDataException("Moonwalk ledge interruption kept visual rather than movement facing.");
                if (enabled && scenario == 9 && samus.ReadMovementType(bus) == SamusMovementType.Knockback &&
                    samus.ReadPoseXDirection(bus) != (left ? 8 : 4))
                    throw new InvalidDataException("Moonwalk hurt interruption kept visual rather than movement facing.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4}," +
                    $"{samus.Health:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackDirection:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && reports++ < 20) Console.WriteLine($"MOONWALK {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 120) throw new InvalidDataException("Incomplete Moonwalk case.");
            cases++;
        }
        if (cases != 168) throw new InvalidDataException("Incomplete Moonwalk matrix.");
        Console.WriteLine($"Moonwalk: {cases} cases, {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
