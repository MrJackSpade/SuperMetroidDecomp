using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>Compare actual air-to-liquid morph trajectories with bounded cartridge execution.</summary>
internal static class WaterballComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "4826266B38AC11C60A7556B8DA35385DAC9736FAE3DEE00192113164FCFEE4EC")
            throw new InvalidDataException("Use the accepted native waterball v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 32400 || rows.Any(row => row.Length != 20))
            throw new InvalidDataException("Incomplete waterball matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            bool left = group.First()[0] == "1";
            int medium = int.Parse(group.First()[1]), run = int.Parse(group.First()[2]), timing = int.Parse(group.First()[3]);
            if ((((left ? 1 : 0) * 3 + medium) * 2 + run) * 9 + timing != cases++)
                throw new InvalidDataException("Reordered waterball cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int index = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(index, y == 40 || x == 0 || x == 143 || (y == 32 && (left ? x >= 88 : x < 56)) ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(index, 0);
            }
            runtime.Plms.Reset();
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster);
            samus.EquippedBeams = 0; samus.Health = samus.MaxHealth = 1499;
            samus.XPosition = left ? (ushort)2176 : (ushort)128; samus.YPosition = 491;
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
            if (medium == 0) samus.LiquidPhysics.ConfigureWater(560, 0x80);
            else samus.LiquidPhysics.ConfigureLavaAcid(560, acid: medium == 2);
            var audio = new CartridgeAudioState();
            foreach (var row in group)
            {
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered waterball frames.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                int launch = run == 1 ? 96 : 64, first = launch + 10, second = launch + 20 + timing * 2;
                int forward = left ? 0x200 : 0x100;
                int expected = frame < launch ? 0x8000 | forward :
                    (frame == launch + 8 ? 0 : 0x80) | (frame == first || frame == second ? 0x400 : 0) |
                    (frame < first || frame > second ? forward : 0);
                if (input != expected) throw new InvalidDataException("Changed waterball input timeline.");
                var publication = new GameplayAudioFramePublication(audio);
                runtime.StepFrame(input,
                    queueEchoSound: () => publication.QueueEcho(runtime));
                publication.PublishPrefix(runtime);
                if (run == 1 && timing == 6)
                    WaterballSequenceAssertions.Verify(samus, medium, frame);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.HorizontalSpeed.SpeedBoostCounter:X4},{samus.MorphBallBounceState:X4}," +
                    $"{samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.LiquidPhysics.LiquidPhysicsType:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"WATERBALL {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                frame++;
            }
            if (frame != 300) throw new InvalidDataException("Truncated waterball trajectory.");
        }
        if (cases != 108) throw new InvalidDataException("Incomplete waterball cases.");
        Console.WriteLine($"Waterball: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
