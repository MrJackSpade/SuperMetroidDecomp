using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Real crumble PLM lifecycle with controller-driven falling turn comparisons.</summary>
internal static class QuickDropCrumbleAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "D790D2F30DCBCB6D8FC945B4F253594483B1DD681EEF8765F5ED044A3AC5CF24")
            throw new InvalidDataException("Use the accepted native crumble v3 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 3600 || rows.Any(row => row.Length != 19))
            throw new InvalidDataException("Incomplete quick-drop matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int delay = int.Parse(seed[1]), age = int.Parse(seed[2]);
            if (int.Parse(seed[0]) * 30 + delay * 6 + age != cases)
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
                level.SetForegroundEntry(index, 0xb000);
                if (!runtime.Plms.TrySpawnSamusContactCrumbleBlock(level, index, new RoomBlockBehavior(0)))
                    throw new InvalidDataException("Failed to spawn crumble fixture.");
            }
            for (int tick = 0; tick < age; tick++)
                runtime.Plms.Step(bus, level, runtime.BackgroundStreamer!, 0, 0, 0);
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
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered quick-drop frames.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expected = frame >= delay && frame < 30 ? (ushort)(left ? 0x100 : 0x200) : (ushort)0;
                if (input != expected) throw new InvalidDataException("Changed crumble input.");
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{level.GetCollisionBlockByIndex(16 * level.WidthInBlocks + 64).LevelWord:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"QUICK DROP {group.Key} frame={frame}: {actual} != {string.Join(',', row[5..])}");
                    reported = true;
                }
                // Exercise the timing distinction, not just two paths that both fall
                // through a block which has already disappeared before contact.
                if (age == 2 && delay == 0 && frame == 1 &&
                    (samus.Kinematics.YFixed != 0x00edffff ||
                     samus.Kinematics.VerticalSpeedFixed != 0x00033800 ||
                     level.GetCollisionBlockByIndex(16 * level.WidthInBlocks + 64).LevelWord != 0x0053))
                    throw new InvalidDataException("Turn did not preserve fall speed on the crumble contact frame.");
                if (age == 2 && delay == 0 && frame == 6 &&
                    (samus.Kinematics.YFixed != 0x00ff2fff ||
                     samus.Kinematics.VerticalSpeedFixed != 0x0003c400 ||
                     samus.ReadMovementType(bus) != SamusMovementType.Falling))
                    throw new InvalidDataException("Quick Drop did not retain velocity through turn completion.");
                if (age == 2 && delay == 1 && frame == 2 &&
                    (samus.Kinematics.YFixed != 0x00ecffff ||
                     samus.Kinematics.VerticalSpeedFixed != 0 ||
                     samus.ReadMovementType(bus) != SamusMovementType.Falling))
                    throw new InvalidDataException("Late-turn landing did not return to falling when support disappeared.");
                frame++;
            }
            if (frame != 60) throw new InvalidDataException("Incomplete quick-drop case.");
            cases++;
        }
        if (cases != 60) throw new InvalidDataException("Incomplete quick-drop cases.");
        Console.WriteLine($"Quick-drop crumble: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
