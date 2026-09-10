using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-driven Kago attempts against a falling, damage-dealing Kzan pair.</summary>
internal static class KagoKzanAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "B213A17EB63EC3A0F7192239D929B737ACDB4BA072E0DA64BFF3881195BE63BC")
            throw new InvalidDataException("Use the accepted native Kago Kzan capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 5760 || rows.Any(row => row.Length != 25))
            throw new InvalidDataException("Incomplete Kago Kzan matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int pattern = int.Parse(seed[1]), delay = int.Parse(seed[2]);
            if (int.Parse(seed[0]) * 36 + pattern * 12 + delay != cases)
                throw new InvalidDataException("Reordered Kago Kzan cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 32 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 999;
            samus.XPosition = 1024; samus.YPosition = pattern == 2 ? (ushort)505 : (ushort)491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = pattern == 2 ? (byte)(left ? 0x41 : 0x1d) :
                left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((pattern == 2 ? 0x0400 : 0) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            var selected = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord(0xdfff,1024,448,0,0xa800,0,0x40,0x8008),
                 new RoomEnemyPopulationRecord(0xe03f,1024,448,0,0x0900,0,0x40,0x8008)]);
            runtime.Enemies.Load(selected, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(),
                () => 0, samus: samus, level: level);
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered Kago Kzan frames.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expected = 0;
                if (pattern == 0 && frame >= 8 + delay && frame < 40) expected = (ushort)(left ? 0x100 : 0x200);
                if (pattern == 1 && (frame == 8 + delay || frame == 12 + delay)) expected = 0x400;
                if (pattern == 2 && frame == 8 + delay) expected = 0x800;
                if (input != expected) throw new InvalidDataException("Changed Kago input.");
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{samus.Health:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4},{samus.KnockbackDirection:X4}," +
                    $"{runtime.Enemies.Slots[0].YPosition:X4}{runtime.Enemies.Slots[0].YSubposition:X4},{(ushort)runtime.Enemies.KzanStates[0]!.Function:X4},{runtime.Enemies.Slots[1].YPosition:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"KAGO {group.Key} frame={frame}: {actual} != {string.Join(',', row[5..])}");
                    reported = true;
                }
                // Adjacent input frames separate the successful uninterruptible turn
                // from normal damage knockback. Both still lose the full 200 health.
                if (pattern == 0 && delay is 2 or 3 && frame == 11 &&
                    (samus.Health != 799 || (samus.KnockbackDirection == 0) != (delay == 2)))
                    throw new InvalidDataException("Kzan turn timing no longer separates damage from knockback.");
                // Hurt movement can leave ground-ball art airborne. Its downward stop
                // must not run the falling-ball bounce/landing transition table.
                if (pattern == 1 && delay == 0 && frame == 24 &&
                    (samus.Kinematics.YFixed != 0x01f1a7ff || samus.Kinematics.YSpeed != 0 ||
                     samus.Kinematics.YSubspeed != 0x1c00 || samus.Kinematics.YDirection != 2))
                    throw new InvalidDataException("Ground-ball collision erased the native post-hurt vertical state.");
                // Enemy and block pose-expansion passes must retain their original
                // order: the later terrain observation restores the floor fraction.
                if (pattern == 0 && delay == 11 && frame == 26 &&
                    samus.Kinematics.YFixed != 0x01ebffff)
                    throw new InvalidDataException("Landing expansion reordered enemy/terrain fraction writes.");
                frame++;
            }
            if (frame != 80) throw new InvalidDataException("Incomplete Kago Kzan case.");
            cases++;
        }
        if (cases != 72) throw new InvalidDataException("Incomplete Kago Kzan cases.");
        Console.WriteLine($"Kago Kzan timeline: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
