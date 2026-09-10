using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Input-driven arm pumping against cartridge CPU movement on matched terrain.</summary>
internal static class ArmPumpComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "99556B48C8A16E32548FD7BDDBE3D3B42029ED8A2C0D8A8DF38D67B2608E1134")
            throw new InvalidDataException("Use the accepted native arm-pumping v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 14400 || rows.Any(row => row.Length != 19))
            throw new InvalidDataException("Incomplete arm-pumping matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1", charge = seed[1] == "1";
            int terrain = int.Parse(seed[2]), pattern = int.Parse(seed[3]);
            if (int.Parse(seed[0]) * 60 + int.Parse(seed[1]) * 30 + terrain * 6 + pattern != cases)
                throw new InvalidDataException("Reordered arm-pumping cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                bool slope = terrain is 2 or 3 && (left ? x < 61 : x > 66);
                bool wall = terrain == 4 && y < 16 && x == (left ? 57 : 70);
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 16 ? (ushort)(slope ? 0x1000 : 0x8000) : (ushort)(wall ? 0x8000 : 0));
                level.SetBehavior(index, y == 16 && slope ? (byte)((terrain == 2 ? 0x12 : 0x1b) | (left ? 0x40 : 0)) : (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
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
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered arm-pumping frames.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort expected = (ushort)(charge ? 0x40 : 0);
                if (frame >= 40) expected |= (ushort)(left ? 0x200 : 0x100);
                if (terrain == 1 && frame >= 55) expected |= 0x80;
                if (frame >= 60)
                {
                    int phase = (frame - 60) % 8;
                    if (pattern == 1 && phase < 4) expected |= 0x10;
                    if (pattern == 2 && phase < 4) expected |= 0x20;
                    if (pattern == 3) expected |= (ushort)(phase < 4 ? 0x10 : 0x20);
                    if (pattern == 4 && phase < 4) expected |= 0x40;
                    if (pattern == 5 && phase < 4) expected |= 0x30;
                }
                if (input != expected) throw new InvalidDataException("Changed arm-pumping input timeline.");
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"ARM {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 120) throw new InvalidDataException("Incomplete arm-pumping case.");
            cases++;
        }
        if (cases != 120) throw new InvalidDataException("Incomplete arm-pumping cases.");
        Console.WriteLine($"Arm pumping: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
