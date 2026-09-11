using System.Reflection;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;

/// <summary>Compare the real production fade callbacks to independently executed 65816 wrappers.</summary>
internal static class PhantoonFadeComparisonAudit
{
    public static int Run(string rom, string csv)
    {
        byte[] capture = File.ReadAllBytes(csv);
        if (Convert.ToHexString(SHA256.HashData(capture)) != "A5C86B65561B3CC7695125294D5F54DB41A636F85E711949344FEF9C7883E2A0")
            throw new InvalidDataException("Unexpected original-CPU fade fixture; review its provenance before replacing the digest.");
        var input = File.ReadLines(csv).Skip(1).Select(line => line.Split(',').Select(int.Parse).ToArray()).ToArray();
        int words = 0, calls = 0;
        foreach (var scenario in input.GroupBy(row => (FadeIn: row[0] != 0, Denominator: row[1], Health: row[2])))
        {
            var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
            var boss = runtime.Enemies.Phantoon!;
            boss.Body.Health = (ushort)scenario.Key.Health;
            boss.Eye!.VariableE = boss.Eye.VariableF = 0;
            int band = Math.Min(7, (scenario.Key.Health - 1) / 312);
            for (int color = 0; color < 16; color++)
            {
                int address = 0xa7cb41 + band * 32 + color * 2;
                ushort initial = scenario.Key.FadeIn ? (ushort)0 : (ushort)(runtime.AddressSpace.ReadByte(address) |
                    runtime.AddressSpace.ReadByte(address + 1) << 8);
                runtime.Cgram.SetColor(112 + color, initial);
            }
            // Keep the callback private in production. Reflection here invokes that exact
            // implementation, not a retyped C# oracle or a diagnostic-only approximation.
            var callback = typeof(RoomEnemySystem).GetMethod(scenario.Key.FadeIn ? "AdvancePhantoonFadeIn" : "AdvancePhantoonFadeOut",
                BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new MissingMethodException("Phantoon fade callback");
            foreach (var frame in scenario.GroupBy(row => row[3]))
            {
                object[] arguments = scenario.Key.FadeIn
                    ? [boss.Body, boss, (ushort)scenario.Key.Denominator, (byte)frame.Key]
                    : [boss, (ushort)scenario.Key.Denominator, (byte)frame.Key];
                callback.Invoke(runtime.Enemies, arguments);
                calls++;
                foreach (var row in frame)
                {
                    if (boss.Eye.VariableE != row[4] || boss.Eye.VariableF != row[5] || runtime.Cgram.Colors[112 + row[6]] != row[7])
                        throw new InvalidDataException($"Native fade mismatch: {scenario.Key}, frame {frame.Key}, color {row[6]}; " +
                            $"numerator {boss.Eye.VariableE}/{row[4]}, complete {boss.Eye.VariableF}/{row[5]}, color {runtime.Cgram.Colors[112 + row[6]]}/{row[7]}.");
                    words++;
                }
            }
        }
        if (calls != 800 || words != 12800) throw new InvalidDataException("Incomplete native fade comparison.");
        Console.WriteLine($"Phantoon fades: {calls} original-CPU calls, {words} exact colors, numerator and completion timing match.");
        return 0;
    }
}
