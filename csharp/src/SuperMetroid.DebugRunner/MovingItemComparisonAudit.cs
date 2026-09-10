using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Complete moving-item trajectories against original cartridge execution.</summary>
internal static class MovingItemComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "D181C67647DD84414C4BA273E45FE89B43F27534D9422219C3A3DD308901FEBA")
            throw new InvalidDataException("Use the accepted native moving-item v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 6402 || rows.Any(row => row.Length != 25))
            throw new InvalidDataException("Incomplete moving-item matrix.");
        int mismatches = 0, cases = 0, remoteTriggers = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            var seed = group.First();
            bool left = seed[0] == "1";
            int speed = int.Parse(seed[1]), gap = int.Parse(seed[2]);
            int delay = int.Parse(seed[3]);
            int caseIndex = (((left ? 1 : 0) * 3 + speed) * 17 + gap) * 9 + delay;
            if (caseIndex != cases)
                throw new InvalidDataException("Reordered moving-item cases.");
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
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = (ushort)(left ? 997 + gap : 1051 - gap); samus.YPosition = 472;
            samus.Kinematics.YDirection = 2; samus.HorizontalSpeed.AccelerationMode = 2;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.HorizontalSpeed.ExtraRunSpeed = (ushort)(speed * 2);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(0x0300 | (left ? 4 : 8));
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            var selected = new RemoteItemPopulation(bus, (byte)(left ? 61 : 66), 29);
            if (runtime.Plms.LoadRoomPopulation(selected, level, runtime.BackgroundStreamer!, runtime.Vram,
                RemoteItemFixtureData.PopulationPointer, runtime.System, areaIndex: AreaId.Crateria,
                getSamus: () => samus, isAreaTorizoDefeated: () => false) != 1)
                throw new InvalidDataException("Expected one moving-item owner.");
            runtime.Plms.Step(selected, level, runtime.BackgroundStreamer!, 0, 0, 0);
            runtime.Controller1.Latch((ushort)(0x80 | (left ? 0x200 : 0x100)));
            int frame = 0; bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered moving-item frames.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                ushort direction = (ushort)(left ? 0x200 : 0x100);
                if (frame >= delay) direction = (ushort)(left ? 0x100 : 0x200);
                ushort expected = (ushort)(0x80 | direction);
                if (input != expected) throw new InvalidDataException("Changed moving-item input timeline.");
                // The managed hitbox is eager after a pose change; cartridge alpha
                // refreshes its radius latch before movement. Sample that same boundary.
                ushort movementXRadius = samus.Kinematics.XRadius, movementYRadius = samus.Kinematics.YRadius;
                runtime.StepFrame(input);
                bool triggered = runtime.Plms.CollectiblePickupEvents.Count != 0;
                bool outsideItem = left ? samus.XPosition - samus.Kinematics.XRadius >= MovingDoorFixtureData.DoorEdge(left)
                    : samus.XPosition + samus.Kinematics.XRadius - 1 < MovingDoorFixtureData.DoorEdge(left);
                if (triggered && outsideItem) remoteTriggers++;
                if (triggered && frame != group.Count() - 1)
                    throw new InvalidDataException("Managed item triggered before the native terminal frame.");
                int lastGap = MovingDoorFixtureData.LastSuccessfulGap(left);
                if (speed == 0 && delay == 0 && frame == 1 && gap == lastGap &&
                    (!triggered || !outsideItem || samus.HorizontalSpeed.BaseFixed != MovingDoorFixtureData.TriggerBaseSpeed ||
                     samus.HorizontalSpeed.AccelerationMode != 1))
                    throw new InvalidDataException("Remote spin-turn trigger lost its distance or momentum witness.");
                if (speed == 0 && delay == 0 && gap == lastGap + 1 && triggered)
                    throw new InvalidDataException("Adjacent out-of-range turn unexpectedly triggered the item.");
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4},{samus.ReadPoseXDirection(bus):X2}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{runtime.Projectiles.FlareCounter:X4},{movementXRadius:X4},{movementYRadius:X4},{(triggered ? (byte)runtime.Plms.CollectiblePickupEvents[0].MessageBoxIndex : 0):X4},{samus.Missiles:X4},{samus.MaxMissiles:X4},{(runtime.System.HasCollectedItemBit(0) ? 1 : 0):X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported && mismatches < 30) Console.WriteLine($"MOVING ITEM {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 24 && group.Last()[21] == "0000") throw new InvalidDataException("Truncated non-triggering moving-item case.");
            cases++;
        }
        if (cases != 918) throw new InvalidDataException("Incomplete moving-item cases.");
        if (remoteTriggers != 390) throw new InvalidDataException("The matrix no longer exercises the expected remote item triggers.");
        Console.WriteLine($"Moving item: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }

}
