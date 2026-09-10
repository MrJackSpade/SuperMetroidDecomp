using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Original-CPU comparison of departing echo radius, position and deletion.</summary>
internal static class SparkDepartureAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "DF0DDF8BDD56259EB2EBEFA8F288FA975625D35192AA6E3EF0B86E1AEFCF534A")
            throw new InvalidDataException("Use accepted spark-departure-466-v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var level = CartridgeRoomAssets.Load(bus, CartridgeRoomHeader.Load(bus, 0xcd13)).LevelData;
        int differences = 0, records = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => row[0]))
        {
            var samus = new SamusState { XPosition = 128, YPosition = 128, Pose = byte.Parse(group.Key) };
            var projectiles = new SamusProjectileSystem();
            // Only the handler boundary is seeded. Initialization, movement and viewport
            // deletion below execute production code using the pinned cartridge tables.
            typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.Phase))!
                .SetValue(samus.Shinespark, ShinesparkPhase.CrashFinish);
            samus.Shinespark.Step(bus, level, samus, 0, projectiles: projectiles);
            int frame = 0, caseDifferences = 0;
            foreach (string[] row in group)
            {
                int expectedFrame = int.Parse(row[1]);
                if (expectedFrame != frame)
                {
                    for (int slot = 4; slot >= 3; slot--)
                        if (projectiles.Slots[slot].PreInstruction == SamusProjectilePreInstruction.ShinesparkEcho)
                            projectiles.StepShinesparkEcho(bus, samus, projectiles.Slots[slot], 0, 0);
                    frame = expectedFrame;
                }
                var echo = row[2] == "3" ? samus.Shinespark.FirstReleasedCrashEcho : samus.Shinespark.SecondReleasedCrashEcho;
                string actual = $"{(echo.Active ? 1 : 0)},{echo.Radius:X4},{echo.XPosition:X4},{echo.YPosition:X4}";
                if (actual != string.Join(',', row[3..])) caseDifferences++;
                records++;
            }
            Console.WriteLine($"Departure pose {int.Parse(group.Key):X2}: {caseDifferences} mismatches.");
            differences += caseDifferences;
        }
        if (records != 480) throw new InvalidDataException("Incomplete departure matrix.");
        Console.WriteLine($"Departure: {records} records, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
