using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>Compare delayed held-dash trajectories with bounded cartridge execution.</summary>
internal static class QuickChargeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "808E12F14CD82102B5955A20BFC567D6D4C4ECCE3710BD60EC1A887ACB671A8A")
            throw new InvalidDataException("Use the accepted native quick-charge v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 8082 || rows.Any(row => row.Length != 15))
            throw new InvalidDataException("Incomplete quick-charge matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..2])))
        {
            bool left = group.First()[0] == "1";
            int delay = int.Parse(group.First()[1]);
            if ((left ? 41 : 0) + delay != cases++)
                throw new InvalidDataException("Reordered quick-charge cases.");
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
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 99;
            samus.XPosition = 1024; samus.YPosition = 491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(left ? 4 : 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int frame = 0; bool reported = false;
            bool store = false;
            var audio = new CartridgeAudioState();
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != frame) throw new InvalidDataException("Reordered quick-charge frames.");
                ushort input = ushort.Parse(row[3], NumberStyles.HexNumber);
                int expected = store ? 0x8400 : (left ? 0x200 : 0x100) | (frame >= delay ? 0x8000 : 0);
                if (input != expected) throw new InvalidDataException("Changed quick-charge input timeline.");
                var publication = new GameplayAudioFramePublication(audio);
                runtime.StepFrame(input,
                    queueEchoSound: () => publication.QueueEcho(runtime));
                publication.PublishPrefix(runtime);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.HorizontalSpeed.SpeedBoostCounter:X4},{samus.Shinespark.ShineTimer:X4}";
                if (actual != string.Join(',', row[4..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"QUICK CHARGE {group.Key} frame={frame}: {actual} != {string.Join(',', row[4..])}");
                    reported = true;
                }
                store = (samus.HorizontalSpeed.SpeedBoostCounter & 0xff00) >= 0x0400;
                frame++;
            }
            if (samus.Shinespark.ShineTimer != 179 || samus.Shinespark.Phase != ShinesparkPhase.Stored)
                throw new InvalidDataException("The trajectory did not store a usable shinespark.");
            // The neighboring delay misses the first charging window, not merely
            // an animation endpoint. Assert the actual storage frame and distance.
            int? expectedFrame = delay switch { 0 => 91, 25 => 87, 26 => 116, _ => null };
            uint? expectedDistance = delay switch { 0 => 0x01e74000u, 25 => 0x01566000u, 26 => 0x022c0000u, _ => null };
            long distance = Math.Abs((long)samus.Kinematics.XFixed - (1024L << 16));
            if (expectedFrame is not null && (frame != expectedFrame || distance != expectedDistance))
                throw new InvalidDataException("Quick-charge distance/timing boundary changed.");
        }
        if (cases != 82) throw new InvalidDataException("Incomplete quick-charge cases.");
        Console.WriteLine($"Quick charge: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
