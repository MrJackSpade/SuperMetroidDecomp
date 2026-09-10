using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>Compare multi-tap and stutter trajectories with bounded cartridge execution.</summary>
internal static class ShortChargeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "2BA37E14FD5F3B7A762EF6A5F20520A98FF7C926A782FB3EC974A21C653A85DA")
            throw new InvalidDataException("Use the accepted native short-charge v2 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 16358 || rows.Any(row => row.Length != 17))
            throw new InvalidDataException("Incomplete short-charge matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => string.Join(',', row[..4])))
        {
            bool left = group.First()[0] == "1";
            int taps = int.Parse(group.First()[1]), pattern = int.Parse(group.First()[2]), shift = int.Parse(group.First()[3]);
            if ((((left ? 1 : 0) * 3 + taps - 2) * 7 + pattern) * 3 + shift + 1 != cases++)
                throw new InvalidDataException("Reordered short-charge cases.");
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
                if (int.Parse(row[4]) != frame) throw new InvalidDataException("Reordered short-charge frames.");
                ushort input = ushort.Parse(row[5], NumberStyles.HexNumber);
                int expected = store ? 0x8400 : ShortChargeInputs.At(frame, left, taps, pattern, shift);
                if (input != expected) throw new InvalidDataException("Changed short-charge input timeline.");
                var publication = new GameplayAudioFramePublication(audio);
                runtime.StepFrame(input,
                    queueEchoSound: () => publication.QueueEcho(runtime));
                publication.PublishPrefix(runtime);
                string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{(byte)samus.ReadMovementType(bus):X2}," +
                    $"{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.HorizontalSpeed.BaseFixed:X8}," +
                    $"{samus.HorizontalSpeed.ExtraRunSpeed:X4}{samus.HorizontalSpeed.ExtraRunSubspeed:X4},{samus.HorizontalSpeed.AccelerationMode:X4}," +
                    $"{samus.HorizontalSpeed.SpeedBoostCounter:X4},{samus.Shinespark.ShineTimer:X4}";
                if (actual != string.Join(',', row[6..]))
                {
                    mismatches++;
                    if (!reported) Console.WriteLine($"SHORT CHARGE {group.Key} frame={frame}: {actual} != {string.Join(',', row[6..])}");
                    reported = true;
                }
                store = (samus.HorizontalSpeed.SpeedBoostCounter & 0xff00) >= 0x0400;
                if (((pattern == 5 && frame == 1) || (pattern == 6 && frame == 40)) &&
                    (samus.ReadMovementType(bus) != SamusMovementType.Standing || samus.HorizontalSpeed.SpeedBoostCounter != 0))
                    throw new InvalidDataException("Early/long forward release failed to reset the running charge.");
                if (shift == 0 && pattern < 5)
                {
                    int expectedStage = frame < 25 ? 0 : frame < 50 ? 1 : frame < 70 ? 2 : frame < 85 ? 3 : 4;
                    if ((samus.HorizontalSpeed.SpeedBoostCounter >> 8) != expectedStage)
                        throw new InvalidDataException("Animation-linked charge stages changed.");
                    // Single-frame early taps advance charge without sustained dash
                    // acceleration. This checks the distinction the technique exploits.
                    if (taps == 4 && frame is 25 or 50 or 70 &&
                        ((samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed) != expectedStage * 0x1000)
                        throw new InvalidDataException("Charge stages incorrectly require accumulated dash speed.");
                }
                frame++;
            }
            if (samus.Shinespark.ShineTimer != 179 || samus.Shinespark.Phase != ShinesparkPhase.Stored)
                throw new InvalidDataException("The trajectory did not store a usable shinespark.");
            if (shift == 0 && pattern < 5 && frame != 87)
                throw new InvalidDataException("Published short charge missed the native storage frame.");
            if ((pattern >= 5 || shift == 1) && frame <= 87)
                throw new InvalidDataException("The failing input unexpectedly retained the short charging window.");
            if (shift == 0 && pattern < 5)
            {
                long distance = Math.Abs((long)samus.Kinematics.XFixed - (1024L << 16));
                if (distance != ShortChargeInputs.StoredDistance(taps, pattern))
                    throw new InvalidDataException("Short-charge fixed-point storage distance changed.");
            }

        }
        if (cases != 126) throw new InvalidDataException("Incomplete short-charge cases.");
        Console.WriteLine($"Short charge: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
