using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete ceiling-door trajectories against original cartridge execution.</summary>
internal static class CeilingDoorComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "54C2AA986B8EC1BA37964BFF23C5700CF98DDC5BA0F18D4619FF97A48A5396DF")
            throw new InvalidDataException("Use the accepted native ceiling-door v3 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 993 || rows.Any(row => row.Length != 22))
            throw new InvalidDataException("Incomplete ceiling-door matrix.");
        int mismatches = 0, cases = 0, remoteTriggers = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int speed = int.Parse(seed[1]), ceiling = int.Parse(seed[2]);
            int height = int.Parse(seed[3]);
            int caseIndex = (((left ? 1 : 0) * 3 + speed) * 2 + ceiling) * 9 + height;
            if (caseIndex != cases)
                throw new InvalidDataException("Reordered ceiling-door cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 48 ? (ushort)0x8000 : y == 28 && x == (left ? 63 : 64) ? (ushort)0x9000 : y == 28 && ceiling != 0 && x == (left ? 64 : 63) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = (ushort)(476 + height);
            samus.Kinematics.YDirection = 1; samus.Kinematics.YSpeed = 4; samus.HorizontalSpeed.AccelerationMode = 2;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.HorizontalSpeed.ExtraRunSpeed = (ushort)(speed * 2);
            samus.HorizontalSpeed.HasRunningMomentum = true;
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x0300 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch((ushort)(0x80 | (left ? 0x200 : 0x100)));
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered ceiling-door frames.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort direction = (ushort)(left ? 0x200 : 0x100);
                ushort expected = (ushort)(0x80 | direction);
                if (input != expected) throw new InvalidDataException("Changed ceiling-door input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                bool triggered = level.PendingDoorTransition is not null;
                bool outsideDoor = samus.YPosition - movementYRadius >= 464;
                if (triggered && ceiling != 0 && outsideDoor) remoteTriggers++;
                if (triggered && frame != group.Count() - 1)
                    throw new InvalidDataException("Door triggered before native terminal frame.");
                if (ceiling != 0 && triggered && outsideDoor &&
                    (samus.HorizontalSpeed.ExtraRunSpeed != speed * 2 || samus.HorizontalSpeed.BaseFixed != 0x16000))
                    throw new InvalidDataException("Ceiling remote trigger lost horizontal momentum.");
                if (!left && speed == 0 && ceiling == 1 && height == 4 && frame == 1 &&
                    (!triggered || !outsideDoor || samus.YPosition != 476))
                    throw new InvalidDataException("The ceiling-first-frame witness lost its remote trigger.");
                if (!left && speed == 0 && ceiling == 1 && height == 3 && triggered)
                    throw new InvalidDataException("Adjacent ceiling-first rejection unexpectedly triggered.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4},{level.PendingDoorTransition?.Pointer ?? 0:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"CEILING DOOR {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 64 && group.Last()[21] == "0000") throw new InvalidDataException("Truncated non-triggering ceiling-door case.");
            cases++;
        }
        if (cases != 108) throw new InvalidDataException("Incomplete ceiling-door cases.");
        if (remoteTriggers != 12) throw new InvalidDataException("The matrix no longer exercises the expected remote door triggers.");
        Console.WriteLine($"Ceiling door: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

}
