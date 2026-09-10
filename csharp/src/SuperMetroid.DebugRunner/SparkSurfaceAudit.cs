using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Liquid-surface shinespark crossings, health and crash entry through the full gameplay dispatcher.</summary>
internal static class SparkSurfaceAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "94E65FA92F45E63FD67C329234E27ADCC435C210B5CB41CEC95A367F89C66963")
            throw new InvalidDataException("Use the accepted surface-travel capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            if (group.Last()[11] != "1") throw new InvalidDataException("Capture must reach crash entry.");
            if (seed[13] == "0000" || group.Last()[13] != "0000" ||
                !group.Any(row => row[13] == "0000" && row[11] == "0" &&
                    int.Parse(row[5], NumberStyles.HexNumber) is >= 0xc9 and <= 0xce))
                throw new InvalidDataException("Capture must leave liquid while the spark is still active.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            var level = runtime.LevelData!;
            for (int x = 0; x < 16; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y < 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + 15, 0x8000);
            }
            samus.Health = samus.MaxHealth = 999;
            if (int.Parse(seed[2]) < 2) samus.LiquidPhysics.ConfigureWater(128, 0x80);
            else samus.LiquidPhysics.ConfigureLavaAcid(128, acid: int.Parse(seed[2]) >= 4);
            samus.XPosition = 128;
            samus.Pose = seed[1] == "1" ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
            if ((int.Parse(seed[2]) & 1) != 0) samus.EquippedItems |= (ushort)SamusEquipmentFlags.GravitySuit;
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
                    $"{samus.XPosition:X4}{samus.Kinematics.XSubposition:X4},{samus.YPosition:X4}{samus.Kinematics.YSubposition:X4}," +
                    $"{samus.Health:X4},{(samus.Shinespark.Phase == ShinesparkPhase.Crash ? 1 : 0)},{samus.SubunitHealth:X4},{samus.LiquidPhysics.LiquidPhysicsType:X4}," +
                    $"{samus.Kinematics.YAcceleration:X4}{samus.Kinematics.YSubacceleration:X4}";
                if (actual != string.Join(',', row[5..]))
                {
                    if (failures == 0) Console.WriteLine($"First mismatch {group.Key} frame {frame}: {actual}; native {string.Join(',', row[5..])}");
                    failures++;
                }
            }
            cases++;
            differences += failures;
            if (failures != 0) Console.WriteLine($"Travel {group.Key}: {failures} mismatches.");
        }
        if (cases != 24) throw new InvalidDataException("Incomplete travel matrix.");
        Console.WriteLine($"Spark surface: {cases} cases, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
