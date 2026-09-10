using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Bomb-family reaction dispatch and real PLM destruction during falling turns.</summary>
internal static class QuickDropBombAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "295138040F122275E2BF5DEDD7C346DBAA1980D48C354005B441E2621A071ADA")
            throw new InvalidDataException("Use the accepted native Quick Drop bomb capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 9600 || rows.Any(row => row.Length != 21))
            throw new InvalidDataException("Incomplete quick-drop matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..5])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            bool power = seed[1] == "1", permanent = seed[2] == "1";
            int delay = int.Parse(seed[3]), blast = int.Parse(seed[4]);
            if ((int.Parse(seed[0]) * 4 + int.Parse(seed[1]) * 2 + int.Parse(seed[2])) * 20 + delay * 4 + blast != cases)
                throw new InvalidDataException("Reordered quick-drop cases.");
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
            for (int x = 62; x <= 66; x++)
            {
                int index = 16 * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, 0xf000);
                level.SetBehavior(index, permanent ? (byte)4 : (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1032; samus.YPosition = 233;
            samus.Kinematics.YSpeed = 3; samus.Kinematics.YDirection = 2;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FallingLeftPose : SamusPoseIds.FallingRightPose;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x0600 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[5]) != frame) throw new InvalidDataException("Reordered quick-drop frames.");
                ushort input = ushort.Parse(row[6], NumberStyles.HexNumber);
                ushort expected = frame >= delay && frame < 30 ? (ushort)(left ? 0x100 : 0x200) : (ushort)0;
                if (input != expected) throw new InvalidDataException("Changed bomb-case input.");
                if (frame == blast)
                    for (int x = 62; x <= 66; x++)
                        SamusBombProjectileSystem.CollectSingleBombedBlockReaction(
                            level, x, 16, new List<BombBlockReaction>(), runtime.Plms,
                            AreaId.Crateria, power ? (ushort)0x0300 : (ushort)0x0500);
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{level.GetCollisionBlockByIndex(16 * level.WidthInBlocks + 64).LevelWord:X4}";
                if (actual != string.Join(',', row[7..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"QUICK DROP {group.Key} frame={frame}: {actual} != {string.Join(',', row[7..])}");
                    reported = true;
                }
                // Every family/respawn/facing combination must include a real solid
                // contact followed by destruction, and not just pre-cleared terrain.
                if (blast == 1 && delay == 0 && frame == 0 &&
                    level.GetCollisionBlockByIndex(16 * level.WidthInBlocks + 64).LevelWord != 0xf000)
                    throw new InvalidDataException("Bomb fixture lost its initial solid floor.");
                if (blast == 1 && delay == 0 && frame == 1 &&
                    (samus.Kinematics.YFixed != 0x00edffff ||
                     samus.Kinematics.VerticalSpeedFixed != 0x00033800 ||
                     level.GetCollisionBlockByIndex(16 * level.WidthInBlocks + 64).LevelWord != 0x0053))
                    throw new InvalidDataException("Bomb contact no longer preserves Quick Drop velocity.");
                if (blast == 1 && delay == 0 && frame == 6 &&
                    (samus.Kinematics.YFixed != 0x00ff2fff ||
                     samus.Kinematics.VerticalSpeedFixed != 0x0003c400 ||
                     samus.ReadMovementType(bus) != SamusMovementType.Falling))
                    throw new InvalidDataException("Bomb Quick Drop lost velocity on turn completion.");
                if (blast == 1 && delay == 1 && frame == 2 &&
                    (samus.Kinematics.YFixed != 0x00ecffff ||
                     samus.Kinematics.VerticalSpeedFixed != 0 ||
                     samus.ReadMovementType(bus) != SamusMovementType.Falling))
                    throw new InvalidDataException("Late bomb turn no longer loses velocity and restarts falling.");
                frame++;
            }
            if (frame != 60) throw new InvalidDataException("Incomplete quick-drop case.");
            cases++;
        }
        if (cases != 160) throw new InvalidDataException("Incomplete quick-drop cases.");
        Console.WriteLine($"Quick-drop bomb: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
