using System.Reflection;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Crash-entry retained-word diagnostic; does not claim combo-to-echo integration.</summary>
internal static class SparkRetainedAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "A448393C73B278C99EE0B6C9DDCD1B450A5808289F67950479C8C2D41997599A")
            throw new InvalidDataException("Use accepted spark-retained-466-v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var level = CartridgeRoomAssets.Load(bus, CartridgeRoomHeader.Load(bus, 0xcd13)).LevelData;
        int mismatches = 0, cases = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            string[] seed = group.First();
            var samus = new SamusState { XPosition = 192, YPosition = 192, Health = 29,
                Pose = seed[0] == "1" ? SamusPoseIds.ShinesparkHorizontalLeftPose : SamusPoseIds.ShinesparkHorizontalRightPose };
            // This seeds the modeled counterpart of retained $0ABC before the real
            // crash entry. It intentionally exposes an entry routine that resets it.
            typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.CrashAngularTravel))!
                .SetValue(samus.Shinespark, ushort.Parse(seed[1]));
            typeof(SamusShinesparkState).GetMethod("BeginCrash", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(samus.Shinespark, [bus, samus]);
            string[][] expected = group.ToArray();
            int frame = 0, differences = 0;
            for (; frame < 100 && samus.Shinespark.Phase != ShinesparkPhase.CrashFinish; frame++)
            {
                samus.Shinespark.Step(bus, level, samus, (ushort)frame);
                var state = samus.Shinespark;
                ushort handler = state.Phase switch
                {
                    ShinesparkPhase.Crash => 0xd346,
                    ShinesparkPhase.CrashEchoCircle => 0xd3f3,
                    ShinesparkPhase.CrashFinish => 0xd40d,
                    _ => throw new InvalidDataException("Unexpected crash phase.")
                };
                var echo = samus.HorizontalSpeed;
                string actual = $"{handler:X4},{((state.CrashSubphase << 8) | state.CrashRadius):X4}," +
                    $"{state.CrashAngularTravel:X4},{echo.FirstSpeedEchoXPosition:X4},{echo.FirstSpeedEchoYPosition:X4}," +
                    $"{echo.SecondSpeedEchoXPosition:X4},{echo.SecondSpeedEchoYPosition:X4}";
                if (frame >= expected.Length || actual != string.Join(',', expected[frame][3..])) differences++;
            }
            if (frame != expected.Length) differences++;
            Console.WriteLine($"Crash {group.Key}: managed={frame} native={expected.Length} calls, {differences} mismatches.");
            mismatches += differences;
            cases++;
        }
        if (cases != 8) throw new InvalidDataException("Incomplete retained-word matrix.");
        return mismatches == 0 ? 0 : 1;
    }
}
