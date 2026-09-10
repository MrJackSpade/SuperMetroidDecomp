using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Sand shinespark attempts, health and crash entry through the full gameplay dispatcher.</summary>
internal static class SparkSandAudit
{
    public static int Run(string rom, string trace, bool entry = false)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            (entry ? "E83A95054A51FAC0AF7877467D65EAF8D748F70D64A2C2254D967F27A815A89E" :
            "BA0837A6A362939F04A8C2A6B76B119883930D1A82E2B0C14FA636AA30FE9EE5"))
            throw new InvalidDataException("Use the accepted sand-travel capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..3])))
        {
            var seed = group.First();
            bool gravity = (int.Parse(seed[2]) & 1) != 0;
            if (entry && !group.Any(row =>
                {
                    int x = int.Parse(row[8][..4], NumberStyles.HexNumber) >> 4;
                    int y = int.Parse(row[9][..4], NumberStyles.HexNumber) >> 4;
                    int pose = int.Parse(row[5], NumberStyles.HexNumber);
                    return pose is >= 0xc9 and <= 0xce && x is >= 1 and < 15 &&
                        y is >= 8 and < 16 && (y < 13 || (seed[1] == "1" ? x <= 4 : x >= 11));
                }))
                throw new InvalidDataException("Entry capture never places an active spark inside sand.");
            if (entry || gravity ? group.Last()[11] != "1" :
                group.Last()[3] != "255" || group.Last()[6] != "0000" ||
                group.Any(row => ushort.Parse(row[5], NumberStyles.HexNumber) is >= 0xc9 and <= 0xce))
                throw new InvalidDataException("Sand capture must prove launch termination or suitless expiry without launch.");
            var runtime = FlatFloorMovementFixture.Create(bus, water: true);
            var samus = runtime.Samus!;
            var level = runtime.LevelData!;
            for (int x = 0; x < 16; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y < 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + 15, 0x8000);
            }
            // Synthetic geometry needs Maridia's area-specific BTS table. No retail room
            // content is being claimed; change only this fixture's header identity.
            typeof(SuperMetroidRuntime).GetProperty(nameof(SuperMetroidRuntime.ActiveRoom))!
                .SetValue(runtime, runtime.ActiveRoom! with { AreaIndex = AreaId.Maridia });
            for (int y = entry ? 8 : 13; y < 16; y++)
                for (int x = 1; x < 15; x++)
                {
                    if (entry && y >= 13 && (seed[1] == "1" ? x > 4 : x < 11)) continue;
                    int index = y * level.WidthInBlocks + x;
                    level.SetForegroundEntry(index, 0x3000);
                    level.SetBehavior(index, (byte)(int.Parse(seed[2]) < 2 ? 0x82 : 0x83));
                }
            samus.Health = samus.MaxHealth = 99;
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
        if (cases != 24) throw new InvalidDataException("Incomplete travel matrix.");
        Console.WriteLine($"Spark sand: {cases} cases, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
