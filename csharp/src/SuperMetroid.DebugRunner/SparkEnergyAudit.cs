using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Dry/water shinespark travel, health and crash entry through the full gameplay dispatcher.</summary>
internal static class SparkEnergyAudit
{
    public static int Run(string rom, string trace, bool water = false)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            (water ? "59406F8B686F99FD126F88D170A7EBAE9BC58039D8297C73BCE5E580B01193D9" :
            "E33BA87405556B25520B2DF9B8A3E428295A4D7AA51F980981F0FB25D02C2BDE"))
            throw new InvalidDataException("Use the accepted dry-energy or water-travel capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            if (group.Last()[11] != "1") throw new InvalidDataException("Capture must reach crash entry.");
            var runtime = FlatFloorMovementFixture.Create(bus, water);
            var samus = runtime.Samus!;
            var level = runtime.LevelData!;
            for (int x = 0; x < 16; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y < 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + 15, 0x8000);
            }
            samus.Health = water ? (ushort)99 : new ushort[] { 1, 28, 29, 30, 31, 99 }[int.Parse(seed[2])];
            samus.XPosition = 128;
            samus.Pose = seed[1] == "1" ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
            if (water && seed[2] == "1") samus.EquippedItems |= (ushort)SamusEquipmentFlags.GravitySuit;
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
                    $"{samus.Health:X4},{(samus.Shinespark.Phase == ShinesparkPhase.Crash ? 1 : 0)}";
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
        if (cases != (water ? 12 : 36)) throw new InvalidDataException("Incomplete travel matrix.");
        Console.WriteLine($"Spark energy: {cases} cases, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
