using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-driven Kago attempts for airborne passage through Kamer and Kzan platforms.</summary>
internal static class KagoPassageAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "FE785002F166321AE3A0674B1CB98ADC3BCCFF5FDC9BA7B523795953AF3D526B")
            throw new InvalidDataException("Use the accepted native Kago passage capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 72000 || rows.Any(row => row.Length != 29))
            throw new InvalidDataException("Incomplete Kago passage matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..5])))
        {
            var seed = group.First();
            int actor = int.Parse(seed[0]), geometry = int.Parse(seed[1]);
            bool left = seed[2] == "1";
            int pattern = int.Parse(seed[3]), delay = int.Parse(seed[4]);
            if (((actor * 2 + geometry) * 2 + (left ? 1 : 0)) * 75 + pattern * 25 + delay != cases)
                throw new InvalidDataException("Reordered Kago passage cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 48 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 999;
            samus.XPosition = 1024; samus.YPosition = (ushort)(448 + geometry * 16 + (pattern == 2 ? 12 : 0));
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = pattern == 2 ? (byte)(left ? 0x32 : 0x31) :
                (byte)(left ? 0x2a : 0x29);
            samus.Kinematics.YSpeed = 3; samus.Kinematics.YDirection = 2;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)((pattern == 2 ? 0x0800 : 0x0600) | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            var population = new List<RoomEnemyPopulationRecord>
            {
                new((ushort)(actor == 2 ? 0xdfff : (actor == 0 ? 0xd5ff : 0xd83f)), 1024, 512,
                    (ushort)(actor == 0 ? 0x10 : 0), 0xa800, (ushort)(actor == 0 ? 0x0404 : 0),
                    (ushort)(actor == 2 ? 0x40 : (actor == 0 ? 0x8000 : 0)),
                    (ushort)(actor == 2 ? 0x8018 : (actor == 0 ? 8 : 0x2800))),
            };
            if (actor == 2) population.Add(new(0xe03f,1024,524,0,0x0900,0,0,0));
            var selected = new PopulationSelectionAddressSpace(bus, population);
            runtime.Enemies.Load(selected, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(),
                () => 0, samus: samus, level: level);
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[5]) != frame) throw new InvalidDataException("Reordered Kago passage frames.");
                ushort input = ushort.Parse(row[6], NumberStyles.HexNumber);
                ushort expected = 0;
                if (pattern == 0 && frame >= delay && frame < 40) expected = (ushort)(left ? 0x100 : 0x200);
                if (pattern == 1 && (frame == delay || frame == 4 + delay)) expected = 0x400;
                if (pattern == 2 && frame == delay) expected = 0x800;
                if (input != expected) throw new InvalidDataException("Changed Kago input.");
                runtime.StepFrame(input);
                if (pattern == 1 && frame == 30)
                    VerifyPassageWindow(actor, geometry, delay, samus, runtime.Enemies.Slots[0]);
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
                frame++;
            }
            if (frame != 80) throw new InvalidDataException("Incomplete Kago passage case.");
            cases++;
        }
        if (cases != 900) throw new InvalidDataException("Incomplete Kago passage cases.");
        Console.WriteLine($"Kago passage timeline: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

    private static void VerifyPassageWindow(
        int actor, int geometry, int delay, SamusState samus, RoomEnemySlot platform)
    {
        // Adjacent one-frame windows from original CPU execution, with no lateral
        // input: a successful morph passes THROUGH the platform, not off its edge.
        int successDelay = (actor, geometry) switch
        {
            (0, 0) => 6, (0, 1) => 3,
            (1, 0) => 7, (1, 1) => 3,
            (2, 0) => 6, (2, 1) => 2,
            _ => throw new InvalidDataException("Unknown passage fixture."),
        };
        if (delay != successDelay && delay != successDelay + 1)
            return;
        bool passed = delay == successDelay;
        if (samus.Kinematics.XFixed != 1024u << 16 ||
            (passed ? samus.YPosition <= platform.YPosition + 32 : samus.YPosition >= platform.YPosition) ||
            samus.Health != (passed && actor == 2 ? 799 : 999) ||
            samus.KnockbackDirection != 0)
            throw new InvalidDataException($"Kago actor {actor}, height {geometry}, delay {delay}: {(passed ? "passage" : "adjacent failed timing")} witness changed.");
    }
}
