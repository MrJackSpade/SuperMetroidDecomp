using System.Reflection;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Successive crash handlers with real projectile alpha/instruction processing.</summary>
internal static class SparkSequenceAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "ABF726C92E5A57AEA03FB9F3A015BF42C4400ED7C0DC6C8C00C55B3AE7827856")
            throw new InvalidDataException("Use accepted spark-sequence-466-v2 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var level = CartridgeRoomAssets.Load(bus, CartridgeRoomHeader.Load(bus, 0xcd13)).LevelData;
        int mismatches = 0, records = 0, cases = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..4])))
        {
            string[] seed = group.First();
            byte pose = seed[1] == "1" ? SamusPoseIds.ShinesparkHorizontalLeftPose : SamusPoseIds.ShinesparkHorizontalRightPose;
            var samus = new SamusState { XPosition = 128, YPosition = 128, Pose = pose,
                EquippedBeams = (ushort)(0x1000 | ushort.Parse(seed[0])),
                PowerBombs = 2, MaxPowerBombs = 2, SelectedHudItem = 3 };
            var projectiles = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            void Alpha() => projectiles.StepFrame(bus, level, samus, 0, 0, 0, 0, bombs,
                projectileProducerEnabled: false);
            if (seed[3] == "1")
            {
                typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.Phase))!
                    .SetValue(samus.Shinespark, ShinesparkPhase.CrashFinish);
                samus.Shinespark.Step(bus, level, samus, 0, projectiles: projectiles);
            }
            if (seed[0] != "0" && !projectiles.TryActivateCombo(bus, samus, bombs, out _))
                throw new InvalidDataException("Expected combo allocation.");
            for (int i = 0; i < int.Parse(seed[2]); i++) Alpha();
            foreach (var spark in group.GroupBy(row => row[4]))
            {
                cases++;
                samus.Pose = pose;
                samus.Health = 29;
                typeof(SamusShinesparkState).GetMethod("BeginCrash", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(samus.Shinespark, [bus, samus]);
                string[][] expected = spark.ToArray();
                int frame = 0, differences = 0;
                for (; frame < 100; frame++)
                {
                    Alpha();
                    bool finish = samus.Shinespark.Phase == ShinesparkPhase.CrashFinish;
                    samus.Shinespark.Step(bus, level, samus, 0, projectiles: projectiles);
                    var state = samus.Shinespark;
                    ushort handler = state.Phase switch
                    {
                        ShinesparkPhase.Inactive => 0,
                        ShinesparkPhase.Crash => 0xd346,
                        ShinesparkPhase.CrashEchoCircle => 0xd3f3,
                        ShinesparkPhase.CrashFinish => 0xd40d,
                        _ => throw new InvalidDataException("Unexpected crash phase.")
                    };
                    var first = state.FirstReleasedCrashEcho;
                    var second = state.SecondReleasedCrashEcho;
                    string actual = $"{handler:X4},{projectiles.ProjectileCounter:X4},{((state.CrashSubphase << 8) | state.CrashRadius):X4}," +
                        $"{first.XPosition:X4},{first.YPosition:X4},{(first.Active ? 64 : 0):X4}," +
                        $"{second.XPosition:X4},{second.YPosition:X4},{(second.Active ? 64 : 0):X4}," +
                        string.Concat(projectiles.Slots.Select(slot => $"{slot.Type:X4}"));
                    if (frame >= expected.Length || actual != string.Join(',', expected[frame][6..]))
                    {
                        if (differences == 0)
                            Console.WriteLine($"First difference {group.Key}/{spark.Key}/{frame}: {actual}; native={(frame < expected.Length ? string.Join(',', expected[frame][6..]) : "ended")}");
                        differences++;
                    }
                    records++;
                    if (finish) { frame++; break; }
                }
                if (frame != expected.Length) differences++;
                Console.WriteLine($"Sequence {group.Key} spark {spark.Key}: {frame}/{expected.Length} calls, {differences} mismatches.");
                mismatches += differences;
                for (int i = 0; i < 20; i++) Alpha();
            }
        }
        if (cases != 180) throw new InvalidDataException("Incomplete successive-spark matrix.");
        if (records != 11688) mismatches++;
        Console.WriteLine($"Spark sequences: {records} records, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
