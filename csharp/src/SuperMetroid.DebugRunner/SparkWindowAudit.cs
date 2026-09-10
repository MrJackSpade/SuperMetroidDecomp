using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Grounded held-forward and one-frame-tap launch windows through the full gameplay dispatcher.</summary>
internal static class SparkWindowAudit
{
    public static int Run(string rom, string trace, bool tap = false)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            (tap ? "49165A9EFFD5CEB5C4625B2FCE77E171588A844B9C7B084D664F38A2CEA3FBD0" :
            "CA323D81D610223AA0C05579467F2359718231DCED8263DE394C4FEBE2D89A2D"))
            throw new InvalidDataException("Use accepted held/tap spark-window capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            var runtime = FlatFloorMovementFixture.Create(bus, water: seed[0] != "0");
            var samus = runtime.Samus!;
            samus.XPosition = 128;
            samus.Pose = seed[1] == "1" ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.SpeedBooster |
                (seed[0] == "2" ? SamusEquipmentFlags.GravitySuit : 0));
            int failures = 0;
            foreach (var row in group)
            {
                int frame = int.Parse(row[3]);
                if (frame == 20)
                {
                    samus.HorizontalSpeed.SpeedBoostCounter = SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter;
                    samus.Shinespark.TryStoreFromSpeedBooster(samus.HorizontalSpeed.SpeedBoostCounter);
                }
                runtime.StepFrame(ushort.Parse(row[4], NumberStyles.HexNumber));
                string actual = $"{samus.Pose:X4},{samus.Shinespark.ShineTimer:X4},{samus.Shinespark.StartStopTimer:X4}," +
                    $"{samus.XPosition:X4}{samus.Kinematics.XSubposition:X4},{samus.YPosition:X4}{samus.Kinematics.YSubposition:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    if (failures == 0) Console.WriteLine($"First mismatch {group.Key} frame {frame}: {actual}; native {string.Join(',', row[5..])}");
                    failures++;
                }
            }
            cases++;
            differences += failures;
            if (failures != 0) Console.WriteLine($"Window {group.Key}: {failures} mismatches.");
        }
        if (cases != 216) throw new InvalidDataException("Incomplete window matrix.");
        Console.WriteLine($"Spark window: {cases} cases, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
