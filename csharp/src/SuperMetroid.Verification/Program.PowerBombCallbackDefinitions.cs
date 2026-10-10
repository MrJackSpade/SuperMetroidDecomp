using System.Text.RegularExpressions;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Finds the shared disassembly source directory by walking upward from the current directory for its bank A0 listing.</summary>
    private static string FindCallbackDisassemblySource()
    {
        // Linked worktrees share the separately checked-out diagnostic reference.
        for (DirectoryInfo? directory = new(Environment.CurrentDirectory);
             directory is not null; directory = directory.Parent)
        {
            string source = Path.Combine(directory.FullName, "upstream-disassembly", "src");
            if (File.Exists(Path.Combine(source, "bank_A0.asm")))
                return source;
        }
        throw new DirectoryNotFoundException(
            "Callback verification requires upstream-disassembly/src/bank_A0.asm " +
            "in this checkout or an ancestor containing the shared reference checkout.");
    }
    /// <summary>Compares native enemy-header Power Bomb callbacks with the ten bank-qualified literal-RTL identities and checks every bank/pointer pair.</summary>
    private static void VerifyPowerBombCallbackDefinitions(SuperMetroidAddressSpace rom)
    {
        int headers = 0;
        var literalCallbacks = new HashSet<int>();
        foreach (string line in File.ReadLines(Path.Combine(FindCallbackDisassemblySource(), "bank_A0.asm")))
        {
            Match match = Regex.Match(line, @"^EnemyHeaders_\w+:\s*;([0-9A-F]{6});");
            if (!match.Success)
                continue;
            int address = Convert.ToInt32(match.Groups[1].Value, 16);
            byte bank = rom.ReadByte(address + 12);
            ushort callback = (ushort)(rom.ReadByte(address + 40) | rom.ReadByte(address + 41) << 8);
            bool expected = callback != 0 && rom.ReadByte((bank << 16) | callback) == 0x6b;
            AssertEqual(expected, EnemyPowerBombCallbackDefinitions.IsLiteralNoOp(bank, callback),
                $"header {address:X6} native Power Bomb classification");
            if (expected)
                literalCallbacks.Add((bank << 16) | callback);
            headers++;
        }
        AssertTrue(headers > 150, "all named native enemy headers enumerated");
        AssertEqual(10, literalCallbacks.Count, "complete Power Bomb literal no-op identity set");
        // No other bank/pointer may accidentally alias a classified identity. A zero
        // pointer is always common damage, regardless of the bank's bytes at offset zero.
        for (int bank = 0; bank <= byte.MaxValue; bank++)
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(literalCallbacks.Contains((bank << 16) | pointer),
                EnemyPowerBombCallbackDefinitions.IsLiteralNoOp((byte)bank, (ushort)pointer),
                "Power Bomb bank-qualified callback identity");
        Console.WriteLine($"Power Bomb callbacks: {headers} named headers match native opcode classification; all 16777216 bank/pointer pairs preserve the ten literal no-ops and zero/common distinction.");
    }
}
