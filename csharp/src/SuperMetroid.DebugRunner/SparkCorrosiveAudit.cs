using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Lava/acid shinespark travel, health and crash entry through the full gameplay dispatcher.</summary>
internal static class SparkCorrosiveAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "E779CABF32F0C8AB9E457583E22555638113DC1BBD80030A7906339C76B68638")
            throw new InvalidDataException("Use the accepted corrosive-travel capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            if (group.Last()[11] != "1") throw new InvalidDataException("Capture must reach crash entry.");
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
            samus.LiquidPhysics.ConfigureLavaAcid(8, acid: int.Parse(seed[2]) >= 2);
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
                    $"{samus.Health:X4},{(samus.Shinespark.Phase == ShinesparkPhase.Crash ? 1 : 0)},{samus.SubunitHealth:X4}";
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
        Console.WriteLine($"Spark corrosive: {cases} cases, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
