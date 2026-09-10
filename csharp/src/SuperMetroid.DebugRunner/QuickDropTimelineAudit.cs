using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Controller-driven turnaround entry, ceiling contact, and animation completion.</summary>
internal static class QuickDropTimelineAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "DB3FBBF371C2D0152072C3D7F2F9BA23A2B939A7D5A6AD608D4F12DA6ADDC700")
            throw new InvalidDataException("Use the accepted native Quick Drop timeline capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 24640 || rows.Any(row => row.Length != 18))
            throw new InvalidDataException("Incomplete quick-drop matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int delay = int.Parse(seed[1]), remove = int.Parse(seed[2]);
            if (int.Parse(seed[0]) * 154 + delay * 14 + remove != cases)
                throw new InvalidDataException("Reordered quick-drop cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y is 12 or 16 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
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
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered quick-drop frames.");
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                ushort expected = frame >= 8 && frame < 50 ? (ushort)0x80 : (ushort)0;
                if (frame >= 8 + delay && frame < 50) expected |= (ushort)(left ? 0x100 : 0x200);
                if (input != expected) throw new InvalidDataException("Changed quick-drop inputs.");
                if (remove < 13 && frame == 8 + remove)
                    for (int x = 0; x < level.WidthInBlocks; x++) level.SetForegroundEntry(12 * level.WidthInBlocks + x, 0);
                runtime.StepFrame(input);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"QUICK DROP {group.Key} frame={frame}: {actual} != {string.Join(',', row[5..])}");
                    reported = true;
                }
                // These named witnesses ensure the matrix actually contains a successful
                // turn, its one-frame-late failure, and normal ceiling response after
                // animation completion. Matching two equally inactive setups is not enough.
                if (delay == 2 && remove == 4 && frame == 16 &&
                    (samus.Kinematics.YFixed != 0x00ccd000 ||
                     samus.Kinematics.VerticalSpeedFixed != 0x00041c00 ||
                     samus.ReadMovementType(bus) != SamusMovementType.NormalJumping))
                    throw new InvalidDataException("Successful ceiling continuation did not survive turn completion.");
                if (delay == 3 && remove == 4 && frame == 11 &&
                    (samus.Kinematics.YFixed != 0x00e30000 ||
                     samus.Kinematics.VerticalSpeedFixed != 0 || samus.Kinematics.YDirection != 2))
                    throw new InvalidDataException("One-frame-late turn no longer fails at the ceiling.");
                if (delay == 2 && remove == 13 && frame == 17 &&
                    (samus.Kinematics.YFixed != 0x00e30000 ||
                     samus.Kinematics.VerticalSpeedFixed != 0 || samus.Kinematics.YDirection != 2))
                    throw new InvalidDataException("Persistent ceiling did not stop the completed turn.");
                frame++;
            }
            if (frame != 80) throw new InvalidDataException("Incomplete quick-drop case.");
            cases++;
        }
        if (cases != 308) throw new InvalidDataException("Incomplete quick-drop cases.");
        Console.WriteLine($"Quick-drop timeline: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
