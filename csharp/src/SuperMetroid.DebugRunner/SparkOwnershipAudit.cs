using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Independent drawing-word and projectile ownership comparison for #466.</summary>
internal static class SparkOwnershipAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "25FFBFE7013658F9123ED41F67928C3A410EDD256E450B160432654C0F251D5F")
            throw new InvalidDataException("Use accepted spark-ownership-466-v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var level = CartridgeRoomAssets.Load(bus, CartridgeRoomHeader.Load(bus, 0xcd13)).LevelData;
        int mismatches = 0, records = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            string[] seed = group.First();
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                Pose = SamusPoseIds.ShinesparkHorizontalRightPose,
                EquippedBeams = (ushort)(0x1000 | ushort.Parse(seed[0])),
                PowerBombs = 2, MaxPowerBombs = 2, SelectedHudItem = 3 };
            var projectiles = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            int differences = 0;
            foreach (string[] row in group)
            {
                int stage = int.Parse(row[2]);
                if (stage < 2)
                {
                    bool combo = stage == (seed[1] == "0" ? 0 : 1);
                    if (combo)
                    {
                        if (!projectiles.TryActivateCombo(bus, samus, bombs, out _))
                            throw new InvalidDataException("Expected combo allocation.");
                    }
                    else
                    {
                        typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.Phase))!
                            .SetValue(samus.Shinespark, ShinesparkPhase.CrashFinish);
                        samus.Shinespark.Step(bus, level, samus, 0, projectiles.ProjectileCounter);
                    }
                }
                else
                {
                    // Only the departing handlers advance, matching the native boundary
                    // fixture. Ordinary combo trajectory is covered by its separate audits.
                    samus.Shinespark.StepReleasedCrashEchoProjectiles(bus, samus, 0, 0);
                }
                var first = samus.Shinespark.FirstReleasedCrashEcho;
                var second = samus.Shinespark.SecondReleasedCrashEcho;
                // Raw pre-instruction addresses are recorded for diagnosis. Comparison
                // here targets counter/type ownership and independent drawing words.
                string expected = string.Join(',', new[] { row[3], row[4], row[6], row[7], row[8],
                    row[9], row[11], row[12], row[13] });
                string actual = $"{projectiles.ProjectileCounter:X4},{projectiles.Slots[3].Type:X4}," +
                    $"{(first.Active ? 64 : 0):X4},{first.XPosition:X4},{first.YPosition:X4}," +
                    $"{projectiles.Slots[4].Type:X4},{(second.Active ? 64 : 0):X4},{second.XPosition:X4},{second.YPosition:X4}";
                if (actual != expected) differences++;
                records++;
            }
            Console.WriteLine($"Ownership {group.Key}: {differences} mismatches.");
            mismatches += differences;
        }
        if (records != 176) throw new InvalidDataException("Incomplete ownership matrix.");
        Console.WriteLine($"Spark ownership: {records} records, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
