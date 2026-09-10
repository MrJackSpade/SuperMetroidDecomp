using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Aerial neutral/shoot/aim launch transitions through the full gameplay dispatcher.</summary>
internal static class SparkAerialAudit
{
    public static int Run(string rom, string trace, bool restrictions = false)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            (restrictions ? "226191CABC7AD7862AC7CFDA2A01DEEDE039E2A264B2AD1B7087AF38A7080C1D" :
            "50BF582B79BD6EFBD70D861FB7319698AD57E722AC22BC9D75A83FEE0598E647"))
            throw new InvalidDataException("Use accepted aerial spark capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            if (!group.Any(row => int.Parse(row[3]) == (restrictions ? 36 : 30)))
                throw new InvalidDataException("Capture ended before the tested aerial input.");
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
            if (failures != 0) Console.WriteLine($"Aerial {group.Key}: {failures} mismatches.");
        }
        if (cases != (restrictions ? 48 : 24)) throw new InvalidDataException("Incomplete window matrix.");
        Console.WriteLine($"Spark aerial: {cases} cases, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
