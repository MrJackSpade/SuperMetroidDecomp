using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-driven Kago attempts against both native Kamer platform families.</summary>
internal static class KagoKamerAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "DA7113BBF6B97D8C95B422A584F118A6580E8D26D2EFE48CCA02071B78151E1D")
            throw new InvalidDataException("Use the accepted native Kago Kamer capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 34560 || rows.Any(row => row.Length != 29))
            throw new InvalidDataException("Incomplete Kago Kamer matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..5])))
        {
            var seed = group.First();
            int actor = int.Parse(seed[0]), geometry = int.Parse(seed[1]);
            bool left = seed[2] == "1";
            int pattern = int.Parse(seed[3]), delay = int.Parse(seed[4]);
            if (((actor * 2 + geometry) * 2 + (left ? 1 : 0)) * 36 + pattern * 12 + delay != cases)
                throw new InvalidDataException("Reordered Kago Kamer cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == (geometry == 0 ? 32 : 48) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 999;
            samus.XPosition = 1024; samus.YPosition = (ushort)((geometry == 0 ? 491 : 483) + (pattern == 2 ? 14 : 0));
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = pattern == 2 ? (byte)(left ? 0x41 : 0x1d) :
                left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((pattern == 2 ? 0x0400 : 0) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            var selected = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord((ushort)(actor == 0 ? 0xd5ff : 0xd83f),
                    (ushort)(actor == 1 && geometry == 0 ? 992 : 1024),
                    (ushort)(geometry == 1 ? 512 : (actor == 1 ? 488 : 448)),
                    (ushort)(actor == 1 ? 0 : (geometry == 1 ? 0x10 : 0x110)),
                    0xa800, (ushort)(actor == 1 ? 0 : 0x0404),
                    (ushort)(actor == 1 ? 1 : 0x8000), (ushort)(actor == 1 ? 0x2810 : 8))]);
            runtime.Enemies.Load(selected, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(),
                () => 0, samus: samus, level: level);
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[5]) != frame) throw new InvalidDataException("Reordered Kago Kamer frames.");
                ushort input = ushort.Parse(row[6], NumberStyles.HexNumber);
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
                    $"{runtime.Enemies.Slots[0].XPosition:X4}{runtime.Enemies.Slots[0].XSubposition:X4},{runtime.Enemies.Slots[0].YPosition:X4}{runtime.Enemies.Slots[0].YSubposition:X4}," +
                    $"{samus.Kinematics.ExtraXDisplacement:X4}{samus.Kinematics.ExtraXSubdisplacement:X4},{samus.Kinematics.ExtraYDisplacement:X4}{samus.Kinematics.ExtraYSubdisplacement:X4},{runtime.Enemies.Slots[0].SpritemapPointer:X4}";
                if (actual != string.Join(',', row[7..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"KAGO {group.Key} frame={frame}: {actual} != {string.Join(',', row[7..])}");
                    reported = true;
                }
                if (samus.Health != 999 || samus.KnockbackDirection != 0)
                    throw new InvalidDataException("Kamer contact acquired non-native damage or knockback.");
                // The initial dispatcher runs its wait routine immediately. With wait
                // eight, the first physical move is frame eight, not frame nine.
                if (actor == 0 && frame == 8 && runtime.Enemies.Slots[0].YPosition !=
                    (geometry == 0 ? 449 : 511))
                    throw new InvalidDataException("Kamer initial dispatch delayed its first movement.");
                // A clear upward carry preserves the prospective running input. The
                // original one-pixel forward check must still happen before that pose.
                if (actor == 0 && geometry == 1 && pattern == 0 && delay == 0 && frame == 15 &&
                    (samus.Kinematics.XFixed != (left ? 0x04034000u : 0x03fcc000u) ||
                     samus.Kinematics.YFixed != 0x01db0000 || samus.Kinematics.ExtraYDisplacement != 0xffff))
                    throw new InvalidDataException("Rising-platform run transition omitted native displacement.");
                if (actor == 1 && geometry == 1 && frame == 0 &&
                    (samus.Kinematics.ExtraXDisplacement != 1 || runtime.Enemies.Slots[0].XPosition != 1025))
                    throw new InvalidDataException("Horizontal Kamer did not carry its rider on the first frame.");
                frame++;
            }
            if (frame != 120) throw new InvalidDataException("Incomplete Kago Kamer case.");
            cases++;
        }
        if (cases != 288) throw new InvalidDataException("Incomplete Kago Kamer cases.");
        Console.WriteLine($"Kago Kamer timeline: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
